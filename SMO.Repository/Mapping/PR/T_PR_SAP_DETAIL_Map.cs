using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_PR_SAP_DETAIL_Map : BaseMapping<T_PR_SAP_DETAIL>
    {
        public T_PR_SAP_DETAIL_Map()
        {
            Table("T_PR_SAP_DETAIL");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.ITEM_NUMBER);
            Map(x => x.MATERIAL_CODE);
            Map(x => x.MATERIAL_NAME);
            Map(x => x.QUANTITY);
            Map(x => x.UNIT);
            Map(x => x.PRICE);
            Map(x => x.SUM_VALUE);
            Map(x => x.CURRENCY);
        }
    }
}
