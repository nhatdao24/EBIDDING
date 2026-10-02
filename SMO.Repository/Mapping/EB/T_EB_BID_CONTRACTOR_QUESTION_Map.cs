using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_CONTRACTOR_QUESTION_Map : BaseMapping<T_EB_BID_CONTRACTOR_QUESTION>
    {
        public T_EB_BID_CONTRACTOR_QUESTION_Map()
        {
            Table("T_EB_BID_CONTRACTOR_QUESTION");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.CONTENTS);
            Map(x => x.TYPE);
            Map(x => x.IS_BID_CLOSE).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_ANSWER).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_ALL).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListAnswer).KeyColumn("QUESTION_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListFile).KeyColumn("QUESTION_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.Contractor).Column("CONTRACTOR_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
