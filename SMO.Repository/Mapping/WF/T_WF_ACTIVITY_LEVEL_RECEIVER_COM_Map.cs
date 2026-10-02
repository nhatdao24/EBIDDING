using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.MD
{
    public class T_WF_ACTIVITY_LEVEL_RECEIVER_COM_Map : BaseMapping<T_WF_ACTIVITY_LEVEL_RECEIVER_COM>
    {
        public T_WF_ACTIVITY_LEVEL_RECEIVER_COM_Map()
        {
            Table("T_WF_ACTIVITY_LEVEL_RECEIVER_COM");
            Id(x => x.PKID);
            Map(x => x.FK_RECEIVER).Not.Nullable();
            Map(x => x.TYPE).Not.Nullable();
            Map(x => x.SUBJECT).Not.Nullable();
            Map(x => x.CONTENTS).Not.Nullable();
            Map(x => x.ACTIVE).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
