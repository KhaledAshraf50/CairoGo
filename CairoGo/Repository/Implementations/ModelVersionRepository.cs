using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Models.Responsemodel;
using CairoGo.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class ModelVersionRepository : Repository<ModelVersion>, IModelVersionRepository
        {
            public ModelVersionRepository(CairoGoDbContext _Db) : base(_Db)
            {
            }

            public async Task<ModelVersion?> GetActiveModelAsync()
            {
                return await _DbSet.FirstOrDefaultAsync(m => m.IsActive);
            }

            public async Task<List<ModelVersion>> GetAllModelsAsync()
            {
                return await _DbSet.OrderByDescending(m => m.DeployedAt)
                                   .ToListAsync();
            }

            public async Task<ModelVersion?> GetModelByNameAsync(string name)
            {
                return await _DbSet.FirstOrDefaultAsync(m => m.Name == name);
            }

            public async Task<List<ModelVersion>> GetModelsByTypeAsync(string type)
            {
                return await _DbSet.Where(m => m.Type == type)
                                   .OrderByDescending(m => m.DeployedAt)
                                   .ToListAsync();
            }

            public async Task<bool> ActivateModelAsync(Guid modelId)
            {
                var model = await _DbSet.FindAsync(modelId);
                if (model == null)
                    return false;

                // Deactivate all other models
                var activeModels = await _DbSet.Where(m => m.IsActive).ToListAsync();
                foreach (var activeModel in activeModels)
                {
                    activeModel.IsActive = false;
                }

                // Activate the selected model
                model.IsActive = true;

                return true;
            }

            public async Task<bool> DeprecateModelAsync(Guid modelId)
            {
                var model = await _DbSet.FindAsync(modelId);
                if (model == null)
                    return false;

                model.DeprecatedAt = DateTime.UtcNow;
                model.IsActive = false;

                return true;
            }

            public async Task<bool> UpdateModelMetricsAsync(Guid modelId, float ctr, float conversion)
            {
                var model = await _DbSet.FindAsync(modelId);
                if (model == null)
                    return false;

                model.AverageCTR = ctr;
                model.AverageConversion = conversion;

                return true;
            }

            public async Task<bool> IncrementPredictionCountAsync(Guid modelId)
            {
                var model = await _DbSet.FindAsync(modelId);
                if (model == null)
                    return false;

                model.PredictionCount++;

                return true;
            }

            public async Task<ModelPerformanceStats> GetModelPerformanceAsync(Guid modelId)
            {
                var model = await _DbSet.FindAsync(modelId);

                if (model == null)
                {
                    return new ModelPerformanceStats();
                }

                return new ModelPerformanceStats
                {
                    ModelName = model.Name,
                    ModelType = model.Type,
                    AverageCTR = model.AverageCTR,
                    AverageConversion = model.AverageConversion,
                    TotalPredictions = model.PredictionCount,
                    DeployedAt = model.DeployedAt,
                    IsActive = model.IsActive
                };
            }
        }
    
}
