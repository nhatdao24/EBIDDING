using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_PROJECT_Map : BaseMapping<T_EB_CONTRACTOR_PROJECT>
    {
        public T_EB_CONTRACTOR_PROJECT_Map()
        {
            Table("T_EB_CONTRACTOR_PROJECT");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.TEN_DU_AN);
            Map(x => x.MO_TA);
            Map(x => x.CHU_DAU_TU);
            Map(x => x.GIA_TRI_CURRENCY);
            Map(x => x.BAT_DAU);
            Map(x => x.KET_THUC);
            Map(x => x.LOAI_VON);
        }
    }
}
