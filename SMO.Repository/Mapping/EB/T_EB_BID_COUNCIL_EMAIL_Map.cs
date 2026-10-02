using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_COUNCIL_EMAIL_Map : BaseMapping<T_EB_BID_COUNCIL_EMAIL>
    {
        public T_EB_BID_COUNCIL_EMAIL_Map()
        {
            Table("T_EB_BID_COUNCIL_EMAIL");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.USER_NAME);
            Map(x => x.EMAIL_ID);
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Email).Column("EMAIL_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
