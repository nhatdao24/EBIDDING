using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_PROJECT_Map : BaseMapping<T_MD_PROJECT>
    {
        public T_MD_PROJECT_Map()
        {
            Table("T_MD_PROJECT");
            Id(x => x.CODE);
            Map(x => x.NAME);
            Map(x => x.NAME_EN);
            Map(x => x.SAP_CODE);
            Map(x => x.GIAM_DOC);
            Map(x => x.DIA_DIEM);
            Map(x => x.DIA_DIEM_EN);
            Map(x => x.GIA_TRI_HOP_DONG);
            Map(x => x.GIA_TRI_THUC_HIEN);
            Map(x => x.TIEN_DO);
            Map(x => x.NGAY_BAT_DAU);
            Map(x => x.NGAY_KET_THUC);
            Map(x => x.NGUOI_THEO_DOI);
            Map(x => x.DON_VI);
            HasMany(x => x.ListUserProjects).KeyColumn("PROJECT_CODE").LazyLoad().Inverse().Cascade.Delete();
        }
    }
}
