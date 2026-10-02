using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_RQ_REQUEST_TO : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REQUEST_ID { get; set; }
        public virtual DateTime TO_DATE { get; set; }
        public virtual string STATUS { get; set; }
        public virtual string USERNAMES { get; set; }
        public virtual string PROCESS { get; set; }
        public virtual string ACTIVITY { get; set; }
        public virtual string C_LEVEL { get; set; }
        public virtual T_RQ_REQUEST Request { get; set; }
        public virtual T_WF_PROCESS WFProcess { get; set; }
        public virtual T_WF_ACTIVITY WFActivity { get; set; }
        public virtual T_WF_ACTIVITY_LEVEL WFLevel { get; set; }

        //public virtual IList<T_WF_ACTIVITY_LEVEL> ListLevel { get; set; }
    }
}
