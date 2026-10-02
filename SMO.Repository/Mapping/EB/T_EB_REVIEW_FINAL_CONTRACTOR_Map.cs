using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_FINAL_CONTRACTOR_Map : BaseMapping<T_EB_REVIEW_FINAL_CONTRACTOR>
    {
        public T_EB_REVIEW_FINAL_CONTRACTOR_Map()
        {
            Table("T_EB_REVIEW_FINAL_CONTRACTOR");
            Id(x => x.PKID);
            Map(x => x.REVIEW_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.SCORE);
            Map(x => x.RATE);
            Map(x => x.NOTES);
            References(x => x.Contractor).Column("CONTRACTOR_ID").Not.Insert().Not.Update().LazyLoad();
            References(x => x.ReviewFinal).Column("REVIEW_ID").Not.Insert().Not.Update().LazyLoad();
            HasMany(x => x.ListDetail).KeyColumn("HEADER_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListNote).KeyColumn("HEADER_ID").LazyLoad().Inverse().Cascade.Delete();
        }
    }
}
