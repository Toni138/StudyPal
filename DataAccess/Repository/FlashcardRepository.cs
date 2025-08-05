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
    public class FlashcardRepository : Repository<Flashcard>, IFlashcardRepository
    {
        private readonly ApplicationDbContext _db;
        public FlashcardRepository(ApplicationDbContext db) : base(db)
        {

            _db = db;

        }

    }
}
