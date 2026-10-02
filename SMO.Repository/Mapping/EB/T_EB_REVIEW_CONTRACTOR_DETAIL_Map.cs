using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_CONTRACTOR_DETAIL_Map : BaseMapping<T_EB_REVIEW_CONTRACTOR_DETAIL>
    {
        public T_EB_REVIEW_CONTRACTOR_DETAIL_Map()
        {
            Table("T_EB_REVIEW_CONTRACTOR_DETAIL");
            Id(x => x.PKID);
            Map(x => x.HEADER_ID);
            Map(x => x.INFO_BID);
            Map(x => x.INFO_BID_EBID);
            Map(x => x.MATERIAL_TYPE);
            Map(x => x.CONG_VIEC);
            Map(x => x.GIA_TRI);
            Map(x => x.RATIO_IN_PROJECT);
            Map(x => x.TOTAL_SCORE);
            Map(x => x.MEDIUM_SCORE);
            Map(x => x.RATE);
        }
    }
}
