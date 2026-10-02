using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_Map : BaseMapping<T_EB_BID>
    {
        public T_EB_BID_Map()
        {
            Table("T_EB_BID");
            Id(x => x.PKID);
            Map(x => x.PARENT_BID);
            Map(x => x.COMPANY_CODE).Not.Update();
            Map(x => x.TEN_GOI_THAU);
            Map(x => x.TEN_GOI_THAU_KD);
            Map(x => x.TEN_GOI_THAU_EN);
            Map(x => x.NGAY_MO_THAU);
            Map(x => x.NGAY_DONG_THAU);
            Map(x => x.NGAY_KT_CHAM_THAU);
            Map(x => x.MO_TA).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.MO_TA_KD).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.MO_TA_EN).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.REASON).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.TYPE);
            Map(x => x.REQUIRED_CODE);
            Map(x => x.STATUS);
            Map(x => x.GIAI_DOAN);
            Map(x => x.COMMENT_ID);
            Map(x => x.USER_APPROVE);
            Map(x => x.USER_MANAGE);
            Map(x => x.IS_PUBLIC).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_BROADCAST_MAIL).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_MULTIL_ROUND).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.ROUND);
            Map(x => x.PROJECT_CODE);
            Map(x => x.CLOSE_DATE);
            Map(x => x.PACKAGE_CODE);
            Map(x => x.SCOPE_CODE);
            Map(x => x.SCOPE_TYPE_CODE);
            Map(x => x.TOTAL_PRICE);
            Map(x => x.BID_TYPE);
            HasMany(x => x.ListFile).KeyColumn("BID_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListNew).KeyColumn("BID_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListContractor).KeyColumn("BID_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListHistory).KeyColumn("BID_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.BidType).Column("TYPE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Project).Column("PROJECT_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.UserApprove).Column("USER_APPROVE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.UserSent).Column("CREATE_BY").Not.Insert().Not.Update().LazyLoad();
            References(x => x.UserManage).Column("USER_MANAGE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Package).Column("PACKAGE_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Scope).Column("SCOPE_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.ScopeType).Column("SCOPE_TYPE_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
