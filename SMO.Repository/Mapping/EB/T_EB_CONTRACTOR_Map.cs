using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_Map : BaseMapping<T_EB_CONTRACTOR>
    {
        public T_EB_CONTRACTOR_Map()
        {
            Table("T_EB_CONTRACTOR");
            Id(x => x.PKID);
            Map(x => x.USER_NAME);
            Map(x => x.MA_SO_THUE);
            Map(x => x.DMS);
            Map(x => x.TEN_NGAN);
            Map(x => x.TEN_DAI);
            Map(x => x.DIA_CHI);
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
            Map(x => x.LOGO_PATH);
            Map(x => x.REQUEST_DATE);
            Map(x => x.LINH_VUC_CHINH);
            Map(x => x.LINH_VUC_CHINH_TEXT);
            Map(x => x.XEP_HANG);
            Map(x => x.DIEM_DANH_GIA);
            Map(x => x.NGAY_DANH_GIA);
            Map(x => x.REVIEW_FINAL_CODE);
            Map(x => x.GHI_CHU);
            Map(x => x.ACTIVE_AREA);
            Map(x => x.SO_LUONG_DU_AN);
            Map(x => x.SO_LUONG_DU_AN_TEXT);
            Map(x => x.TUY_CHON);
            Map(x => x.TUY_CHON_TEXT);
            Map(x => x.NGUOI_NHAN_THONG_BAO);
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Country).Column("QUOC_GIA").Not.Insert().Not.Update().LazyLoad();
            References(x => x.FileLogo)
                              .Column("LOGO_PATH")
                              .PropertyRef("REFERENCE_ID")
                              .Not.Insert()
                              .Not.Update()
                              .LazyLoad();
            References(x => x.FileGioiThieu)
                                .Column("FILE_GIOI_THIEU_ID")
                                .PropertyRef("REFERENCE_ID")
                                .Not.Insert()
                                .Not.Update()
                                .LazyLoad();
            References(x => x.NumberOfProject).Column("SO_LUONG_DU_AN").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Option).Column("TUY_CHON").Not.Insert().Not.Update().LazyLoad();
            HasMany(x => x.ListMarket).KeyColumn("CONTRACTOR_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListProduct).KeyColumn("CONTRACTOR_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListScope).KeyColumn("CONTRACTOR_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListContact).KeyColumn("CONTRACTOR_ID").LazyLoad().Inverse().Cascade.Delete();
            HasMany(x => x.ListRevenue).KeyColumn("CONTRACTOR_ID").LazyLoad().Inverse().Cascade.Delete();
        }
    }
}
