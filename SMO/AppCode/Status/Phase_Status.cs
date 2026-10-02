using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public class Phase_Status
    {
        public const string KhoiTao = "01";
        public const string NopHoSo = "02";
        public const string ChamThau = "03";
        public const string ChonNhaThau = "04";
        public const string DongThau = "05";
        public const string VongTiepTheo = "06";
        public const string HuyGoiThau = "07";
        public const string ThuongThaoLai = "08";
        public static string GetStatusText(string status, string lang = "vi")
        {
            if (lang == "vi")
            {
                switch (status)
                {
                    case KhoiTao:
                        return "Khởi tạo";
                    case NopHoSo:
                        return "Nộp hồ sơ";
                    case ChamThau:
                        return "Chấm thầu";
                    case ChonNhaThau:
                        return "Chọn nhà thầu";
                    case DongThau:
                        return "Kết thúc";
                    case VongTiepTheo:
                        return "Vòng tiếp theo";
                    case HuyGoiThau:
                        return "Gói thầu bị hủy";
                    case ThuongThaoLai:
                        return "Thương thảo lại";
                    default:
                        return status;
                }
            }
            else
            {
                switch (status)
                {
                    case KhoiTao:
                        return "New";
                    case NopHoSo:
                        return "Submission";
                    case ChamThau:
                        return "Examination";
                    case ChonNhaThau:
                        return "Approval";
                    case DongThau:
                        return "Closed";
                    case VongTiepTheo:
                        return "New Round";
                    case HuyGoiThau:
                        return "Bidding package is canceled";
                    case ThuongThaoLai:
                        return "Request to renegotiate";
                    default:
                        return status;
                }
            }
        }

        public static string GetStatusColor(string status)
        {
            switch (status)
            {
                case KhoiTao:
                    return "bg-indigo";
                case NopHoSo:
                    return "bg-cyan";
                case ChamThau:
                    return "bg-purple";
                case ChonNhaThau:
                    return "bg-pink";
                case DongThau:
                    return "bg-blue-grey";
                case VongTiepTheo:
                    return "bg-blue-grey";
                case HuyGoiThau:
                    return "bg-blue-grey";
                case ThuongThaoLai:
                    return "bg-blue-grey";
                default:
                    return "";
            }
        }
    }
}