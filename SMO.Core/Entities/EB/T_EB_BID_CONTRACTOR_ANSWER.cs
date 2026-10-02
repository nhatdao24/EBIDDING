using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_CONTRACTOR_ANSWER : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string QUESTION_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string CONTENTS { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual ISet<T_EB_BID_CONTRACTOR_ANSWER_FILE> ListFile { get; set; }

        public T_EB_BID_CONTRACTOR_ANSWER()
        {
            ListFile = new HashSet<T_EB_BID_CONTRACTOR_ANSWER_FILE>(new List<T_EB_BID_CONTRACTOR_ANSWER_FILE>());
        }
    }
}
