using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_SPEC_COUNCIL_SCORE_Map : BaseMapping<T_EB_BID_SPEC_COUNCIL_SCORE>
    {
        public T_EB_BID_SPEC_COUNCIL_SCORE_Map()
        {
            Table("T_EB_BID_SPEC_COUNCIL_SCORE");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.SPEC_ID);
            Map(x => x.SCORE);
            Map(x => x.NOTES);
            Map(x => x.CONTENTS);
            Map(x => x.USER_SCORE);
        }
    }
}
