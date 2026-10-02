using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_INTERNAL_ANSWER : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string PARENT_ID { get; set; }
        public virtual string QUESTION_ID { get; set; }
        public virtual string CONTENTS { get; set; }
        public virtual string USER_REPLY { get; set; }
        public virtual bool IS_SEND_MAIL { get; set; }
        public virtual string BID_ID { get; set; }

        public virtual ISet<T_EB_BID_INTERNAL_ANSWER_FILE> ListFile { get; set; }

        public T_EB_BID_INTERNAL_ANSWER()
        {
            ListFile = new HashSet<T_EB_BID_INTERNAL_ANSWER_FILE>(new List<T_EB_BID_INTERNAL_ANSWER_FILE>());
        }
    }
}
