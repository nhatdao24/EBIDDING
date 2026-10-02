using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_FINAL_Map : BaseMapping<T_EB_REVIEW_FINAL>
    {
        public T_EB_REVIEW_FINAL_Map()
        {
            Table("T_EB_REVIEW_FINAL");
            Id(x => x.CODE);
            Map(x => x.DESCRIPTION);
            Map(x => x.FROM_DATE);
            Map(x => x.TO_DATE);
            Map(x => x.USER_APPROVE);
            Map(x => x.STATUS);
            Map(x => x.APPROVE_DATE);
            HasMany(x => x.ListUser).KeyColumn("REVIEW_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListHistory).KeyColumn("REVIEW_ID").Inverse().Cascade.Delete();
            HasMany(x => x.ListContractor).KeyColumn("REVIEW_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.UserCreate).Column("CREATE_BY").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
