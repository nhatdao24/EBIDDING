using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_PR_EB_DETAIL_Map : BaseMapping<T_PR_EB_DETAIL>
    {
        public T_PR_EB_DETAIL_Map()
        {
            Table("T_PR_EB_DETAIL");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.ITEM_NUMBER);
            Map(x => x.MATERIAL_CODE);
            Map(x => x.QUANTITY);
            Map(x => x.UNIT_CODE);
            Map(x => x.PRICE);
            Map(x => x.SUM_VALUE);
            Map(x => x.CURRENCY_CODE);
            Map(x => x.SCOPE_CODE);
            Map(x => x.TIME_RENT);
            Map(x => x.RENT_UNIT_CODE);
            Map(x => x.NOTE);
            References(x => x.Material).Column("MATERIAL_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Unit).Column("UNIT_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.RentUnit).Column("RENT_UNIT_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Scope).Column("SCOPE_CODE").Not.Insert().Not.Update().LazyLoad().Nullable();
            //References(x => x.File).Column("FILE_ID").Not.Insert().Not.Update().LazyLoad();


        }
    }
}
