using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMO.Service.WF
{
    public class WorkFlowService : GenericService<T_WF_PROCESS, ProcessRepo>
    {
        public T_WF_PROCESS ObjProcess { get; set; }
        public T_WF_ACTIVITY ObjActivity { get; set; }
        public T_WF_ACTIVITY_LEVEL ObjLevel { get; set; }
        public T_WF_ACTIVITY_LEVEL_SENDER ObjSender { get; set; }
        public T_WF_ACTIVITY_LEVEL_RECEIVER ObjReceiver { get; set; }
        public T_WF_ACTIVITY_LEVEL_RECEIVER_COM ObjCom { get; set; }
        public WorkFlowService() : base()
        {
            this.ObjProcess = new T_WF_PROCESS();
            this.ObjActivity = new T_WF_ACTIVITY();
            this.ObjLevel = new T_WF_ACTIVITY_LEVEL();
            this.ObjSender = new T_WF_ACTIVITY_LEVEL_SENDER();
            this.ObjReceiver = new T_WF_ACTIVITY_LEVEL_RECEIVER();
            this.ObjCom = new T_WF_ACTIVITY_LEVEL_RECEIVER_COM();
        }

        public List<NodeWorkFlow> BuildTree()
        {
            var lstNode = new List<NodeWorkFlow>();
            this.GetAll();
            var idParentAll = Guid.NewGuid().ToString();
            lstNode.Add(new NodeWorkFlow()
            {
                id = idParentAll, name = "Danh sách workflow", type = "ALL"
            });

            foreach (var process in this.ObjList)
            {
                lstNode.Add(new NodeWorkFlow()
                {
                    id = process.CODE,
                    name = process.NAME,
                    type = WorkFlowType.Process.ToString(),
                    pId = idParentAll
                });

                foreach (var activity in process.ListActivity.OrderBy(x => x.C_ORDER))
                {
                    lstNode.Add(new NodeWorkFlow()
                    {
                        id = activity.PKID,
                        name = activity.NAME,
                        pId = activity.FK_PROCESS,
                        type = WorkFlowType.Activity.ToString()
                    });

                    #region Standard
                    if (activity.TYPE == ActivityType.Standard.ToString())
                    {
                        
                        foreach (var level in activity.ListLevel)
                        {
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = level.PKID,
                                name = level.NAME,
                                pId = level.FK_ACTIVITY,
                                type = WorkFlowType.Level.ToString()
                            });

                            var idSender = Guid.NewGuid().ToString();
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = idSender,
                                name = "Người gửi",
                                pId = level.PKID
                            });

                            foreach (var sender in level.ListSender)
                            {
                                lstNode.Add(new NodeWorkFlow()
                                {
                                    id = sender.PKID,
                                    name = sender.USERNAMES,
                                    pId = idSender,
                                    type = WorkFlowType.Sender.ToString()
                                });

                                var idReceiver = Guid.NewGuid().ToString();
                                lstNode.Add(new NodeWorkFlow()
                                {
                                    id = idReceiver,
                                    name = "Người nhận",
                                    pId = sender.PKID
                                });

                                foreach (var receiver in sender.ListReceiver)
                                {
                                    lstNode.Add(new NodeWorkFlow()
                                    {
                                        id = receiver.PKID,
                                        name = receiver.USERNAMES,
                                        pId = idReceiver,
                                        type = WorkFlowType.Receiver.ToString()
                                    });

                                    foreach (var com in receiver.ListCom)
                                    {
                                        lstNode.Add(new NodeWorkFlow()
                                        {
                                            id = com.PKID,
                                            name = com.TYPE,
                                            pId = receiver.PKID,
                                            type = WorkFlowType.Com.ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Approve
                    else if (activity.TYPE == ActivityType.Approve.ToString())
                    {
                        foreach (var level in activity.ListLevel.OrderBy(x => x.C_ORDER))
                        {
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = level.PKID,
                                name = level.NAME,
                                pId = level.FK_ACTIVITY,
                                type = WorkFlowType.Level.ToString()
                            });

                            var idApprove = Guid.NewGuid().ToString();
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = idApprove,
                                name = "Phê duyệt",
                                pId = level.PKID
                            });

                            var idSenderApprove = Guid.NewGuid().ToString();
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = idSenderApprove,
                                name = "Người gửi",
                                pId = idApprove
                            });

                            var idReject = Guid.NewGuid().ToString();
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = idReject,
                                name = "Từ chối",
                                pId = level.PKID
                            });

                            var idSenderReject = Guid.NewGuid().ToString();
                            lstNode.Add(new NodeWorkFlow()
                            {
                                id = idSenderReject,
                                name = "Người gửi",
                                pId = idReject
                            });

                            foreach (var sender in level.ListSender)
                            {
                                if (sender.TYPE == ApproveType.Approve.ToString())
                                {
                                    lstNode.Add(new NodeWorkFlow()
                                    {
                                        id = sender.PKID,
                                        name = sender.USERNAMES,
                                        pId = idSenderApprove,
                                        type = WorkFlowType.Sender.ToString()
                                    });
                                }

                                else if (sender.TYPE == ApproveType.Reject.ToString())
                                {
                                    lstNode.Add(new NodeWorkFlow()
                                    {
                                        id = sender.PKID,
                                        name = sender.USERNAMES,
                                        pId = idSenderReject,
                                        type = WorkFlowType.Sender.ToString()
                                    });
                                }

                                var idReceiver = Guid.NewGuid().ToString();
                                lstNode.Add(new NodeWorkFlow()
                                {
                                    id = idReceiver,
                                    name = "Người nhận",
                                    pId = sender.PKID
                                });

                                foreach (var receiver in sender.ListReceiver)
                                {
                                    lstNode.Add(new NodeWorkFlow()
                                    {
                                        id = receiver.PKID,
                                        name = receiver.USERNAMES,
                                        pId = idReceiver,
                                        type = WorkFlowType.Receiver.ToString()
                                    });

                                    foreach (var com in receiver.ListCom)
                                    {
                                        lstNode.Add(new NodeWorkFlow()
                                        {
                                            id = com.PKID,
                                            name = com.TYPE,
                                            pId = receiver.PKID,
                                            type = WorkFlowType.Com.ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }

            return lstNode;
        }

        public void GetProcessById(string id)
        {
            this.ObjProcess = UnitOfWork.Repository<ProcessRepo>().Get(id);
        }

        public void GetActivityById(string id)
        {
            this.ObjActivity = UnitOfWork.Repository<ActivityRepo>().Get(id);
        }

        public void GetLevelById(string id)
        {
            this.ObjLevel = UnitOfWork.Repository<ActivityLevelRepo>().Get(id);
        }

        public void GetSenderById(string id)
        {
            this.ObjSender = UnitOfWork.Repository<ActivityLevelSenderRepo>().Get(id);
        }

        public void GetReceiverById(string id)
        {
            this.ObjReceiver = UnitOfWork.Repository<ActivityLevelReceiverRepo>().Get(id);
        }

        public void GetComById(string id)
        {
            this.ObjCom = UnitOfWork.Repository<ActivityLevelReceiverComRepo>().Get(id);
        }

        public void CreateProcess()
        {
            try
            {
                this.ObjProcess.ACTIVE = true;
                if (!UnitOfWork.Repository<ProcessRepo>().CheckExist(x => x.CODE == this.ObjProcess.CODE))
                {
                    UnitOfWork.BeginTransaction();
                    UnitOfWork.Repository<ProcessRepo>().Create(this.ObjProcess);
                    UnitOfWork.Commit();
                }
                else
                {
                    this.State = false;
                    this.MesseageCode = "1101";
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CreateActivity()
        {
            try
            {
                if (!UnitOfWork.Repository<ActivityRepo>().CheckExist(x => x.CODE == this.ObjActivity.CODE))
                {
                    UnitOfWork.BeginTransaction();
                    this.ObjActivity.PKID = Guid.NewGuid().ToString();
                    UnitOfWork.Repository<ActivityRepo>().Create(this.ObjActivity);

                    this.ObjLevel.PKID = Guid.NewGuid().ToString();
                    this.ObjLevel.NAME = "Level 1";
                    this.ObjLevel.FK_ACTIVITY = this.ObjActivity.PKID;
                    this.ObjLevel.C_ORDER = 1;
                    UnitOfWork.Repository<ActivityLevelRepo>().Create(this.ObjLevel);

                    UnitOfWork.Commit();
                }
                else
                {
                    this.State = false;
                    this.MesseageCode = "1101";
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CreateLevel()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                this.ObjLevel.PKID = Guid.NewGuid().ToString();
                UnitOfWork.Repository<ActivityLevelRepo>().Create(this.ObjLevel);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CreateSender()
        {
            try
            {
                this.ObjSender.PKID = Guid.NewGuid().ToString();
                UnitOfWork.BeginTransaction();
                UnitOfWork.Repository<ActivityLevelSenderRepo>().Create(this.ObjSender);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CreateReceiver()
        {
            try
            {
                this.ObjReceiver.PKID = Guid.NewGuid().ToString();
                if (!UnitOfWork.Repository<ActivityLevelReceiverRepo>().CheckExist(x => x.USERNAMES == this.ObjReceiver.USERNAMES && x.FK_SENDER == this.ObjReceiver.FK_SENDER))
                {
                    UnitOfWork.BeginTransaction();
                    UnitOfWork.Repository<ActivityLevelReceiverRepo>().Create(this.ObjReceiver);
                    var com1 = new T_WF_ACTIVITY_LEVEL_RECEIVER_COM()
                    {
                        ACTIVE = false,
                        PKID = Guid.NewGuid().ToString(),
                        FK_RECEIVER = this.ObjReceiver.PKID,
                        SUBJECT = "",
                        CONTENTS = "",
                        TYPE = "SMS"
                    };

                    var com2 = new T_WF_ACTIVITY_LEVEL_RECEIVER_COM()
                    {
                        ACTIVE = false,
                        PKID = Guid.NewGuid().ToString(),
                        FK_RECEIVER = this.ObjReceiver.PKID,
                        SUBJECT = "",
                        CONTENTS = "",
                        TYPE = "EMAIL"
                    };

                    var com3 = new T_WF_ACTIVITY_LEVEL_RECEIVER_COM()
                    {
                        ACTIVE = false,
                        PKID = Guid.NewGuid().ToString(),
                        FK_RECEIVER = this.ObjReceiver.PKID,
                        SUBJECT = "",
                        CONTENTS = "",
                        TYPE = "NOTIFY"
                    };
                    UnitOfWork.Repository<ActivityLevelReceiverComRepo>().Create(com1);
                    UnitOfWork.Repository<ActivityLevelReceiverComRepo>().Create(com2);
                    UnitOfWork.Repository<ActivityLevelReceiverComRepo>().Create(com3);
                    UnitOfWork.Commit();
                }
                else
                {
                    this.State = false;
                    this.MesseageCode = "1101";
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateProcess()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<ProcessRepo>().Update(this.ObjProcess);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateActivity()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<ActivityRepo>().Update(this.ObjActivity);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateLevel()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<ActivityLevelRepo>().Update(this.ObjLevel);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateSender()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<ActivityLevelSenderRepo>().Update(this.ObjSender);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }


        public void UpdateReceiver()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<ActivityLevelReceiverRepo>().Update(this.ObjReceiver);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateCom()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<ActivityLevelReceiverComRepo>().Update(this.ObjCom);
                UnitOfWork.Commit();
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
