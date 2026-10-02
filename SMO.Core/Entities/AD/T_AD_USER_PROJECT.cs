using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.AD
{
    public class T_AD_USER_PROJECT : BaseEntity
    {
        public virtual string PKID { get; set; }

        public virtual string PROJECT_CODE { get; set; }
        public virtual string USER_NAME { get; set; }

        private T_AD_USER _User;
        public virtual T_AD_USER User
        {
            get
            {
                if (_User == null)
                {
                    _User = new T_AD_USER();
                }
                return _User;
            }

            set
            {
                _User = value;
            }

        }

    }
}
