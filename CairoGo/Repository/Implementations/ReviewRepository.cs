using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Models.Responsemodel;
using CairoGo.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(CairoGoDbContext _Db) : base(_Db)
        {
        }

        public async Task<List<Review>> GetPlaceReviewsAsync(Guid placeId)
        {
            return await _DbSet.Where(p => p.PlaceId == placeId)
                               .Include(p => p.User)  //User Info
                               .OrderByDescending(r => r.CreatedAt)
                               .ToListAsync();
        }

        public async Task<Review?> GetUserReviewForPlaceAsync(Guid userId, Guid placeId)
        {
            return await _DbSet.FirstOrDefaultAsync(r => r.UserId == userId && r.PlaceId == placeId);
                
        }

        public async Task<List<Review>> GetUserReviewsAsync(Guid userId)
        {
            return await _DbSet.Where(r => r.UserId == userId)
                               .Include(r => r.Place) // place info
                               .OrderByDescending(r => r.CreatedAt)
                               .ToListAsync();
        }

        public async Task<bool> HasUserReviewedPlaceAsync(Guid userId, Guid placeId)
        {
            return await _DbSet.AnyAsync(r => r.UserId == userId && r.PlaceId == placeId);
        }

        public async Task<bool> IncrementHelpfulCountAsync(Guid reviewId)
        {
            var review = await _DbSet.FindAsync(reviewId);
            if (review == null)
                return false;

            review.HelpfulCount++;
            return true;
        }

        public async Task<bool> UpdateReviewAsync(Review review)
        {
            var existing = await _DbSet.FindAsync(review.ReviewId);
            if (existing == null)
                return false;
            existing.Rating = review.Rating;
            existing.Comment = review.Comment;
            existing.PhotosJson = review.PhotosJson;
            existing.VisitDate = review.VisitDate;

            return true;
        }
        public async Task<PlaceRatingStats> GetPlaceRatingStatsAsync(Guid placeId)
        {
            var reviews = await _DbSet.Where(r => r.PlaceId == placeId)
                                      .Select(r => r.Rating)
                                      .ToListAsync();
            if (!reviews.Any())
            {
                return new PlaceRatingStats
                {
                    AverageRating = 0,
                    TotalReviews = 0
                };
            }

            return new PlaceRatingStats
            {
                AverageRating = reviews.Average(),
                TotalReviews = reviews.Count,
                FiveStars = reviews.Count(r => r >= 4.5f),
                FourStars = reviews.Count(r => r >= 3.5f && r < 4.5f),
                ThreeStars = reviews.Count(r => r >= 2.5f && r < 3.5f),
                TwoStars = reviews.Count(r => r >= 1.5f && r < 2.5f),
                OneStar = reviews.Count(r => r < 1.5f)
            };
        }
    }

}
