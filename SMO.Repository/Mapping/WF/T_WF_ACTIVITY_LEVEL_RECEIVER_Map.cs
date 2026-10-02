using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_WF_ACTIVITY_LEVEL_RECEIVER_Map : BaseMapping<T_WF_ACTIVITY_LEVEL_RECEIVER>
    {
        public T_WF_ACTIVITY_LEVEL_RECEIVER_Map()
        {
            Table("T_WF_ACTIVITY_LEVEL_RECEIVER");
            Id(x => x.PKID);
            Map(x => x.FK_SENDER).Not.Nullable();
            Map(x => x.USERNAMES).Not.Nullable();
            Map(x => x.NOTES).Nullable();
            HasMany(x => x.ListCom).KeyColumn("FK_RECEIVER").Inverse().Cascade.All();
        }
    }
}
