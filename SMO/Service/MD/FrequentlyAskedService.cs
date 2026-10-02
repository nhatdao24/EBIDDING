using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;

namespace SMO.Service.MD
{
    public class FrequentlyAskedService : GenericService<T_FAQ_FREQUENTLY_ASKED, FrequentlyAskedRepo>
    {
        public T_FAQ_QUESTION ObjQuestion { get; set; }
        public FrequentlyAskedService() : base()
        {
            ObjQuestion = new T_FAQ_QUESTION();
        }


        public void CreateQuestion()
        {
            try
            {
                this.ObjQuestion.PKID = Guid.NewGuid().ToString();
                UnitOfWork.BeginTransaction();
                UnitOfWork.Repository<FAQQuestionRepo>().Create(this.ObjQuestion);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Create()
        {
            this.ObjDetail.PKID = Guid.NewGuid().ToString();
            base.Create();
        }
    }
}
