using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_WF_ACTIVITY_LEVEL_SENDER_Map : BaseMapping<T_WF_ACTIVITY_LEVEL_SENDER>
    {
        public T_WF_ACTIVITY_LEVEL_SENDER_Map()
        {
            Table("T_WF_ACTIVITY_LEVEL_SENDER");
            Id(x => x.PKID);
            Map(x => x.FK_LEVEL);
            Map(x => x.USERNAMES);
            Map(x => x.TYPE);
            Map(x => x.NOTES).Nullable();
            HasMany(x => x.ListReceiver).KeyColumn("FK_SENDER").Inverse().Cascade.All();
        }
    }
}
