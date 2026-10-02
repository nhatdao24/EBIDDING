using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_PR_EB_HEADER_Map : BaseMapping<T_PR_EB_HEADER>
    {
        public T_PR_EB_HEADER_Map()
        {
            Table("T_PR_EB_HEADER");
            Id(x => x.CODE);
            Map(x => x.REQUIRE_NAME);
            Map(x => x.COMPANY_CODE).Not.Update();
            Map(x => x.REQUIRE_DATE);
            Map(x => x.DESTINATION_DATE);
            Map(x => x.STATUS);
            Map(x => x.DESCRIPTION);
            Map(x => x.COMMENT_ID);
            Map(x => x.USER_APPROVE);
            Map(x => x.REASON);
            Map(x => x.PACKAGE_CODE);
            Map(x => x.PROJECT_CODE);
            Map(x => x.SCOPE_CODE);
            Map(x => x.BID_TYPE_CODE);
            Map(x => x.SCOPE_TYPE_CODE);
            Map(x => x.IS_SELECTED).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_RENT).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListDetail).KeyColumn("HEADER_CODE").Inverse().Cascade.All();
            HasMany(x => x.ListBidRequirePR).KeyColumn("PR_CODE").Inverse().Cascade.All();
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
