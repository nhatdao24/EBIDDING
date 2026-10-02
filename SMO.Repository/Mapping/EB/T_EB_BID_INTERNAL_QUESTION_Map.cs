using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_INTERNAL_QUESTION_Map : BaseMapping<T_EB_BID_INTERNAL_QUESTION>
    {
        public T_EB_BID_INTERNAL_QUESTION_Map()
        {
            Table("T_EB_BID_INTERNAL_QUESTION");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CONTENTS).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.IS_BID_CLOSE).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_ANSWER).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.IS_SEND_MAIL).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListAnswer).KeyColumn("QUESTION_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListFile).KeyColumn("QUESTION_ID").LazyLoad().Inverse().Cascade.Delete();
        }
    }
}
