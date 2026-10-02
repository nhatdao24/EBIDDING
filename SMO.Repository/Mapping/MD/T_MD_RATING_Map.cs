using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_RATING_Map : BaseMapping<T_MD_RATING>
    {
        public T_MD_RATING_Map()
        {
            Table("T_MD_RATING");
            Id(x => x.PKID);
            Map(x => x.FROM_VALUE);
            Map(x => x.TO_VALUE);
            Map(x => x.RATING);
            Map(x => x.IS_BLACK_LIST).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
