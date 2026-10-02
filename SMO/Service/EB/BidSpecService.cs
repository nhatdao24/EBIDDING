using NHibernate.Linq;
using NPOI.HSSF.Record.Chart;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using System.Web.WebPages;
using System.Threading.Tasks;

namespace SMO.Service.EB
{
    public class BidSpecService : GenericService<T_EB_BID_SPEC, BidSpecRepo>
    {
        public T_EB_BID_SPEC ObjHeader { get; set; }
        public List<T_EB_BID_SPEC_DETAIL> ObjListDetail { get; set; }
        public T_EB_BID_SPEC_CONTRACTOR ObjSpecContractor { get; set; }
        public List<T_EB_BID_SPEC_CONTRACTOR> ObjListSpecContractor { get; set; }
        public List<T_EB_BID_SPEC_COUNCIL_SCORE> ObjListSpecCouncilScore { get; set; }
        public List<T_EB_BID_SPEC_SYSTEM_SCORE> ObjListSpecSystemScore { get; set; }
        public List<T_EB_BID_SPEC_USER_SCORE> ObjListSpecUserScore { get; set; }
        public List<T_EB_BID_SPEC_COUNCIL> ObjListSpecCouncil { get; set; }

        public BidSpecService() : base()
        {
            ObjHeader = new T_EB_BID_SPEC();
            ObjSpecContractor = new T_EB_BID_SPEC_CONTRACTOR();
            ObjListDetail = new List<T_EB_BID_SPEC_DETAIL>();
            ObjListSpecContractor = new List<T_EB_BID_SPEC_CONTRACTOR>();
            ObjListSpecCouncilScore = new List<T_EB_BID_SPEC_COUNCIL_SCORE>();
            ObjListSpecSystemScore = new List<T_EB_BID_SPEC_SYSTEM_SCORE>();
            ObjListSpecUserScore = new List<T_EB_BID_SPEC_USER_SCORE>();
            ObjListSpecCouncil = new List<T_EB_BID_SPEC_COUNCIL>();
        }

        public List<NodeSpec> BuildTreeSpec()
        {
            var lstNode = new List<NodeSpec>();
            this.Search();
            foreach (var i in this.ObjList)
            {
                i.Dictionary = UnitOfWork.Repository<DictionaryRepo>().Queryable().FirstOrDefault(x => x.CODE == i.ROLE);
            }
            var Tongtrongso = ObjList.Sum(x => x.RATIO);
            if (Tongtrongso > 100.00M) {

            }

            foreach (var item in this.ObjList.OrderBy(x => x.C_ORDER))
            {
                var node = new NodeSpec()
                {
                    id = item.PKID,
                    pId = item.PARENT_ID,
                    name = item.NAME + $" <span style='color:#f24b3f;'> -{item.Dictionary?.C_VALUE}- </span>" ,
                    role = item.Dictionary?.C_VALUE,
                    ratio = item.RATIO,
                    icon = "/Content/zTreeStyle/img/diy/donvi.gif",
                    type = item.TYPE
                };
                if (item.TYPE == "NUMBER")
                {
                    node.name = node.name + $"<span class='spMaOnTree'>Nhập số ({item.RATIO.ToStringVN()}%)</span>";
                }
                else if (item.TYPE == "TEXT")
                {
                    node.name = node.name + $"<span class='spMaOnTree'>Nhập chữ ({item.RATIO.ToStringVN()}%)</span>";
                }
                else if (item.TYPE == "SELECT")
                {
                    node.name = node.name + $"<span class='spMaOnTree'>Danh sách chọn ({item.RATIO.ToStringVN()}%)</span>";
                }
                else if (item.TYPE == "GROUP")
                {
                    node.name = node.name + "<span class='spMaOnTree'>Nhóm</span>";
                }
                lstNode.Add(node);
            }
            return lstNode;
        }

        /// <summary>
        /// Lấy danh sách điểm cho màn hình xem điểm tiêu chí
        /// </summary>
        public void GetListViewSpecScore()
        {
            this.Search();
            var total = 0;

            // Lấy thông tin value đã nhập của nhà thầu
            this.ObjListSpecContractor = UnitOfWork.Repository<BidSpecContractorRepo>().Search(this.ObjSpecContractor, this.NumerRecordPerPage, this.Page, out total).ToList();

            // Lấy thông tin điểm chấm của hội đồng thầu
            var objSearchSpecCouncilScore = new T_EB_BID_SPEC_COUNCIL_SCORE()
            {
                BID_ID = this.ObjSpecContractor.BID_ID
            };
            this.ObjListSpecCouncilScore = UnitOfWork.Repository<BidSpecCouncilScoreRepo>().Search(objSearchSpecCouncilScore, this.NumerRecordPerPage, this.Page, out total).ToList();

            //Lấy thông tin kết quả thẩm định của hội đồng thầu
            var objSearchSpecCouncil = new T_EB_BID_SPEC_COUNCIL()
            {
                BID_ID = this.ObjSpecContractor.BID_ID
            };
            this.ObjListSpecCouncil = UnitOfWork.Repository<BidSpecCouncilRepo>().Search(objSearchSpecCouncil, this.NumerRecordPerPage, this.Page, out total).ToList();
            this.BuildListViewSpecScore();
            this.GetSystemScore();
            this.GetUserScore();
        }

        public void BuildListViewSpecScore()
        {
            // Loại bỏ những tiêu chí về giá ở giai đoạn trước chấm thầu
            var serviceBid = new BidService();
            serviceBid.Get(this.ObjDetail.BID_ID);

            var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID).ToList();

