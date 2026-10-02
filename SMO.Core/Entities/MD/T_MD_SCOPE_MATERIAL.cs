using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.MD
{
    public class T_MD_SCOPE_MATERIAL :BaseEntity
    {
        [Key] 
        public virtual string PKID { get; set; }
        public virtual string SCOPE_CODE { get; set; }
        public virtual string MATERIAL_CODE { get; set; }
       

        private T_MD_SCOPE _Scope;
        public virtual T_MD_SCOPE Scope
        {
            get
            {
                if (_Scope == null)
                {
                    _Scope = new T_MD_SCOPE();
                }
                return _Scope;
            }

            set
            {
                _Scope = value;
            }

        }

    }
}
