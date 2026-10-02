using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_FINAL_USER_Map : BaseMapping<T_EB_REVIEW_FINAL_USER>
    {
        public T_EB_REVIEW_FINAL_USER_Map()
        {
            Table("T_EB_REVIEW_FINAL_USER");
            Id(x => x.PKID);
            Map(x => x.REVIEW_ID);
            Map(x => x.USER_NAME);
            Map(x => x.C_LEVEL);
            Map(x => x.IS_APPROVED).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.ROLE);
            Map(x => x.DATE_APPROVE);
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
