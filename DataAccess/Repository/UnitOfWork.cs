using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public  IUserRepository User {  get; set; }
        public IUserStatsRepository UserStats { get; set; }
        public IFlashcardRepository Flashcard {get; set;}
        public IStudySessionRepository StudySession { get; set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new UserRepository(_db);
            UserStats = new UserStatsRepository(_db);
            Flashcard = new FlashcardRepository(_db);
            StudySession = new StudySessionRepository(_db);

        }
        public void Save()
        {
            _db.SaveChanges();
        }
        public async Task<int> SaveAsync()
        {
            return await _db.SaveChangesAsync();
        }

    }
}
