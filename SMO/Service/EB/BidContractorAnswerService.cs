using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.CM;
using SMO.Repository.Implement.EB;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class BidContractorAnswerService : GenericService<T_EB_BID_CONTRACTOR_ANSWER, BidContractorAnswerRepo>
    {
        public BidContractorAnswerService() : base()
        {

        }

        public void CreateAnswer(HttpRequestBase Request)
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
                    UnitOfWork.Repository<BidContractorAnswerFileRepo>().Create(new T_EB_BID_CONTRACTOR_ANSWER_FILE()
                    {
                        ANSWER_ID = this.ObjDetail.PKID,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                // Tạo header
                if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
                {
                    this.ObjDetail.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
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
                var lstUserNotify = new List<string>();

                var question = UnitOfWork.Repository<BidContractorQuestionRepo>().Get(this.ObjDetail.QUESTION_ID);
                var bid = UnitOfWork.Repository<BidRepo>().Get(question.BID_ID);

                if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
                {
                    lstUserNotify.Add(bid.CREATE_BY);
                    if (!string.IsNullOrEmpty(bid.USER_MANAGE))
                    {
                        lstUserNotify.Add(bid.USER_MANAGE);
                    }

                    foreach (var user in lstUserNotify)
                    {
                        var newId = Guid.NewGuid().ToString();

                        string strTemplate = @"
                        <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                            <div class='icon-circle {2}'>
                                <i class='material-icons'>{3}</i>
                            </div>
                            <div class='menu-info'>
                                <span>Gói thầu [{4} - {5}] - Nhà thầu [{6}] trả lời câu hỏi [{7}] ""{8}""</span>
                                <p>
                                    <i class='material-icons'>access_time</i> {9}
                                </p>
                            </div>
                        </a>
                    ";


                        string strContent = string.Format(strTemplate,
                            newId,
                            $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuTraoDoiNhaThau",
                            "bg-indigo",
                            "question_answer",
                            bid.PKID,
                            bid.TEN_GOI_THAU,
                            ProfileUtilities.User.Contractor.TEN_NGAN,
                            UtilsCore.SubString(UtilsCore.StripHTML(question.CONTENTS), 100),
                            UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                            DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Gói thầu {bid.PKID} : Nhà thầu {ProfileUtilities.User.Contractor.TEN_NGAN} trả lời câu hỏi";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
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
                    var contractor = UnitOfWork.Repository<ContractorRepo>().Get(question.CONTRACTOR_ID);
                    lstUserNotify.Add(contractor.USER_NAME);
                    var newId = Guid.NewGuid().ToString();

                    string strTemplate = @"
                            <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                                <div class='icon-circle {2}'>
                                    <i class='material-icons'>{3}</i>
                                </div>
                                <div class='menu-info'>
                                    <span>Gói thầu [{4} - {5}] - CTD trả lời câu hỏi [{6}] ""{7}""</span>
                                    <p>
                                        <i class='material-icons'>access_time</i> {8}
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
                                    <span>Bid [{4} - {5}] - CTD's answer of question [{6}] ""{7}""</span>
                                    <p>
                                        <i class='material-icons'>access_time</i> {8}
                                    </p>
                                </div>
                            </a>
                        ";


                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidContractor?id={bid.PKID}&idMenu=mnuTraoDoiFecon",
                        "bg-indigo",
                        "question_answer",
                        bid.PKID,
                        bid.TEN_GOI_THAU,
                        UtilsCore.SubString(UtilsCore.StripHTML(question.CONTENTS), 100),
                        UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidContractor?id={bid.PKID}&idMenu=mnuTraoDoiFecon",
                        "bg-indigo",
                        "question_answer",
                        bid.PKID,
                        bid.TEN_GOI_THAU_EN,
                        UtilsCore.SubString(UtilsCore.StripHTML(question.CONTENTS), 100),
                        UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {bid.PKID} : CTD trả lời câu hỏi";
                    string strRawContent_EN = $"BID {bid.PKID} : Has new answer from CTD";

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

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception)
            {
            }
        }
    }
}
