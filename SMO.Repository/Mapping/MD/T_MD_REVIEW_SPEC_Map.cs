using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_REVIEW_SPEC_Map : BaseMapping<T_MD_REVIEW_SPEC>
    {
        public T_MD_REVIEW_SPEC_Map()
        {
            Table("T_MD_REVIEW_SPEC");
            Id(x => x.PKID);
            Map(x => x.PARENT_ID);
            Map(x => x.NAME);
            Map(x => x.NAME_EN);
            Map(x => x.DESCRIPTION);
            Map(x => x.DESCRIPTION_EN);
            Map(x => x.C_ORDER);
            Map(x => x.RATIO);
            Map(x => x.MAX_SCORE);
            Map(x => x.VAI_TRO);
            Map(x => x.IS_DEFAULT).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListDetail).KeyColumn("HEADER_ID").LazyLoad().Inverse().Cascade.Delete();
        }
    }
}
