using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_Map : BaseMapping<T_EB_REVIEW>
    {
        public T_EB_REVIEW_Map()
        {
            Table("T_EB_REVIEW");
            Id(x => x.CODE);
            Map(x => x.DESCRIPTION);
            Map(x => x.ORG_CODE);
            Map(x => x.PROJECT_CODE);
            Map(x => x.EVALUATE_DATE);
            Map(x => x.END_DATE);
            Map(x => x.MAX_SCORE);
            Map(x => x.STATUS);
            HasMany(x => x.ListContractor).KeyColumn("REVIEW_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListCouncil).KeyColumn("REVIEW_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.Organize).Column("ORG_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Project).Column("PROJECT_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
