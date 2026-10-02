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
    public class BidInternalAnswerService : GenericService<T_EB_BID_INTERNAL_ANSWER, BidInternalAnswerRepo>
    {
        public BidInternalAnswerService() : base()
        {

        }

        public void CreateAnswer(HttpRequestBase Request)
        {
            var serviceBid = new BidService();
            serviceBid.Get(this.ObjDetail.BID_ID);

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP318", serviceBid.ObjDetail.PROJECT_CODE);
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
                    UnitOfWork.Repository<BidInternalAnswerFileRepo>().Create(new T_EB_BID_INTERNAL_ANSWER_FILE()
                    {
                        ANSWER_ID = this.ObjDetail.PKID,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                // Tạo header
                this.CurrentRepository.Create(this.ObjDetail);

                if (this.ObjDetail.IS_SEND_MAIL)
                {
                    var content = "";
                    var url = ConfigurationManager.AppSettings["URL_WEB"];
                    var urlBid = url + "Admin/OpenBid?bidId=" + this.ObjDetail.BID_ID + "&idMenu=mnuTraoDoiNoiBo";
                    content += "<p>Dear anh/ chị,</p>";
                    content += "<p> Anh/chị vui lòng truy cập hệ thống(<a href=\"" + urlBid + "\">Nhấp tại đây</a>) để xem thông tin chi tiết.</p>";
                    content += "<p><span style = \"color:#27ae60\" > " + this.ObjDetail.CONTENTS + " </span></p>";
                    content += "<p> Trân trọng!</p>";
                    content += "<p><em>Đây là email được gửi tự động từ hệ thống E-bidding, xin vui lòng không trả lời lại mail này.</em></p>";
                    //content += "</br>Vui lòng truy cập hệ thống E-bidding (<a href=\"" + urlBid + "\">Nhấp tại đây</a>) để thực hiện.";
                    var user = UnitOfWork.Repository<UserRepo>().Queryable().FirstOrDefault(x => x.USER_NAME == this.ObjDetail.USER_REPLY);
                    if (user != null && !string.IsNullOrWhiteSpace(user.EMAIL))
                    {
                        var email = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = user.EMAIL,
                            SUBJECT = "[E-bidding] Thông báo trao đổi nội bộ: Gói thầu[" + this.ObjDetail.BID_ID + "] - " + serviceBid.ObjDetail.TEN_GOI_THAU + "",
                            CONTENTS = content,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        };

                        UnitOfWork.Repository<EmailRepo>().Create(email);
                        UnitOfWork.Repository<BidCouncilEmailRepo>().Create(new T_EB_BID_COUNCIL_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            BID_ID = this.ObjDetail.BID_ID,
                            USER_NAME = this.ObjDetail.USER_REPLY,
                            EMAIL_ID = email.PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }
                }
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

                var question = UnitOfWork.Repository<BidInternalQuestionRepo>().Get(this.ObjDetail.QUESTION_ID);
                var bid = UnitOfWork.Repository<BidRepo>().Get(question.BID_ID);

                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == question.BID_ID).ToList();
                var lstCouncilUser = lstCouncil.Select(x => x.USER_NAME).ToList();

                if (!lstCouncilUser.Contains(bid.CREATE_BY) && ProfileUtilities.User.USER_NAME != bid.CREATE_BY)
                {
                    lstCouncilUser.Add(bid.CREATE_BY);
                }

                if (ProfileUtilities.User.USER_NAME == question.CREATE_BY)
                {
                    lstCouncilUser.Remove(question.CREATE_BY);
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
                                <span>Gói thầu [{4} - {5}] - Người dùng [{6}] trả lời câu hỏi [{7}] ""{8}""</span>
                                <p>
                                    <i class='material-icons'>access_time</i> {8}
                                </p>
                            </div>
                        </a>
                    ";


                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={question.BID_ID}&idMenu=mnuTraoDoiNoiBo",
                        "bg-indigo",
                        "question_answer",
                        bid.PKID,
                        bid.TEN_GOI_THAU,
                        ProfileUtilities.User.FULL_NAME,
                        UtilsCore.SubString(UtilsCore.StripHTML(question.CONTENTS), 100),
                        UtilsCore.SubString(UtilsCore.StripHTML(this.ObjDetail.CONTENTS), 100),
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {bid.PKID} - Có câu trả lời mới từ người dùng {ProfileUtilities.User.FULL_NAME}";

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
    }
}
