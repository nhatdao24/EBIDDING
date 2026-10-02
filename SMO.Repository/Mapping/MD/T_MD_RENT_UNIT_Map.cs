using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Conventions.Helpers;
using SMO.Core.Entities.MD;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_RENT_UNIT_Map : BaseMapping<T_MD_RENT_UNIT>
    {
        public T_MD_RENT_UNIT_Map()
        {
            Table("T_MD_RENT_UNIT");
            Id(x => x.CODE); 
            Map(x => x.TEXT);
            Map(x => x.ACTIVE);

        }
    }
}
