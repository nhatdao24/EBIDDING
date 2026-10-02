using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_WF_ACTIVITY_Map : BaseMapping<T_WF_ACTIVITY>
    {
        public T_WF_ACTIVITY_Map()
        {
            Table("T_WF_ACTIVITY");
            Id(x => x.PKID);
            Map(x => x.FK_PROCESS);
            Map(x => x.CODE);
            Map(x => x.NAME);
            Map(x => x.TYPE);
            Map(x => x.C_ORDER);
            Map(x => x.NOTES);
            HasMany(x => x.ListLevel).KeyColumn("FK_ACTIVITY").Inverse().Cascade.All();
        }
    }
}
