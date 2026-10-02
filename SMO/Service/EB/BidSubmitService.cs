using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMO.Service.EB
{
    public class BidSubmitService : GenericService<T_EB_BID_SUBMIT, BidSubmitRepo>
    {
        public string IS_SHOW_ONLY_TONG_HOP { get; set; }
        public BidSubmitService() : base()
        {

        }

        public async Task CreateSubmit(HttpRequestBase Request, string moTaKyThuat, string moTaNangLuc, string moTaGia, string moTaTongHop, string moTaPhapLy)
        {
            try
            {
                var serviceBid = new BidService();
                serviceBid.Get(this.ObjDetail.BID_ID);

                var now = DateTime.Now;
                //if (serviceBid.ObjDetail.NGAY_DONG_THAU < now)
                //{
                //    this.State = false;
                //    if (ProfileUtilities.User.LANGUAGE == "vi")
                //    {
                //        this.ErrorMessage = "Đã hết thời hạn nộp hồ sơ!";
                //    }
                //    else
                //    {
                //        this.ErrorMessage = "Dealine submit file has expried!";
                //    }
                //    return;
                //}

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

                await FileStreamService.InsertFile(lstFileStream);

                UnitOfWork.BeginTransaction();

                // 2. Logic phân loại file và lưu vào bảng liên kết (T_EB_BID_SUBMIT_FILE)
                // Lưu ý: Dùng List thay vì Dictionary lặp đi lặp lại để tránh lỗi truy vấn khi lặp
                var fileList = dictionary.ToList();

                // Ví dụ cho một nhóm file (các nhóm khác làm tương tự)
                var giaFiles = fileList.Where(x => x.Value == "filesGia").Select(x => x.Key).ToList();
                if (giaFiles.Any())
                {
                    var submit = new T_EB_BID_SUBMIT()
                    {
                        BID_ID = this.ObjDetail.BID_ID,
                        CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID,
                        TYPE = FileGroupType.Gia,
                        PKID = Guid.NewGuid().ToString(),
                        DESCRIPTION = moTaGia,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    foreach (var fileItem in giaFiles)
                    {
                        UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
                        {
                            SUBMIT_ID = submit.PKID,
                            FILE_ID = fileItem.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }
                    this.CurrentRepository.Create(submit);
                }

                if (dictionary.Where(x => x.Value == "filesGia").Count() > 0)
                {
                    var submit = new T_EB_BID_SUBMIT();
                    submit.BID_ID = this.ObjDetail.BID_ID;
                    submit.CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID;
                    submit.TYPE = FileGroupType.Gia;
                    submit.PKID = Guid.NewGuid().ToString();
                    submit.DESCRIPTION = moTaGia;
                    submit.CREATE_BY = ProfileUtilities.User.USER_NAME;

                    foreach (var item in dictionary.Where(x => x.Value == "filesGia"))
                    {
                        UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
                        {
                            SUBMIT_ID = submit.PKID,
                            FILE_ID = item.Key.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    this.CurrentRepository.Create(submit);
                }

                Console.WriteLine("7879878978978978978989");

                //ConfigViettinBank: ẨN PHẦN XỬ LỸ CỦA HỒ SƠ PHÁP LÝ, KỸ THUẬT, NĂNG LỰC

                //if (dictionary.Where(x => x.Value == "filesPhapLy").Count() > 0)
                //{
                //    Console.WriteLine("156165156561566516516515156");
                //    var submit = new T_EB_BID_SUBMIT();
                //    submit.BID_ID = this.ObjDetail.BID_ID;
                //    submit.CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID;
                //    submit.TYPE = FileGroupType.PhapLy;
                //    submit.PKID = Guid.NewGuid().ToString();
                //    submit.DESCRIPTION = moTaPhapLy;
                //    submit.CREATE_BY = ProfileUtilities.User.USER_NAME;

                //    foreach (var item in dictionary.Where(x => x.Value == "filesPhapLy"))
                //    {
                //        UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
                //        {
                //            SUBMIT_ID = submit.PKID,
                //            FILE_ID = item.Key.REFERENCE_ID,
                //            CREATE_BY = ProfileUtilities.User.USER_NAME
                //        });
                //    }

                //    this.CurrentRepository.Create(submit);
                //}

                //if (dictionary.Where(x => x.Value == "filesKyThuat").Count() > 0)
                //{
                //    var submit = new T_EB_BID_SUBMIT();
                //    submit.BID_ID = this.ObjDetail.BID_ID;
                //    submit.CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID;
                //    submit.TYPE = FileGroupType.KyThuat;
                //    submit.PKID = Guid.NewGuid().ToString();
                //    submit.DESCRIPTION = moTaKyThuat;
                //    submit.CREATE_BY = ProfileUtilities.User.USER_NAME;

                //    foreach (var item in dictionary.Where(x => x.Value == "filesKyThuat"))
                //    {
                //        UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
                //        {
                //            SUBMIT_ID = submit.PKID,
                //            FILE_ID = item.Key.REFERENCE_ID,
                //            CREATE_BY = ProfileUtilities.User.USER_NAME
                //        });
                //    }

                //    this.CurrentRepository.Create(submit);
                //}


                //if (dictionary.Where(x => x.Value == "filesNangLuc").Count() > 0)
                //{
                //    var submit = new T_EB_BID_SUBMIT();
                //    submit.BID_ID = this.ObjDetail.BID_ID;
                //    submit.CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID;
                //    submit.TYPE = FileGroupType.NangLuc;
                //    submit.PKID = Guid.NewGuid().ToString();
                //    submit.DESCRIPTION = moTaNangLuc;
                //    submit.CREATE_BY = ProfileUtilities.User.USER_NAME;

                //    foreach (var item in dictionary.Where(x => x.Value == "filesNangLuc"))
                //    {
                //        UnitOfWork.Repository<BidSubmitFileRepo>().Create(new T_EB_BID_SUBMIT_FILE()
                //        {
                //            SUBMIT_ID = submit.PKID,
                //            FILE_ID = item.Key.REFERENCE_ID,
                //            CREATE_BY = ProfileUtilities.User.USER_NAME
                //        });
                //    }

                //    this.CurrentRepository.Create(submit);
                //}

                UnitOfWork.Commit();
                this.CreateNotify(serviceBid.ObjDetail, this.ObjDetail.CONTRACTOR_ID);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }

        public void CreateNotify(T_EB_BID bid, string contractorId)
        {
            if (!string.IsNullOrWhiteSpace(contractorId))
            {
                return;
            }
            try
            {
                var serviceContractor = new ContractorService();
                serviceContractor.Get(contractorId);

                var lstUserNotify = new List<string>();
                var newId = Guid.NewGuid().ToString();
                var serviceNotify = new NotifyService();

                lstUserNotify.Add(bid.CREATE_BY);

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


                string strContent = string.Format(strTemplate,
                    newId,
                    $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuHoSoThau",
                    "bg-pink",
                    "attach_file",
                    bid.PKID,
                    bid.TEN_GOI_THAU,
                    serviceContractor.ObjDetail.TEN_NGAN,
                    DateTime.Now.ToString(Global.DateTimeToStringFormat));

                string strRawContent = $"Gói thầu {bid.PKID} : Nhà thầu {serviceContractor.ObjDetail.TEN_NGAN} nộp hồ sơ";

                serviceNotify.ObjDetail = new T_CM_NOTIFY()
                {
                    PKID = newId,
                    CONTENTS = strContent,
                    RAW_CONTENTS = strRawContent,
                    CONTENTS_EN = strContent,
                    RAW_CONTENTS_EN = strRawContent,
                    USER_NAME = bid.CREATE_BY
                };
                serviceNotify.Create();

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
