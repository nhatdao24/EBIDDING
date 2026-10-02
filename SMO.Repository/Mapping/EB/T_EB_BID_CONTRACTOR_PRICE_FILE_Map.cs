using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_CONTRACTOR_PRICE_FILE_Map : BaseMapping<T_EB_BID_CONTRACTOR_PRICE_FILE>
    {
        public T_EB_BID_CONTRACTOR_PRICE_FILE_Map()
        {
            Table("T_EB_BID_CONTRACTOR_PRICE_FILE");
            Id(x => x.PKID);
            Map(x => x.BID_ID).Not.Update();
            Map(x => x.CONTRACTOR_ID).Not.Update();
            Map(x => x.FILE_ID);
            Map(x => x.TYPE).Not.Update();
            References(x => x.FileUpload)
                                .Column("FILE_ID")
                                .PropertyRef("REFERENCE_ID")
                                .Not.Insert()
                                .Not.Update()
                                .LazyLoad();
        }
    }
}
