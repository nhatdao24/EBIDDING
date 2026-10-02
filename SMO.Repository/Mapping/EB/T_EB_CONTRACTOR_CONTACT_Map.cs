using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_CONTACT_Map : BaseMapping<T_EB_CONTRACTOR_CONTACT>
    {
        public T_EB_CONTRACTOR_CONTACT_Map()
        {
            Table("T_EB_CONTRACTOR_CONTACT");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.COUNTRY_CODE);
            Map(x => x.FULL_NAME);
            Map(x => x.EMAIL);
            Map(x => x.PHONE);
            Map(x => x.NOTES);
            Map(x => x.IS_DEFAULT).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
