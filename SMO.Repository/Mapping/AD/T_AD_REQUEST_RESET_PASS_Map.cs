using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.AD
{
    public class T_AD_REQUEST_RESET_PASS_Map : BaseMapping<T_AD_REQUEST_RESET_PASS>
    {
        public T_AD_REQUEST_RESET_PASS_Map()
        {
            Table("T_AD_REQUEST_RESET_PASS");
            Id(x => x.PKID);
            Map(x => x.USER_NAME).Not.Nullable().Not.Update();
            Map(x => x.REQUEST_DATE).Not.Nullable().Not.Update();
        }
    }
}
