using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_CONTRACTOR_QUESTION : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string CONTENTS { get; set; }
        public virtual string TYPE { get; set; }
        public virtual string TYPE_EMAIL { get; set; }
        public virtual bool? IS_BID_CLOSE { get; set; }
        public virtual bool IS_ANSWER { get; set; }
        public virtual bool IS_ALL { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual ISet<T_EB_BID_CONTRACTOR_ANSWER> ListAnswer { get; set; }
        public virtual ISet<T_EB_BID_CONTRACTOR_QUESTION_FILE> ListFile { get; set; }

        public T_EB_BID_CONTRACTOR_QUESTION()
        {
            ListAnswer = new HashSet<T_EB_BID_CONTRACTOR_ANSWER>(new List<T_EB_BID_CONTRACTOR_ANSWER>());
            ListFile = new HashSet<T_EB_BID_CONTRACTOR_QUESTION_FILE>(new List<T_EB_BID_CONTRACTOR_QUESTION_FILE>());
        }
    }
}
