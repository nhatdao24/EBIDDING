using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.MD
{
    [Serializable]
    public class T_MD_TITLE : BaseEntity
    {
        public virtual int PKID { get; set; }

        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string NAME { get; set; }
        public virtual string NAME_EN { get; set; }
    }
}
