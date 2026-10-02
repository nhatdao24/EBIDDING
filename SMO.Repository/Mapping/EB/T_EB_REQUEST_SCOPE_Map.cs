using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REQUEST_SCOPE_Map : BaseMapping<T_EB_REQUEST_SCOPE>
    {
        public T_EB_REQUEST_SCOPE_Map()
        {
            Table("T_EB_REQUEST_SCOPE");
            Id(x => x.PKID);
            Map(x => x.REQUEST_ID);
            Map(x => x.SCOPE_CODE);
            References(x => x.Scope).Column("SCOPE_CODE").Not.Insert().Not.Update().LazyLoad().NotFound.Ignore();
        }
    }
}
