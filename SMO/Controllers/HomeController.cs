using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using SMO.Core.Entities;
using SMO.Hubs;
using SMO.Service.AD;
using SMO.Service.CM;
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
    public class HomeController : Controller
    {
        public ActionResult UnAuthorize(string auth)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertDanger;
            SMOUtilities.GetMessage("1100", result, auth);
            return result.ToJsonResult();
        }
    }
}