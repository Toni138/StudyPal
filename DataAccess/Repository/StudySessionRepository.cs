using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using MyModels;

namespace DataAccess.Repository
{
    public class StudySessionRepository : Repository<StudySession>, IStudySessionRepository
    {
        private readonly ApplicationDbContext _db;
        public StudySessionRepository(ApplicationDbContext db) : base(db)
        {

            _db = db;

        }

    }
}
