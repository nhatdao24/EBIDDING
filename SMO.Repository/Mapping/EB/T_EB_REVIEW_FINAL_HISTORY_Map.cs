using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_FINAL_HISTORY_Map : BaseMapping<T_EB_REVIEW_FINAL_HISTORY>
    {
        public T_EB_REVIEW_FINAL_HISTORY_Map()
        {
            Table("T_EB_REVIEW_FINAL_HISTORY");
            Id(x => x.PKID);
            Map(x => x.REVIEW_ID);
            Map(x => x.STATUS);
            Map(x => x.NOTES);
            Map(x => x.STATUS_DATE);
        }
    }
}
