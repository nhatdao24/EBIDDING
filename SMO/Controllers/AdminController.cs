using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using SMO.Core.Entities;
using SMO.Hubs;
using SMO.Service;
using SMO.Service.AD;
using SMO.Service.CM;
using SMO.Service.EB;
using SMO.Service.MD;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SMO.Controllers
{
    public class AdminController : Controller
    {
        [System.Web.Mvc.Authorize]
        public ActionResult Index()
        {
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //hubContext.Clients.All.testNotify("kakalot");
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;
            //InitLanguage();

            var service = new NotifyService();
            service.GetNotifyOfUser(ProfileUtilities.User.USER_NAME);
            var countVisit = new CountVisitService();
            countVisit.ObjDetail.ACCESS_TYPE = Request.Browser.Browser;
            countVisit.ObjDetail.VIEW_TYPE = "AD";
            countVisit.ObjDetail.IS_MOBILE = Request.Browser.IsMobileDevice;
            countVisit.ObjDetail.CREATE_DATE = DateTime.Now;
            countVisit.ObjDetail.CREATE_BY = ProfileUtilities.User?.USER_NAME;
            countVisit.Create();
            if (lang != "vi")
            {
                foreach (var item in service.ObjList)
                {
                    item.CONTENTS = item.CONTENTS_EN;
                    item.RAW_CONTENTS_EN = item.RAW_CONTENTS_EN;
                }
            }
            ViewBag.NotifyService = service;

            if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
            {
                return View();
            }
            else
            {
                return View("IndexManager");
            }
        }

        public ActionResult ListUserProjectWithRight(string right, string projectCode)
        {
            ViewBag.Right = right;
            ViewBag.ProjectCode = projectCode;
            return PartialView();
        }

        public ActionResult ChangeOrg(string orgId)
        {
            var serviceOrg = new OrganizeService();
            serviceOrg.Get(orgId);
            if (serviceOrg.ObjDetail != null && ProfileUtilities.UserOrg.Count(x => x.ORGANIZE_ID == orgId) > 0)
            {
                ProfileUtilities.User.Organize = serviceOrg.ObjDetail;
            }

            var authorizeService = new AuthorizeService();
            authorizeService.GetInfoUser(ProfileUtilities.User.USER_NAME);
            authorizeService.GetUserRight(orgId);

            ProfileUtilities.UserRight = authorizeService.ListUserRight;
            return RedirectToAction("Index");
        }

        public ActionResult RefreshLanguage()
        {
            this.InitLanguage();
            return Content("OKI");
        }

        public ActionResult ChangLanguage()
        {
            ProfileUtilities.User.IS_CHANGE_LANG = !ProfileUtilities.User.IS_CHANGE_LANG;
            return Content(ProfileUtilities.User.IS_CHANGE_LANG ? "Bật chỉnh sửa ngôn ngữ" : "Tắt chỉnh sửa ngôn ngữ");
        }

        private void InitLanguage()
        {
            var service = new LanguageService();
            service.GetAll();
            foreach (var item in service.ObjList)
            {
                LanguageUtilities.AddToCache(new LanguageObject()
                {
                    Code = item.OBJECT_TYPE + "-" + item.FORM_CODE + "-" + item.FK_CODE,
                    Language = item.LANG,
                    Value = item.VALUE
                });
            }
        }

        public ActionResult SendMail()
        {
            var result = SMOUtilities.SendEmail();
            return Content(result);
        }

        public ActionResult ChonTrinhDuyet()
        {
            return View();
        }

        public ActionResult Menu()
        {
            var service = new MenuService();
            service.GetMenuRole();

            ViewBag.CountRequest = 0;
            ViewBag.CountFeeback = 0;
            if (ProfileUtilities.User.USER_TYPE == UserType.NB)
            {
                var serviceRequest = new RequestService();
                ViewBag.CountRequest = serviceRequest.CurrentRepository.Queryable().Count(x => x.STATUS == EB_Status.KhoiTao);

                var serviceFeedBack = new FAQQuestionService();
                ViewBag.CountFeeback = serviceFeedBack.CurrentRepository.Queryable().Count(x => x.STATUS == false);
            }

            return PartialView("MenuTree", service);
        }

        public ActionResult UnAuthorize(string auth)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertDanger;
            SMOUtilities.GetMessage("1100", result, auth);
            return result.ToJsonResult();
        }

        //[AuthorizeCustom(Right = "R111")]
        public void UploadImage(HttpPostedFileWrapper upload)
        {
            try
            {
                if (upload != null)
                {
                    var supportedTypes = new[] { "png", "jpeg", "jpg" };
                    var fileExt = System.IO.Path.GetExtension(upload.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        throw new Exception("Chỉ chấp nhận ảnh : png, jpeg, jpg!");
                    }
                    else if (upload.ContentLength > (2 * 1024 * 1024))
                    {
                        throw new Exception("Ảnh vượt quá dung lượng 2 MB!");
                    }

                    try
                    {
                        using (var img = Image.FromStream(upload.InputStream))
                        {
                            if (img.RawFormat.Equals(ImageFormat.Png) & img.RawFormat.Equals(ImageFormat.Jpeg))
                            {
                                throw new Exception("Chỉ chấp nhận ảnh : png, jpeg, jpg!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    string imageName = upload.FileName;
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/ImageUpload"), imageName);
                    upload.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

        public ActionResult SelectImage()
        {
            var appData = Server.MapPath("~/Content/ImageUpload");
            List<string> lstUrlImage = new List<string>();
            var images = Directory.GetFiles(appData);
            foreach (var item in images)
            {
                lstUrlImage.Add(Url.Content("/Content/ImageUpload/" + Path.GetFileName(item)));
            }
            ViewBag.ListImage = lstUrlImage;
            return PartialView();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult OpenBid(string bidId, string idMenu = "")
        {
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;

            var linkDetail = "";
            if (ProfileUtilities.User.USER_TYPE == UserType.NB)
            {
                linkDetail = $"/EB/Bid/MenuDetailBidManager?id={bidId}";
            }
            else if (ProfileUtilities.User != null && !string.IsNullOrEmpty(ProfileUtilities.User.USER_NAME))
            {
                var serviceBidContractor = new BidContractorService();
                serviceBidContractor.ObjDetail.BID_ID = bidId;
                serviceBidContractor.ObjDetail.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
                serviceBidContractor.Search();
                if (serviceBidContractor.ObjList.Count > 0)
                {
                    linkDetail = $"/EB/Bid/DetailBidContractorWait?id={bidId}";
                    if (serviceBidContractor.ObjList[0].STATUS == "AP")
                    {
                        linkDetail = $"/EB/Bid/MenuDetailBidContractor?id={bidId}";
                    }
                }
                else
                {
                    var serviceBid = new BidService();
                    serviceBid.Get(bidId);
                    if (serviceBid.ObjDetail != null && serviceBid.ObjDetail.IS_PUBLIC)
                    {
                        linkDetail = $"/EB/Bid/DetailBidContractorOpen?id={bidId}";
                    }
                }
            }

            ViewBag.Link = linkDetail;
            ViewBag.IdMenu = idMenu;
            return View();
        }

        public ActionResult OpenBidEndScore(string bidId)
        {
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;

            var linkDetail = $"/EB/Bid/MenuDetailBidManager?id={bidId}";
            ViewBag.Link = linkDetail;
            return View();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult OpenPR(string id)
        {
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;

            var linkDetail = $"/PR/PREB/Detail?id={id}";
            ViewBag.Link = linkDetail;
            return View();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult OpenBidRequire(string id)
        {
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;

            var linkDetail = $"/EB/BidRequire/Detail?id={id}";
            ViewBag.Link = linkDetail;
            return View();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult OpenReview(string reviewId)
        {
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;

            var linkDetail = $"/EB/Review/MenuDetail?id={reviewId}";
            ViewBag.Link = linkDetail;
            return View();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult OpenReviewFinal(string id)
        {
            if (Request.Browser.Browser == "InternetExplorer" || Request.Browser.Browser == "IE")
            {
                return RedirectToAction("ChonTrinhDuyet");
            }

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Authorize");
            }

            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            ProfileUtilities.User.LANGUAGE = lang;

            var linkDetail = $"/EB/ReviewFinal/Detail?id={id}";
            ViewBag.Link = linkDetail;
            return View();
        }

        public ActionResult RefreshSession()
        {
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}