using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_SUBMIT_Map : BaseMapping<T_EB_BID_SUBMIT>
    {
        public T_EB_BID_SUBMIT_Map()
        {
            Table("T_EB_BID_SUBMIT");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.BID_ID);
            Map(x => x.TYPE);
            Map(x => x.DESCRIPTION);
            HasMany(x => x.ListFile).KeyColumn("SUBMIT_ID").LazyLoad().Inverse().Cascade.Delete();
            References(x => x.Contractor).Column("CONTRACTOR_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
