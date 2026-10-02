using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_SPEC_Map : BaseMapping<T_EB_BID_SPEC>
    {
        public T_EB_BID_SPEC_Map()
        {
            Table("T_EB_BID_SPEC");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.PARENT_ID);
            Map(x => x.NAME);
            Map(x => x.NAME_EN);
            Map(x => x.DESCRIPTION);
            Map(x => x.DESCRIPTION_EN);
            Map(x => x.TYPE);
            Map(x => x.C_ORDER);
            Map(x => x.RATIO);
            Map(x => x.IS_SPEC_PRICE).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_NOT_SHOW_CONTRACTOR).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.ROLE);
            Map(x => x.MD_PKID);
            HasMany(x => x.ListDetail).KeyColumn("HEADER_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.Dictionary).Column("ROLE").Not.Insert().Not.Update().LazyLoad();

        }
    }
}
