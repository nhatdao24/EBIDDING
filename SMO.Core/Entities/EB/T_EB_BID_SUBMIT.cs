using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_SUBMIT : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string TYPE { get; set; }
        public virtual string DESCRIPTION { get; set; }
        public virtual bool IS_INPUT_SPEC { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual ISet<T_EB_BID_SUBMIT_FILE> ListFile { get; set; }

        public T_EB_BID_SUBMIT()
        {
            ListFile = new HashSet<T_EB_BID_SUBMIT_FILE>(new List<T_EB_BID_SUBMIT_FILE>());
        }
    }
}
