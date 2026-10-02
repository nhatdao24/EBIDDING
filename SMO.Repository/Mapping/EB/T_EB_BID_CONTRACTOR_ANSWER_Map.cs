using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_CONTRACTOR_ANSWER_Map : BaseMapping<T_EB_BID_CONTRACTOR_ANSWER>
    {
        public T_EB_BID_CONTRACTOR_ANSWER_Map()
        {
            Table("T_EB_BID_CONTRACTOR_ANSWER");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.QUESTION_ID);
            Map(x => x.CONTENTS);
            HasMany(x => x.ListFile).KeyColumn("ANSWER_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.Contractor).Column("CONTRACTOR_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
