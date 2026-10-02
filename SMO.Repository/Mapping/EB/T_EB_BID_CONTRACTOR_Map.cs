using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_CONTRACTOR_Map : BaseMapping<T_EB_BID_CONTRACTOR>
    {
        public T_EB_BID_CONTRACTOR_Map()
        {
            Table("T_EB_BID_CONTRACTOR");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.STATUS);
            Map(x => x.PRICE);
            Map(x => x.CURRENCY_CODE);
            Map(x => x.EMAIL_SENT_ID);
            Map(x => x.PERCENT_DEBUT);
            Map(x => x.PERCENT_WINNING);
            Map(x => x.IS_SELECTED).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_WINNING).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_SELF_JOIN).Not.Nullable().CustomType<YesNoType>();
            References(x => x.Contractor).Column("CONTRACTOR_ID").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Bid).Column("BID_ID").Not.Insert().Not.Update().LazyLoad();
            References(x => x.EmailSent).Column("EMAIL_SENT_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
