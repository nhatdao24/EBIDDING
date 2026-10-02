using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.RQ
{
    public class T_RQ_REQUEST_Map : BaseMapping<T_RQ_REQUEST>
    {
        public T_RQ_REQUEST_Map()
        {
            Table("T_RQ_REQUEST");
            Id(x => x.PKID);
            Map(x => x.USER_REQUEST);
            Map(x => x.REQUEST_DATE);
            Map(x => x.STATUS);
            Map(x => x.REQUEST_PARENT);
            HasMany(x => x.ListHistory).KeyColumn("REQUEST_ID").Inverse().Cascade.All();
            HasOne(x => x.RequestData).PropertyRef(x => x.Request);
        }
    }
}
