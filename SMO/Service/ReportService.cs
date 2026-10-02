using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO.Service
{
    public class ReportService : BaseService
    {
        public string MaNhaThau { get; set; }
        public string LinhVucChinh { get; set; }
        public string TenGoiThau { get; set; }
        public string TrangThaiGoiThau { get; set; }
        public string GiaiDoan { get; set; }
        public string LoaiGoiThau { get; set; }
        public string KetQua { get; set; }
        public string DonVi { get; set; }
        public string DuAn { get; set; }
        public string NguoiDanhGia { get; set; }
        public string NguoiTaoGoiThau { get; set; }
        public string XepHang { get; set; }
        public string CompanyCode { get; set; }
        public string TrangThaiNopHoSo { get; set; }
        public string TrangThaiThamGia { get; set; }
        public Decimal? GiaTriGoiThau_From { get; set; }
        public Decimal? GiaTriGoiThau_To { get; set; }
        public int? SoVongDauThau_From { get; set; }
        public int? SoVongDauThau_To { get; set; }
        public int? SoNhaThauThamGia_From { get; set; }
        public int? SoNhaThauThamGia_To { get; set; }
        public DateTime? NgayMoThau_TuNgay { get; set; }
        public DateTime? NgayMoThau_DenNgay { get; set; }
        public DateTime? NgayDongThau_TuNgay { get; set; }
        public DateTime? NgayDongThau_DenNgay { get; set; }
        public DateTime? NgayTao_TuNgay { get; set; }
        public DateTime? NgayTao_DenNgay { get; set; }

        public DateTime? DuAn_TuNgay { get; set; }
        public DateTime? DuAn_DenNgay { get; set; }

        public DateTime? NCC_TuNgay { get; set; }
        public DateTime? NCC_DenNgay { get; set; }

        public DateTime? NgayPheDuyet_TuNgay { get; set; }
        public DateTime? NgayPheDuyet_DenNgay { get; set; }

        public DateTime? NgayDanhGia_TuNgay { get; set; }
        public DateTime? NgayDanhGia_DenNgay { get; set; }

        public DateTime? NgayKetThucDanhGia_TuNgay { get; set; }
        public DateTime? NgayKetThucDanhGia_DenNgay { get; set; }
    }
}