            if (lstCouncil.Count(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && ((!string.IsNullOrEmpty(x.VAI_TRO) && (x.VAI_TRO.Contains("XG") || x.VAI_TRO.Contains("TDG"))) || x.IS_QUYET_DINH == true)) == 0)
            {
                this.ObjList = this.ObjList.Where(x => x.IS_SPEC_PRICE == false).ToList();
                //if (serviceBid.ObjDetail.GIAI_DOAN == Phase_Status.NopHoSo)
                //{
                //    this.ObjList = this.ObjList.Where(x => x.IS_SPEC_PRICE == false).ToList();
                //}
            }

            //else if (serviceBid.ObjDetail.GIAI_DOAN != Phase_Status.NopHoSo && lstCouncil.Count(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.VAI_TRO == "XG") > 0)
            //{
            //    this.ObjList = this.ObjList.Where(x => x.IS_SPEC_PRICE == true).ToList();
            //}

            //foreach (var spec in this.ObjList)
            //{
            //    foreach (var specCouncilScore in this.ObjListSpecCouncilScore.Where(x => x.SPEC_ID == spec.PKID))
            //    {
            //        if (spec.TYPE != SpecType.GROUP)
            //        {
            //            specCouncilScore.SCORE_MULTIPLY = (int)(specCouncilScore.SCORE * spec.RATIO);
            //        }
            //    }

            //}
        }


        /// <summary>
        /// Lấy danh sách điểm đã nhập của người chấm thầu
        /// </summary>
        public void GetListSpecCouncil(string bidId)
        {
            var Bid = UnitOfWork.Repository<BidRepo>().Get(bidId);

            //ConfigViettinBank: Xoá phân quyền dự án

            //var projectCode = Bid.PROJECT_CODE;
            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP314", projectCode);

            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage = checkRight.Item2;
            //    return;
            //}

            this.Search();
            var total = 0;

            // Lấy thông tin value đã nhập của nhà thầu
            this.ObjListSpecContractor = UnitOfWork.Repository<BidSpecContractorRepo>().Search(this.ObjSpecContractor, 100000, this.Page, out total).ToList();

            // Lấy thông tin điểm chấm của hội đồng thầu
            var objSearchSpecCouncilScore = new T_EB_BID_SPEC_COUNCIL_SCORE()
            {
                BID_ID = this.ObjSpecContractor.BID_ID,
                USER_SCORE = ProfileUtilities.User.USER_NAME
            };
            this.ObjListSpecCouncilScore = UnitOfWork.Repository<BidSpecCouncilScoreRepo>().Search(objSearchSpecCouncilScore, 100000, this.Page, out total).ToList();

            //Lấy thông tin kết quả thẩm định của hội đồng thầu
            var objSearchSpecCouncil = new T_EB_BID_SPEC_COUNCIL()
            {
                BID_ID = this.ObjSpecContractor.BID_ID
            };
            this.ObjListSpecCouncil = UnitOfWork.Repository<BidSpecCouncilRepo>().Search(objSearchSpecCouncil, this.NumerRecordPerPage, this.Page, out total).ToList();
            this.BuildListSpecCouncil();
            this.GetSystemScore();
        }

        public void BuildListSpecCouncil()
        {
            // Loại bỏ những tiêu chí mà người chấm thầu không có vai trò chấm nó
            var lstRole = new List<string>();
            var serviceBidCouncil = new BidCouncilService();
            serviceBidCouncil.ObjDetail.BID_ID = this.ObjDetail.BID_ID;
            serviceBidCouncil.ObjDetail.USER_NAME = ProfileUtilities.User.USER_NAME;
            serviceBidCouncil.Search();
            if (serviceBidCouncil.ObjList.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(serviceBidCouncil.ObjList[0].VAI_TRO))
                {
                    lstRole = serviceBidCouncil.ObjList[0].VAI_TRO.Split(',').ToList();
                }
            }

            this.ObjList = this.ObjList.Where(x => lstRole.Contains(x.ROLE)).ToList();

            var serviceBidContractor = new BidContractorService();
            serviceBidContractor.ObjDetail.BID_ID = this.ObjDetail.BID_ID;
            serviceBidContractor.Search();

