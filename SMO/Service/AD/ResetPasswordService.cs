using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.CF;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SMO.Service.AD
{
    public class ResetPasswordService : GenericService<T_AD_REQUEST_RESET_PASS, ResetPasswordRepo>
    {
        public ResetPasswordService() : base()
        {

        }

        public void CheckMail()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                var userName = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_NAME == this.ObjDetail.USER_NAME).FirstOrDefault();
                if (userName == null)
                {
                    this.State = false;
                    this.ErrorMessage = "01";
                    UnitOfWork.Rollback();
                    return;
                }
                else
                {
                    var date = DateTime.Now.AddDays(-1);
                    var lstrequest = UnitOfWork.Repository<ResetPasswordRepo>().Queryable().Where(x => x.USER_NAME == this.ObjDetail.USER_NAME && (x.REQUEST_DATE > date)).ToList();
                    if (lstrequest.Count <= 0)
                    {
                        var item = new T_AD_REQUEST_RESET_PASS()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            USER_NAME = this.ObjDetail.USER_NAME,
                            REQUEST_DATE = DateTime.Now,
                        };
                        UnitOfWork.Repository<ResetPasswordRepo>().Create(item);

                        //Gửi mail
                        string TEN_NHA_THAU_NGAN = "{TEN_NHA_THAU_NGAN}";
                        string TEN_NHA_THAU_DAI = "{TEN_NHA_THAU_DAI}";
                        string LINK_TRUY_CAP = "{LINK_TRUY_CAP}";

                        var serviceTemplate = new ConfigTemplateNotifyService();
                        serviceTemplate.GetAll();
                        if (serviceTemplate.ObjList.Count() == 0)
                        {
                            return;
                        }
                        var serviceEmail = new EmailNotifyService();
                        var url = ConfigurationManager.AppSettings["URL_WEB"];

                        var template = serviceTemplate.ObjList.FirstOrDefault();
                        var subjectTemplate = template.RESET_PASS_CONTRACTOR_SUBJECT;
                        var bodyTemplate = template.RESET_PASS_CONTRACTOR_BODY;
                        if (string.IsNullOrWhiteSpace(subjectTemplate) || string.IsNullOrWhiteSpace(bodyTemplate))
                        {
                            return;
                        }

                        var contractor = UnitOfWork.Repository<ContractorRepo>().Queryable().FirstOrDefault(x => x.USER_NAME == this.ObjDetail.USER_NAME);
                        //var linkResetPass = ;
                        if (string.IsNullOrEmpty(contractor.EMAIL))
                        {
                            this.State = false;
                            this.ErrorMessage = "01";
                            UnitOfWork.Rollback();
                            return;
                        }
                        var contentSubject = subjectTemplate
                        .Replace(TEN_NHA_THAU_NGAN, contractor.TEN_NGAN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.TEN_DAI)
                        .Replace(LINK_TRUY_CAP, url + "ResetPassword/ResetPass?id=" + item.PKID);

                        var contentBody = bodyTemplate
                                .Replace(TEN_NHA_THAU_NGAN, contractor.TEN_NGAN)
                                .Replace(TEN_NHA_THAU_DAI, contractor.TEN_DAI)
                                .Replace(LINK_TRUY_CAP, url + "ResetPassword/ResetPass?id=" + item.PKID);
                        foreach (var mail in contractor.EMAIL.Split(';').ToList())
                        {
                            serviceEmail.ObjDetail = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = mail,
                                SUBJECT = contentSubject,
                                CONTENTS = contentBody
                            };
                            serviceEmail.Create();
                        }
                    }
                    else
                    {
                        this.State = false;
                        this.ErrorMessage = "02";
                        UnitOfWork.Rollback();
                        return;
                    }

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

        public void CheckRequest(string id)
        {
            try
            {
                var date = DateTime.Now.AddDays(-1);
                var lstrequest = UnitOfWork.Repository<ResetPasswordRepo>().Queryable().Where(x => x.PKID == id && (x.REQUEST_DATE > date)).ToList();
                if (lstrequest.Count <= 0)
                {
                    this.State = false;
                    this.ErrorMessage = "03";
                }
                else
                {
                    this.ObjDetail = this.CurrentRepository.Get(id);
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.ErrorMessage = "04";
            }

        }

        public void ResetPass()
        {
            try
            {
                if (this.ObjDetail.Password != this.ObjDetail.Password2)
                {
                    this.State = false;
                    this.ErrorMessage = "05";
                    return;
                }
                else
                {
                    UnitOfWork.BeginTransaction();
                    var user = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_NAME == this.ObjDetail.USER_NAME).FirstOrDefault();
                    if (user == null)
                    {
                        this.State = false;
                        this.ErrorMessage = "06";
                        return;
                    }
                    else
                    {
                        user.PASSWORD = UtilsCore.EncryptStringMD5(this.ObjDetail.Password.Trim());
                        UnitOfWork.Repository<UserRepo>().Update(user);
                    }
                    UnitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.ErrorMessage = "06";
            }
        }
    }
}