using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.AD;

namespace SMO.Repository.Mapping.AD
{
    public class T_AD_USER_PROJECT_Map : BaseMapping<T_AD_USER_PROJECT>
    {
        public T_AD_USER_PROJECT_Map()
        {
            Table("T_AD_USER_PROJECT");
            Id(x => x.PKID);
            Map(x => x.PROJECT_CODE);
            Map(x => x.USER_NAME);
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update();

        }
    }
}
