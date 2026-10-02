using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_RQ_REQUEST : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string USER_REQUEST { get; set; }
        public virtual DateTime REQUEST_DATE { get; set; }
        public virtual string STATUS { get; set; }
        public virtual string REQUEST_PARENT { get; set; }
        public virtual IList<T_RQ_REQUEST_HISTORY> ListHistory { get; set; }
        public virtual T_RQ_REQUEST_DATA RequestData { get; set; }

        public T_RQ_REQUEST()
        {
        }
    }
}
