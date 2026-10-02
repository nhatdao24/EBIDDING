using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_RQ_REQUEST_DATA : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REQUEST_ID { get; set; }
        public virtual string OLD_DATA { get; set; }
        public virtual string NEW_DATA { get; set; }
        public virtual T_RQ_REQUEST Request { get; set; }
    }
}
