using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_SPEC_SCORE_Map : BaseMapping<T_EB_REVIEW_SPEC_SCORE>
    {
        public T_EB_REVIEW_SPEC_SCORE_Map()
        {
            Table("T_EB_REVIEW_SPEC_SCORE");
            Id(x => x.PKID);
            Map(x => x.SPEC_ID);
            Map(x => x.REVIEW_CONTRACTOR_DETAIL_ID);
            Map(x => x.SCORE);
            Map(x => x.USER_SCORE);
            Map(x => x.NOTES);
        }
    }
}
