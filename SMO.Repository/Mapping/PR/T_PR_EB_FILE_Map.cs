using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.PR;

namespace SMO.Repository.Mapping.PR
{
    public class T_PR_EB_FILE_Map : BaseMapping<T_PR_EB_FILE>
    {
        public T_PR_EB_FILE_Map()
        {
            Table("T_PR_EB_FILE");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.FILE_ID);
            References(x => x.File)
                                .Column("FILE_ID")
                                .PropertyRef("REFERENCE_ID")
                                .Not.Insert()
                                .Not.Update()
                                .LazyLoad();
        }

    }
}
