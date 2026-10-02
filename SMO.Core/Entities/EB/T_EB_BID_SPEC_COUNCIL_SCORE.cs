using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_SPEC_COUNCIL_SCORE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string SPEC_ID { get; set; }
        public virtual int SCORE { get; set; }
        public virtual string NOTES { get; set; }
        public virtual string CONTENTS { get; set; }
        public virtual string USER_SCORE { get; set; }
        public virtual int SCORE_MULTIPLY { get; set; }
        public virtual string VALUE_OF_SPEC { get; set; }
    }
}
