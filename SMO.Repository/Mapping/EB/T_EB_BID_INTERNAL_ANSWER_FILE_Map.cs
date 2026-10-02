using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_INTERNAL_ANSWER_FILE_Map : BaseMapping<T_EB_BID_INTERNAL_ANSWER_FILE>
    {
        public T_EB_BID_INTERNAL_ANSWER_FILE_Map()
        {
            Table("T_EB_BID_INTERNAL_ANSWER_FILE");
            Id(x => x.FILE_ID);
            Map(x => x.ANSWER_ID);
            References(x => x.FileUpload)
                                .Column("FILE_ID")
                                .PropertyRef("REFERENCE_ID")
                                .Not.Insert()
                                .Not.Update()
                                .LazyLoad();
        }
    }
}
