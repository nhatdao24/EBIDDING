using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_REVIEW_FINAL_NOTE_Map : BaseMapping<T_EB_REVIEW_FINAL_NOTE>
    {
        public T_EB_REVIEW_FINAL_NOTE_Map()
        {
            Table("T_EB_REVIEW_FINAL_NOTE");
            Id(x => x.PKID);
            Map(x => x.HEADER_ID);
            Map(x => x.NOTES);
        }
    }
}
