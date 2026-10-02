using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_OPTION_Map : BaseMapping<T_MD_OPTION>
    {
        public T_MD_OPTION_Map()
        {
            Table("T_MD_OPTION");
            Id(x => x.CODE);
            Map(x => x.TEXT);
            Map(x => x.TEXT_EN);
            Map(x => x.C_ORDER);
            Map(x => x.ACTIVE_TEXT).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
