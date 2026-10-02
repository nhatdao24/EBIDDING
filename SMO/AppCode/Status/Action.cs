using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public class Action
    {
        public const string TaoMoi = "01";
        public const string ChinhSua = "03";
        public const string TrinhDuyet = "04";
        public const string HuyTrinhDuỵet = "05";
        public const string PheDuyet = "06";
        public const string HuyPheDuyet = "07";
        public const string TuChoi = "08";
        public const string YeuCauCapNhat = "09";
        public const string Huy = "10";
        public const string ChamThau = "11";
        public const string GuiYeuCau = "12";
        public const string HuyGuiYeuCau = "13";
        public const string YeuCauCapNhatYeuCau = "14";
        public const string TuChoiYeuCau = "15";
        public const string KetThucThuongThao = "16";
        public const string DongThau = "17";
        public const string ChonNhaThau = "18";
        public const string HuyChonNhaThau = "19";
        public const string ChonDSSoBo = "20";
        public const string KetThucChamThau = "21";
        public const string ThuongThaoLai = "22";


        public static string GetActionText(string action)
        {
            var lang = "vi";
            if (ProfileUtilities.User != null)
            {
                lang = ProfileUtilities.User.LANGUAGE;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            if (lang == "vi")
            {
                switch (action)
                {
                    case TaoMoi:
                        return "Tạo mới";
                    case ChinhSua:
                        return "Chỉnh sửa";
                    case TrinhDuyet:
                        return "Trình duyệt";
                    case HuyTrinhDuỵet:
                        return "Hủy trình duyệt";
                    case PheDuyet:
                        return "Phê duyệt";
                    case HuyPheDuyet:
                        return "Hủy phê duyệt";
                    case TuChoi:
                        return "Từ chối";
                    case YeuCauCapNhat:
                        return "Yêu cầu cập nhật";
                    case Huy:
                        return "Hủy";
                    case ChamThau:
                        return "Chấm thầu";
                    case GuiYeuCau:
                        return "Gửi yêu cầu";
                    case HuyGuiYeuCau:
                        return "Hủy gửi yêu cầu";
                    case YeuCauCapNhatYeuCau:
                        return "Yêu cầu cập nhật";
                    case TuChoiYeuCau:
                        return "Từ chối";
                    case KetThucThuongThao:
                        return "Kết thúc thương thảo";
                    case DongThau:
                        return "Đóng gói thầu";
                    case ChonNhaThau:
                        return "Chọn nhà thầu";
                    case HuyChonNhaThau:
                        return "Hủy chọn nhà thầu";
                    case ChonDSSoBo:
                        return "Chọn danh sách sơ bộ";
                    case KetThucChamThau:
                        return "Kết thúc chấm thầu";
                    case ThuongThaoLai:
                        return "Thương Thảo Lại";
                    default:
                        return action;
                }
            }
            else
            {
                switch (action)
                {
                    case TaoMoi:
                        return "Tạo mới";
                    case ChinhSua:
                        return "Chỉnh sửa";
                    case TrinhDuyet:
                        return "Trình duyệt";
                    case HuyTrinhDuỵet:
                        return "Hủy trình duyệt";
                    case PheDuyet:
                        return "Phê duyệt";
                    case HuyPheDuyet:
                        return "Hủy phê duyệt";
                    case TuChoi:
                        return "Từ chối";
                    case YeuCauCapNhat:
                        return "Yêu cầu cập nhật";
                    case Huy:
                        return "Hủy";
                    case ChamThau:
                        return "Chấm thầu";
                    case GuiYeuCau:
                        return "Gửi yêu cầu";
                    case HuyGuiYeuCau:
                        return "Hủy gửi yêu cầu";
                    case YeuCauCapNhatYeuCau:
                        return "Yêu cầu cập nhật";
                    case TuChoiYeuCau:
                        return "Từ chối";
                    case KetThucThuongThao:
                        return "Kết thúc thương thảo";
                    case DongThau:
                        return "Đóng gói thầu";
                    case ChonNhaThau:
                        return "Chọn nhà thầu";
                    case HuyChonNhaThau:
                        return "Hủy chọn nhà thầu";
                    case ChonDSSoBo:
                        return "Chọn danh sách sơ bộ";
                    case KetThucChamThau:
                        return "Kết thúc chấm thầu";
                    case ThuongThaoLai:
                        return "Thương Thảo Lại";
                    default:
                        return action;
                }
            }

        }

        public static string GetActionColor(string action)
        {
            switch (action)
            {
                case TaoMoi:
                    return "bg-indigo";
                case ChinhSua:
                    return "bg-purple";
                case TrinhDuyet:
                    return "bg-brown";
                case HuyTrinhDuỵet:
                    return "bg-grey";
                case PheDuyet:
                    return "bg-blue";
                case HuyPheDuyet:
                    return "bg-grey";
                case TuChoi:
                    return "bg-red";
                case YeuCauCapNhat:
                    return "bg-amber";
                case Huy:
                    return "bg-grey";
                case GuiYeuCau:
                    return "bg-brown";
                case HuyGuiYeuCau:
                    return "bg-grey";
                case YeuCauCapNhatYeuCau:
                    return "bg-amber";
                case TuChoiYeuCau:
                    return "bg-red";
                case KetThucThuongThao:
                    return "bg-green";
                case DongThau:
                    return "bg-grey";
                case ChonNhaThau:
                    return "bg-brown";
                case HuyChonNhaThau:
                    return "bg-grey";
                case ChonDSSoBo:
                    return "bg-amber";
                case KetThucChamThau:
                    return "bg-brown";
                case ThuongThaoLai:
                    return "bg-grey";
                default:
                    return action;
            }
        }
    }
}