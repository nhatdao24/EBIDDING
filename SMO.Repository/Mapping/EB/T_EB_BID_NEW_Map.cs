using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_NEW_Map : BaseMapping<T_EB_BID_NEW>
    {
        public T_EB_BID_NEW_Map()
        {
            Table("T_EB_BID_NEW");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.TIEU_DE);
            Map(x => x.MO_TA_NGAN);
            Map(x => x.MO_TA_NGAN_EN);
            Map(x => x.IMAGE_PATH);
            Map(x => x.BAI_VIET).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.BAI_VIET_EN).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.IS_PUBLIC).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
