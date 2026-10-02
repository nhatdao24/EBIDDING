using SMO.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        [AuthorizeCustom(Right = "R1001")]
        public ActionResult BaoCaoNhaThau()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1001")]
        public ActionResult ResultBaocaoNhaThau(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1002")]
        public ActionResult BaoCaoTongHopDanhGia()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1002")]
        public ActionResult ResultBaoCaoTongHopDanhGia(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1003")]
        public ActionResult BaoCaoDanhSachNhaThauPhatTrienMoi()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1003")]
        public ActionResult ResultBaoCaoDanhSachNhaThauPhatTrienMoi(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1004")]
        public ActionResult BaoCaoDanhSachBlackList()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1004")]
        public ActionResult ResultBaoCaoDanhSachBlackList(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1008")]
        public ActionResult BaoCaoDanhSachNhaThauDatYeuCau()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1008")]
        public ActionResult ResultBaoCaoDanhSachNhaThauDatYeuCau(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1005")]
        public ActionResult BaoCaoLichSuXepHangNT()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1005")]
        public ActionResult ResultBaoCaoLichSuXepHangNT(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1006")]
        public ActionResult BaoCaoLichSuDanhGiaNT()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1006")]
        public ActionResult ResultBaoCaoLichSuDanhGiaNT(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1007")]
        public ActionResult BaoCaoThongKeThamGiaThau()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1007")]
        public ActionResult ResultBaoCaoThongKeThamGiaThau(ReportService service)
        {
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1009")]
        public ActionResult BaoCaoThongKeGoiThau()
        {
            var service = new ReportService();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R1009")]
        public ActionResult ResultBaoCaoThongKeGoiThau(ReportService service)
        {
            return PartialView(service);
        }
    }
}