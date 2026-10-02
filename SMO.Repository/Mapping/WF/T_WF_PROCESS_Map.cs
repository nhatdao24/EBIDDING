using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_WF_PROCESS_Map : BaseMapping<T_WF_PROCESS>
    {
        public T_WF_PROCESS_Map()
        {
            Table("T_WF_PROCESS");
            Id(x => x.CODE);
            Map(x => x.NAME).Not.Nullable();
            Map(x => x.NOTES).Nullable();
            Map(x => x.ACTIVE).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListActivity).KeyColumn("FK_PROCESS").Inverse().Cascade.All();
        }
    }
}
