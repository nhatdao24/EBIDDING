using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.RQ
{
    public class T_RQ_REQUEST_DATA_Map : BaseMapping<T_RQ_REQUEST_DATA>
    {
        public T_RQ_REQUEST_DATA_Map()
        {
            Table("T_RQ_REQUEST_DATA");
            Id(x => x.PKID);
            Map(x => x.REQUEST_ID);
            Map(x => x.OLD_DATA);
            Map(x => x.NEW_DATA);
            References(x => x.Request).Column("REQUEST_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
