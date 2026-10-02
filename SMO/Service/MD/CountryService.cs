using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;

namespace SMO.Service.MD
{
    public class CountryService : GenericService<T_MD_COUNTRY, CountryRepo>
    {
        public CountryService() : base()
        {

        }
    }
}
