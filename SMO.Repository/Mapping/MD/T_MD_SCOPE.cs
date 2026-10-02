using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_SCOPE_Map : BaseMapping<T_MD_SCOPE>
    {
        public T_MD_SCOPE_Map()
        {
            Table("T_MD_SCOPE");
            Id(x => x.CODE);
            Map(x => x.TYPE_CODE);
            Map(x => x.TEXT);
            Map(x => x.TEXT_EN);
            Map(x => x.ACTIVE_TEXT).Not.Nullable().CustomType<YesNoType>();
            References(x => x.SCOPE_TYPE).Column("TYPE_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
