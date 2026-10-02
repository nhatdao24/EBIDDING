using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_INTERNAL_ANSWER_Map : BaseMapping<T_EB_BID_INTERNAL_ANSWER>
    {
        public T_EB_BID_INTERNAL_ANSWER_Map()
        {
            Table("T_EB_BID_INTERNAL_ANSWER");
            Id(x => x.PKID);
            Map(x => x.PARENT_ID);
            Map(x => x.QUESTION_ID);
            Map(x => x.CONTENTS);
            Map(x => x.USER_REPLY);
            Map(x => x.IS_SEND_MAIL).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListFile).KeyColumn("ANSWER_ID").LazyLoad().Inverse().Cascade.Delete();
        }
    }
}
