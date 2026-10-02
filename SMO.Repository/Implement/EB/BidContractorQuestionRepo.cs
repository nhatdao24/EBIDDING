using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorQuestionRepo : GenericRepository<T_EB_BID_CONTRACTOR_QUESTION>, IBidContractorQuestionRepo
    {
        public BidContractorQuestionRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_CONTRACTOR_QUESTION> Search(T_EB_BID_CONTRACTOR_QUESTION objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_CONTRACTOR_QUESTION>();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID || x.IS_ALL);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.TYPE))
            {
                query = query.Where(x => x.TYPE == objFilter.TYPE);
            }

            if (objFilter.IS_BID_CLOSE.HasValue)
            {
                query = query.Where(x => x.IS_BID_CLOSE == objFilter.IS_BID_CLOSE);
            }

            //query = query.Fetch(x => x.ListAnswer).Eager
            //    .Fetch(x => x.ListFile).Eager;

            query = query
                .Fetch(x => x.Contractor).Eager
                .Fetch(x => x.ListAnswer).Eager
                .Fetch(x => x.ListAnswer.First().ListFile).Eager
                .Fetch(x => x.ListAnswer.First().Contractor).Eager
                .Fetch(x => x.ListAnswer.First().ListFile.First().FileUpload).Eager
                .Fetch(x => x.ListFile).Eager
                .Fetch(x => x.ListFile.First().FileUpload).Eager;

            query = query.TransformUsing(Transformers.DistinctRootEntity);
            total = 0;
            return query.List().ToList();
        }

        public List<T_EB_BID_CONTRACTOR_QUESTION> SearchTopQuestion(List<string> lstBidID)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_CONTRACTOR_QUESTION>();
            query = query.Where(x => x.BID_ID.IsIn(lstBidID));

            query = query
               .Fetch(x => x.Contractor).Eager
               .Fetch(x => x.ListAnswer).Eager
               .Fetch(x => x.ListAnswer.First().ListFile).Eager
               .Fetch(x => x.ListAnswer.First().Contractor).Eager
               .Fetch(x => x.ListAnswer.First().ListFile.First().FileUpload).Eager
               .Fetch(x => x.ListFile).Eager
               .Fetch(x => x.ListFile.First().FileUpload).Eager;

            query = query.TransformUsing(Transformers.DistinctRootEntity);

            return query.List().ToList();
        }

        public List<T_EB_BID_CONTRACTOR_QUESTION> SearchTopQuestionContractor(List<string> lstBidID, string contractorId)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_CONTRACTOR_QUESTION>();
            query = query.Where(x => x.BID_ID.IsIn(lstBidID) && (x.CONTRACTOR_ID == contractorId || x.IS_ALL));

            query = query
               .Fetch(x => x.Contractor).Eager
               .Fetch(x => x.ListAnswer).Eager
               .Fetch(x => x.ListAnswer.First().ListFile).Eager
               .Fetch(x => x.ListAnswer.First().Contractor).Eager
               .Fetch(x => x.ListAnswer.First().ListFile.First().FileUpload).Eager
               .Fetch(x => x.ListFile).Eager
               .Fetch(x => x.ListFile.First().FileUpload).Eager;

            query = query.TransformUsing(Transformers.DistinctRootEntity);

            return query.List().ToList();
        }
    }
}
