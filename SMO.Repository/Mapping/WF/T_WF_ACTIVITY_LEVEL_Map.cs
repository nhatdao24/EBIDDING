using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_WF_ACTIVITY_LEVEL_Map : BaseMapping<T_WF_ACTIVITY_LEVEL>
    {
        public T_WF_ACTIVITY_LEVEL_Map()
        {
            Table("T_WF_ACTIVITY_LEVEL");
            Id(x => x.PKID);
            Map(x => x.FK_ACTIVITY).Not.Nullable();
            Map(x => x.NAME).Not.Nullable();
            Map(x => x.C_ORDER).Nullable();
            Map(x => x.NOTES).Nullable();
            HasMany(x => x.ListSender).KeyColumn("FK_LEVEL").Inverse().Cascade.All();
        }
    }
}