            foreach (var contractor in serviceBidContractor.ObjList)
            {
                foreach (var spec in this.ObjList)
                {
                    var findSpecCouncilScore = this.ObjListSpecCouncilScore.FirstOrDefault(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID && x.SPEC_ID == spec.PKID && x.USER_SCORE == ProfileUtilities.User.USER_NAME);
                    var findSpecCouncil = this.ObjListSpecCouncil.FirstOrDefault(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID && x.SPEC_ID == spec.PKID && x.USER_SCORE == ProfileUtilities.User.USER_NAME);
                    if (findSpecCouncilScore == null)
                    {
                        this.ObjListSpecCouncilScore.Add(new T_EB_BID_SPEC_COUNCIL_SCORE()
                        {
                            CONTRACTOR_ID = contractor.CONTRACTOR_ID,
                            SPEC_ID = spec.PKID,
                            BID_ID = this.ObjDetail.BID_ID,
                            SCORE = 0,
                            USER_SCORE = ProfileUtilities.User.USER_NAME,
                            SCORE_MULTIPLY = 0
                        });
                    }
                    else
                    {
                        findSpecCouncilScore.SCORE_MULTIPLY = (int)(findSpecCouncilScore.SCORE * spec.RATIO);
                    }

                    if (findSpecCouncil != null && findSpecCouncilScore != null)
                    {
                        findSpecCouncilScore.VALUE_OF_SPEC = findSpecCouncil.VALUE;
                    }
                }
            }
        }

        /// <summary>
        /// Lấy tổng điểm hệ thống
        /// </summary>
        public void GetSystemScore()
        {
            foreach (var spec in this.ObjList)
            {
                foreach (var specContractor in this.ObjListSpecContractor.Where(x => x.SPEC_ID == spec.PKID))
                {
                    if (spec.TYPE == SpecType.SELECT)
                    {
                        var findSpecDetail = spec.ListDetail.FirstOrDefault(x => x.PKID == specContractor.VALUE);
                        if (findSpecDetail != null)
                        {
                            this.ObjListSpecSystemScore.Add(new T_EB_BID_SPEC_SYSTEM_SCORE()
                            {
                                BID_ID = this.ObjDetail.BID_ID,
                                CONTRACTOR_ID = specContractor.CONTRACTOR_ID,
                                SPEC_ID = spec.PKID,
                                SCORE = findSpecDetail.SCORE,
                                SCORE_MULTIPLY = (decimal)findSpecDetail.SCORE * (spec.RATIO / 100)
                            });
                        }
                    }
                    else if (spec.TYPE == SpecType.NUMBER)
                    {
                        foreach (var detail in spec.ListDetail)
                        {
                            if (!detail.FROM_VALUE.HasValue)
                            {
                                detail.FROM_VALUE = decimal.MinValue;
                            }
                            if (!detail.TO_VALUE.HasValue)
                            {
                                detail.TO_VALUE = decimal.MaxValue;
                            }
                        }

                        decimal convertValue;
                        if (decimal.TryParse(specContractor.VALUE, out convertValue))
                        {
                            var findSpecDetail = spec.ListDetail.FirstOrDefault(x => x.FROM_VALUE <= convertValue && x.TO_VALUE >= convertValue);
                            if (findSpecDetail != null)
                            {
                                this.ObjListSpecSystemScore.Add(new T_EB_BID_SPEC_SYSTEM_SCORE()
                                {
                                    BID_ID = this.ObjDetail.BID_ID,
                                    CONTRACTOR_ID = specContractor.CONTRACTOR_ID,
                                    SPEC_ID = spec.PKID,
                                    SCORE = findSpecDetail.SCORE,
                                    SCORE_MULTIPLY = (decimal)findSpecDetail.SCORE * (spec.RATIO / 100)
                                });
                            }
                        }
                    }
                }
            }

            //var serviceBidContractor = new BidContractorService();
            //serviceBidContractor.ObjDetail.BID_ID = this.ObjDetail.BID_ID;
            //serviceBidContractor.Search();

            //foreach (var contractor in serviceBidContractor.ObjList)
            //{
            //    foreach (var spec in this.ObjList)
            //    {
            //        var findSpecContractor = this.ObjListSpecContractor.FirstOrDefault(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID && x.SPEC_ID == spec.PKID);
            //        if (findSpecContractor != null)
            //        {
            //            if (spec.TYPE == SpecType.SELECT)
            //            {
            //                var findSpecDetail = spec.ListDetail.FirstOrDefault(x => x.PKID == findSpecContractor.VALUE);
            //                if (findSpecDetail != null)
            //                {
            //                    this.ObjListSpecSystemScore.Add(new T_EB_BID_SPEC_SYSTEM_SCORE()
            //                    {
            //                        BID_ID = this.ObjDetail.BID_ID,
            //                        CONTRACTOR_ID = contractor.CONTRACTOR_ID,
            //                        SPEC_ID = spec.PKID,
            //                        SCORE = findSpecDetail.SCORE,
            //                        SCORE_MULTIPLY = (int)(findSpecDetail.SCORE * spec.RATIO)
            //                    });
            //                }
            //            }
            //            else if (spec.TYPE == SpecType.NUMBER)
            //            {

            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Lấy tổng điểm theo user
        /// </summary>
        public void GetUserScore()
        {
            var serviceBidCouncil = new BidCouncilService();
            serviceBidCouncil.ObjDetail.BID_ID = this.ObjDetail.BID_ID;
            serviceBidCouncil.Search();

            var serviceBidContractor = new BidContractorService();
            serviceBidContractor.ObjDetail.BID_ID = this.ObjDetail.BID_ID;
            serviceBidContractor.Search();

            foreach (var spec in this.ObjList)
            {
                foreach (var contractor in serviceBidContractor.ObjList)
                {
                    var findSpecCouncilScore = this.ObjListSpecCouncilScore.Where(x => x.CONTRACTOR_ID == contractor.CONTRACTOR_ID && x.SPEC_ID == spec.PKID).ToList();
                    if (findSpecCouncilScore.Count > 0)
                    {
                        decimal average = 0;
                        decimal sum = 0;
                        average = (decimal)findSpecCouncilScore.Sum(x => x.SCORE) / (decimal)findSpecCouncilScore.Count;
                        sum = average * (spec.RATIO / 100);

                        this.ObjListSpecUserScore.Add(new T_EB_BID_SPEC_USER_SCORE()
                        {
                            BID_ID = this.ObjDetail.BID_ID,
                            CONTRACTOR_ID = contractor.CONTRACTOR_ID,
                            SPEC_ID = spec.PKID,
                            SCORE = average,
                            SCORE_MULTIPLY = sum
                        });
                    }
                }
            }
        }

        public void UpdateSpecCouncil()
        {
            var Bid = UnitOfWork.Repository<BidRepo>().Get(this.ObjDetail.BID_ID);

            //ConfigViettinBank: Xoá phân quyền dự án

            //var projectCode = Bid.PROJECT_CODE;
            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP314", projectCode);
            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage = checkRight.Item2;
            //    return;
            //}
            try
            {
                UnitOfWork.BeginTransaction();
                var strSQL = "DELETE T_EB_BID_SPEC_COUNCIL WHERE BID_ID = :BID_ID AND USER_SCORE = :USER_SCORE";
                UnitOfWork.GetSession().CreateSQLQuery(strSQL)
                    .SetParameter("BID_ID", this.ObjDetail.BID_ID)
                    .SetParameter("USER_SCORE", ProfileUtilities.User.USER_NAME)
                    .ExecuteUpdate();

                foreach (var item in this.ObjListSpecCouncilScore)
                {
                    if (!string.IsNullOrEmpty(item.PKID) && item.USER_SCORE == ProfileUtilities.User.USER_NAME)
                    {
                        item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<BidSpecCouncilScoreRepo>().Update(item);
                    }
                    else
                    {
                        item.PKID = Guid.NewGuid().ToString();
                        item.USER_SCORE = ProfileUtilities.User.USER_NAME;
                        item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<BidSpecCouncilScoreRepo>().Create(item);
                    }

                    UnitOfWork.Repository<BidSpecCouncilRepo>().Create(new T_EB_BID_SPEC_COUNCIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        CONTRACTOR_ID = item.CONTRACTOR_ID,
                        BID_ID = item.BID_ID,
                        SPEC_ID = item.SPEC_ID,
                        VALUE = item.VALUE_OF_SPEC,
                        USER_SCORE = ProfileUtilities.User.USER_NAME,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }
                UnitOfWork.Commit();
                this.UpdateAdjustedPrice();
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                this.State = false;
            }
        }

        public void UpdateAdjustedPrice()
        {
            try
            {
                var specPrice = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID && x.MD_PKID == Global.IdSpecPrice).FirstOrDefault();
                var lstBidContractorPrice = UnitOfWork.Repository<BidContractorPriceRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID && x.TYPE == "1").OrderBy(x => x.C_ORDER).ToList();
                List<T_EB_BID_SPEC_COUNCIL> lstSpecPrice = new List<T_EB_BID_SPEC_COUNCIL>();
                if (specPrice != null && !string.IsNullOrEmpty(specPrice.PKID))
                {
                    lstSpecPrice = UnitOfWork.Repository<BidSpecCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.BID_ID && x.SPEC_ID == specPrice.PKID).ToList();

                }
                var lstContractorId = this.ObjListSpecCouncilScore.Select(x => x.CONTRACTOR_ID).Distinct().ToList();
                UnitOfWork.BeginTransaction();
                foreach (var item in lstContractorId)
                {
                    var lstSpecCouncilPrice = lstSpecPrice.Where(x => x.CONTRACTOR_ID == item).ToList();
                    var bidContractorPrice = lstBidContractorPrice.FirstOrDefault(x => x.CONTRACTOR_ID == item);
                    decimal price = 0;
                    decimal output = 0;
                    foreach (var spec in lstSpecPrice.Where(x => x.CONTRACTOR_ID == item).ToList())
                    {
                        if (Decimal.TryParse(spec.VALUE, out output))
                        {
                            price += output;
                        }
                    }
                    if (bidContractorPrice != null)
                    {
                        if (lstSpecPrice.Where(x => x.CONTRACTOR_ID == item).Count() > 0)
                        {
                            bidContractorPrice.PRICE = price / lstSpecPrice.Where(x => x.CONTRACTOR_ID == item).Count();
                        }
                        else
                        {
                            bidContractorPrice.PRICE = 0;
                        }
                        bidContractorPrice.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<BidContractorPriceRepo>().Update(bidContractorPrice);
                    }
                    else
                    {
                        decimal sumPrice = 0;
                        if (lstSpecPrice.Where(x => x.CONTRACTOR_ID == item).Count() > 0)
                        {
                            sumPrice = price / lstSpecPrice.Where(x => x.CONTRACTOR_ID == item).Count();
                        }
                        UnitOfWork.Repository<BidContractorPriceRepo>().Create(new T_EB_BID_CONTRACTOR_PRICE()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            BID_ID = this.ObjDetail.BID_ID,
                            CONTRACTOR_ID = item,
                            PRICE = sumPrice,
                            TYPE = "1",
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
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

        /// <summary>
        /// Lấy ra value đã nhập của contractor
        /// </summary>
        /// <param name="contractorId"></param>
        public void GetListSpecContractor(string contractorId)
        {
            try
            {
                this.Search();
                var objSearch = new T_EB_BID_SPEC_CONTRACTOR()
                {
                    CONTRACTOR_ID = contractorId,
                    BID_ID = this.ObjDetail.BID_ID
                };
                var total = 0;
                var listResult = UnitOfWork.Repository<BidSpecContractorRepo>().Search(objSearch, this.NumerRecordPerPage, this.Page, out total);

                foreach (var item in this.ObjList)
                {
                    var find = listResult.FirstOrDefault(x => x.SPEC_ID == item.PKID);
                    if (find != null)
                    {
                        item.VALUE_OF_SPEC = find.VALUE;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                this.State = false;
            }
        }

        //public void UpdateContractorInputSpec(HttpRequestBase Request, string moTaKyThuat, string moTaNangLuc, string moTaGia, string moTaPhapLy)
        //{ 
        //    try
        //    {
        //        UnitOfWork.BeginTransaction();

        //        var strSQL = "DELETE T_EB_BID_SPEC_CONTRACTOR WHERE BID_ID = :BID_ID AND CONTRACTOR_ID = :CONTRACTOR_ID";
        //        UnitOfWork.GetSession().CreateSQLQuery(strSQL)
        //            .SetParameter("BID_ID", this.ObjDetail.BID_ID)
        //            .SetParameter("CONTRACTOR_ID", ProfileUtilities.User.CONTRACTOR_ID)
        //            .ExecuteUpdate();

        //        foreach (var item in this.ObjList)
        //        {
        //            var specContractor = new T_EB_BID_SPEC_CONTRACTOR()
        //            {
        //                PKID = Guid.NewGuid().ToString(),
        //                BID_ID = this.ObjDetail.BID_ID,
        //                SPEC_ID = item.PKID,
        //                CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID,
        //                VALUE = item.VALUE_OF_SPEC,
        //                FIRST_VALUE = item.VALUE_OF_SPEC
        //            };

        //            UnitOfWork.Repository<BidSpecContractorRepo>().Create(specContractor);
        //        }

        //        //Lưu BidSubmit
        //        var serviceBid = new BidService();
        //        serviceBid.Get(this.ObjDetail.BID_ID);

        //        var now = DateTime.Now;
        //        if (serviceBid.ObjDetail.NGAY_DONG_THAU < now)
        //        {
        //            this.State = false;
        //            if (ProfileUtilities.User.LANGUAGE == "vi")
        //            {
        //                this.ErrorMessage = "Đã hết thời hạn nộp hồ sơ!";
        //            }
        //            else
        //            {
        //                this.ErrorMessage = "Dealine submit file has expried!";
        //            }
        //            return;
        //        }

        //        var dictionary = new Dictionary<FILE_STREAM, string>();

        //        // Lưu file vào database
        //        var lstFileStream = new List<FILE_STREAM>();
        //        for (int i = 0; i < Request.Files.AllKeys.Length; i++)
        //        {
        //            var file = Request.Files[i];
        //            var code = Guid.NewGuid().ToString();
        //            var referenceCode = Guid.NewGuid().ToString();
        //            var fileStream = new FILE_STREAM()
        //            {
        //                PKID = code,
        //                FILE_OLD_NAME = file.FileName,
        //                REFERENCE_ID = referenceCode,
        //                FILE_EXT = Path.GetExtension(file.FileName),
        //                FILE_SIZE = file.ContentLength,
        //                FILESTREAM = Request.Files[i]
        //            };

        //            lstFileStream.Add(fileStream);
        //            dictionary.Add(fileStream, Request.Files.GetKey(i));
        //        }
        //        FileStreamService.InsertFile(lstFileStream);

        //        if (dictionary.Where(x => x.Value == "filesGia").Count() > 0)
        //        {
        //            var submit = new T_EB_BID_SUBMIT();
        //            submit.BID_ID = this.ObjDetail.BID_ID;
        //            submit.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
        //            submit.TYPE = FileGroupType.Gia;
        //            submit.PKID = Guid.NewGuid().ToString();
        //            submit.DESCRIPTION = moTaGia;

        //            foreach (var item in dictionary.Where(x => x.Value == "filesGia"))
        //            {
        //                UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
        //                {
        //                    SUBMIT_ID = submit.PKID,
        //                    FILE_ID = item.Key.REFERENCE_ID,
        //                    CREATE_BY = ProfileUtilities.User.USER_NAME
        //                });
        //            }

        //            UnitOfWork.Repository<BidSubmitRepo>().Create(submit);
        //        }

        //        if (dictionary.Where(x => x.Value == "filesPhapLy").Count() > 0)
        //        {
        //            var submit = new T_EB_BID_SUBMIT();
        //            submit.BID_ID = this.ObjDetail.BID_ID;
        //            submit.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
        //            submit.TYPE = FileGroupType.PhapLy;
        //            submit.PKID = Guid.NewGuid().ToString();
        //            submit.DESCRIPTION = moTaPhapLy;

        //            foreach (var item in dictionary.Where(x => x.Value == "filesPhapLy"))
        //            {
        //                UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
        //                {
        //                    SUBMIT_ID = submit.PKID,
        //                    FILE_ID = item.Key.REFERENCE_ID,
        //                    CREATE_BY = ProfileUtilities.User.USER_NAME
        //                });
        //            }

        //            UnitOfWork.Repository<BidSubmitRepo>().Create(submit);
        //        }

        //        if (dictionary.Where(x => x.Value == "filesKyThuat").Count() > 0)
        //        {
        //            var submit = new T_EB_BID_SUBMIT();
        //            submit.BID_ID = this.ObjDetail.BID_ID;
        //            submit.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
        //            submit.TYPE = FileGroupType.KyThuat;
        //            submit.PKID = Guid.NewGuid().ToString();
        //            submit.DESCRIPTION = moTaKyThuat;

        //            foreach (var item in dictionary.Where(x => x.Value == "filesKyThuat"))
        //            {
        //                UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
        //                {
        //                    SUBMIT_ID = submit.PKID,
        //                    FILE_ID = item.Key.REFERENCE_ID,
        //                    CREATE_BY = ProfileUtilities.User.USER_NAME
        //                });
        //            }

        //            UnitOfWork.Repository<BidSubmitRepo>().Create(submit);
        //        }


        //        if (dictionary.Where(x => x.Value == "filesNangLuc").Count() > 0)
        //        {
        //            var submit = new T_EB_BID_SUBMIT();
        //            submit.BID_ID = this.ObjDetail.BID_ID;
        //            submit.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
        //            submit.TYPE = FileGroupType.NangLuc;
        //            submit.PKID = Guid.NewGuid().ToString();
        //            submit.DESCRIPTION = moTaNangLuc;

        //            foreach (var item in dictionary.Where(x => x.Value == "filesNangLuc"))
        //            {
        //                UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
        //                {
        //                    SUBMIT_ID = submit.PKID,
        //                    FILE_ID = item.Key.REFERENCE_ID,
        //                    CREATE_BY = ProfileUtilities.User.USER_NAME
        //                });
        //            }

        //            UnitOfWork.Repository<BidSubmitRepo>().Create(submit);
        //        }

        //        var fullNamUserNT = ProfileUtilities.User.FULL_NAME;

        //        UnitOfWork.Commit();
        //        this.UpdateContractorPrice();
        //        this.CreateNotify(serviceBid.ObjDetail, ProfileUtilities.User.CONTRACTOR_ID);
        //        this.CreateNotiInputSpecEmail(serviceBid.ObjDetail, fullNamUserNT);
        //    }
        //    catch (Exception ex)
        //    {
        //        UnitOfWork.Rollback();
        //        this.Exception = ex;
        //        this.State = false;
        //    }
        //}



        //ConfigViettinBank: Chỉ thực hiện nộp 1 file hồ sơ(tổng hợp)


        public void UpdateContractorInputSpec(HttpRequestBase Request, string moTaTongHop)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                var strSQL = "DELETE T_EB_BID_SPEC_CONTRACTOR WHERE BID_ID = :BID_ID AND CONTRACTOR_ID = :CONTRACTOR_ID";
                UnitOfWork.GetSession().CreateSQLQuery(strSQL)
                    .SetParameter("BID_ID", this.ObjDetail.BID_ID)
                    .SetParameter("CONTRACTOR_ID", ProfileUtilities.User.CONTRACTOR_ID)
                    .ExecuteUpdate();

                foreach (var item in this.ObjList)
                {
                    var specContractor = new T_EB_BID_SPEC_CONTRACTOR()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        BID_ID = this.ObjDetail.BID_ID,
                        SPEC_ID = item.PKID,
                        CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID,
                        VALUE = item.VALUE_OF_SPEC,
                        FIRST_VALUE = item.VALUE_OF_SPEC
                    };

                    UnitOfWork.Repository<BidSpecContractorRepo>().Create(specContractor);
                }

                //Lưu BidSubmit
                var serviceBid = new BidService();
                serviceBid.Get(this.ObjDetail.BID_ID);

                var now = DateTime.Now;
                if (serviceBid.ObjDetail.NGAY_DONG_THAU < now)
                {
                    this.State = false;
                    if (ProfileUtilities.User.LANGUAGE == "vi")
                    {
                        this.ErrorMessage = "Đã hết thời hạn nộp hồ sơ!";
                    }
                    else
                    {
                        this.ErrorMessage = "Dealine submit file has expried!";
                    }
                    return;
                }

                var dictionary = new Dictionary<FILE_STREAM, string>();

                // Lưu file vào database
                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    var file = Request.Files[i];
                    var code = Guid.NewGuid().ToString();
                    var referenceCode = Guid.NewGuid().ToString();
                    var fileStream = new FILE_STREAM()
                    {
                        PKID = code,
                        FILE_OLD_NAME = file.FileName,
                        REFERENCE_ID = referenceCode,
                        FILE_EXT = Path.GetExtension(file.FileName),
                        FILE_SIZE = file.ContentLength,
                        FILESTREAM = Request.Files[i]
                    };

                    lstFileStream.Add(fileStream);
                    dictionary.Add(fileStream, Request.Files.GetKey(i));
                }
                FileStreamService.InsertFile(lstFileStream);

                if (dictionary.Where(x => x.Value == "filesTongHop").Count() > 0)
                {
                    var submit = new T_EB_BID_SUBMIT();
                    submit.BID_ID = this.ObjDetail.BID_ID;
                    submit.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
                    submit.TYPE = FileGroupType.TongHop;
                    submit.PKID = Guid.NewGuid().ToString();
                    submit.DESCRIPTION = moTaTongHop;

                    foreach (var item in dictionary.Where(x => x.Value == "filesTongHop"))
                    {
                        UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
                        {
                            SUBMIT_ID = submit.PKID,
                            FILE_ID = item.Key.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    UnitOfWork.Repository<BidSubmitRepo>().Create(submit);
                }


                var fullNamUserNT = ProfileUtilities.User.FULL_NAME;

                UnitOfWork.Commit();
                this.UpdateContractorPrice();
                this.CreateNotify(serviceBid.ObjDetail, ProfileUtilities.User.CONTRACTOR_ID);
                this.CreateNotiInputSpecEmail(serviceBid.ObjDetail, fullNamUserNT);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }

        public void UpdateContractorPrice()
        {
            try
            {
                var bidPrice = this.CurrentRepository.Queryable()
                                .Where(x => x.MD_PKID == Global.IdSpecPrice && x.BID_ID == this.ObjDetail.BID_ID).FirstOrDefault();
                var specPrice = UnitOfWork.Repository<BidContractorPriceRepo>().Queryable()
                                .Where(x => x.BID_ID == this.ObjDetail.BID_ID && x.TYPE == "0" && x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID)
                                .FirstOrDefault();

                UnitOfWork.BeginTransaction();
                if (bidPrice != null && specPrice == null)
                {
                    var specContractor = UnitOfWork.Repository<BidSpecContractorRepo>().Queryable()
                                            .Where(x => x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID && x.BID_ID == this.ObjDetail.BID_ID && x.SPEC_ID == bidPrice.PKID)
                                            .Select(x => x.VALUE)
                                            .FirstOrDefault();
                    var firstSpecContractor = UnitOfWork.Repository<BidSpecContractorRepo>().Queryable()
                                            .Where(x => x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID && x.BID_ID == this.ObjDetail.BID_ID && x.SPEC_ID == bidPrice.PKID)
                                            .Select(x => x.FIRST_VALUE)
                                            .FirstOrDefault();
                    var contractorPrice = new T_EB_BID_CONTRACTOR_PRICE()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        BID_ID = this.ObjDetail.BID_ID,
                        CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID,
                        PRICE = specContractor.AsDecimal(),
                        FIRST_PRICE = firstSpecContractor.AsDecimal(),
                        TYPE = "0",
                        CREATE_BY = ProfileUtilities.User.USER_NAME,
                    };
                    UnitOfWork.Repository<BidContractorPriceRepo>().Create(contractorPrice);
                }
                else if (bidPrice != null && specPrice != null)
                {
                    var specContractor = UnitOfWork.Repository<BidSpecContractorRepo>().Queryable()
                                            .Where(x => x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID && x.BID_ID == this.ObjDetail.BID_ID && x.SPEC_ID == bidPrice.PKID)
                                            .Select(x => x.VALUE)
                                            .FirstOrDefault();
                    specPrice.PRICE = specContractor.AsDecimal();
                    specPrice.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<BidContractorPriceRepo>().Update(specPrice);
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

        public override void Create()
        {
            try
            {
                this.ObjHeader.PKID = Guid.NewGuid().ToString();
                this.ObjHeader.CREATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Create(this.ObjHeader);

                var order = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    detail.PKID = Guid.NewGuid().ToString();
                    detail.HEADER_ID = this.ObjHeader.PKID;
                    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    detail.C_ORDER = order;
                    order++;
                }
                UnitOfWork.Repository<BidSpecDetailRepo>().Create(this.ObjListDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                this.State = false;
            }
        }

        public override void Update()
        {
            try
            {
                // Kiểm tra hack thay đổi thông tin header
                var poCheck = this.CurrentRepository.Get(this.ObjHeader.PKID);
                UnitOfWork.GetSession().Evict(poCheck);
                if (poCheck == null)
                {
                    this.State = false;
                    this.ErrorMessage = "Thông tin không hợp lệ!";
                    return;
                }

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);

                var lstDetail = UnitOfWork.Repository<BidSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == this.ObjHeader.PKID).ToList();
                //Tìm những detail bị xóa
                var lstDetailDelete = lstDetail.Where(x => !this.ObjListDetail.Select(y => y.PKID).Contains(x.PKID)).ToList();
                foreach (var detail in lstDetailDelete)
                {
                    UnitOfWork.Repository<BidSpecDetailRepo>().Delete(detail);
                }

                var number = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    var find = lstDetail.FirstOrDefault(x => x.PKID == detail.PKID);
                    if (find != null)
                    {
                        find.C_ORDER = number;
                        find.FROM_VALUE = detail.FROM_VALUE;
                        find.TO_VALUE = detail.TO_VALUE;
                        find.VALUE = detail.VALUE;
                        find.VALUE_EN = detail.VALUE_EN;
                        find.SCORE = detail.SCORE;
                        find.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<BidSpecDetailRepo>().Update(find);
                    }
                    else
                    {
                        detail.C_ORDER = number;
                        detail.HEADER_ID = this.ObjHeader.PKID;
                        detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        detail.PKID = Guid.NewGuid().ToString();
                        UnitOfWork.Repository<BidSpecDetailRepo>().Create(detail);
                    }
                    number++;
                }


                //var strSQL = "DELETE T_EB_BID_SPEC_DETAIL WHERE HEADER_ID = :HEADER_ID;";
                //UnitOfWork.GetSession().CreateSQLQuery(strSQL)
                //    .SetParameter("HEADER_ID", this.ObjHeader.PKID)
                //    .ExecuteUpdate();

                //foreach (var detail in this.ObjListDetail)
                //{
                //    detail.PKID = Guid.NewGuid().ToString();
                //    detail.HEADER_ID = this.ObjHeader.PKID;
                //    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                //}

                //var order = 1;
                //foreach (var detail in this.ObjListDetail)
                //{
                //    detail.PKID = Guid.NewGuid().ToString();
                //    detail.HEADER_ID = this.ObjHeader.PKID;
                //    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                //    detail.C_ORDER = order;
                //    order++;
                //}

                //UnitOfWork.Repository<BidSpecDetailRepo>().Create(this.ObjListDetail);
                UnitOfWork.Commit();
                var listSpec = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == this.ObjHeader.BID_ID).ToList();
                var tongTrongSo = listSpec.Sum(x => x.RATIO);
                var ts = this.ObjDetail.RATIO;
                if (tongTrongSo > 100.00M)
                {

                    this.State = false;
                    this.ErrorMessage = "Tỉ lệ không được vượt quá 100% !";
                    return;
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateTree(List<NodeSpec> lstNode)
        {
            try
            {
                var strSql = "";
                var order = 0;
                UnitOfWork.BeginTransaction();
                decimal CountRatio = 0;

                foreach (var item in lstNode)
                {
                    CountRatio += item.ratio;
                    if (!string.IsNullOrWhiteSpace(item.pId))
                    {
                        strSql = "UPDATE T_EB_BID_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = :PARENT WHERE PKID = :PKID";
                        UnitOfWork.GetSession().CreateSQLQuery(strSql)
                            .SetParameter("C_ORDER", order)
                            .SetParameter("PARENT", item.pId)
                            .SetParameter("PKID", item.id)
                            .ExecuteUpdate();
                    }
                    else
                    {
                        strSql = "UPDATE T_EB_BID_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = '' WHERE PKID = :PKID";
                        UnitOfWork.GetSession().CreateSQLQuery(strSql)
                            .SetParameter("C_ORDER", order)
                            .SetParameter("PKID", item.id)
                            .ExecuteUpdate();
                    }
                    order++;
                }
                if(CountRatio > 100.00M)
                {
                   
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

        public override void Get(object id, dynamic param = null)
        {
            try
            {
                this.ObjHeader = this.CurrentRepository.Get(id, param);
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void AddSpecMDToBid(string lstSpec, string bidId)
        {
            try
            {
                var lstSpecCopy = new List<T_EB_BID_SPEC>();
                var lstSpecDetailCopy = new List<T_EB_BID_SPEC_DETAIL>();
                foreach (var specId in lstSpec.Split('|'))
                {
                    var specMd = UnitOfWork.Repository<MdBidSpecRepo>().Get(specId);
                    var lstSpecDetailMd = UnitOfWork.Repository<MdBidSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == specId).ToList();
                    if (specMd != null && !specMd.IS_NOT_COPY)
                    {
                        var parentId = "";
                        var find = lstSpecCopy.FirstOrDefault(x => x.MD_PKID == specMd.PARENT_ID);
                        if (find != null)
                        {
                            parentId = find.PKID;
                        }

                        var specCopy = new T_EB_BID_SPEC()
                        {
                            MD_PKID = specMd.PKID,
                            PKID = Guid.NewGuid().ToString(),
                            BID_ID = bidId,
                            PARENT_ID = parentId,
                            C_ORDER = specMd.C_ORDER,
                            NAME = specMd.NAME,
                            NAME_EN = specMd.NAME_EN,
                            DESCRIPTION = specMd.DESCRIPTION,
                            DESCRIPTION_EN = specMd.DESCRIPTION_EN,
                            IS_NOT_SHOW_CONTRACTOR = specMd.IS_NOT_SHOW_CONTRACTOR,
                            IS_SPEC_PRICE = specMd.IS_SPEC_PRICE,
                            ROLE = specMd.ROLE,
                            TYPE = specMd.TYPE,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        };
                        lstSpecCopy.Add(specCopy);

                        foreach (var specDetail in lstSpecDetailMd)
                        {
                            lstSpecDetailCopy.Add(new T_EB_BID_SPEC_DETAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = specCopy.PKID,
                                C_ORDER = specDetail.C_ORDER,
                                FROM_VALUE = specDetail.FROM_VALUE,
                                SCORE = specDetail.SCORE,
                                TO_VALUE = specDetail.SCORE,
                                VALUE = specDetail.VALUE,
                                VALUE_EN = specDetail.VALUE_EN,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }
                }

                UnitOfWork.BeginTransaction();

                foreach (var item in lstSpecCopy)
                {
                    UnitOfWork.Repository<BidSpecRepo>().Create(item);
                }

                foreach (var item in lstSpecDetailCopy)
                {
                    UnitOfWork.Repository<BidSpecDetailRepo>().Create(item);
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

        public override void Delete(string strLstSelected)
        {
            try
            {
                if (this.CurrentRepository.Queryable().Where(x => x.PARENT_ID == strLstSelected).Count() > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Tiêu chí này đang là cha của những tiêu chí khác!";
                    return;
                }
                var lstSpecDetail = UnitOfWork.Repository<BidSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == strLstSelected).ToList();

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Delete(strLstSelected);
                UnitOfWork.Repository<BidSpecDetailRepo>().Delete(lstSpecDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CreateNotify(T_EB_BID bid, string contractorId)
        {
            try
            {
                var serviceContractor = new ContractorService();
                serviceContractor.Get(contractorId);

                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                lstUserNotify.Add(bid.CREATE_BY);
                if (!string.IsNullOrEmpty(bid.USER_MANAGE))
                {
                    lstUserNotify.Add(bid.USER_MANAGE);
                }

                foreach (var item in lstUserNotify)
                {
                    var newId = Guid.NewGuid().ToString();

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}]: Nhà thầu [{6}] nộp hồ sơ</span>
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
                            <span>Bid [{4} - {5}]: Nhà thầu [{6}] submit documents</span>
                            <p>
                                <i class='material-icons'>access_time</i> {7}
                            </p>
                        </div>
                    </a>
                ";


                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuHoSoThau",
                        "bg-pink",
                        "attach_file",
                        bid.PKID,
                        bid.TEN_GOI_THAU,
                        serviceContractor.ObjDetail.TEN_NGAN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));
                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuHoSoThau",
                        "bg-pink",
                        "attach_file",
                        bid.PKID,
                        bid.TEN_GOI_THAU,
                        serviceContractor.ObjDetail.TEN_NGAN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {bid.PKID} : Nhà thầu {serviceContractor.ObjDetail.TEN_NGAN} nộp hồ sơ";
                    string strRawContent_EN = $"BID {bid.PKID} : Contractor {serviceContractor.ObjDetail.TEN_NGAN} submit documents";


                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = item
                    };
                    serviceNotify.Create();
                }

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception ex)
            {

            }
        }

        public void CreateNotiInputSpecEmail(T_EB_BID bid, string fullNamUserNT)
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
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";
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
                        .Replace(LOAI_CONG_VIEC, "Nhà thầu nộp hồ sơ")
                        .Replace(MA, bid.PKID)
                        .Replace(TEN, bid.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, "Nhà thầu nộp hồ sơ")
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + bid.PKID + "&idMenu=mnuHoSoThau")
                        .Replace(USER_NAME_NGUOI_DE_XUAT, bid.CREATE_BY)
                        .Replace(DU_AN, bid.Project.NAME)
                        .Replace(DU_AN_EN, bid.Project.NAME_EN)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, bid.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, fullNamUserNT)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, bid.UserApprove.FULL_NAME);

                if (bid.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, bid.Project.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Nhà thầu nộp hồ sơ")
                        .Replace(MA, bid.PKID)
                        .Replace(TEN, bid.TEN_GOI_THAU)
                        .Replace(DU_AN, bid.Project.NAME)
                        .Replace(DU_AN_EN, bid.Project.NAME_EN)

                        .Replace(TRANG_THAI, "Nhà thầu nộp hồ sơ")
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + bid.PKID + "&idMenu=mnuHoSoThau")
                        .Replace(USER_NAME_NGUOI_DE_XUAT, bid.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, bid.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, fullNamUserNT)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, bid.UserApprove.FULL_NAME);

                if (bid.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, bid.Project.NAME);
                }
              
                serviceEmail.ObjDetail = new T_CM_EMAIL()
                {
                    PKID = Guid.NewGuid().ToString(),
                    EMAIL = bid.UserSent.EMAIL,
                    SUBJECT = contentSubject,
                    CONTENTS = contentBody
                };
            serviceEmail.Create();
            }
            catch (Exception)
            {

            }
        }
    }
}
