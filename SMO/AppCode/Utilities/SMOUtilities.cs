using Azure.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using NHibernate.Proxy;
using SMO.Core.Entities;
using SMO.Hubs;
using SMO.Repository.Common;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.CM;
using SMO.Service.EB;
using SMO.Service.MD;
using SMO.SMS_GW;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace SMO
{
    public class SMOUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgCode"></param>
        /// <param name="service"></param>
        /// <param name="transferObject"></param>
        /// <param name="paramObject"></param>
        public static void GetMessage(string msgCode, BaseService service, TransferObject transferObject, params object[] paramObject)
        {
            var id = "ERR" + DateTime.Now.ToString("yyMMddHHmmssFF");
            var code = msgCode;
            if (!string.IsNullOrWhiteSpace(service.MesseageCode))
            {
                code = service.MesseageCode;
            }
            transferObject.Message.Code = code;
            transferObject.Message.Message = MessageUtilities.GetMessage(code);
            if (!string.IsNullOrWhiteSpace(transferObject.Message.Message))
            {
                transferObject.Message.Message = string.Format(transferObject.Message.Message, paramObject);
            }

            if (!string.IsNullOrWhiteSpace(service.ErrorMessage))
            {
                transferObject.Message.Detail = service.ErrorMessage + "<br/>";
            }

            if (service.Exception != null)
            {
                //transferObject.Message.Detail += service.Exception.ToString();
                transferObject.Message.Detail += "Xem chi tiết lỗi theo mã lỗi sau : " + id;
                Log.Error(id + Environment.NewLine + service.Exception.ToString());
            }
        }

        public static void GetMessage(string msgCode, TransferObject transferObject, params object[] paramObject)
        {
            transferObject.Message.Code = msgCode;
            transferObject.Message.Message = MessageUtilities.GetMessage(msgCode);
            if (!string.IsNullOrWhiteSpace(transferObject.Message.Message))
            {
                transferObject.Message.Message = string.Format(transferObject.Message.Message, paramObject);
            }
        }

        /// <summary>
        /// Lấy dữ liệu nhiều bảng, chỉ với một lần kết nối với database
        /// Params : listSelect là một List<SqlSelectMutil>. Cho nên khi khởi tạo dữ liệu phải thật cẩn thận. Dynamic object gồm 2 thuộc tính : Table, Where
        /// Khai báo như sau (chú ý : nếu có mệnh đề where thì object.Where = "CONDITION" - không cần thêm chữ WHERE ở condition)
        /// var lstSelect = new List<SqlSelectMutil>();
        /// lstSelect.Add(new SqlSelectMutil(){ Table = "T_AD_ROLE", Where = ""});
        /// lstSelect.Add(new SqlSelectMutil(){ Table = "T_AD_USER", Where = "1=1" });
        /// lstSelect.Add(new SqlSelectMutil(){ Table = "T_MD_BATCH", Where = "" });
        /// </summary>
        /// <param name="listSelect"></param>
        /// <returns>Dataset chứa các table kết quả. Các table đó có TableName = object.Table đã được truyền vào</returns> 
        public static DataSet GetMultilpleTable(List<SqlSelectMutil> listSelect)
        {
            DataSet dsResult = new DataSet();
            if (listSelect.Count > 0)
            {
                var strSelect = new System.Text.StringBuilder();
                foreach (var item in listSelect)
                {
                    if (string.IsNullOrEmpty(item.SQL))
                    {
                        var select = item.Select;
                        if (string.IsNullOrWhiteSpace(item.Select))
                        {
                            select = "*";
                        }
                        if (string.IsNullOrWhiteSpace(item.Where))
                        {
                            strSelect.Append(string.Format(" SELECT {0} FROM {1};", select, item.Table));
                        }
                        else
                        {
                            strSelect.Append(string.Format(" SELECT {0} FROM {1} WHERE {2};", select, item.Table, item.Where));
                        }
                    }
                    else
                    {
                        strSelect.Append(item.SQL);
                    }
                }

                using (SqlConnection objConn = new SqlConnection(ConfigurationManager.ConnectionStrings["SMO_MSSQL_Connection"].ConnectionString))
                {
                    SqlCommand objCmd = new SqlCommand(strSelect.ToString());
                    objCmd.Connection = objConn;
                    objCmd.CommandType = CommandType.Text;
                    try
                    {
                        objConn.Open();
                        var adapter = new SqlDataAdapter(objCmd);
                        adapter.Fill(dsResult);
                        int i = 0;
                        foreach (var item in listSelect)
                        {
                            dsResult.Tables[i].TableName = item.Table;
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        objConn.Close();
                    }
                }
            }
            return dsResult;
        }

        /// <summary>
        /// Kiểm tra class is Proxy do Nhibernate tạo ra
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsProxy(object obj)
        {
            return NHibernateProxyHelper.IsProxy(obj);
        }

        public static void SendNotify(List<string> lstUserNotify)
        {
            var serviceNotify = new NotifyService();
            JsonSerializerSettings serializerSetting = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            foreach (var user in lstUserNotify)
            {
                serviceNotify.GetNotifyOfUser(user);
                if (serviceNotify.IntCountNew > 0)
                {
                    hubContext.Clients.Group(user).RefreshNotify(JsonConvert.SerializeObject(serviceNotify, serializerSetting));
                }
            }
        }

        public static string BytesToSize(double bytes)
        {
            var sizes = new string[] { "Bytes", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0 Byte";
            var i = Convert.ToInt32(Math.Floor(Math.Log(bytes) / Math.Log(1024)));
            return (Math.Round(bytes / Math.Pow(1024, i), 2)).ToString() + ' ' + sizes[i];
        }

        public static string GetIconOfFile(string ext)
        {
            ext = ext.ToLower();
            var icon = "file-48.png";
            switch (ext)
            {
                case ".xlsx":
                    icon = "excel-48.png";
                    break;
                case ".xls":
                    icon = "excel-48.png";
                    break;
                case ".docx":
                    icon = "word-48.png";
                    break;
                case ".doc":
                    icon = "word-48.png";
                    break;
                case ".pptx":
                    icon = "powerpoint-48.png";
                    break;
                case ".ppt":
                    icon = "powerpoint-48.png";
                    break;

                case ".txt":
                    icon = "txt-48.png";
                    break;

                case ".zip":
                    icon = "zip-48.png";
                    break;
                case ".7z":
                    icon = "zip-48.png";
                    break;
                case ".rar":
                    icon = "zip-48.png";
                    break;

                case ".jpg":
                    icon = "image-48.png";
                    break;
                case ".png":
                    icon = "image-48.png";
                    break;
                case ".bmp":
                    icon = "image-48.png";
                    break;
                case ".jpeg":
                    icon = "image-48.png";
                    break;

                case ".psd":
                    icon = "photoshop-48.png";
                    break;
                case ".dwg":
                    icon = "dwg-48.png";
                    break;
                case ".dxf":
                    icon = "dxf-48.png";
                    break;

                case ".pdf":
                    icon = "pdf-48.png";
                    break;

                default:
                    break;
            }
            return icon;
        }

        public static string BroadcastMail()
        {
            string strResult = string.Empty;
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            try
            {
                // Tìm những gói thầu public đã được phê duyệt và chưa gửi mail thông báo rộng rãi
                var lstBid = UnitOfWork.Repository<BidRepo>().Queryable().Where(
                        x => x.IS_PUBLIC == true &&
                        x.IS_BROADCAST_MAIL == false &&
                        x.STATUS == EB_Status.DaPheDuyet
                    ).ToList();

                foreach (var bid in lstBid)
                {
                    // Tìm danh sách sản phẩm của đề nghị thầu
                    var lstMaterialCode = UnitOfWork.Repository<BidRequireDetailRepo>().Queryable().Where(
                            x => x.HEADER_CODE == bid.REQUIRED_CODE
                        ).Select(x => x.MATERIAL_CODE).Distinct().ToList();
                    // Tìm danh sách loại sản phẩm
                    var lstMaterialType = UnitOfWork.Repository<MaterialRepo>().Queryable().Where(
                            x => lstMaterialCode.Contains(x.CODE)
                        ).Select(x => x.TYPE).Distinct().ToList();
                    // Tìm danh sách mã nhà thầu dựa trên danh sách loại sản phẩm
                    var lstContractorId = UnitOfWork.Repository<ContractorProductRepo>().Queryable().Where(
                            x => lstMaterialType.Contains(x.PRODUCT_CODE)
                        ).Select(x => x.CONTRACTOR_ID).Distinct().ToList();
                    // Tìm danh sách nhà thầu và email của nhà thầu để gửi
                    var lstContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(
                            x => lstContractorId.Contains(x.PKID)
                        ).ToList();

                    var bidService = new BidService();
                    bidService.ObjDetail = bid;
                    bidService.CreateInviteEmailContractor(lstContractor);

                    UnitOfWork.BeginTransaction();
                    bid.IS_BROADCAST_MAIL = true;
                    UnitOfWork.Repository<BidRepo>().Update(bid);
                    UnitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                strResult = ex.ToString();
            }
            return strResult;
        }

        public static string SendEmail()
        {
            string tenantId = ConfigurationManager.AppSettings["ida:TenantID"];
            string appId = ConfigurationManager.AppSettings["ida:AppId"];
            string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
            string accEmail = ConfigurationManager.AppSettings["ida:AccEmail"];
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var service = new EmailNotifyService();

            string strResult = "";
            try
            {
                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };
                var clientSecretCredential = new ClientSecretCredential(
                    tenantId, appId, appSecret, options);

                var graphClient = new GraphServiceClient(clientSecretCredential, scopes);


                var checkDate = DateTime.Now.AddDays(-2);
                var lstEmail = service.UnitOfWork.GetSession().QueryOver<T_CM_EMAIL>().Where(
                        x => !x.IS_SEND &&
                        x.CREATE_DATE > checkDate
                        && x.NUMBER_RETRY < 6
                    ).OrderBy(x => x.CREATE_DATE).Asc
                    .Take(5).Skip(0)
                    .List();
                foreach (var item in lstEmail)
                {
                    try
                    {
                        var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                        {
                            Message = new Microsoft.Graph.Models.Message
                            {
                                Subject = item.SUBJECT,
                                Body = new ItemBody
                                {
                                    ContentType = BodyType.Html,
                                    Content = item.CONTENTS,
                                },
                                ToRecipients = new List<Recipient>()
                            },
                            SaveToSentItems = true,
                        };
                        foreach (var email in item.EMAIL.Split(new[] { ";", ",", "|" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            requestBody.Message.ToRecipients.Add(new Recipient()
                            {
                                EmailAddress = new EmailAddress()
                                {
                                    Address = email
                                }
                            });
                        }

                        graphClient.Users[accEmail].SendMail.PostAsync(requestBody).Wait();
                        item.IS_SEND = true;
                    }
                    catch (Exception ex)
                    {
                        item.IS_SEND = false;
                        item.NUMBER_RETRY++;
                        strResult += $"EMAIL {item.PKID} : {item.EMAIL} bị lỗi : {ex.ToString()}";
                        item.ERROR_MSG = ex.ToString();
                    }
                    service.ObjDetail = item;
                    service.Update();
                }
            }
            catch (Exception ex)
            {
                strResult = ex.ToString();
            }

            return strResult;
        }

        public static string AutoCloseReview()
        {
            var strResult = "";
            var service = new ReviewService();
            service.AutoCloseOrOpenReview();
            if (!service.State)
            {
                strResult = service.Exception.ToString();
            }
            return strResult;
        }

        public static string AutoSendMail2DayEndBid()
        {
            var strResult = "";
            var service = new BidService();
            service.AutoSendMail2DayEndBid();
            if (!service.State)
            {
                strResult = service.Exception.ToString();
            }
            return strResult;
        }
    }
}