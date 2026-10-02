using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_CONTRACTOR_EMAIL_Map : BaseMapping<T_EB_BID_CONTRACTOR_EMAIL>
    {
        public T_EB_BID_CONTRACTOR_EMAIL_Map()
        {
            Table("T_EB_BID_CONTRACTOR_EMAIL");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.EMAIL_ID);
            Map(x => x.TYPE).Nullable();
            References(x => x.Contractor).Column("CONTRACTOR_ID").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Email).Column("EMAIL_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
