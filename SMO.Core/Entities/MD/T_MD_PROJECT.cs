using SharpSapRfc;
using SMO.Core.Entities.AD;
using SMO.Core.Entities.MD;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_MD_PROJECT : BaseEntity
    {

        public virtual string CODE { get; set; }

        [Required(ErrorMessage = "Trường này bắt buộc nhập (required)", AllowEmptyStrings = false)]
        [StringLength(500, ErrorMessage = "Tên dự án không vượt quá 500 ký tự.")]
        public virtual string NAME { get; set; }
        public virtual string NAME_EN { get; set; }

        public virtual string SAP_CODE { get; set; }

        public virtual string GIAM_DOC { get; set; }

        public virtual string DIA_DIEM { get; set; }
        public virtual string DIA_DIEM_EN { get; set; }

        public virtual decimal GIA_TRI_HOP_DONG { get; set; }

        public virtual decimal GIA_TRI_THUC_HIEN { get; set; }

        public virtual string TIEN_DO { get; set; }
        public virtual DateTime? NGAY_BAT_DAU { get; set; }
        public virtual DateTime? NGAY_KET_THUC { get; set; }

        public virtual string NGUOI_THEO_DOI { get; set; }

        public virtual IList<T_AD_USER> ListUser { get; set; }

        public virtual string DON_VI { get; set; }
        public virtual IList<T_AD_USER_PROJECT> ListUserProjects { get; set; }
        public T_MD_PROJECT()
        {
            ListUserProjects = new List<T_AD_USER_PROJECT>();
            ListUser = new List<T_AD_USER>();
        }
    }
}
