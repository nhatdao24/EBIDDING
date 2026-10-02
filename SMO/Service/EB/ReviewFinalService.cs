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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class ReviewFinalService : GenericService<T_EB_REVIEW_FINAL, ReviewFinalRepo>
    {
        public List<T_EB_REVIEW_FINAL_CONTRACTOR> ObjListContractor { get; set; }
        public List<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL> ObjListContractorDetail { get; set; }
        public List<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL> ObjListContractorDetailOld { get; set; }
        public List<T_EB_REVIEW_FINAL_NOTE> ObjListNote { get; set; }
        public IList<T_MD_RATING> ObjListRating { get; set; }
        public T_AD_USER ObjSearchUser { get; set; }
        public List<T_AD_USER> ObjListSearchUser { get; set; }
        public ReviewFinalService() : base()
        {
            ObjListContractor = new List<T_EB_REVIEW_FINAL_CONTRACTOR>();
            ObjListContractorDetail = new List<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>();
            ObjListContractorDetailOld = new List<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>();
            ObjListNote = new List<T_EB_REVIEW_FINAL_NOTE>();
            ObjSearchUser = new T_AD_USER();
            ObjListSearchUser = new List<T_AD_USER>();
        }

        public void Search_ProcessByMe()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjList = UnitOfWork.Repository<ReviewFinalRepo>().Search_ProcessByMe(this.ObjDetail, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SearchUser()
        {
            try
            {
                var lstUserName = new List<string>();
                //Tìm trong T_AD_USER_RIGHT
                var findUserRight = UnitOfWork.Repository<UserRightRepo>().Queryable().Where(x => x.IS_ADD == true && x.FK_RIGHT == "R2107").ToList();
                lstUserName.AddRange(findUserRight.Select(x => x.USER_NAME));

                //Tìm theo ROLE
                var findRole = UnitOfWork.Repository<RoleDetailRepo>().Queryable().Where(x => x.FK_RIGHT == "R2107").ToList();
                if (findRole.Count > 0)
                {
                    var lstRole = findRole.Select(x => x.FK_ROLE).ToList();
                    //Tìm user gán với role
                    var findUserRole = UnitOfWork.Repository<UserRoleRepo>().Queryable().Where(x => lstRole.Contains(x.ROLE_CODE)).ToList();
                    lstUserName.AddRange(findUserRole.Select(x => x.USER_NAME));

                    //Tìm user group gán với role
                    var findUserGroupRole = UnitOfWork.Repository<UserGroupRoleRepo>().Queryable().Where(x => lstRole.Contains(x.ROLE_CODE)).ToList();
                    var lstGroup = findUserGroupRole.Select(x => x.USER_GROUP_CODE).ToList();
                    var findUserUserGroup = UnitOfWork.Repository<UserUserGroupRepo>().Queryable().Where(x => lstGroup.Contains(x.USER_GROUP_CODE));

                    lstUserName.AddRange(findUserUserGroup.Select(x => x.USER_NAME));
                }

                lstUserName = lstUserName.Distinct().ToList();

                var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstUserName.Contains(x.USER_NAME) && x.USER_TYPE == UserType.NB).OrderBy(x => x.USER_NAME).ToList();
                foreach (var obj in lstUser)
                {
                    this.ObjListSearchUser.Add(obj);
                }

                if (!string.IsNullOrEmpty(this.ObjSearchUser.USER_NAME))
                {
                    this.ObjListSearchUser = this.ObjListSearchUser.Where(x => x.USER_NAME.ToLower().Contains(this.ObjSearchUser.USER_NAME.ToLower()) || x.FULL_NAME.ToLower().Contains(this.ObjSearchUser.USER_NAME.ToLower())).ToList();
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Create()
        {
            if (this.ObjDetail.FROM_DATE > this.ObjDetail.TO_DATE)
            {
                this.State = false;
                this.ErrorMessage = "(Từ ngày) phải nhỏ hơn hoặc bằng (Đến ngày) !";
                return;
            }
            try
            {
                this.ObjDetail.CODE = this.GetSequence("REVIEW_FINAL");
                this.ObjDetail.STATUS = Review_Final_Status.KhoiTao;
                this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;

                var lstMdRating = this.UnitOfWork.Repository<MdRatingRepo>().GetAll();
                var lstReview = this.UnitOfWork.Repository<ReviewRepo>().Queryable().Where(x => this.ObjDetail.FROM_DATE <= x.END_DATE && x.END_DATE <= this.ObjDetail.TO_DATE && x.STATUS == Review_Status.KetThuc).ToList();
                UnitOfWork.BeginTransaction();
                foreach (var item in this.ObjDetail.ListUser.Where(x => x.IS_DELETE != "1").ToList())
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.REVIEW_ID = this.ObjDetail.CODE;
                    item.IS_APPROVED = false;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<ReviewFinalUserRepo>().Create(item);
                }

                List<T_EB_REVIEW_SPEC_SCORE> lstReviewSpecScore = new List<T_EB_REVIEW_SPEC_SCORE>();
                foreach (var item in lstReview)
                {
                    var lstReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>().Queryable().Where(x => x.REVIEW_ID == item.CODE).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
                    if (lstReviewSpec.Sum(x => x.RATIO) > 0)
                    {
                        foreach (var contractor in item.ListContractor)
                        {
                            var headerId = Guid.NewGuid().ToString();
                            if (this.ObjListContractor.Count(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID) == 0)
                            {
                                this.ObjListContractor.Add(new T_EB_REVIEW_FINAL_CONTRACTOR()
                                {
                                    PKID = headerId,
                                    REVIEW_ID = this.ObjDetail.CODE,
                                    CONTRACTOR_ID = contractor.CONTRACTOR_ID,
                                    CREATE_BY = ProfileUtilities.User.USER_NAME
                                });
                            }
                            else
                            {
                                headerId = this.ObjListContractor.FirstOrDefault(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID).PKID;
                            }

                            var lstDetail = contractor.ListDetail;
                            if (lstDetail.Count() > 0)
                            {
                                var lstIdDetail = lstDetail.Select(x => x.PKID).ToList();
                                lstReviewSpecScore.AddRange(this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID)).ToList());
                            }

                            foreach (var detail in contractor.ListDetail)
                            {
                                //Tính điểm trung bình theo từng công việc
                                //decimal score = 0;
                                //foreach (var spec in lstReviewSpec)
                                //{
                                //    var findSpecCouncil = lstReviewSpecScore.Where(x => x.SPEC_ID == spec.PKID && x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID).ToList();
                                //    decimal value = 0;
                                //    if (findSpecCouncil.Count > 0)
                                //    {
                                //        value = ((decimal)findSpecCouncil.Sum(x => x.SCORE) / (decimal)findSpecCouncil.Count);
                                //    }
                                //    if (lstReviewSpec.Count(x => x.PARENT_ID == spec.PKID) == 0)
                                //    {
                                //        score = value * spec.RATIO;
                                //        detail.TOTAL_SCORE += score;
                                //    }
                                //}
                                //decimal medium_score = detail.TOTAL_SCORE / lstReviewSpec.Sum(x => x.RATIO);
                                var rate = "";
                                if (lstMdRating.Count(x => x.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= x.TO_VALUE) > 0 && !string.IsNullOrEmpty(lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= x.TO_VALUE).RATING))
                                {
                                    rate = lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= x.TO_VALUE).RATING;
                                }
                                //Add T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL
                                this.ObjListContractorDetail.Add(new T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL()
                                {
                                    PKID = Guid.NewGuid().ToString(),
                                    HEADER_ID = headerId,
                                    REVIEW_ID = item.CODE,
                                    REVIEW_FINAL_ID = this.ObjDetail.CODE,
                                    REVIEW_CONTRACTOR_DETAIL_ID = detail.PKID,
                                    ORG_CODE = item.ORG_CODE,
                                    INFO_BID = detail.INFO_BID,
                                    INFO_BID_EBID = detail.INFO_BID_EBID,
                                    CONG_VIEC = detail.CONG_VIEC,
                                    PROJECT_CODE = item.PROJECT_CODE,
                                    GIA_TRI = detail.GIA_TRI,
                                    RATIO_IN_PROJECT = detail.RATIO_IN_PROJECT,
                                    SCORE = detail.MEDIUM_SCORE,
                                    RATE = rate,
                                    CREATE_BY = ProfileUtilities.User.USER_NAME
                                });
                            }
                        }
                    }
                }

                //Tính điểm và xếp loại
                foreach (var item in this.ObjListContractor)
                {
                    var lstDetail = this.ObjListContractorDetail.Where(x => x.HEADER_ID == item.PKID).ToList();
                    var totalScoreDetail = lstDetail.Sum(x => x.SCORE);
                    item.SCORE = totalScoreDetail / lstDetail.Count();
                    item.SCORE = Math.Round(item.SCORE, 2);
                    var rate = lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= item.SCORE && item.SCORE <= x.TO_VALUE);
                    if (rate != null)
                    {
                        item.RATE = rate.RATING;
                    }
                }

                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.KhoiTao,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });

                UnitOfWork.Repository<ReviewFinalContractorRepo>().Create(this.ObjListContractor);
                UnitOfWork.Repository<ReviewFinalContractorDetailRepo>().Create(this.ObjListContractorDetail);
                this.CurrentRepository.Create(this.ObjDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Update()
        {
            if (this.ObjDetail.FROM_DATE > this.ObjDetail.TO_DATE)
            {
                this.State = false;
                this.ErrorMessage = "(Từ ngày) phải nhỏ hơn hoặc bằng (Đến ngày) !";
                return;
            }
            dynamic param = new ExpandoObject();
            param.IsFetch_ListContractor = true;
            T_EB_REVIEW_FINAL reviewFinalOld = this.CurrentRepository.Get(this.ObjDetail.CODE, param);
            var lstMdRating = this.UnitOfWork.Repository<MdRatingRepo>().GetAll();
            try
            {
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                var lstOldUser = UnitOfWork.Repository<ReviewFinalUserRepo>().Queryable().Where(x => x.REVIEW_ID == this.ObjDetail.CODE).ToList();
                var lstReview = this.UnitOfWork.Repository<ReviewRepo>().Queryable().Where(x => this.ObjDetail.FROM_DATE <= x.END_DATE && x.END_DATE <= this.ObjDetail.TO_DATE && x.STATUS == Review_Status.KetThuc).ToList();
                var lstReviewCode = lstReview.Select(x => x.CODE).ToList();
                var lstReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>().Queryable().Where(x => lstReviewCode.Contains(x.REVIEW_ID)).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
                var lstOldContractorId = this.ObjListContractor.Select(x => x.CONTRACTOR_ID).ToList();

                IList<T_EB_REVIEW_CONTRACTOR> lstContractor = new List<T_EB_REVIEW_CONTRACTOR>();
                IList<T_EB_REVIEW_CONTRACTOR_DETAIL> lstContractorDetail = new List<T_EB_REVIEW_CONTRACTOR_DETAIL>();
                foreach (var item in lstReview)
                {
                    foreach (var contractor in item.ListContractor)
                    {
                        lstContractor.Add(contractor);
                        foreach (var detail in contractor.ListDetail)
                        {
                            lstContractorDetail.Add(detail);
                        }
                    }
                }
                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                var content = "";
                if (reviewFinalOld.FROM_DATE != this.ObjDetail.FROM_DATE || reviewFinalOld.TO_DATE != this.ObjDetail.TO_DATE)
                {
                    //Xóa toàn bộ list Detial
                    this.UnitOfWork.GetSession().Query<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>().Where(x => x.REVIEW_FINAL_ID == this.ObjDetail.CODE).Delete();

                    //Lưu nội dung thay đổi nhà thầu
                    content = "Thông tin nhà thầu bị thay đổi</br>";
                    foreach (var item in reviewFinalOld.ListContractor)
                    {
                        content += $"&nbsp;&nbsp;Nhà thầu: [{item.Contractor.TEN_NGAN}] - [Điểm TB: {item.SCORE} | Xếp loại: {item.RATE}] </br>";
                        foreach (var detail in item.ListDetail)
                        {
                            content += $"&nbsp;&nbsp;&nbsp;&nbsp;- {detail.INFO_BID}: [{detail.INFO_BID_EBID}] - [Điểm TB: {detail.SCORE} | Xếp loại: {detail.RATE}] </br>";
                        }
                    }

                    List<string> lstNewContractorId = new List<string>();

                    foreach (var item in lstReview)
                    {
                        foreach (var contractor in lstContractor.Where(x => x.REVIEW_ID == item.CODE).ToList())
                        {
                            List<T_EB_REVIEW_SPEC_SCORE> lstReviewSpecScore = new List<T_EB_REVIEW_SPEC_SCORE>();
                            if (!lstNewContractorId.Contains(contractor.CONTRACTOR_ID))
                            {
                                lstNewContractorId.Add(contractor.CONTRACTOR_ID);
                            }
                            var oldContractor = reviewFinalOld.ListContractor.FirstOrDefault(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID);
                            var lsNewContractorDetailId = lstContractorDetail.Where(x => x.HEADER_ID == contractor.PKID).Select(x => x.PKID).ToList();

                            var a = this.ObjListContractor.Count(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID);

                            //Nếu chưa tồn tại NCC thì insert
                            var headerId = Guid.NewGuid().ToString();
                            if (this.ObjListContractor.Count(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID) == 0)
                            {
                                this.ObjListContractor.Add(new T_EB_REVIEW_FINAL_CONTRACTOR()
                                {
                                    PKID = headerId,
                                    REVIEW_ID = this.ObjDetail.CODE,
                                    CONTRACTOR_ID = contractor.CONTRACTOR_ID,
                                    IS_NEW_CONTRACTOR = true,
                                    CREATE_BY = ProfileUtilities.User.USER_NAME
                                });
                            }
                            else
                            {
                                headerId = this.ObjListContractor.FirstOrDefault(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID).PKID;
                            }

                            //Lấy điểm theo từng công việc
                            var lstDetail = lstContractorDetail.Where(x => x.HEADER_ID == contractor.PKID).ToList();
                            if (lstDetail.Count() > 0)
                            {
                                var lstIdDetail = lstDetail.Select(x => x.PKID).ToList();
                                //var lstReviewSpecScore = this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID)).ToList();
                                lstReviewSpecScore.AddRange(this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID)).ToList());
                            }

                            foreach (var detail in lstContractorDetail.Where(x => x.HEADER_ID == contractor.PKID).ToList())
                            {
                                //Tính điểm trung bình theo từng công việc
                                //decimal score = 0;
                                //foreach (var spec in lstReviewSpec.Where(x => x.REVIEW_ID == item.CODE).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList())
                                //{
                                //    var findSpecCouncil = lstReviewSpecScore.Where(x => x.SPEC_ID == spec.PKID && x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID).ToList();
                                //    decimal value = 0;
                                //    if (findSpecCouncil.Count > 0)
                                //    {
                                //        value = ((decimal)findSpecCouncil.Sum(x => x.SCORE) / (decimal)findSpecCouncil.Count);
                                //    }
                                //    if (lstReviewSpec.Where(x => x.REVIEW_ID == item.CODE).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).Count(x => x.PARENT_ID == spec.PKID) == 0)
                                //    {
                                //        score = value * spec.RATIO;
                                //        detail.TOTAL_SCORE += score;
                                //    }
                                //}
                                //decimal medium_score = detail.TOTAL_SCORE / lstReviewSpec.Where(x => x.REVIEW_ID == item.CODE).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).Sum(x => x.RATIO);
                                var rate = "";
                                if (lstMdRating.Count(x => x.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= x.TO_VALUE) > 0 && !string.IsNullOrEmpty(lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= x.TO_VALUE).RATING))
                                {
                                    rate = lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= detail.MEDIUM_SCORE && detail.MEDIUM_SCORE <= x.TO_VALUE).RATING;
                                }

                                //Add T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL
                                this.ObjListContractorDetail.Add(new T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL()
                                {
                                    PKID = Guid.NewGuid().ToString(),
                                    HEADER_ID = headerId,
                                    REVIEW_ID = item.CODE,
                                    REVIEW_FINAL_ID = this.ObjDetail.CODE,
                                    ORG_CODE = item.ORG_CODE,
                                    PROJECT_CODE = item.PROJECT_CODE,
                                    INFO_BID = detail.INFO_BID,
                                    INFO_BID_EBID = detail.INFO_BID_EBID,
                                    CONG_VIEC = detail.CONG_VIEC,
                                    GIA_TRI = detail.GIA_TRI,
                                    RATIO_IN_PROJECT = detail.RATIO_IN_PROJECT,
                                    SCORE = detail.MEDIUM_SCORE,
                                    RATE = rate,
                                    CREATE_BY = ProfileUtilities.User.USER_NAME
                                });

                            }
                        }
                    }

                    //Nếu khoảng ngày thay đổi nhỏ hơn khoảng ngày ban đầu thì xóa những NCC ko nằm trong khoảng ngày mới
                    List<string> lstDeleteId = new List<string>();
                    foreach (var oldId in lstOldContractorId)
                    {
                        if (!lstNewContractorId.Contains(oldId))
                        {
                            lstDeleteId.Add(oldId);
                        }
                    }
                    var lstDeleteContractor = this.ObjListContractor.Where(x => lstDeleteId.Contains(x.CONTRACTOR_ID)).ToList();
                    if (lstDeleteContractor.Count > 0)
                    {
                        this.ObjListContractor = this.ObjListContractor.Where(x => !lstDeleteId.Contains(x.CONTRACTOR_ID)).ToList();
                        UnitOfWork.Repository<ReviewFinalContractorRepo>().Delete(lstDeleteContractor);
                    }

                    //Lưu NCC
                    foreach (var contractor in this.ObjListContractor)
                    {
                        //if (contractor.IS_NEW_CONTRACTOR)
                        //{
                        //    var lstDetail = this.ObjListContractorDetail.Where(x => x.HEADER_ID == contractor.PKID).ToList();
                        //    var totalScoreDetail = lstDetail.Sum(x => x.SCORE);
                        //    contractor.SCORE = totalScoreDetail / lstDetail.Count();
                        //    var rate = lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= contractor.SCORE && contractor.SCORE <= x.TO_VALUE);
                        //    if (rate != null)
                        //    {
                        //        contractor.RATE = rate.RATING;
                        //    }
                        //}

                        var lstDetail = this.ObjListContractorDetail.Where(x => x.HEADER_ID == contractor.PKID).ToList();
                        var totalScoreDetail = lstDetail.Sum(x => x.SCORE);
                        if (lstDetail.Count() > 0)
                        {
                            contractor.SCORE = totalScoreDetail / lstDetail.Count();
                        }
                        else
                        {
                            contractor.SCORE = 0;
                        }

                        var rate = lstMdRating.FirstOrDefault(x => x.FROM_VALUE <= contractor.SCORE && contractor.SCORE <= x.TO_VALUE);
                        if (rate != null)
                        {
                            contractor.RATE = rate.RATING;
                        }
                        contractor.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<ReviewFinalContractorRepo>().CreateOrUpdate(contractor);

                        //Lưu comment
                        if (!string.IsNullOrEmpty(contractor.COMMENT))
                        {
                            UnitOfWork.Repository<ReviewFinalNoteRepo>().Create(new T_EB_REVIEW_FINAL_NOTE()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = contractor.PKID,
                                NOTES = contractor.COMMENT,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }
                }
                else
                {
                    content = "Thông tin bị thay đổi</br>";
                    foreach (var item in reviewFinalOld.ListContractor)
                    {
                        this.ObjListContractorDetail.AddRange(item.ListDetail);
                        var contractor = this.ObjListContractor.FirstOrDefault(x => x.CONTRACTOR_ID == item.CONTRACTOR_ID);
                        content += $"&nbsp;&nbsp;Nhà thầu: [{item.Contractor.TEN_NGAN}] - [Điểm TB: {item.SCORE} --> {contractor.SCORE} | Xếp loại: {item.RATE} --> {contractor.RATE}] </br>";
                    }

                    //Lưu NCC
                    foreach (var contractor in this.ObjListContractor)
                    {
                        contractor.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<ReviewFinalContractorRepo>().CreateOrUpdate(contractor);

                        //Lưu comment
                        if (!string.IsNullOrEmpty(contractor.COMMENT))
                        {
                            UnitOfWork.Repository<ReviewFinalNoteRepo>().Create(new T_EB_REVIEW_FINAL_NOTE()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = contractor.PKID,
                                NOTES = contractor.COMMENT,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }
                }





                //Xóa detail nếu ko thay đổi khoảng ngày
                if (reviewFinalOld.FROM_DATE == this.ObjDetail.FROM_DATE && reviewFinalOld.TO_DATE == this.ObjDetail.TO_DATE)
                {
                    this.ObjListContractorDetail = new List<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>();
                }

                if (reviewFinalOld.CREATE_BY == ProfileUtilities.User.USER_NAME && reviewFinalOld.STATUS == Review_Final_Status.KhoiTao)
                {
                    var lstUserId = this.ObjDetail.ListUser.Where(x => x.IS_DELETE == "1" && !string.IsNullOrEmpty(x.PKID)).Select(x => x.PKID).ToList();

                    //Cập nhật User
                    foreach (var item in this.ObjDetail.ListUser)
                    {
                        if (!string.IsNullOrEmpty(item.PKID) && item.IS_DELETE != "1")
                        {
                            item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                            UnitOfWork.Repository<ReviewFinalUserRepo>().Update(item);
                        }
                        else if (string.IsNullOrEmpty(item.PKID) && item.IS_DELETE != "1")
                        {
                            item.PKID = Guid.NewGuid().ToString();
                            item.REVIEW_ID = this.ObjDetail.CODE;
                            item.IS_APPROVED = false;
                            item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                            UnitOfWork.Repository<ReviewFinalUserRepo>().Create(item);
                        }
                    }

                    //var lstUserDelete = lstOldUser.Where(x => !lstUserName.Contains(x.USER_NAME)).ToList();
                    //UnitOfWork.Repository<ReviewFinalUserRepo>().Delete(lstUserDelete);
                    this.UnitOfWork.GetSession().Query<T_EB_REVIEW_FINAL_USER>().Where(x => x.REVIEW_ID == this.ObjDetail.CODE && lstUserId.Contains(x.PKID)).Delete();
                }

                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.ChinhSua,
                    STATUS_DATE = DateTime.Now,
                    NOTES = content,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });

                UnitOfWork.Repository<ReviewFinalContractorDetailRepo>().Delete(this.ObjListContractorDetailOld);
                UnitOfWork.Repository<ReviewFinalContractorDetailRepo>().Create(this.ObjListContractorDetail);
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

        public void AddContractorToReview(string reviewId, string lstContractor)
        {
            try
            {
                var lstReviewContractor = UnitOfWork.Repository<ReviewFinalContractorRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId).ToList();
                if (lstReviewContractor.Count(x => lstContractor.Contains(x.CONTRACTOR_ID)) > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Nhà thầu đã tồn tại trong bảng xếp loại!";
                    return;
                }
                var lstEBContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(x => lstContractor.Contains(x.PKID)).ToList();
                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                var content = "Thêm mới nhà thầu </br>";
                foreach (var contractorId in lstContractor.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var contractor = lstEBContractor.FirstOrDefault(x => x.PKID == contractorId);
                    var reviewContractorId = Guid.NewGuid().ToString();
                    var item = new T_EB_REVIEW_FINAL_CONTRACTOR()
                    {
                        PKID = reviewContractorId,
                        REVIEW_ID = reviewId,
                        CONTRACTOR_ID = contractorId,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    var detail = new T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        HEADER_ID = reviewContractorId,
                        INFO_BID = "Nhà thầu ko có đánh giá trong khoảng thời gian đã chọn.",
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    content += $"&nbsp;&nbsp;&nbsp;&nbsp;Nhà thầu: [{item.Contractor.TEN_NGAN}] </br>";
                    // Tạo mới history
                    UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                    {
                        REVIEW_ID = this.ObjDetail.CODE,
                        STATUS = Review_Final_Status.ChinhSua,
                        STATUS_DATE = DateTime.Now,
                        NOTES = content,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    UnitOfWork.Repository<ReviewFinalContractorRepo>().Create(item);
                    UnitOfWork.Repository<ReviewFinalContractorDetailRepo>().Create(detail);
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void DeleteContractor(string contractorId, string reviewId)
        {
            try
            {
                var contractor = UnitOfWork.Repository<ReviewFinalContractorRepo>().Queryable().FirstOrDefault(x => x.CONTRACTOR_ID == contractorId && x.REVIEW_ID == reviewId);
                UnitOfWork.BeginTransaction();
                if (contractor != null)
                {
                    UnitOfWork.Repository<ReviewFinalContractorRepo>().Delete(contractor);
                    UnitOfWork.GetSession().Query<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>().Where(x => x.HEADER_ID == contractor.PKID).Delete();

                    // Tạo mới history
                    var content = "Xóa nhà thầu khỏi bảng xếp loại!</br>";
                    content += $"&nbsp;&nbsp;&nbsp;&nbsp;Nhà thầu: [{contractor.Contractor.TEN_NGAN}] - [Điểm TB: {contractor.SCORE} | Xếp loại: {contractor.RATE}] </br>";

                    UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                    {
                        REVIEW_ID = this.ObjDetail.CODE,
                        STATUS = Review_Final_Status.ChinhSua,
                        STATUS_DATE = DateTime.Now,
                        NOTES = content,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
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

        public void Send(string code)
        {
            try
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListContractor = true;
                this.Get(code, param);
                if (this.ObjDetail.ListUser.Count(x => x.C_LEVEL == 0) > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Thứ tự phê duyệt phải lớn hơn 0!";
                    return;
                }

                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = Review_Final_Status.ChoPheDuyet;

                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.ChoPheDuyet,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CancelSend(string code)
        {
            try
            {
                this.Get(code);

                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = Review_Final_Status.KhoiTao;

                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.HuyTrinhDuyet,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Cancel(string code)
        {
            try
            {
                this.Get(code);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = Review_Final_Status.Huy;

                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.Huy,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Approve(string code)
        {
            try
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListContractor = true;
                this.Get(code, param);

                var userApprove = this.ObjDetail.ListUser.FirstOrDefault(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == false);
                if (userApprove == null)
                {
                    this.State = false;
                    return;
                }

                // Kiểm tra user level thấp hơn đã phê duyệt hay chưa
                if (this.ObjDetail.ListUser.FirstOrDefault(x => x.C_LEVEL < userApprove.C_LEVEL && x.IS_APPROVED == false) != null)
                {
                    this.State = false;
                    return;
                }

                userApprove.IS_APPROVED = true;
                userApprove.DATE_APPROVE = DateTime.Now;
                this.ObjDetail.STATUS = Review_Final_Status.DaPheDuyet;
                if (this.ObjDetail.ListUser.Count(x => x.IS_APPROVED == false) > 0)
                {
                    this.ObjDetail.STATUS = Review_Final_Status.DangPheDuyet;
                }
                else
                {
                    this.ObjDetail.APPROVE_DATE = DateTime.Now;
                }
                userApprove.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                //Cập nhật xếp hạng cho nhà thầu
                if (this.ObjDetail.STATUS == Review_Final_Status.DaPheDuyet)
                {
                    var lstContractorId = this.ObjListContractor.Select(x => x.CONTRACTOR_ID).ToList();
                    var lstContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(x => lstContractorId.Contains(x.PKID)).ToList();
                    foreach (var item in lstContractor)
                    {
                        var reviewContractor = this.ObjListContractor.FirstOrDefault(x => x.CONTRACTOR_ID == item.PKID);
                        item.XEP_HANG = reviewContractor.RATE;
                        item.DIEM_DANH_GIA = reviewContractor.SCORE;
                        item.NGAY_DANH_GIA = this.ObjDetail.CREATE_DATE.Value;
                        item.REVIEW_FINAL_CODE = this.ObjDetail.CODE;
                        item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<ContractorRepo>().Update(item);
                    }
                }
                UnitOfWork.Repository<ReviewFinalUserRepo>().Update(userApprove);
                this.CurrentRepository.Update(this.ObjDetail);

                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.DaPheDuyet,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });

                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CancelApprove(string code)
        {
            try
            {
                this.Get(code);

                var userApprove = this.ObjDetail.ListUser.FirstOrDefault(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == true);
                if (userApprove == null)
                {
                    this.State = false;
                    return;
                }

                // Kiểm tra user level thấp hơn đã phê duyệt hay chưa
                if (this.ObjDetail.ListUser.FirstOrDefault(x => x.C_LEVEL < userApprove.C_LEVEL && x.IS_APPROVED == false) != null)
                {
                    this.State = false;
                    return;
                }

                userApprove.IS_APPROVED = false;
                userApprove.DATE_APPROVE = null;
                this.ObjDetail.STATUS = Review_Final_Status.ChoPheDuyet;
                if (this.ObjDetail.ListUser.Count(x => x.C_LEVEL < userApprove.C_LEVEL && x.IS_APPROVED == true) > 0)
                {
                    this.ObjDetail.STATUS = Review_Final_Status.DangPheDuyet;
                }
                userApprove.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.HuyPheDuyet,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Reject(string code)
        {
            try
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListContractor = true;
                this.Get(code, param);

                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = Review_Final_Status.TuChoi;

                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                foreach (var item in this.ObjDetail.ListUser)
                {
                    item.IS_APPROVED = false;
                    UnitOfWork.Repository<ReviewFinalUserRepo>().Update(item);
                }

                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo mới history
                UnitOfWork.Repository<ReviewFinalHistoryRepo>().Create(new T_EB_REVIEW_FINAL_HISTORY()
                {
                    REVIEW_ID = this.ObjDetail.CODE,
                    STATUS = Review_Final_Status.TuChoi,
                    STATUS_DATE = DateTime.Now,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Get(object id, dynamic param = null)
        {
            try
            {
                this.ObjDetail = this.CurrentRepository.Get(id, param);
                this.ObjListContractor = this.ObjDetail.ListContractor.ToList();
                this.ObjListRating = this.UnitOfWork.Repository<MdRatingRepo>().GetAll().OrderBy(x => x.RATING).ToList();
                this.ObjDetail.ListHistory = UnitOfWork.Repository<ReviewFinalHistoryRepo>().Queryable().Where(x => x.REVIEW_ID == id).ToList();
                this.ObjDetail.ListUser = UnitOfWork.Repository<ReviewFinalUserRepo>().Queryable().Where(x => x.REVIEW_ID == id).ToList();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CreateNotify()
        {
            try
            {
                var lstUserNotify = new List<string>();
                var newId = Guid.NewGuid().ToString();
                var serviceNotify = new NotifyService();
                var lstUser = new List<string>();
                lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_TYPE == UserType.NB).Select(x => x.USER_NAME).ToList();

                var history = UnitOfWork.Repository<ReviewFinalHistoryRepo>().Queryable().Where(x => x.REVIEW_ID == this.ObjDetail.CODE).OrderByDescending(x => x.CREATE_DATE).Select(x => x.STATUS).FirstOrDefault();
                var ReviewFinalCurrentUser = this.ObjDetail.ListUser.Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == true).FirstOrDefault();
                var lstCurrentLevel = this.ObjDetail.ListUser.Where(x => x.IS_APPROVED == false).ToList();
                if (ReviewFinalCurrentUser != null)
                {
                    lstCurrentLevel = lstCurrentLevel.Where(x => x.C_LEVEL == ReviewFinalCurrentUser.C_LEVEL && x.USER_NAME != ReviewFinalCurrentUser.USER_NAME && x.IS_APPROVED == false).ToList();
                }

                if (this.ObjDetail.STATUS == Review_Final_Status.ChoPheDuyet || (this.ObjDetail.STATUS == Review_Final_Status.DangPheDuyet && lstCurrentLevel.Count < 1))
                {
                    var lstUserApprove = this.ObjDetail.ListUser;
                    if (this.ObjDetail.STATUS == Review_Final_Status.DangPheDuyet && ReviewFinalCurrentUser != null)
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false && x.C_LEVEL > ReviewFinalCurrentUser.C_LEVEL).ToList();
                    }
                    else
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false).ToList();
                    }

                    var minLevel = lstUserApprove.Min(x => x.C_LEVEL);
                    var lstUserNameApprove = lstUserApprove.Where(x => x.C_LEVEL == minLevel).Select(x => x.USER_NAME).ToList();

                    lstUserNotify.AddRange(lstUserNameApprove);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Xếp hạng NCC [{4} - {5}] đang chờ phê duyệt!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        Guid.NewGuid().ToString(),
                        $"/EB/ReviewFinal/Detail?id={this.ObjDetail.CODE}",
                        Review_Final_Status.GetStatusColor(this.ObjDetail.STATUS),
                        Review_Final_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.CODE,
                        this.ObjDetail.DESCRIPTION,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Xếp hạng NCC {this.ObjDetail.CODE} đang chờ phê duyệt!";


                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent,
                            RAW_CONTENTS_EN = strRawContent,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjDetail.STATUS == Review_Final_Status.KhoiTao && history == Review_Final_Status.HuyTrinhDuyet)
                {
                    var lstUserApprove = this.ObjDetail.ListUser;
                    var minLevel = lstUserApprove.Min(x => x.C_LEVEL);
                    var lstUserNameApprove = lstUserApprove.Where(x => x.C_LEVEL == minLevel).Select(x => x.USER_NAME).ToList();

                    lstUserNotify.AddRange(lstUserNameApprove);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Xếp hạng NCC [{4} - {5}] đã hủy trình duyệt!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNameApprove)
                    {

                        string strContent = string.Format(strTemplate,
                        Guid.NewGuid().ToString(),
                        $"/EB/ReviewFinal/Detail?id={this.ObjDetail.CODE}",
                        Review_Final_Status.GetStatusColor(this.ObjDetail.STATUS),
                        Review_Final_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.CODE,
                        this.ObjDetail.DESCRIPTION,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Xếp hạng NCC {this.ObjDetail.CODE} đã hủy trình duyệt!";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent,
                            RAW_CONTENTS_EN = strRawContent,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjDetail.STATUS == Review_Final_Status.DaPheDuyet)
                {
                    lstUserNotify.Add(this.ObjDetail.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Xếp hạng NCC [{4} - {5}] đã được phê duyệt!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/ReviewFinal/Detail?id={this.ObjDetail.CODE}",
                        Review_Final_Status.GetStatusColor(this.ObjDetail.STATUS),
                        Review_Final_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.CODE,
                        this.ObjDetail.DESCRIPTION,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Xếp hạng NCC {this.ObjDetail.CODE} đã được phê duyệt!";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent,
                            RAW_CONTENTS_EN = strRawContent,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjDetail.STATUS == Review_Final_Status.TuChoi)
                {
                    lstUserNotify.Add(this.ObjDetail.CREATE_BY);
                    var query = UnitOfWork.Repository<ReviewFinalHistoryRepo>().Queryable();
                    var lstUserApproval = query.Where(x => x.REVIEW_ID == this.ObjDetail.CODE && x.STATUS == Review_Final_Status.DaPheDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.CREATE_BY).ToList();
                    if (lstUserApproval.Count > 0)
                    {
                        foreach (var user in lstUserApproval)
                        {
                            lstUserNotify.Add(user);
                        }
                    }

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Xếp hạng NCC [{4} - {5}] đã bị từ chối!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/ReviewFinal/Detail?id={this.ObjDetail.CODE}",
                        Review_Final_Status.GetStatusColor(this.ObjDetail.STATUS),
                        Review_Final_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.CODE,
                        this.ObjDetail.DESCRIPTION,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Xếp hạng NCC {this.ObjDetail.CODE} đã bị từ chối!";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent,
                            RAW_CONTENTS_EN = strRawContent,
                            USER_NAME = userName
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

        public void CreateNotiEmail()
        {
            try
            {
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

                var history = UnitOfWork.Repository<ReviewFinalHistoryRepo>().Queryable().Where(x => x.REVIEW_ID == this.ObjDetail.CODE).OrderByDescending(x => x.CREATE_DATE).Select(x => x.STATUS).FirstOrDefault();
                var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_TYPE == UserType.NB).ToList();
                var userCreate = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_NAME == this.ObjDetail.CREATE_BY).FirstOrDefault();

                if (this.ObjDetail.STATUS == Review_Final_Status.DaPheDuyet || this.ObjDetail.STATUS == Review_Final_Status.TuChoi)
                {
                    subjectTemplate = template.CONG_VIEC_HOAN_THANH_SUBJECT;
                    bodyTemplate = template.CONG_VIEC_HOAN_THANH_BODY;
                }

                if (string.IsNullOrWhiteSpace(subjectTemplate) || string.IsNullOrWhiteSpace(bodyTemplate))
                {
                    return;
                }

                var contentSubject = subjectTemplate
                        .Replace(LOAI_CONG_VIEC, "Xếp hạng NCC")
                        .Replace(MA, this.ObjDetail.CODE)
                        .Replace(TEN, this.ObjDetail.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Final_Status.GetStatusText(this.ObjDetail.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReviewFinal?id=" + this.ObjDetail.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserCreate.FULL_NAME);

                if (this.ObjDetail.ListUser.Count > 0)
                {
                    var fullName = "";
                    foreach (var user in this.ObjDetail.ListUser)
                    {
                        fullName += user.USER_NAME + ", ";
                    }
                    contentSubject = contentSubject.Replace(HO_TEN_NGUOI_PHE_DUYET, fullName);
                }
                if (this.ObjDetail.UserCreate.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DON_VI_PHONG_BAN, this.ObjDetail.UserCreate.Organize.NAME);
                }

                var lstMailNotify = new List<string>();
                var reviewFinalCurrentUser = this.ObjDetail.ListUser.Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == true).FirstOrDefault();
                var lstCurrentLevel = this.ObjDetail.ListUser.Where(x => x.IS_APPROVED == false).ToList();
                if (reviewFinalCurrentUser != null)
                {
                    lstCurrentLevel = lstCurrentLevel.Where(x => x.C_LEVEL == reviewFinalCurrentUser.C_LEVEL && x.USER_NAME != reviewFinalCurrentUser.USER_NAME && x.IS_APPROVED == false).ToList();
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Xếp hạng NCC")
                        .Replace(MA, this.ObjDetail.CODE)
                        .Replace(TEN, this.ObjDetail.DESCRIPTION)
                        .Replace(TRANG_THAI, Review_Final_Status.GetStatusText(this.ObjDetail.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenReviewFinal?id=" + this.ObjDetail.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserCreate.FULL_NAME);

                if (this.ObjDetail.ListUser.Count > 0)
                {
                    var fullName = "";
                    foreach (var user in this.ObjDetail.ListUser)
                    {
                        fullName += user.USER_NAME + ", ";
                    }
                    contentBody =  contentBody.Replace(HO_TEN_NGUOI_PHE_DUYET, fullName);
                }
                if (this.ObjDetail.UserCreate.Organize != null)
                {
                    contentBody = contentBody.Replace(DON_VI_PHONG_BAN, this.ObjDetail.UserCreate.Organize.NAME);
                }

                if (this.ObjDetail.STATUS == Review_Final_Status.ChoPheDuyet || (this.ObjDetail.STATUS == Review_Final_Status.DangPheDuyet && lstCurrentLevel.Count < 1) || (this.ObjDetail.STATUS == EB_Status.KhoiTao && history == Review_Final_Status.HuyTrinhDuyet))
                {
                    var lstUserApprove = this.ObjDetail.ListUser;
                    if (this.ObjDetail.STATUS == Review_Final_Status.DangPheDuyet && reviewFinalCurrentUser != null)
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false && x.C_LEVEL > reviewFinalCurrentUser.C_LEVEL).ToList();
                    }
                    else
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false).ToList();
                    }
                    var minLevel = lstUserApprove.Min(x => x.C_LEVEL);
                    var lstUserNameApprove = lstUserApprove.Where(x => x.C_LEVEL == minLevel).Select(x => x.USER_NAME).ToList();

                    foreach (var userName in lstUserNameApprove)
                    {
                        var userMail = lstUser.Where(x => x.USER_NAME == userName).FirstOrDefault();
                        serviceEmail.ObjDetail = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = userMail.EMAIL,
                            SUBJECT = contentSubject,
                            CONTENTS = contentBody
                        };
                        serviceEmail.Create();
                    }

                }
                else if (this.ObjDetail.STATUS == Review_Final_Status.DaPheDuyet)
                {
                    lstMailNotify.Add(userCreate.EMAIL);

                    var userSend = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).FirstOrDefault();

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
                }
                else if (this.ObjDetail.STATUS == Review_Final_Status.TuChoi)
                {
                    var mailUserSendPackage = lstUser.Where(x => x.USER_NAME == this.ObjDetail.CREATE_BY).Select(x => x.EMAIL).FirstOrDefault();
                    lstMailNotify.Add(mailUserSendPackage);

                    var lstUserApprove = this.ObjDetail.ListUser.Where(x => x.IS_APPROVED == true).Select(x => x.USER_NAME).ToList();
                    var lstMailUserApprove = lstUser.Where(x => lstUserApprove.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                    lstMailNotify.AddRange(lstMailUserApprove);

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
                }
                else if ((this.ObjDetail.STATUS == Review_Final_Status.ChoPheDuyet || this.ObjDetail.STATUS == Review_Final_Status.DangPheDuyet) && history == Review_Final_Status.HuyPheDuyet)
                {

                    var lstUserApprove = this.ObjDetail.ListUser.Where(x => x.USER_NAME != ProfileUtilities.User.USER_NAME).Select(x => x.USER_NAME).ToList();
                    var lstMailUserApprove = lstUser.Where(x => lstUserApprove.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                    lstMailNotify.AddRange(lstMailUserApprove);
                    lstMailNotify.Add(userCreate.EMAIL);

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
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}