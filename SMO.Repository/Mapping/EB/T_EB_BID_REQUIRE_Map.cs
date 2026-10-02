using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_REQUIRE_Map : BaseMapping<T_EB_BID_REQUIRE>
    {
        public T_EB_BID_REQUIRE_Map()
        {
            Table("T_EB_BID_REQUIRE");
            Id(x => x.CODE);
            Map(x => x.COMPANY_CODE).Not.Update();
            Map(x => x.NAME);
            Map(x => x.DESCRIPTION);
            Map(x => x.STATUS);
            Map(x => x.BID_ID);
            Map(x => x.COMMENT_ID);
            Map(x => x.USER_APPROVE);
            Map(x => x.REASON);
            Map(x => x.PACKAGE_CODE);
            Map(x => x.PROJECT_CODE);
            Map(x => x.SCOPE_CODE);
            Map(x => x.BID_TYPE_CODE);
            Map(x => x.SCOPE_TYPE_CODE);
            Map(x => x.NOTES).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.NOTES_EN).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.IS_RENT).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListUser).KeyColumn("HEADER_CODE").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListDetail).KeyColumn("HEADER_CODE").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListPR).KeyColumn("HEADER_CODE").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListFile).KeyColumn("HEADER_CODE").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListHistory).KeyColumn("HEADER_CODE").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.UserApprove).Column("USER_APPROVE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.UserSent).Column("CREATE_BY").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Package).Column("PACKAGE_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Project).Column("PROJECT_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Scope).Column("SCOPE_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.ScopeType).Column("SCOPE_TYPE_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.BidType).Column("BID_TYPE_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
