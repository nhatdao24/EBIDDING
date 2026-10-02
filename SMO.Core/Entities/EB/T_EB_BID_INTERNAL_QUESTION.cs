using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_INTERNAL_QUESTION : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTENTS { get; set; }
        public virtual bool? IS_BID_CLOSE { get; set; }
        public virtual bool IS_ANSWER { get; set; }
        public virtual bool IS_SEND_MAIL { get; set; }
        public virtual ISet<T_EB_BID_INTERNAL_ANSWER> ListAnswer { get; set; }
        public virtual ISet<T_EB_BID_INTERNAL_QUESTION_FILE> ListFile { get; set; }

        public T_EB_BID_INTERNAL_QUESTION()
        {
            ListAnswer = new HashSet<T_EB_BID_INTERNAL_ANSWER>(new List<T_EB_BID_INTERNAL_ANSWER>());
            ListFile = new HashSet<T_EB_BID_INTERNAL_QUESTION_FILE>(new List<T_EB_BID_INTERNAL_QUESTION_FILE>());
        }
    }
}
