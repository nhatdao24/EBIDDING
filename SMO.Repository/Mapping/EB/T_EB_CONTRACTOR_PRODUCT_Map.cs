using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_PRODUCT_Map : BaseMapping<T_EB_CONTRACTOR_PRODUCT>
    {
        public T_EB_CONTRACTOR_PRODUCT_Map()
        {
            Table("T_EB_CONTRACTOR_PRODUCT");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.PRODUCT_CODE);
            Map(x => x.DESCRIPTION);
            References(x => x.Product).Column("PRODUCT_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
