using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_SPEC_COUNCIL_Map : BaseMapping<T_EB_BID_SPEC_COUNCIL>
    {
        public T_EB_BID_SPEC_COUNCIL_Map()
        {
            Table("T_EB_BID_SPEC_COUNCIL");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.SPEC_ID);
            Map(x => x.VALUE);
            Map(x => x.USER_SCORE);
        }
    }
}
