using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.CM;
using SMO.Repository.Implement.EB;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class BidInternalQuestionService : GenericService<T_EB_BID_INTERNAL_QUESTION, BidInternalQuestionRepo>
    {
        public List<T_EB_BID_COUNCIL_EMAIL> ObjListEmail { get; set; }
        public BidInternalQuestionService() : base()
        {
            ObjListEmail = new List<T_EB_BID_COUNCIL_EMAIL>();
        }

        public void GetListEmail(string bidId)
        {
            try
            {
                var serviceBid = new BidService();
                serviceBid.Get(bidId);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP318", serviceBid.ObjDetail.PROJECT_CODE);
                //if(checkRight != null)
                //{
                //    int iTotalRecord = 0;
                //    this.ObjListEmail = UnitOfWork.Repository<BidCouncilEmailRepo>().Search(new T_EB_BID_COUNCIL_EMAIL() { BID_ID = bidId }, 0, 0, out iTotalRecord).ToList();
                //}
                
                //else
                //{
                //    this.State = false;
                //    return; 
                //}

                
                    int iTotalRecord = 0;
                    this.ObjListEmail = UnitOfWork.Repository<BidCouncilEmailRepo>().Search(new T_EB_BID_COUNCIL_EMAIL() { BID_ID = bidId }, 0, 0, out iTotalRecord).ToList();

            }
            catch (Exception ex)
            {
                this.State = false;
            }
        }

        public void CreateQuestion(HttpRequestBase Request)
        {
            try
            {
                var serviceBid = new BidService();
                serviceBid.Get(this.ObjDetail.BID_ID);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP318", serviceBid.ObjDetail.PROJECT_CODE);

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
                    UnitOfWork.Repository<BidInternalQuestionFileRepo>().Create(new T_EB_BID_INTERNAL_QUESTION_FILE()
                    {
                        QUESTION_ID = this.ObjDetail.PKID,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                // Tạo header
             

                this.ObjDetail.IS_BID_CLOSE = false;
                this.ObjDetail.IS_SEND_MAIL = false;
                if (serviceBid.ObjDetail.GIAI_DOAN != Phase_Status.NopHoSo)
                {
                    this.ObjDetail.IS_BID_CLOSE = true;
                }

                this.CurrentRepository.Create(this.ObjDetail);
                UnitOfWork.Commit();

                this.CreateNotify(serviceBid.ObjDetail);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }

        public void CreateNotify(T_EB_BID bid)
        {
            try
            {
                var serviceNotify = new NotifyService();
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID).ToList();
                var lstCouncilUser = lstCouncil.Select(x => x.USER_NAME).ToList();
                if (!lstCouncilUser.Contains(bid.CREATE_BY) && ProfileUtilities.User.USER_NAME != bid.CREATE_BY)
                {
                    lstCouncilUser.Add(bid.CREATE_BY);
                }

                var lstUserNotify = new List<string>();

                foreach (var council in lstCouncilUser)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council);

                    string strTemplate = @"
                        <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                            <div class='icon-circle {2}'>
                                <i class='material-icons'>{3}</i>
                            </div>
                            <div class='menu-info'>
                                <span>Gói thầu [{4} - {5}] - Người dùng [{6}] hỏi ""{7}""</span>
                                <p>
                                    <i class='material-icons'>access_time</i> {8}
                                </p>
                            </div>
                        </a>
                    ";


                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.BID_ID}&idMenu=mnuTraoDoiNoiBo",
                        "bg-orange",
                        "help_outline",
                        bid.PKID,
                        bid.TEN_GOI_THAU,
                        ProfileUtilities.User.FULL_NAME,
                        UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.BID_ID} - Có câu hỏi mới từ người dùng {ProfileUtilities.User.FULL_NAME}";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = council
                    };
                    serviceNotify.Create();
                }
                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception ex)
            {

            }
        }

        public void CreateEmail(string listCouncil, string bidId, string subject, string content, HttpRequestBase Request)
        {
            try
            {
                var serviceBid = new BidService();
                serviceBid.Get(bidId);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP318", serviceBid.ObjDetail.PROJECT_CODE);

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

                List<string> result = listCouncil.Split('|').ToList();
                var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => result.Contains(x.USER_NAME)).ToList();
                UnitOfWork.BeginTransaction();
                this.ObjDetail.PKID = Guid.NewGuid().ToString();
                this.ObjDetail.BID_ID = bidId;
                this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;

                // Tạo header
             
                var url = ConfigurationManager.AppSettings["URL_WEB"];
                var urlBid = url + "Admin/OpenBid?bidId=" + this.ObjDetail.BID_ID + "&idMenu=mnuTraoDoiNoiBo";
                content += "<p> Anh/chị vui lòng truy cập hệ thống(<a href=\"" + urlBid + "\">Nhấp tại đây</a>) để xem thông tin chi tiết và cho ý kiến (nếu có)!</p>";

                this.ObjDetail.IS_BID_CLOSE = false;
                this.ObjDetail.IS_SEND_MAIL = true;
                this.ObjDetail.CONTENTS = content;
                if (serviceBid.ObjDetail.GIAI_DOAN != Phase_Status.NopHoSo)
                {
                    this.ObjDetail.IS_BID_CLOSE = true;
                }
                this.CurrentRepository.Create(this.ObjDetail);

                // Tạo file
                foreach (var file in lstFileStream)
                {
                    UnitOfWork.Repository<BidInternalQuestionFileRepo>().Create(new T_EB_BID_INTERNAL_QUESTION_FILE()
                    {
                        QUESTION_ID = this.ObjDetail.PKID,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                //Gửi mail
                foreach (var userName in listCouncil.Split('|'))
                {
                    var user = lstUser.FirstOrDefault(x => x.USER_NAME == userName);
                    if (user != null && !string.IsNullOrWhiteSpace(user.EMAIL))
                    {
                        var email = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = user.EMAIL,
                            SUBJECT = subject,
                            CONTENTS = content,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        };

                        UnitOfWork.Repository<EmailRepo>().Create(email);
                        UnitOfWork.Repository<BidCouncilEmailRepo>().Create(new T_EB_BID_COUNCIL_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            BID_ID = bidId,
                            USER_NAME = userName,
                            EMAIL_ID = email.PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }
                }
                UnitOfWork.Commit();
                this.CreateNotify(serviceBid.ObjDetail);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }
    }
}
