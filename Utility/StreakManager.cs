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

        public void UpdateStreak(UserStats userStats, bool? hasReviewedFlashcardsToday, bool? hasTakenStudySessionToday)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var lastActivityDate = userStats.LastActivityDate?.Date;

            // If already updated today, do nothing
            if (lastActivityDate == today)
                return;

            bool didSomethingToday =  (hasReviewedFlashcardsToday ?? false) || (hasTakenStudySessionToday ?? false);


            if (didSomethingToday)
            {
                if (lastActivityDate == yesterday)
                {
                    userStats.CurrentStreak += 1;
                }
                else
                {
                    if (userStats.CurrentStreak > userStats.LongestStreak)
                        userStats.LongestStreak = userStats.CurrentStreak;

                    userStats.CurrentStreak = 1;
                }

                userStats.LastActivityDate = today;
            }
            else
            {
                // User hasn’t done anything today, check if yesterday was inactive too
                if (lastActivityDate != yesterday)
                {
                    if (userStats.CurrentStreak > userStats.LongestStreak)
                        userStats.LongestStreak = userStats.CurrentStreak;

                    userStats.CurrentStreak = 0;
                }

                // Do not update LastActivityDate here — no activity today
                return;
            }

            _unitOfWork.UserStats.Update(userStats);
            _unitOfWork.Save();
        }
    }
    }
