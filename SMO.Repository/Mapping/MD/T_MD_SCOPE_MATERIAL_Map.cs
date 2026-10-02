using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Type;
using SMO.Core.Entities.MD;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_SCOPE_MATERIAL_Map : BaseMapping<T_MD_SCOPE_MATERIAL>
    {
        public T_MD_SCOPE_MATERIAL_Map()
        {
            Table("T_MD_SCOPE_MATERIAL");
            Id(x => x.PKID);
            Map(x => x.SCOPE_CODE);
            Map(x => x.MATERIAL_CODE);
            References(x => x.Scope).Column("SCOPE_CODE").Not.Insert().Not.Update();

        }

    }
}
