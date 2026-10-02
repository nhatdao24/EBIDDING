using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public class Review_Final_Status
    {
        public const string KhoiTao = "01";
        public const string KetThuc = "02";
        public const string ChoPheDuyet = "03";
        public const string TuChoi = "04";
        public const string DaPheDuyet = "05";
        public const string Huy = "06";
        public const string ChinhSua = "07";
        public const string DangPheDuyet = "08";
        public const string HuyTrinhDuyet = "09";
        public const string ChuaGuiYeuCau = "10";
        public const string DaGuiYeuCau = "11";
        public const string HuyPheDuyet = "12";
        public static string GetStatusText(string status)
        {
            switch (status)
            {
                case KhoiTao:
                    return "Đang xếp hạng";
                case KetThuc:
                    return "Kết thúc";
                case ChoPheDuyet:
                    return "Chờ phê duyệt";
                case TuChoi:
                    return "Từ chối";
                case DaPheDuyet:
                    return "Đã phê duyệt";
                case Huy:
                    return "Đã bị hủy";
                case ChinhSua:
                    return "Chỉnh sửa thông tin xếp hạng";
                case DangPheDuyet:
                    return "Đang phê duyệt";
                case HuyTrinhDuyet:
                    return "Hủy trình duyệt";
                case ChuaGuiYeuCau:
                    return "Chưa gửi yêu cầu xếp hạng";
                case DaGuiYeuCau:
                    return "Đã gửi yêu cầu";
                case HuyPheDuyet:
                    return "Hủy phê duyệt";
                default:
                    return status;
            }
        }

        public static string GetStatusColor(string status)
        {
            switch (status)
            {
                case KhoiTao:
                    return "bg-indigo";
                case KetThuc:
                    return "bg-blue-grey";
                case ChoPheDuyet:
                    return "bg-brown";
                case TuChoi:
                    return "bg-deep-orange";
                case DaPheDuyet:
                    return "bg-blue";
                case Huy:
                    return "bg-red";
                case ChinhSua:
                    return "bg-purple";
                case DangPheDuyet:
                    return "bg-teal";
                case HuyTrinhDuyet:
                    return "bg-amber";
                case ChuaGuiYeuCau:
                    return "bg-amber";
                case DaGuiYeuCau:
                    return "bg-green";
                case HuyPheDuyet:
                    return "bg-amber";
                default:
                    return "";
            }
        }

        public static string GetStatusIcon(string status)
        {
            switch (status)
            {
                case KhoiTao:
                    return "note_add";
                case ChoPheDuyet:
                    return "hourglass_empty";
                case TuChoi:
                    return "cancel";
                case DaPheDuyet:
                    return "done_all";
                case Huy:
                    return "delete";
                default:
                    return "";
            }
        }
    }
}