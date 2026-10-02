using NHibernate;
using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using SMO.SAPINT;
using SMO.SAPINT.Function;
using System;
using System.Configuration;
using System.Data;
using SMO.Service.AD;
using System.Data.SqlClient;
using System.Linq;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.AD;
using iTextSharp.text;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using SMO.Core.Entities.MD;
using SMO.Core.Entities.AD;
using Microsoft.Graph.Models.CallRecords;
using SMO.Service.CM;

namespace SMO.Service.MD
{
    public class ProjectService : GenericService<T_MD_PROJECT, ProjectRepo>
    {
        public IList<T_AD_USER> ObjUser { get; set; }
        public string SearchUserName {get; set;}
        public IList<T_AD_USER_PROJECT> ObjListUserProject { get; set; }

        public ProjectService() : base()
        {
            ObjUser = new List<T_AD_USER>();
            ObjListUserProject = new List<T_AD_USER_PROJECT>();

        }

        public override void Create()
        {
            this.ObjDetail.CODE = Guid.NewGuid().ToString();
            base.Create();
        }

        public IList<T_AD_USER> SearchUserProjectForAdd(string projectCode, string userName)
        {
            int pageSize = 20; 
            int pageIndex = 1;
            int total;
            var allUsers = UnitOfWork.Repository<UserRepo>()
                             .GetAll()
                             .Where(u => u.USER_TYPE == "NB");

            var usersInProject = UnitOfWork.Repository<UserProjectRepo>()
                 .GetAll()
                 .Where(up => up.PROJECT_CODE == projectCode)
                 .Select(up => up.USER_NAME);

            var usersNotInProject = allUsers
                 .Where(user => !usersInProject.Contains(user.USER_NAME));

            if (!string.IsNullOrEmpty(userName))
            {
                usersNotInProject = usersNotInProject
                    .Where(user => user.USER_NAME.IndexOf(userName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   user.FULL_NAME.IndexOf(userName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            total = usersNotInProject.Count();

            int startIndex = pageSize * (pageIndex - 1);
            var lstT = usersNotInProject
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();

            return lstT;
        }

        public IList<T_AD_USER_PROJECT> SearchListUserProject(string projectCode)
        {
            var queryListUserProject = UnitOfWork.Repository<UserProjectRepo>().GetAll();

            var filteredList = queryListUserProject
                               .Where(userProject => userProject.PROJECT_CODE == projectCode)
                               .OrderBy(userProject => userProject.USER_NAME) 
                               .ToList(); 
            return filteredList.OrderBy(userProject => userProject.USER_NAME).ToList();

        }

        public void AddUserProjectToUser(string lstUserProject, string ProjectCode)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                foreach (var userProject in lstUserProject.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    UnitOfWork.Repository<UserProjectRepo>().Create(new T_AD_USER_PROJECT
                    {
                        PKID = Guid.NewGuid().ToString(),
                        PROJECT_CODE = ProjectCode,
                        USER_NAME = userProject,
                        CREATE_BY = ProfileUtilities.User?.USER_NAME,
                        CREATE_DATE = DateTime.Now,
                    });
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void DeleteUserOfProject(string lstUserProject, string ProjectCode)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                var deletedUsers = new List<string>();

                foreach (var userName in lstUserProject.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var item = UnitOfWork.Repository<UserProjectRepo>().Queryable()
                        .FirstOrDefault(x => x.USER_NAME == userName && x.PROJECT_CODE == ProjectCode);

                    if (item != null)
                    {
                        deletedUsers.Add(userName);
                        UnitOfWork.Repository<UserProjectRepo>().Delete(item);

                        var rights = UnitOfWork.Repository<RightProjectUserRepo>().Queryable()
                            .Where(x => x.USER_NAME == userName && x.PROJECT_CODE == ProjectCode).ToList();

                        foreach (var right in rights)
                        {
                            UnitOfWork.Repository<RightProjectUserRepo>().Delete(right);
                        }
                    }
                }

                UnitOfWork.Commit();
                #region Create thông báo

                var usersInProject = UnitOfWork.Repository<UserProjectRepo>()
                    .GetAll()
                    .Where(up => up.PROJECT_CODE == ProjectCode)
                    .Select(up => up.USER_NAME)
                    .ToList();

                var projectName = UnitOfWork.Repository<ProjectRepo>().Queryable()
                    .Where(p => p.CODE == ProjectCode)
                    .Select(p => p.NAME)
                    .FirstOrDefault();

                var serviceNotify = new NotifyService();

                foreach (var remainingUser in usersInProject)
                {
                    if (deletedUsers.Contains(remainingUser)) continue;

                    var newId = Guid.NewGuid().ToString();
                    string strTemplate = @"
                        <a href='#' id='a{0}' onclick='SendNotifyIsReaded(""{0}"");'>
                            <div class='icon-circle bg-blue'>
                                <i class='material-icons'>people</i>
                            </div>
                            <div class='menu-info'>
                                <span>Người dùng [{1}] đã bị xóa khỏi dự án [{2}]!</span>
                                <p>
                                    <i class='material-icons'>access_time</i> {3}
                                </p>
                            </div>
                        </a>
                    ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        string.Join(", ", deletedUsers),
                        projectName,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Người dùng {string.Join(", ", deletedUsers)} đã bị xóa khỏi dự án {projectName}";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = remainingUser
                    };
                    serviceNotify.Create();
                }

                SMOUtilities.SendNotify(usersInProject);
                #endregion


            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public List<NodeRight> BuildTreeRight(string userName, string projectCode)
        {
            var lstNode = new List<NodeRight>();

            var lstAllRight = UnitOfWork.GetSession().Query<T_AD_RIGHT_PROJECT>().ToList();

            var lstUserRights = UnitOfWork.GetSession()
                                  .Query<T_AD_RIGHT_PROJECT_USER>()
                                  .Where(right => right.PROJECT_CODE == projectCode && right.USER_NAME == userName)
                                  .Select(right => right.RIGHT_CODE)
                                  .ToList();

            foreach (var item in lstAllRight)
            {
                var node = new NodeRight()
                {
                    id = item.CODE,
                    pId = item.PARENT,
                    name = "<span class='spMaOnTree'>" + item.CODE + "</span>" + item.NAME,
                    @checked = lstUserRights.Contains(item.CODE) ? "true" : "false"
                };

                lstNode.Add(node);
            }

            return lstNode;
        }

        public void UpdateRightProJectOfUser(string userName, string rightList, string statusList, string projectCode)
        {
            try
            {
                var existingRights = UnitOfWork.GetSession()
                                .Query<T_AD_RIGHT_PROJECT_USER>()
                                .Where(right => right.PROJECT_CODE == projectCode && right.USER_NAME == userName)
                                .ToList();

                var currentRights = existingRights.Select(right => right.RIGHT_CODE).ToList();
                var lstStatus = statusList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var lstRight = rightList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                UnitOfWork.BeginTransaction();

                for (int i = 0; i < lstRight.Length; i++)
                {
                    var right = lstRight[i];

                    if (i < lstStatus.Length && bool.TryParse(lstStatus[i], out bool status))
                    {
                        if (existingRights.Any(x => x.RIGHT_CODE == right))
                        {
                            if (!status)
                            {
                                var rightToRemove = existingRights.FirstOrDefault(x => x.RIGHT_CODE == right);
                                if (rightToRemove != null)
                                {
                                    UnitOfWork.GetSession().Delete(rightToRemove);
                                }
                            }
                        }
                        else
                        {
                            if (status)
                            {
                                UnitOfWork.Repository<RightProjectUserRepo>().Create(new T_AD_RIGHT_PROJECT_USER
                                {
                                    PKID = Guid.NewGuid().ToString(),
                                    PROJECT_CODE = projectCode,
                                    USER_NAME = userName,
                                    RIGHT_CODE = right,
                                    CREATE_BY = ProfileUtilities.User?.USER_NAME,
                                    CREATE_DATE = DateTime.Now,
                                });
                            }
                        }
                    }
                }

                UnitOfWork.Commit();
                this.State = true;
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }
    }

}
