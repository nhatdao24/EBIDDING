using SharpSapRfc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SMO.Core.Entities
{
    public partial class T_CF_TEMPLATE_NOTIFY : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string INVITE_SUBJECT { get; set; }
        public virtual string INVITE_BODY { get; set; }
        public virtual string NEXT_ROUND_SUBJECT { get; set; }
        public virtual string NEXT_ROUND_BODY { get; set; }
        public virtual string INVITE_COUNCIL_SUBJECT { get; set; }
        public virtual string INVITE_COUNCIL_BODY { get; set; }
        public virtual string SCORE_SUBJECT { get; set; }
        public virtual string SCORE_BODY { get; set; }
        public virtual string WIN_SUBJECT { get; set; }
        public virtual string WIN_BODY { get; set; }
        public virtual string FAIL_SUBJECT { get; set; }
        public virtual string FAIL_BODY { get; set; }
        public virtual string ACCOUNT_SUBJECT { get; set; }
        public virtual string ACCOUNT_BODY { get; set; }
        public virtual string REQUEST_ACCOUNT_SUBJECT { get; set; }
        public virtual string REQUEST_ACCOUNT_BODY { get; set; }
        public virtual string REJECT_REQUEST_ACCOUNT_SUBJECT { get; set; }
        public virtual string REJECT_REQUEST_ACCOUNT_BODY { get; set; }

        public virtual string CONG_VIEC_XU_LY_SUBJECT { get; set; }
        public virtual string CONG_VIEC_XU_LY_BODY { get; set; }
        public virtual string CONG_VIEC_HOAN_THANH_SUBJECT { get; set; }
        public virtual string CONG_VIEC_HOAN_THANH_BODY { get; set; }
        public virtual string FEED_BACK_SUBJECT { get; set; }
        public virtual string FEED_BACK_BODY { get; set; }

        public virtual string UPDATE_INFO_BID_SUBJECT { get; set; }
        public virtual string UPDATE_INFO_BID_BODY { get; set; }
        public virtual string RESET_PASS_SUBJECT { get; set; }
        public virtual string RESET_PASS_BODY { get; set; }

        public virtual string THUONG_THAO_LAI_SUBJECT { get; set; }
        public virtual string THUONG_THAO_LAI_BODY { get; set; }


        public virtual string THANKS_SUBJECT { get; set; }
        public virtual string THANKS_BODY { get; set; }

        public virtual string KET_THUC_THUONG_THAO_SUBJECT { get; set; }
        public virtual string KET_THUC_THUONG_THAO_BODY { get; set; }

        public virtual string Y_KIEN_LANH_DAO_SUBJECT { get; set; }
        public virtual string Y_KIEN_LANH_DAO_BODY { get; set; }

        public virtual string RESET_PASS_CONTRACTOR_SUBJECT { get; set; }
        public virtual string RESET_PASS_CONTRACTOR_BODY { get; set; }

        public virtual string BID_DESCRIPTION {  get; set; }
        public virtual string BID_DESCRIPTION_EN {  get; set; }
    }
}
