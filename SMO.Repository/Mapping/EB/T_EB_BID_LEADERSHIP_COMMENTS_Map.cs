using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_LEADERSHIP_COMMENTS_Map : BaseMapping<T_EB_BID_LEADERSHIP_COMMENTS>
    {
        public T_EB_BID_LEADERSHIP_COMMENTS_Map()
        {
            Table("T_EB_BID_LEADERSHIP_COMMENTS");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.USER_NAME);
            Map(x => x.CONTENTS);
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
