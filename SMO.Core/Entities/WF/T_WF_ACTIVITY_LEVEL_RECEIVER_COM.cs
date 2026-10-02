using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_WF_ACTIVITY_LEVEL_RECEIVER_COM : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string FK_RECEIVER { get; set; }
        public virtual string TYPE { get; set; }
        public virtual string SUBJECT { get; set; }
        public virtual string CONTENTS { get; set; }
    }
}
