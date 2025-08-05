using System;
using DataAccess.Repository.IRepository;

namespace Utility
{
    public class StreakManager
    {
        private readonly IUnitOfWork _unitOfWork;

        public StreakManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void UpdateStreak(UserStats userStats)
        {
            var today = DateTime.Today;

            if (userStats.LastActivityDate == null)
            {
                userStats.CurrentStreak = 1;
            }
            else
            {
                var last = userStats.LastActivityDate.Value.Date;

                if (last == today)
                {
                    // Already updated today — do nothing
                    return;
                }
                else if (last == today.AddDays(-1))
                {
                    userStats.CurrentStreak += 1;
                }
                else
                {
                    userStats.CurrentStreak = 1;
                }
            }
            if (userStats.CurrentStreak > userStats.LongestStreak)
            {
                userStats.LongestStreak = userStats.CurrentStreak;
            }

            userStats.LastActivityDate = today;

            _unitOfWork.UserStats.Update(userStats);
            _unitOfWork.Save();
        }
    }
}
