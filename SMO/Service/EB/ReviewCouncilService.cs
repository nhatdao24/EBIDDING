using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.AD;
using SMO.Service.CF;
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
    public class ReviewCouncilService : GenericService<T_EB_REVIEW_COUNCIL, ReviewCouncilRepo>
    {
        public string ReviewId { get; set; }
        public ReviewCouncilService() : base()
        {

        }

        public List<NodeUser> BuildTreeUser()
        {
            var lstNode = new List<NodeUser>();
            var serviceOrganize = new OrganizeService();
            serviceOrganize.GetAll();
            foreach (var organize in serviceOrganize.ObjList.OrderBy(x => x.C_ORDER))
            {
                var nodeOrganize = new NodeUser()
                {
                    id = organize.PKID,
                    pId = organize.PARENT,
                    name = organize.NAME,
                    icon = "/Content/zTreeStyle/img/diy/donvi.gif",
                    type = "OG",
                    nocheck = "true"
                };
                lstNode.Add(nodeOrganize);
                foreach (var user in organize.ListUser.OrderBy(x => x.USER_NAME))
                {
                    var nodeUser = new NodeUser()
                    {
                        id = user.USER_NAME,
                        pId = organize.PKID,
                        name = user.FULL_NAME + "<span class='spMaOnTree'>" + user.USER_NAME + "</span>",
                        userName = user.USER_NAME,
                        fullName = user.FULL_NAME,
                        email = user.EMAIL,
                        phone = user.PHONE,
                        icon = "/Content/zTreeStyle/img/diy/user.gif",
                        type = "US"
                    };
                    if (this.ObjList.Count(x => x.USER_NAME == user.USER_NAME) > 0)
                    {
                        nodeUser.@checked = "true";
                    }
                    lstNode.Add(nodeUser);
                }
            }
            return lstNode;
        }

        public void AddCouncilToReview(string reviewId, string lstCouncilID)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                foreach (var userName in lstCouncilID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var item = new T_EB_REVIEW_COUNCIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        REVIEW_ID = reviewId,
                        USER_NAME = userName,
                        STATUS = Review_Status.KhoiTao,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    UnitOfWork.Repository<ReviewCouncilRepo>().Create(item);
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Update()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                foreach (var item in this.ObjList)
                {
                    this.CurrentRepository.Update(item);
                }
                UnitOfWork.Commit();
                this.ObjList = new List<T_EB_REVIEW_COUNCIL>();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }

        /// <summary>
        /// Gửi email nhắc nhở 
        /// </summary>
        public void CreateNotiEmail(string reviewId, string lstReviewCouncil)
        {
            try
            {
                var lstReviewCouncilID = lstReviewCouncil.Split(',').ToList();
                var review = UnitOfWork.Repository<ReviewRepo>().Get(reviewId);
                var UserSent = UnitOfWork.Repository<UserRepo>().Queryable().FirstOrDefault(x => x.USER_NAME == review.CREATE_BY);
                var lstCouncil = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && lstReviewCouncilID.Contains(x.PKID))
                    .ToList();

                var lstCouncilUserName = lstCouncil.Select(x => x.USER_NAME).ToList();
                var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstCouncilUserName.Contains(x.USER_NAME)).ToList();

                string LOAI_CONG_VIEC = "{LOAI_CONG_VIEC}";
                string MA = "{MA}";
                string TEN = "{TEN}";
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";
                string USER_NAME_NGUOI_DE_XUAT = "{USER_NAME_NGUOI_DE_XUAT}";
                string HO_TEN_NGUOI_DE_XUAT = "{HO_TEN_NGUOI_DE_XUAT}";
                string USER_NAME_NGUOI_PHE_DUYET = "{USER_NAME_NGUOI_PHE_DUYET}";
                string HO_TEN_NGUOI_PHE_DUYET = "{HO_TEN_NGUOI_PHE_DUYET}";
                string DON_VI_PHONG_BAN = "{DON_VI_PHONG_BAN}";
                string TRANG_THAI = "{TRANG_THAI}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";

                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }

                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                var serviceEmail = new EmailNotifyService();
                var url = ConfigurationManager.AppSettings["URL_WEB"];

                var subjectTemplate = template.CONG_VIEC_XU_LY_SUBJECT;
                var bodyTemplate = template.CONG_VIEC_XU_LY_BODY;

                var contentSubject = subjectTemplate
                        .Replace(LOAI_CONG_VIEC, "Đánh giá NCC")
                        .Replace(MA, review.CODE)
                        .Replace(DU_AN, review.Project.NAME)
                        .Replace(DU_AN_EN, review.Project.NAME_EN)

                        .Replace(TEN, string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Status.GetStatusText(review.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReview?reviewId=" + review.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, review.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, "")
                        .Replace(HO_TEN_NGUOI_DE_XUAT, string.IsNullOrEmpty(UserSent.FULL_NAME) ? "" : UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, "");

                if (review.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DON_VI_PHONG_BAN, review.Organize.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Đánh giá NCC")
                        .Replace(MA, review.CODE)
                        .Replace(DU_AN_EN, review.Project.NAME_EN)
                        .Replace(TEN, string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Status.GetStatusText(review.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReview?reviewId=" + review.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, review.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, "")
                        .Replace(HO_TEN_NGUOI_DE_XUAT, string.IsNullOrEmpty(UserSent.FULL_NAME) ? "" : UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, "");

                if (review.Organize != null)
                {
                    contentBody = contentBody.Replace(DON_VI_PHONG_BAN, review.Organize.NAME);
                }
                UnitOfWork.BeginTransaction();
                foreach (var council in lstCouncil)
                {
                    var user = lstUser.FirstOrDefault(x => x.USER_NAME == council.USER_NAME);
                    if (user != null && !string.IsNullOrEmpty(user.EMAIL) && string.IsNullOrEmpty(council.EMAIL_ID))
                    {
                        serviceEmail.ObjDetail = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = user.EMAIL,
                            SUBJECT = contentSubject,
                            CONTENTS = contentBody
                        };
                        serviceEmail.Create();

                        //Update ReviewCouncil
                        council.EMAIL_ID = serviceEmail.ObjDetail.PKID;
                        council.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<ReviewCouncilRepo>().Update(council);
                    }
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.ErrorMessage = "Có lỗi trong quá trình gửi email";
            }
        }

        /// <summary>
        /// Tạo thông báo nhắc nhở
        /// </summary>
        public void CreateNotifyCouncil(string reviewId, string lstReviewCouncil)
        {
            try
            {
                var review = UnitOfWork.Repository<ReviewRepo>().Get(reviewId);
                var lstReviewCouncilID = lstReviewCouncil.Split(',').ToList();
                var lstUserName = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && lstReviewCouncilID.Contains(x.PKID)).Select(x => x.USER_NAME).ToList();

                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var council in lstUserName)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bạn được mời tham gia đánh giá NCC [{4} - {5}] </span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Review/MenuDetail?id={reviewId}",
                        Review_Status.GetStatusText(review.STATUS),
                        "how_to_reg",
                        review.CODE,
                        string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Đánh giá NCC {reviewId} bạn được mời tham gia đánh giá NCC";

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

        public void Delete(string strLstSelected, string reviewId)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                var lstReviewCouncil = this.CurrentRepository.Queryable().Where(x => x.REVIEW_ID == reviewId).ToList();
                var lstReviewContractor = UnitOfWork.Repository<ReviewContractorRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId).ToList();
                List<string> lstCouncil = new List<string>();
                List<string> lstDetailId = new List<string>();
                foreach (var item in strLstSelected.Split(','))
                {
                    var reviewCouncil = lstReviewCouncil.FirstOrDefault(x => x.PKID == item);
                    if (reviewCouncil != null)
                    {
                        lstCouncil.Add(reviewCouncil.USER_NAME);
                    }
                }
                foreach (var contractor in lstReviewContractor)
                {
                    foreach (var detail in contractor.ListDetail)
                    {
                        lstDetailId.Add(detail.PKID);
                    }
                }
                this.UnitOfWork.GetSession().Query<T_EB_REVIEW_SPEC_SCORE>().Where(x => lstDetailId.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && lstCouncil.Contains(x.USER_SCORE)).Delete();
                UnitOfWork.Commit();
                base.Delete(strLstSelected);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }
    }
}
