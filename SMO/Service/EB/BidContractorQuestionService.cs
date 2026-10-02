using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using NHibernate.Criterion;
using SMO.Service.CM;
using NHibernate.Linq;
using SMO.Repository.Implement.CM;
using System.Data.SqlTypes;

namespace SMO.Service.EB
{
    public class BidContractorQuestionService : GenericService<T_EB_BID_CONTRACTOR_QUESTION, BidContractorQuestionRepo>
    {
        public List<T_EB_BID_CONTRACTOR_EMAIL> ObjListEmail { get; set; }
        public BidContractorQuestionService() : base()
        {
            ObjListEmail = new List<T_EB_BID_CONTRACTOR_EMAIL>();
        }

        public void GetListEmail(string bidId, string type)
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjListEmail = UnitOfWork.Repository<BidContractorEmailRepo>().Search(new T_EB_BID_CONTRACTOR_EMAIL() { BID_ID = bidId, TYPE = type }, 0, 0, out iTotalRecord).ToList();
            }
            catch (Exception ex)
            {
                this.State = false;
            }
        }
        public void CreateEmail(string listContractor, string bidId, string subject, string content, string typeEmail)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                string TEN_NHA_THAU_DAI = "{TEN_NHA_THAU_DAI}";
                string TEN_NHA_THAU_NGAN = "{TEN_NHA_THAU_NGAN}";
                foreach (var contractorId in listContractor.Split('|'))
                {
                    var contractor = UnitOfWork.Repository<ContractorRepo>().Queryable().FirstOrDefault(x => x.PKID == contractorId);

                    if (contractor != null && !string.IsNullOrWhiteSpace(contractor.EMAIL))
                    {
                        var lstContractorEmail = contractor.EMAIL.Split(';').ToList();
                        var contentSubject = subject
                                            .Replace(TEN_NHA_THAU_DAI, contractor.TEN_DAI)
                                            .Replace(TEN_NHA_THAU_NGAN, contractor.TEN_NGAN);
                        var contentBody = content
                                            .Replace(TEN_NHA_THAU_DAI, contractor.TEN_DAI)
                                            .Replace(TEN_NHA_THAU_NGAN, contractor.TEN_NGAN);
                        foreach (var item in lstContractorEmail)
                        {
                            var email = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = item,
                                SUBJECT = contentSubject,
                                CONTENTS = contentBody,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            };

                            UnitOfWork.Repository<EmailRepo>().Create(email);
                            UnitOfWork.Repository<BidContractorEmailRepo>().Create(new T_EB_BID_CONTRACTOR_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                BID_ID = bidId,
                                CONTRACTOR_ID = contractorId,
                                EMAIL_ID = email.PKID,
                                TYPE = typeEmail == "" ? null : typeEmail,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }

                    }
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }

        public void CreateQuestion(HttpRequestBase Request)
        {
            try
            {
                // Lưu file vào database
                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    var file = Request.Files[i];
                    var code = Guid.NewGuid().ToString();
                    var referenceCode = Guid.NewGuid().ToString();
                    lstFileStream.Add(new FILE_STREAM()
                    {
                        PKID = code,
                        FILE_OLD_NAME = file.FileName,
                        REFERENCE_ID = referenceCode,
                        FILE_EXT = Path.GetExtension(file.FileName),
                        FILE_SIZE = file.ContentLength,
                        FILESTREAM = Request.Files[i]
                    });
                }
                FileStreamService.InsertFile(lstFileStream);

                UnitOfWork.BeginTransaction();

                this.ObjDetail.PKID = Guid.NewGuid().ToString();
                this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;

                // Tạo file
                foreach (var file in lstFileStream)
                {
                    UnitOfWork.Repository<BidContractorQuestionFileRepo>().Create(new T_EB_BID_CONTRACTOR_QUESTION_FILE()
                    {
                        QUESTION_ID = this.ObjDetail.PKID,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                // Tạo header
                if (ProfileUtilities.User.USER_TYPE == UserType.NB && string.IsNullOrWhiteSpace(this.ObjDetail.CONTRACTOR_ID))
                {
                    this.ObjDetail.IS_ALL = true;
                }
                if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
                {
                    this.ObjDetail.TYPE = UserType.NhaThau;
                    this.ObjDetail.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
                }

                var serviceBid = new BidService();
                serviceBid.Get(this.ObjDetail.BID_ID);

                this.ObjDetail.IS_BID_CLOSE = false;
                if (serviceBid.ObjDetail.GIAI_DOAN != Phase_Status.NopHoSo)
                {
                    this.ObjDetail.IS_BID_CLOSE = true;
                }
                this.CurrentRepository.Create(this.ObjDetail);
                UnitOfWork.Commit();

                this.CreateNotify();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }


        public void CreateNotify()
        {
            try
            {
                var serviceNotify = new NotifyService();
                var serviceBid = new BidService();
                serviceBid.Get(this.ObjDetail.BID_ID);
                var lstUserNotify = new List<string>();

                var serviceContractor = new ContractorService();

                // Gửi notify cho COTECCONS
                if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(serviceBid.ObjDetail.CREATE_BY);
                    if (!string.IsNullOrEmpty(serviceBid.ObjDetail.USER_MANAGE))
                    {
                        lstUserNotify.Add(serviceBid.ObjDetail.USER_MANAGE);
                    }
                    serviceContractor.Get(ProfileUtilities.User.CONTRACTOR_ID);

                    foreach (var user in lstUserNotify)
                    {

                        string strTemplate = @"
                        <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                            <div class='icon-circle {2}'>
                                <i class='material-icons'>{3}</i>
                            </div>
                            <div class='menu-info'>
                                <span>Gói thầu [{4} - {5}] - Nhà thầu [{6}] hỏi ""{7}""</span>
                                <p>
                                    <i class='material-icons'>access_time</i> {8}
                                </p>
                            </div>
                        </a>
                    ";


                        string strContent = string.Format(strTemplate,
                            Guid.NewGuid().ToString(),
                            $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.BID_ID}&idMenu=mnuTraoDoiNhaThau",
                            "bg-orange",
                            "help_outline",
                            serviceBid.ObjDetail.PKID,
                            serviceBid.ObjDetail.TEN_GOI_THAU,
                            serviceContractor.ObjDetail.TEN_NGAN,
                            UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                            DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Gói thầu {serviceBid.ObjDetail.PKID} : Nhà thầu {serviceContractor.ObjDetail.TEN_NGAN} hỏi COTECCONS";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent,
                            RAW_CONTENTS_EN = strRawContent,
                            USER_NAME = user
                        };
                        serviceNotify.Create();
                    }
                }
                else
                {
                    var lstContractor = new List<T_EB_CONTRACTOR>();
                    var find = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID && x.STATUS == "AP").Fetch(x => x.Contractor).ToList();

                    if (!string.IsNullOrWhiteSpace(this.ObjDetail.CONTRACTOR_ID))
                    {
                        find = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID && x.STATUS == "AP" && x.CONTRACTOR_ID == this.ObjDetail.CONTRACTOR_ID).Fetch(x => x.Contractor).ToList();
                    }

                    lstContractor.AddRange(find.Select(x => x.Contractor));

                    foreach (var contractor in lstContractor)
                    {
                        lstUserNotify.Add(contractor.USER_NAME);
                        string newId = Guid.NewGuid().ToString();

                        string strTemplate = @"
                            <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                                <div class='icon-circle {2}'>
                                    <i class='material-icons'>{3}</i>
                                </div>
                                <div class='menu-info'>
                                    <span>Gói thầu [{4} - {5}] - COTECCONS hỏi ""{6}""</span>
                                    <p>
                                        <i class='material-icons'>access_time</i> {7}
                                    </p>
                                </div>
                            </a>
                        ";

                        string strTemplate_EN = @"
                            <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                                <div class='icon-circle {2}'>
                                    <i class='material-icons'>{3}</i>
                                </div>
                                <div class='menu-info'>
                                    <span>Bid [{4} - {5}] COTECCONS's question ""{6}""</span>
                                    <p>
                                        <i class='material-icons'>access_time</i> {7}
                                    </p>
                                </div>
                            </a>
                        ";


                        string strContent = string.Format(strTemplate,
                            newId,
                            $"/EB/Bid/MenuDetailBidContractor?id={this.ObjDetail.BID_ID}&idMenu=mnuTraoDoiCOTECCONS",
                            "bg-orange",
                            "help_outline",
                            serviceBid.ObjDetail.PKID,
                            serviceBid.ObjDetail.TEN_GOI_THAU,
                            UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                            DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strContent_EN = string.Format(strTemplate_EN,
                            newId,
                            $"/EB/Bid/MenuDetailBidContractor?id={this.ObjDetail.BID_ID}&idMenu=mnuTraoDoiCOTECCONS",
                            "bg-orange",
                            "help_outline",
                            serviceBid.ObjDetail.PKID,
                            serviceBid.ObjDetail.TEN_GOI_THAU_EN,
                            UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                            DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Gói thầu {serviceBid.ObjDetail.PKID} : Có câu hỏi mới từ COTECCONS";
                        string strRawContent_EN = $"BID {serviceBid.ObjDetail.PKID} : Has new question from COTECCONS";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent_EN,
                            RAW_CONTENTS_EN = strRawContent_EN,
                            USER_NAME = contractor.USER_NAME
                        };
                        serviceNotify.Create();

                    }
                }
                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception ex)
            {

            }
        }

        public List<T_EB_BID> TopQuestion()
        {
            var serviceBid = new BidService();
            serviceBid.NumerRecordPerPage = 10;
            serviceBid.Page = 1;
            serviceBid.Search();
            var lstBidID = new List<string>();
            foreach (var item in serviceBid.ObjList) {

                //ConfigViettinBank: Xoá phân quyền dự án

                //var projectCode = item.PROJECT_CODE; 
                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP317", projectCode); 
                //if (checkRight.Item1) 
                //{ 
                //    lstBidID.Add(item.PKID);
                //} 
               
                    lstBidID.Add(item.PKID);
                
            }

            this.ObjList = UnitOfWork.Repository<BidContractorQuestionRepo>().SearchTopQuestion(lstBidID);
            return serviceBid.ObjList;
        }

        public List<T_EB_BID> TopQuestionContractor()
        {
            var serviceBid = new BidService();
            serviceBid.NumerRecordPerPage = 10;
            serviceBid.Page = 1;
            serviceBid.SearchBidOfContractor();

            var lstBidID = serviceBid.ObjList.Select(x => x.PKID).ToList();
            this.ObjList = UnitOfWork.Repository<BidContractorQuestionRepo>().SearchTopQuestionContractor(lstBidID, ProfileUtilities.User.CONTRACTOR_ID);
            return serviceBid.ObjList;
        }
    }
}
