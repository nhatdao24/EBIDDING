using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_LEADERSHIP_COMMENTS : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual string CONTENTS { get; set; }
        public virtual T_AD_USER User { get; set; }

        public T_EB_BID_LEADERSHIP_COMMENTS()
        {
            User = new T_AD_USER();
        }
    }
}
