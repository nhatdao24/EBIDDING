using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Type;
using SMO.Core.Entities.MD;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_TITLE_Map : BaseMapping<T_MD_TITLE>
    {
        public T_MD_TITLE_Map() 
        {
            Table("T_MD_TITLE");
            Id(x => x.PKID);
            Map(x => x.NAME).Nullable();
            Map(x => x.NAME_EN).Nullable();
        }
    }
}
