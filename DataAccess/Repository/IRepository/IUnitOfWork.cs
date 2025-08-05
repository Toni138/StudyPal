using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public  interface IUnitOfWork
    {
        IUserRepository User { get; }
        IUserStatsRepository UserStats { get; }
        IFlashcardRepository Flashcard { get; }
        IStudySessionRepository StudySession { get; }
        void Save();
        Task<int> SaveAsync();
    }
}
