using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_NUMBER_OF_PROJECT_Map : BaseMapping<T_MD_NUMBER_OF_PROJECT>
    {
        public T_MD_NUMBER_OF_PROJECT_Map()
        {
            Table("T_MD_NUMBER_OF_PROJECT");
            Id(x => x.CODE);
            Map(x => x.TEXT);
            Map(x => x.TEXT_EN);
            Map(x => x.C_ORDER);
            Map(x => x.ACTIVE_TEXT).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
