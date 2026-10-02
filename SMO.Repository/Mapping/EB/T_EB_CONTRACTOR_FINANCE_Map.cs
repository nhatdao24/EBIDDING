using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_FINANCE_Map : BaseMapping<T_EB_CONTRACTOR_FINANCE>
    {
        public T_EB_CONTRACTOR_FINANCE_Map()
        {
            Table("T_EB_CONTRACTOR_FINANCE");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.YEAR);
            Map(x => x.DOANH_THU);
            Map(x => x.DOANH_THU_CURRENCY);
            Map(x => x.LOI_NHUAN_RONG);
            Map(x => x.LOI_NHUAN_RONG_CURRENCY);
            Map(x => x.LOI_NHUAN_GOP);
            Map(x => x.LOI_NHUAN_GOP_CURRENCY);
            Map(x => x.VON_CSH);
            Map(x => x.VON_CSH_CURRENCY);
            Map(x => x.FILE_ID);
            References(x => x.File)
                    .Column("FILE_ID")
                    .PropertyRef("REFERENCE_ID")
                    .Not.Insert()
                    .Not.Update()
                    .LazyLoad();
        }
    }
}
