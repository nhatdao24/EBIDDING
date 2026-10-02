using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Core.Entities.MD;
using SMO.Repository.Common;
using SMO.Repository.Interface.AD;
using SMO.Repository.Interface.MD;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SMO.Repository.Implement.MD
{
    public class MaterialRepo : GenericRepository<T_MD_MATERIAL>, IMaterialRepo
    {
        public MaterialRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_MD_MATERIAL> Search(T_MD_MATERIAL objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_MD_MATERIAL>();

            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.IsLike($"%{objFilter.CODE}%") ||
                                         x.TEXT.IsLike($"%{objFilter.CODE}%"));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.TYPE))
            {
                query = query.Where(x => x.TYPE == objFilter.TYPE);
            }

            if(objFilter.IS_PUBLIC == true)
            {
                query = query.Where(x => x.IS_PUBLIC == false);
            }
            query = query.OrderBy(x => x.CODE).Asc;

            query = query.TransformUsing(Transformers.DistinctRootEntity);

            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
