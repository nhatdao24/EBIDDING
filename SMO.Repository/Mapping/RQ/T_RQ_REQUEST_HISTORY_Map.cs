using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.RQ
{
    public class T_RQ_REQUEST_HISTORY_Map : BaseMapping<T_RQ_REQUEST_HISTORY>
    {
        public T_RQ_REQUEST_HISTORY_Map()
        {
            Table("T_RQ_REQUEST_HISTORY");
            Id(x => x.PKID);
            Map(x => x.REQUEST_ID);
            Map(x => x.USERNAME);
            Map(x => x.ACTIVITY);
            Map(x => x.PROCESS);
            Map(x => x.C_LEVEL);
            Map(x => x.HISTORY_DATE);
            Map(x => x.COMMENTS);
            Map(x => x.APPROVE_TYPE);
            References(x => x.WFProcess).Column("PROCESS").Not.Insert().Not.Update().LazyLoad();
            References(x => x.WFActivity).Column("ACTIVITY").Not.Insert().Not.Update().LazyLoad();
            References(x => x.WFLevel).Column("C_LEVEL").Not.Insert().Not.Update().LazyLoad();

        }
    }
}
