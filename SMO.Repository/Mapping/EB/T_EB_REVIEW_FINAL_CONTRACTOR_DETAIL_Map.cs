using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL_Map : BaseMapping<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>
    {
        public T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL_Map()
        {
            Table("T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL");
            Id(x => x.PKID);
            Map(x => x.HEADER_ID);
            Map(x => x.REVIEW_FINAL_ID);
            Map(x => x.REVIEW_ID);
            Map(x => x.REVIEW_CONTRACTOR_DETAIL_ID);
            Map(x => x.ORG_CODE);
            Map(x => x.PROJECT_CODE);
            Map(x => x.INFO_BID);
            Map(x => x.INFO_BID_EBID);
            Map(x => x.MATERIAL_TYPE);
            Map(x => x.CONG_VIEC);
            Map(x => x.GIA_TRI);
            Map(x => x.RATIO_IN_PROJECT);
            Map(x => x.SCORE);
            Map(x => x.RATE);
            References(x => x.Org).Column("ORG_CODE").Not.Insert().Not.Update().LazyLoad();
            References(x => x.Project).Column("PROJECT_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
