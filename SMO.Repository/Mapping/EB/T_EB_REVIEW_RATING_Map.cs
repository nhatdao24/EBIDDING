using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_RATING_Map : BaseMapping<T_EB_REVIEW_RATING>
    {
        public T_EB_REVIEW_RATING_Map()
        {
            Table("T_EB_REVIEW_RATING");
            Id(x => x.PKID);
            Map(x => x.REVIEW_ID);
            Map(x => x.FROM_VALUE);
            Map(x => x.TO_VALUE);
            Map(x => x.RATING);
            Map(x => x.C_ORDER);
        }
    }
}
