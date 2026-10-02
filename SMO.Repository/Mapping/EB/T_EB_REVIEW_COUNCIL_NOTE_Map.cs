using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_COUNCIL_NOTE_Map : BaseMapping<T_EB_REVIEW_COUNCIL_NOTE>
    {
        public T_EB_REVIEW_COUNCIL_NOTE_Map()
        {
            Table("T_EB_REVIEW_COUNCIL_NOTE");
            Id(x => x.PKID);
            Map(x => x.REVIEW_CONTRACTOR_DETAIL_ID);
            Map(x => x.USER_SCORE);
            Map(x => x.NOTES);
        }
    }
}
