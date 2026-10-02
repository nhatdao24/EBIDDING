using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REQUEST_REVENUE_Map : BaseMapping<T_EB_REQUEST_REVENUE>
    {
        public T_EB_REQUEST_REVENUE_Map()
        {
            Table("T_EB_REQUEST_REVENUE");
            Id(x => x.PKID);
            Map(x => x.REQUEST_ID);
            Map(x => x.YEAR);
            Map(x => x.VALUE);
            Map(x => x.UNIT_CODE);
        }
    }
}
