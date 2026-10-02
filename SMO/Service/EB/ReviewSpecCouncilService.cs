using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class ReviewSpecCouncilService : BaseService
    {
        public IUnitOfWork UnitOfWork { get; set; }
        public T_EB_REVIEW ObjReview { get; set; }
        public List<T_EB_REVIEW_SPEC> ObjListReviewSpec { get; set; }
        public List<T_EB_REVIEW_SPEC_SCORE> ObjListReviewSpecScore { get; set; }
        public List<T_EB_REVIEW_CONTRACTOR> ObjListReviewContractor { get; set; }
        public List<T_EB_REVIEW_CONTRACTOR_DETAIL> ObjListReviewContractorDetail { get; set; }
        public List<T_EB_REVIEW_COUNCIL_NOTE> ObjListReviewCouncilNote { get; set; }
        public IList<T_MD_RATING> ObjListRating { get; set; }
        public List<T_EB_REVIEW_COUNCIL> ObjListReviewCouncil { get; set; }
        public T_MD_PROJECT ObjProject { get; set; }
        public List<string> ListContractorId { get; set; }
        public ReviewSpecCouncilService()
        {
            UnitOfWork = new NHUnitOfWork();
            ObjReview = new T_EB_REVIEW();
            ObjListReviewSpec = new List<T_EB_REVIEW_SPEC>();
            ObjListReviewContractor = new List<T_EB_REVIEW_CONTRACTOR>();
            ObjListReviewSpecScore = new List<T_EB_REVIEW_SPEC_SCORE>();
            ObjListReviewCouncilNote = new List<T_EB_REVIEW_COUNCIL_NOTE>();
            ObjListRating = new List<T_MD_RATING>();
            ObjListReviewCouncil = new List<T_EB_REVIEW_COUNCIL>();
            ObjListReviewContractorDetail = new List<T_EB_REVIEW_CONTRACTOR_DETAIL>();
            ObjProject = new T_MD_PROJECT();
            ListContractorId = new List<string>();
        }

        public void GetSummaryReview(string reviewId, string lstContractor, string lstCouncil)
        {
            List<T_EB_REVIEW_CONTRACTOR> lstReviewContractor = new List<T_EB_REVIEW_CONTRACTOR>();
            this.ObjListReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
            this.ObjListReviewContractor = this.UnitOfWork.Repository<ReviewContractorRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId).ToList();
            this.ObjListRating = this.UnitOfWork.Repository<MdRatingRepo>().GetAll();
            this.ObjListReviewCouncil = this.UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && x.STATUS == Review_Status.KetThuc).ToList();
            this.ObjReview = this.UnitOfWork.Repository<ReviewRepo>().Get(reviewId);
            this.ObjProject = this.UnitOfWork.Repository<ProjectRepo>().Get(this.ObjReview.PROJECT_CODE);
            lstReviewContractor = this.ObjListReviewContractor;
            this.ListContractorId = this.ObjListReviewContractor.Select(x => x.CONTRACTOR_ID).ToList();
            if (this.ObjReview.STATUS == Review_Status.KhoiTao)
            {
                this.ObjListReviewCouncil = this.ObjListReviewCouncil.Where(x => x.STATUS == Review_Status.KetThuc).ToList();
                if (this.ObjReview.CREATE_BY != ProfileUtilities.User.USER_NAME)
                {
                    this.ObjListReviewCouncil = this.ObjListReviewCouncil.Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).ToList();
                }
            }
            if (!string.IsNullOrEmpty(lstContractor))
            {
                this.ListContractorId = lstContractor.Split('|').ToList();
                lstReviewContractor = this.ObjListReviewContractor.Where(x => ListContractorId.Contains(x.CONTRACTOR_ID)).ToList();
            }
            if (!string.IsNullOrEmpty(lstCouncil))
            {
                this.ObjListReviewCouncil = this.ObjListReviewCouncil.Where(x => lstCouncil.Contains(x.USER_NAME) && x.STATUS == Review_Status.KetThuc).ToList();
            }
            foreach (var reviewContractor in lstReviewContractor)
            {
                var lstDetail = reviewContractor.ListDetail;
                if (lstDetail.Count() > 0)
                {
                    // Lấy danh sách điểm, và ghi chú theo người chấm điểm
                    var lstIdDetail = lstDetail.Select(x => x.PKID).ToList();
                    if (this.ObjReview.STATUS == Review_Status.KhoiTao)
                    {
                        var lstCouncilEnd = this.ObjListReviewCouncil.Select(x => x.USER_NAME).ToList();
                        this.ObjListReviewSpecScore.AddRange(this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && lstCouncilEnd.Contains(x.USER_SCORE)).ToList());
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(lstCouncil))
                        {
                            List<string> lstCouncilId = lstCouncil.Split('|').ToList();
                            this.ObjListReviewSpecScore.AddRange(this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && lstCouncilId.Contains(x.USER_SCORE)).ToList());
                        }
                        else
                        {
                            var lstCouncilEnd = this.ObjListReviewCouncil.Select(x => x.USER_NAME).ToList();
                            this.ObjListReviewSpecScore.AddRange(this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && lstCouncilEnd.Contains(x.USER_SCORE)).ToList());
                        }
                    }

                    //this.ObjListReviewCouncilNote.AddRange(this.UnitOfWork.Repository<ReviewCouncilNoteRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID)).ToList());
                }
                if (reviewContractor.ListDetail.Count > 0)
                {
                    foreach (var detail in reviewContractor.ListDetail)
                    {

                        detail.TOTAL_SCORE = 0;
                        decimal score = 0;
                        foreach (var spec in this.ObjListReviewSpec)
                        {
                            var findSpecCouncil = this.ObjListReviewSpecScore.Where(x => x.SPEC_ID == spec.PKID && x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID).ToList();
                            if (this.ObjReview.STATUS != Review_Status.KetThuc && this.ObjListReviewSpecScore.Count(x => x.SPEC_ID == spec.PKID && x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID && x.USER_SCORE == ProfileUtilities.User.USER_NAME) > 0)
                            {
                                findSpecCouncil = findSpecCouncil.Where(x => x.USER_SCORE == ProfileUtilities.User.USER_NAME).ToList();
                            }
                            decimal value = 0;
                            if (findSpecCouncil.Count > 0)
                            {
                                value = ((decimal)findSpecCouncil.Sum(x => x.SCORE) / (decimal)findSpecCouncil.Count);
                            }

                            score = value * spec.RATIO;
                            detail.TOTAL_SCORE += score;
                        }
                    }
                }
            }

        }

        public void Generate_ExportExcelSumary(ref MemoryStream outFileStream, string path, string reviewId)
        {
            try
            {
                this.GetSummaryReview(reviewId, "", "");
                var review = this.UnitOfWork.Repository<ReviewRepo>().Get(reviewId);
                var project = this.UnitOfWork.Repository<ProjectRepo>().Get(review.PROJECT_CODE);

                //Mở file Template
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                IWorkbook templateWorkbook;
                templateWorkbook = new XSSFWorkbook(fs);
                fs.Close();
                ISheet sheet = templateWorkbook.GetSheetAt(0);

                //Số hàng và số cột hiện tại
                int numRowCur = 0;
                int numColCur = 0;

                //Style cần dùng
                ICellStyle styleCellHeader = templateWorkbook.CreateCellStyle();
                styleCellHeader.CloneStyleFrom(sheet.GetRow(6).Cells[0].CellStyle);
                styleCellHeader.WrapText = true;

                ICellStyle styleCellDetail = templateWorkbook.CreateCellStyle();
                styleCellDetail.CloneStyleFrom(sheet.GetRow(9).Cells[0].CellStyle);
                styleCellDetail.WrapText = true;

                #region Tạo header
                if (project != null)
                {
                    var row1 = ReportUtilities.CreateRow(ref sheet, 1);
                    row1.Cells[0].SetCellValue("Giám đốc dự án: " + project.GIAM_DOC);
                    row1.Cells[4].SetCellValue("DỰ ÁN: " + project.NAME.ToUpper());

                    var row2 = ReportUtilities.CreateRow(ref sheet, 2);
                    row2.Cells[0].SetCellValue("Địa điểm: " + project.DIA_DIEM);
                    row2.Cells[4].SetCellValue("Đơn vị: " + review.Organize.NAME);

                    var row3 = ReportUtilities.CreateRow(ref sheet, 3);
                    row3.Cells[0].SetCellValue("Giá trị HĐ: " + project.GIA_TRI_HOP_DONG.ToStringVN());
                    row3.Cells[4].SetCellValue("Ngày đánh giá: " + review.EVALUATE_DATE.ToString("dd/MM/yyyy"));

                    var row4 = ReportUtilities.CreateRow(ref sheet, 4);
                    row4.Cells[0].SetCellValue("Giá trị thực hiện: " + project.GIA_TRI_THUC_HIEN.ToStringVN());
                    row4.Cells[4].SetCellValue("Người lập: " + review.USER_CREATE.FULL_NAME);

                    var row5 = ReportUtilities.CreateRow(ref sheet, 5);
                    row5.Cells[0].SetCellValue("Tiến độ: " + project.TIEN_DO);
                }

                var numberSpec = 1;
                numColCur = 3;
                var row6 = ReportUtilities.CreateRow(ref sheet, 6);
                var row7 = ReportUtilities.CreateRow(ref sheet, 7);
                var row8 = ReportUtilities.CreateRow(ref sheet, 8);
                var row9 = ReportUtilities.CreateRow(ref sheet, 9);
                row6.Cells[4].SetCellValue("CÁC CHỈ TIÊU ĐÁNH GIÁ");

                foreach (var item in this.ObjListReviewSpec)
                {
                    row6.Cells[numColCur + numberSpec].CellStyle = styleCellHeader;

                    row7.Cells[numColCur + numberSpec].CellStyle = styleCellHeader;
                    row7.Cells[numColCur + numberSpec].SetCellValue(numberSpec);

                    row8.Cells[numColCur + numberSpec].CellStyle = styleCellHeader;
                    row8.Cells[numColCur + numberSpec].SetCellValue(item.NAME);

                    row9.Cells[numColCur + numberSpec].CellStyle = styleCellHeader;
                    row9.Cells[numColCur + numberSpec].SetCellValue("Hệ số: " + item.RATIO);

                    numberSpec++;
                }

                CellRangeAddress mergeHeader = new CellRangeAddress(6, 6, 4, 2 + numberSpec);
                sheet.AddMergedRegion(mergeHeader);

                numColCur = numberSpec + 2;
                row6.Cells[numColCur + 1].SetCellValue("Tổng điểm");
                row6.Cells[numColCur + 2].SetCellValue("Điểm trung bình");
                row6.Cells[numColCur + 3].SetCellValue("Xếp loại");
                row6.Cells[numColCur + 4].SetCellValue("Ghi chú");


                row6.Cells[numColCur + 1].CellStyle = styleCellHeader;
                row6.Cells[numColCur + 2].CellStyle = styleCellHeader;
                row6.Cells[numColCur + 3].CellStyle = styleCellHeader;
                row6.Cells[numColCur + 4].CellStyle = styleCellHeader;

                row7.Cells[numColCur + 1].CellStyle = styleCellHeader;
                row7.Cells[numColCur + 2].CellStyle = styleCellHeader;
                row7.Cells[numColCur + 3].CellStyle = styleCellHeader;
                row7.Cells[numColCur + 4].CellStyle = styleCellHeader;

                row8.Cells[numColCur + 1].CellStyle = styleCellHeader;
                row8.Cells[numColCur + 2].CellStyle = styleCellHeader;
                row8.Cells[numColCur + 3].CellStyle = styleCellHeader;
                row8.Cells[numColCur + 4].CellStyle = styleCellHeader;

                row9.Cells[numColCur + 1].CellStyle = styleCellHeader;
                row9.Cells[numColCur + 2].CellStyle = styleCellHeader;
                row9.Cells[numColCur + 3].CellStyle = styleCellHeader;
                row9.Cells[numColCur + 4].CellStyle = styleCellHeader;

                CellRangeAddress mergeHeader1 = new CellRangeAddress(6, 9, numColCur + 1, numColCur + 1);
                sheet.AddMergedRegion(mergeHeader1);

                CellRangeAddress mergeHeader2 = new CellRangeAddress(6, 9, numColCur + 2, numColCur + 2);
                sheet.AddMergedRegion(mergeHeader2);

                CellRangeAddress mergeHeader3 = new CellRangeAddress(6, 9, numColCur + 3, numColCur + 3);
                sheet.AddMergedRegion(mergeHeader3);

                CellRangeAddress mergeHeader4 = new CellRangeAddress(6, 9, numColCur + 4, numColCur + 4);
                sheet.AddMergedRegion(mergeHeader4);
                #endregion

                #region Tạo detail
                var numberRow = 1;
                numRowCur = 9;
                var rowHeightDetail = sheet.GetRow(9).Height;
                decimal sumMax = this.ObjListReviewSpec.Sum(x => x.MAX_SCORE);

                foreach (var reviewContractor in this.ObjListReviewContractor)
                {
                    if (reviewContractor.ListDetail.Count > 0)
                    {
                        foreach (var detail in reviewContractor.ListDetail)
                        {
                            numberRow++;
                            numRowCur++;
                            decimal sumScore = 0;
                            IRow rowCur = ReportUtilities.CreateRow(ref sheet, numRowCur);
                            rowCur.Height = rowHeightDetail;

                            for (int i = 0; i < numberSpec + 7; i++)
                            {
                                rowCur.Cells[i].CellStyle = styleCellDetail;
                            }

                            rowCur.Cells[0].SetCellValue(numberRow);
                            rowCur.Cells[1].SetCellValue(reviewContractor.Contractor.TEN_NGAN);
                            rowCur.Cells[2].SetCellValue(detail.INFO_BID);
                            rowCur.Cells[3].SetCellValue(detail.GIA_TRI.ToStringVN());
                            //if (this.ObjProject != null && this.ObjProject.GIA_TRI_THUC_HIEN > 0)
                            //{
                            //    rowCur.Cells[4].SetCellValue(((detail.GIA_TRI / this.ObjProject.GIA_TRI_THUC_HIEN) * 100).ToStringVN() + " %");
                            //}
                            //else
                            //{
                            //    rowCur.Cells[4].SetCellValue("0 %");
                            //}

                            numColCur = 4;
                            foreach (var spec in this.ObjListReviewSpec)
                            {
                                var findSpecCouncil = this.ObjListReviewSpecScore.Where(x => x.SPEC_ID == spec.PKID && x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID).ToList();
                                decimal value = 0;
                                if (findSpecCouncil.Count > 0)
                                {
                                    //value = (findSpecCouncil.Sum(x => x.SCORE) / findSpecCouncil.Count) * ((decimal)spec.RATIO) / 100;
                                    value = ((decimal)findSpecCouncil.Sum(x => x.SCORE) / (decimal)findSpecCouncil.Count);
                                }
                                sumScore += value;
                                rowCur.Cells[numColCur].SetCellValue(value.ToStringVN());

                                numColCur++;
                            }
                            numColCur = numColCur - 1;
                            rowCur.Cells[numColCur + 1].SetCellValue(detail.TOTAL_SCORE.ToStringVN());
                            rowCur.Cells[numColCur + 2].SetCellValue((detail.TOTAL_SCORE / this.ObjListReviewSpec.Sum(x => x.RATIO)).ToStringVN());
                            var rate = "";
                            if (this.ObjListRating.FirstOrDefault(m => m.FROM_VALUE <= (detail.TOTAL_SCORE / this.ObjListReviewSpec.Sum(x => x.RATIO)) && (detail.TOTAL_SCORE / this.ObjListReviewSpec.Sum(x => x.RATIO)) <= m.TO_VALUE) != null)
                            {
                                rate = this.ObjListRating.FirstOrDefault(m => m.FROM_VALUE <= (detail.TOTAL_SCORE / this.ObjListReviewSpec.Sum(x => x.RATIO)) && (detail.TOTAL_SCORE / this.ObjListReviewSpec.Sum(x => x.RATIO)) <= m.TO_VALUE).RATING;
                            }
                            rowCur.Cells[numColCur + 3].SetCellValue(rate);

                            var ghiChu = string.Empty;
                            foreach (var item in this.ObjListReviewCouncilNote.Where(x => x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID))
                            {
                                ghiChu += item.NOTES + ". ";
                            }
                            rowCur.Cells[numColCur + 4].SetCellValue(ghiChu);
                        }
                    }
                }
                #endregion

                var rowFooter = ReportUtilities.CreateRow(ref sheet, numRowCur + 2);
                rowFooter.Cells[3].SetCellValue("GIÁM ĐỐC DỰ ÁN");
                rowFooter.Cells[7].SetCellValue("PHÒNG CUNG ỨNG");

                templateWorkbook.Write(outFileStream);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.ToString();
                this.State = false;
            }
        }

        /// <summary>
        /// Lấy danh sách điểm đã nhập của người đánh giá
        /// </summary>
        /// <param name="reviewId"></param>
        public void GetListSpecCouncil(string reviewId)
        {
            this.ObjReview = this.UnitOfWork.Repository<ReviewRepo>().Get(reviewId);
            this.ObjListReviewCouncil = this.UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId).ToList();
            this.ObjListRating = this.UnitOfWork.Repository<MdRatingRepo>().GetAll();
            this.ObjProject = this.UnitOfWork.Repository<ProjectRepo>().Get(this.ObjReview.PROJECT_CODE);
            this.ObjListReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>()
                .Queryable().Where(x => x.REVIEW_ID == reviewId).OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
            // Lấy vai trò của người đánh giá
            //var council = this.UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().FirstOrDefault(x => x.REVIEW_ID == reviewId && x.USER_NAME == ProfileUtilities.User.USER_NAME);
            //if (council != null)
            //{
            //    this.ObjListReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>()
            //    .GetAll().OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
            //    if (!string.IsNullOrWhiteSpace(council.VAI_TRO))
            //    {
            //        this.ObjListReviewSpec = this.UnitOfWork.Repository<ReviewSpecRepo>()
            //            .Queryable().Where(x => x.VAI_TRO == "" || x.VAI_TRO == null || x.VAI_TRO == council.VAI_TRO)
            //            .OrderBy(x => x.C_ORDER).ThenBy(x => x.PKID).ToList();
            //    }
            //}

            this.ObjListReviewContractor = this.UnitOfWork.Repository<ReviewContractorRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId).ToList();
            foreach (var reviewContractor in this.ObjListReviewContractor)
            {
                var lstDetail = reviewContractor.ListDetail;
                if (lstDetail.Count() > 0)
                {
                    // Lấy danh sách điểm, và ghi chú theo người chấm điểm
                    var lstIdDetail = lstDetail.Select(x => x.PKID).ToList();
                    var lstSpecScore = this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && x.USER_SCORE == ProfileUtilities.User.USER_NAME).ToList();
                    var lstCouncilNote = this.UnitOfWork.Repository<ReviewCouncilNoteRepo>().Queryable().Where(x => lstIdDetail.Contains(x.REVIEW_CONTRACTOR_DETAIL_ID) && x.USER_SCORE == ProfileUtilities.User.USER_NAME).ToList();
                    foreach (var detail in lstDetail)
                    {
                        detail.TOTAL_SCORE = 0;
                    }
                    // Tạo danh sách điểm
                    foreach (var spec in this.ObjListReviewSpec)
                    {
                        foreach (var detail in lstDetail)
                        {
                            var find = lstSpecScore.FirstOrDefault(x =>
                                x.SPEC_ID == spec.PKID &&
                                x.USER_SCORE == ProfileUtilities.User.USER_NAME &&
                                x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID
                            );
                            if (find != null)
                            {
                                this.ObjListReviewSpecScore.Add(find);
                                if (find.SCORE.HasValue)
                                {
                                    detail.TOTAL_SCORE += find.SCORE.Value * spec.RATIO;
                                }
                            }
                            else
                            {
                                if (this.ObjListReviewSpec.Count(x => x.PARENT_ID == spec.PKID) == 0)
                                {
                                    this.ObjListReviewSpecScore.Add(new T_EB_REVIEW_SPEC_SCORE()
                                    {
                                        SPEC_ID = spec.PKID,
                                        REVIEW_CONTRACTOR_DETAIL_ID = detail.PKID,
                                        USER_SCORE = ProfileUtilities.User.USER_NAME
                                    });
                                }
                            }
                        }
                    }
                    // Tạo danh sách note
                    foreach (var detail in lstDetail)
                    {
                        var find = lstCouncilNote.FirstOrDefault(x =>
                           x.REVIEW_CONTRACTOR_DETAIL_ID == detail.PKID
                        );
                        if (find == null)
                        {
                            this.ObjListReviewCouncilNote.Add(new T_EB_REVIEW_COUNCIL_NOTE()
                            {
                                REVIEW_CONTRACTOR_DETAIL_ID = detail.PKID,
                                USER_SCORE = ProfileUtilities.User.USER_NAME
                            });
                        }
                        else
                        {
                            this.ObjListReviewCouncilNote.Add(find);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lưu thông tin điểm, ghi chú của người đánh giá
        /// </summary>
        public void UpdateReviewSpecCouncil()
        {
            try
            {
                var lstContractorDetailId = this.ObjListReviewContractorDetail.Select(x => x.PKID).ToList();
                var lstContractorDetail = UnitOfWork.Repository<ReviewContractorDetailRepo>().Queryable().Where(x => lstContractorDetailId.Contains(x.PKID)).ToList();
                this.UnitOfWork.BeginTransaction();

                foreach (var item in this.ObjListReviewSpecScore)
                {
                    if (string.IsNullOrWhiteSpace(item.PKID))
                    {
                        item.PKID = Guid.NewGuid().ToString();
                        item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Create(item);
                    }
                    else
                    {
                        item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        this.UnitOfWork.Repository<ReviewSpecScoreRepo>().Update(item);
                    }
                }

                foreach (var item in this.ObjListReviewCouncilNote)
                {
                    if (string.IsNullOrWhiteSpace(item.PKID))
                    {
                        item.PKID = Guid.NewGuid().ToString();
                        item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        this.UnitOfWork.Repository<ReviewCouncilNoteRepo>().Create(item);
                    }
                    else
                    {
                        item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        this.UnitOfWork.Repository<ReviewCouncilNoteRepo>().Update(item);
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

                this.UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }

        /// <summary>
        /// Thay đổi trạng thái đánh giá của user hội đồng
        /// </summary>
        /// <param name="id"></param>
        public void EndReview(string id)
        {
            var council = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().FirstOrDefault(x => x.REVIEW_ID == id && x.USER_NAME == ProfileUtilities.User.USER_NAME && x.STATUS == Review_Status.KhoiTao);
            this.UpdateReviewSpecCouncil();
            if (this.State == false)
            {
                return;
            }
            try
            {
                UnitOfWork.BeginTransaction();
                if (council != null)
                {
                    council.STATUS = Review_Status.KetThuc;
                    council.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<ReviewCouncilRepo>().Update(council);
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

        public void OpenReview(string id)
        {
            try
            {
                var council = UnitOfWork.Repository<ReviewCouncilRepo>().Queryable().FirstOrDefault(x => x.REVIEW_ID == id && x.USER_NAME == ProfileUtilities.User.USER_NAME && x.STATUS == Review_Status.KetThuc);
                UnitOfWork.BeginTransaction();
                if (council != null)
                {
                    council.STATUS = Review_Status.KhoiTao;
                    council.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<ReviewCouncilRepo>().Update(council);
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
    }
}