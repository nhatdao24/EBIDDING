using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.RQ
{
    public class T_RQ_REQUEST_TO_Map : BaseMapping<T_RQ_REQUEST_TO>
    {
        public T_RQ_REQUEST_TO_Map()
        {
            Table("T_RQ_REQUEST_TO");
            Id(x => x.PKID);
            Map(x => x.REQUEST_ID);
            Map(x => x.TO_DATE);
            Map(x => x.STATUS);
            Map(x => x.USERNAMES);
            Map(x => x.PROCESS);
            Map(x => x.ACTIVITY);
            Map(x => x.C_LEVEL);
            References(x => x.Request).Column("REQUEST_ID").Not.Insert().Not.Update().LazyLoad();
            References(x => x.WFProcess).Column("PROCESS").Not.Insert().Not.Update().LazyLoad();
            References(x => x.WFActivity).Column("ACTIVITY").Not.Insert().Not.Update().LazyLoad();
            References(x => x.WFLevel).Column("C_LEVEL").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
