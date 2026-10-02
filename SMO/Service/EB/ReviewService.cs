using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.CM;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
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
    public class ReviewService : GenericService<T_EB_REVIEW, ReviewRepo>
    {
        public List<T_EB_REVIEW_SPEC> ObjListReviewSpec { get; set; }
        public List<T_EB_REVIEW_SPEC_DETAIL> ObjListReviewSpecDetail { get; set; }
        public ReviewService() : base()
        {
            ObjListReviewSpec = new List<T_EB_REVIEW_SPEC>();
            ObjListReviewSpecDetail = new List<T_EB_REVIEW_SPEC_DETAIL>();
        }

        public void Search_ProcessByMe()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjList = UnitOfWork.Repository<ReviewRepo>().Search_ProcessByMe(this.ObjDetail, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }
        public override void Create()
        {
            try
            {
                if (this.ObjDetail.EVALUATE_DATE >= this.ObjDetail.END_DATE)
                {
                    this.State = false;
                    this.ErrorMessage = "Ngày kết thúc phải lớn hơn ngày bắt đầu!";
                    return;
                }
                this.ObjDetail.CODE = this.GetSequence("REVIEW");
                this.ObjDetail.STATUS = Review_Status.ChuaGuiYeuCau;
                var lstMdSpecDefault = UnitOfWork.Repository<MdReviewSpecRepo>().GetAll();
                var lstMdSpecDetail = UnitOfWork.Repository<MdReviewSpecDetailRepo>().GetAll();
                UnitOfWork.BeginTransaction();
                foreach (var item in lstMdSpecDefault.Where(x => x.IS_DEFAULT == true).OrderBy(x => x.C_ORDER).ToList())
                {
                    if (this.ObjListReviewSpec.Count(x => x.MD_PKID == item.PKID) == 0)
                    {
                        var reviewSpecId = Guid.NewGuid().ToString();
                        this.ObjListReviewSpec.Add(new T_EB_REVIEW_SPEC()
                        {
                            PKID = reviewSpecId,
                            REVIEW_ID = this.ObjDetail.CODE,
                            MD_PKID = item.PKID,
                            NAME = item.NAME,
                            NAME_EN = item.NAME_EN,
                            DESCRIPTION = item.DESCRIPTION,
                            DESCRIPTION_EN = item.DESCRIPTION_EN,
                            C_ORDER = item.C_ORDER,
                            RATIO = item.RATIO,
                            MAX_SCORE = item.MAX_SCORE,
                            VAI_TRO = item.VAI_TRO,
                            IS_DEFAULT = item.IS_DEFAULT
                        });

                        if (lstMdSpecDefault.Count(x => x.PARENT_ID == item.PKID) > 0)
                        {
                            foreach (var child in lstMdSpecDefault.Where(x => x.PARENT_ID == item.PKID).OrderBy(x => x.C_ORDER).ToList())
                            {
                                var reviewSpecId2 = Guid.NewGuid().ToString();
                                this.ObjListReviewSpec.Add(new T_EB_REVIEW_SPEC()
                                {
                                    PKID = reviewSpecId2,
                                    PARENT_ID = reviewSpecId,
                                    REVIEW_ID = this.ObjDetail.CODE,
                                    MD_PKID = child.PKID,
                                    NAME = child.NAME,
                                    NAME_EN = child.NAME_EN,
                                    DESCRIPTION = child.DESCRIPTION,
                                    DESCRIPTION_EN = child.DESCRIPTION_EN,
                                    C_ORDER = child.C_ORDER,
                                    RATIO = child.RATIO,
                                    MAX_SCORE = child.MAX_SCORE,
                                    //VAI_TRO = child.VAI_TRO,
                                    IS_DEFAULT = child.IS_DEFAULT
                                });

                                foreach (var detail in lstMdSpecDetail.Where(x => x.HEADER_ID == child.PKID).ToList())
                                {
                                    this.ObjListReviewSpecDetail.Add(new T_EB_REVIEW_SPEC_DETAIL()
                                    {
                                        PKID = Guid.NewGuid().ToString(),
                                        HEADER_ID = reviewSpecId2,
                                        FROM_VALUE = detail.FROM_VALUE,
                                        TO_VALUE = detail.TO_VALUE,
                                        VALUE = detail.VALUE,
                                        VALUE_EN = detail.VALUE_EN,
                                        SCORE = detail.SCORE,
                                        C_ORDER = detail.C_ORDER
                                    });
                                }
                            }
                        }

                        //Add Detail
                        foreach (var detail in lstMdSpecDetail.Where(x => x.HEADER_ID == item.PKID).ToList())
                        {
                            this.ObjListReviewSpecDetail.Add(new T_EB_REVIEW_SPEC_DETAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = reviewSpecId,
                                FROM_VALUE = detail.FROM_VALUE,
                                TO_VALUE = detail.TO_VALUE,
                                VALUE = detail.VALUE,
                                VALUE_EN = detail.VALUE_EN,
                                SCORE = detail.SCORE,
                                C_ORDER = detail.C_ORDER
                            });
                        }
                    }
                }
                UnitOfWork.Repository<ReviewSpecRepo>().Create(this.ObjListReviewSpec);
                UnitOfWork.Repository<ReviewSpecDetailRepo>().Create(this.ObjListReviewSpecDetail);
                base.Create();
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
                if (this.ObjDetail.EVALUATE_DATE >= this.ObjDetail.END_DATE)
                {
                    this.State = false;
                    this.ErrorMessage = "Ngày kết thúc phải lớn hơn ngày bắt đầu!";
                    return;
                }
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void OpenReview(string reviewId)
        {
            try
            {
                var review = this.CurrentRepository.Get(reviewId);
                UnitOfWork.BeginTransaction();
                if (review.END_DATE > DateTime.Now)
                {
                    review.STATUS = Review_Status.KhoiTao;
                }
                this.CurrentRepository.Update(review);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void EndReview(string id)
        {
            this.Get(id);
            try
            {
                UnitOfWork.BeginTransaction();
                this.ObjDetail.STATUS = Review_Status.KetThuc;
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.CurrentRepository.Update(this.ObjDetail);
                this.UpdateDetail(id);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }
        public void UpdateDetail(string id)
        {
            this.Get(id);
            var lstReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>()
                .Queryable().Where(x => x.REVIEW_ID == id).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
            var lstReviewCouncil = this.ObjDetail.ListCouncil.Where(x => x.REVIEW_ID == id && x.STATUS == Review_Status.KetThuc).Select(x => x.USER_NAME).ToList();
            var lstRating = this.UnitOfWork.Repository<MdRatingRepo>().GetAll();
            try
            {
                UnitOfWork.BeginTransaction();
                foreach (var reviewContractor in this.ObjDetail.ListContractor)
                {
                    var lstDetail = reviewContractor.ListDetail;
                    if (lstDetail.Count() > 0)
                    {
                        // Lấy danh sách điểm, và ghi chú theo người chấm điểm
                        var lstIdDetail = lstDetail.Select(x => x.PKID).ToList();
                        var lstSpecScore = this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && lstReviewCouncil.Contains(x.USER_SCORE)).ToList();
                        // Tạo danh sách điểm
                        foreach (var detail in lstDetail)
                        {
                            detail.TOTAL_SCORE = 0;
                            foreach (var spec in lstReviewSpec)
                            {
                                var find = lstSpecScore.Where(x =>
                                x.SPEC_ID == spec.PKID &&
                                x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID
                                ).ToList();
                                if (find.Count() > 0)
                                {
                                    decimal totalSpecScore = 0;
                                    decimal mediumScore = 0;

                                    foreach (var score in find)
                                    {
                                        if (score.SCORE.HasValue)
                                        {
                                            totalSpecScore += score.SCORE.Value;
                                        }
                                    }
                                    if (totalSpecScore > 0)
                                    {
                                        mediumScore = totalSpecScore / find.Count();
                                        detail.TOTAL_SCORE += mediumScore * spec.RATIO;
                                    }
                                }
                            }
                            detail.MEDIUM_SCORE = detail.TOTAL_SCORE / lstReviewSpec.Sum(x => x.RATIO);
                            var rate = lstRating.FirstOrDefault(m => m.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= m.TO_VALUE);
                            if (rate != null)
                            {
                                detail.RATE = rate.RATING;
                            }
                            UnitOfWork.Repository<ReviewContractorDetailRepo>().Update(detail);
                        }
                    }
                }

                //Cập nhật tổng điểm, điểm TB, xếp loại
                //foreach (var item in this.ObjListReviewContractorDetail)
                //{
                //    var detail = lstContractorDetail.FirstOrDefault(x => x.PKID == item.PKID);
                //    detail.TOTAL_SCORE = item.TOTAL_SCORE;
                //    detail.MEDIUM_SCORE = item.MEDIUM_SCORE;
                //    detail.RATE = item.RATE;
                //    UnitOfWork.Repository<ReviewContractorDetailRepo>().Update(detail);
                //}
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void AutoCloseOrOpenReview()
        {
            try
            {
                List<string> lstReviewId = new List<string>();
                //Lấy ra tất cả các review đang đánh giá
                var lstReview = this.CurrentRepository.Queryable().Where(x => x.STATUS != Review_Status.KetThuc).ToList();
                var dateNow = DateTime.Now;
                UnitOfWork.BeginTransaction();
                foreach (var item in lstReview)
                {
                    if (item.END_DATE <= dateNow && item.STATUS == Review_Status.KhoiTao)
                    {
                        item.STATUS = Review_Status.KetThuc;
                        this.CurrentRepository.Update(item);
                        this.UpdateDetail(item.CODE);
                        this.CreateNotiEmailKetThuc(item.CODE);
                        this.CreateNotifyKetThuc(item.CODE);
                    }
                    if (item.STATUS == Review_Status.DaGuiYeuCau && dateNow >= item.EVALUATE_DATE)
                    {
                        item.STATUS = Review_Status.KhoiTao;
                        this.CurrentRepository.Update(item);
                    }
                    if (item.END_DATE == dateNow.AddDays(2) && item.STATUS == Review_Status.KhoiTao)
                    {
                        this.CreateNotiEmail2NgayKetThuc(item.CODE);
                    }
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

        /// <summary>
        /// Gửi email nhắc nhở 
        /// </summary>
        public void CreateNotiEmail(string reviewId)
        {
            try
            {
                var review = this.CurrentRepository.Get(reviewId);
                var UserSent = UnitOfWork.Repository<UserRepo>().Queryable().FirstOrDefault(x => x.USER_NAME == review.CREATE_BY);
                var lstCouncil = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && (x.EMAIL_ID == null || x.EMAIL_ID == ""))
                    .ToList();

                var lstCouncilUserName = lstCouncil.Select(x => x.USER_NAME).ToList();
                var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstCouncilUserName.Contains(x.USER_NAME)).ToList();

                string LOAI_CONG_VIEC = "{LOAI_CONG_VIEC}";
                string MA = "{MA}";
                string TEN = "{TEN}";
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
                if (review.STATUS == Review_Status.KetThuc)
                {
                    subjectTemplate = template.CONG_VIEC_HOAN_THANH_SUBJECT;
                    bodyTemplate = template.CONG_VIEC_HOAN_THANH_BODY;
                }

                review.STATUS = Review_Status.DaGuiYeuCau;

                var contentSubject = subjectTemplate
                        .Replace(LOAI_CONG_VIEC, "Đánh giá NCC")
                        .Replace(MA, review.CODE)
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

                //Cập nhật Status review
                review.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.CurrentRepository.Update(review);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.ErrorMessage = "Có lỗi trong quá trình gửi email";
            }
        }

        /// <summary>
        /// Gửi email nhắc nhở 
        /// </summary>
        public void CreateNotiEmailKetThuc(string reviewId)
        {
            try
            {
                var lstMailNotify = new List<string>();
                var review = this.CurrentRepository.Get(reviewId);
                lstMailNotify.Add(review.USER_CREATE.EMAIL);
                var lstCouncil = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId)
                    .ToList();

                var lstCouncilUserName = lstCouncil.Select(x => x.USER_NAME).ToList();
                var lstCouncilEmail = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstCouncilUserName.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                lstMailNotify.AddRange(lstCouncilEmail);

                string LOAI_CONG_VIEC = "{LOAI_CONG_VIEC}";
                string MA = "{MA}";
                string TEN = "{TEN}";
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

                var subjectTemplate = template.CONG_VIEC_HOAN_THANH_SUBJECT;
                var bodyTemplate = template.CONG_VIEC_HOAN_THANH_BODY;

                var contentSubject = subjectTemplate
                        .Replace(LOAI_CONG_VIEC, "Đánh giá NCC")
                        .Replace(MA, review.CODE)
                        .Replace(TEN, string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Status.GetStatusText(Review_Status.KetThuc))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReview?reviewId=" + review.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, review.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, "")
                        .Replace(HO_TEN_NGUOI_DE_XUAT, string.IsNullOrEmpty(review.USER_CREATE.FULL_NAME) ? "" : review.USER_CREATE.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, "");

                if (review.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DON_VI_PHONG_BAN, review.Organize.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Đánh giá NCC")
                        .Replace(MA, review.CODE)
                        .Replace(TEN, string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Status.GetStatusText(Review_Status.KetThuc))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReview?reviewId=" + review.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, review.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, "")
                        .Replace(HO_TEN_NGUOI_DE_XUAT, string.IsNullOrEmpty(review.USER_CREATE.FULL_NAME) ? "" : review.USER_CREATE.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, "");

                if (review.Organize != null)
                {
                    contentBody = contentBody.Replace(DON_VI_PHONG_BAN, review.Organize.NAME);
                }
                UnitOfWork.BeginTransaction();
                foreach (var mail in lstMailNotify)
                {
                    serviceEmail.ObjDetail = new T_CM_EMAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        EMAIL = mail,
                        SUBJECT = contentSubject,
                        CONTENTS = contentBody
                    };
                    serviceEmail.Create();
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.ErrorMessage = "Có lỗi trong quá trình gửi email";
            }
        }

        /// <summary>
        /// Tạo thông báo nhắc nhở
        /// </summary>
        public void CreateNotifyKetThuc(string reviewId)
        {
            try
            {
                var review = this.CurrentRepository.Get(reviewId);
                var lstUserName = review.ListCouncil.Select(x => x.USER_NAME).ToList();
                lstUserName.Add(review.CREATE_BY);

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
                            <span>Đánh giá NCC [{4} - {5}] đã kết thúc</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Review/MenuDetail?id={review.CODE}",
                        Review_Status.GetStatusText(review.STATUS),
                        "how_to_reg",
                        review.CODE,
                        string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Đánh giá NCC {review.CODE} đã kết thúc";

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
                this.State = false;
                this.Exception = ex;
            }

        }

        /// <summary>
        /// Tạo thông báo nhắc nhở
        /// </summary>
        public void CreateNotifyCouncil(string reviewId)
        {
            try
            {
                var review = this.CurrentRepository.Get(reviewId);
                var lstUserName = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && (x.EMAIL_ID == null || x.EMAIL_ID == "")).Select(x => x.USER_NAME).ToList();

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
                this.State = false;
                this.Exception = ex;
            }

        }

        /// <summary>
        /// Gửi email nhắc nhở còn 2 ngày kết thúc đánh giá
        /// </summary>
        public void CreateNotiEmail2NgayKetThuc(string reviewId)
        {
            try
            {
                var lstMailNotify = new List<string>();
                var review = this.CurrentRepository.Get(reviewId);
                var lstCouncil = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && x.STATUS != Review_Status.KetThuc)
                    .ToList();

                var lstCouncilUserName = lstCouncil.Select(x => x.USER_NAME).ToList();
                var lstCouncilEmail = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstCouncilUserName.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                lstMailNotify.AddRange(lstCouncilEmail);

                string LOAI_CONG_VIEC = "{LOAI_CONG_VIEC}";
                string MA = "{MA}";
                string TEN = "{TEN}";
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
                        .Replace(TEN, string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Status.GetStatusText(Review_Status.KetThuc))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReview?reviewId=" + review.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, review.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, "")
                        .Replace(HO_TEN_NGUOI_DE_XUAT, string.IsNullOrEmpty(review.USER_CREATE.FULL_NAME) ? "" : review.USER_CREATE.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, "");

                if (review.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DON_VI_PHONG_BAN, review.Organize.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Đánh giá NCC")
                        .Replace(MA, review.CODE)
                        .Replace(TEN, string.IsNullOrEmpty(review.DESCRIPTION) ? "" : review.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Status.GetStatusText(Review_Status.KetThuc))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReview?reviewId=" + review.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, review.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, "")
                        .Replace(HO_TEN_NGUOI_DE_XUAT, string.IsNullOrEmpty(review.USER_CREATE.FULL_NAME) ? "" : review.USER_CREATE.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, "");

                if (review.Organize != null)
                {
                    contentBody = contentBody.Replace(DON_VI_PHONG_BAN, review.Organize.NAME);
                }
                UnitOfWork.BeginTransaction();
                foreach (var mail in lstMailNotify)
                {
                    serviceEmail.ObjDetail = new T_CM_EMAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        EMAIL = mail,
                        SUBJECT = contentSubject,
                        CONTENTS = contentBody
                    };
                    serviceEmail.Create();
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.ErrorMessage = "Có lỗi trong quá trình gửi email";
            }
        }
    }
}