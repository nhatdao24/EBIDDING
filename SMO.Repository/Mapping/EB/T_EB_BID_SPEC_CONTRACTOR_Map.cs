using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_SPEC_CONTRACTOR_Map : BaseMapping<T_EB_BID_SPEC_CONTRACTOR>
    {
        public T_EB_BID_SPEC_CONTRACTOR_Map()
        {
            Table("T_EB_BID_SPEC_CONTRACTOR");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.SPEC_ID);
            Map(x => x.FIRST_VALUE);
            Map(x => x.VALUE);
        }
    }
}
