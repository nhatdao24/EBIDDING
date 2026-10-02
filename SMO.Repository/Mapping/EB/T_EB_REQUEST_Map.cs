using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REQUEST_Map : BaseMapping<T_EB_REQUEST>
    {
        public T_EB_REQUEST_Map()
        {
            Table("T_EB_REQUEST");
            Id(x => x.PKID);
            Map(x => x.DMS);
            Map(x => x.MA_SO_THUE);
            Map(x => x.TEN_NGAN);
            Map(x => x.TEN_DAI);
            Map(x => x.DIA_CHI);
            Map(x => x.DIA_CHI_VP);
            Map(x => x.LOAI_HINH_DOANH_NGHIEP);
            Map(x => x.QUOC_GIA);
            Map(x => x.SO_LUONG_NHAN_VIEN);
            Map(x => x.VON_DIEU_LE);
            Map(x => x.VON_DIEU_LE_UNIT);
            Map(x => x.NGUOI_DAI_DIEN);
            Map(x => x.WEBSITE);
            Map(x => x.EMAIL);
            Map(x => x.PHONE);
            Map(x => x.FILE_GIOI_THIEU_ID);
            Map(x => x.FILE_HO_SO_ID);
            Map(x => x.LOGO_PATH);
            Map(x => x.COMMENT);
            Map(x => x.STATUS);
            Map(x => x.LINH_VUC_CHINH);
            Map(x => x.LINH_VUC_CHINH_TEXT);
            Map(x => x.ACTIVE_AREA);
            Map(x => x.SO_LUONG_DU_AN);
            Map(x => x.SO_LUONG_DU_AN_TEXT);
            Map(x => x.TUY_CHON);
            Map(x => x.TUY_CHON_TEXT);
            Map(x => x.BID_ID);
            Map(x => x.NOTES);
            Map(x => x.NGUOI_NHAN_THONG_BAO);
            HasMany(x => x.ListRevenue).KeyColumn("REQUEST_ID").LazyLoad().Inverse().NotFound.Ignore().Cascade.Delete();
            HasMany(x => x.ListScope).KeyColumn("REQUEST_ID").LazyLoad().Inverse().NotFound.Ignore().Cascade.Delete();
            References(x => x.Country).Column("QUOC_GIA").Not.Insert().Not.Update().LazyLoad();
            References(x => x.FileImport).Column("FILE_HO_SO_ID").Not.Insert().Not.Update().LazyLoad();
            References(x => x.NumberOfProject).Column("SO_LUONG_DU_AN").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Option).Column("TUY_CHON").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
