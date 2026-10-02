using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_PACKAGE_Map : BaseMapping<T_MD_PACKAGE>
    {
        public T_MD_PACKAGE_Map()
        {
            Table("T_MD_PACKAGE");
            Id(x => x.CODE);
            Map(x => x.TEXT);
        }
    }
}

