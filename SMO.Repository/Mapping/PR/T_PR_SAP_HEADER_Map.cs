using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_PR_SAP_HEADER_Map : BaseMapping<T_PR_SAP_HEADER>
    {
        public T_PR_SAP_HEADER_Map()
        {
            Table("T_PR_SAP_HEADER");
            Id(x => x.CODE);
            Map(x => x.COMPANY_NAME);
            Map(x => x.REQUIRE_NAME);
            Map(x => x.REQUIRE_USER);
            Map(x => x.DEPARTMENT_NAME);
            Map(x => x.REQUIRE_DATE);
            Map(x => x.DESTINATION_DATE);
            Map(x => x.IS_SELECTED).Not.Nullable().CustomType<YesNoType>();
            HasMany(x => x.ListDetail).KeyColumn("HEADER_CODE").Inverse().Cascade.All();
            HasMany(x => x.ListBidRequirePR).KeyColumn("PR_CODE").Inverse().Cascade.All();
        }
    }
}
