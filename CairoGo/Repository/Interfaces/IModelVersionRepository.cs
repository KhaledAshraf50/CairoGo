using CairoGo.Models.Entity;
using CairoGo.Models.Responsemodel;

namespace CairoGo.Repository.Interfaces
{
    public interface IModelVersionRepository : IBaseRepo<ModelVersion>
    {
        // Get active model version
        Task<ModelVersion?> GetActiveModelAsync();

        // Get all model versions
        Task<List<ModelVersion>> GetAllModelsAsync();

        // Get model by name
        Task<ModelVersion?> GetModelByNameAsync(string name);

        // Get models by type
        Task<List<ModelVersion>> GetModelsByTypeAsync(string type);

        // Activate model (deactivate others)
        Task<bool> ActivateModelAsync(Guid modelId);

        // Deprecate model
        Task<bool> DeprecateModelAsync(Guid modelId);

        // Update model metrics
        Task<bool> UpdateModelMetricsAsync(Guid modelId, float ctr, float conversion);

        // Increment prediction count
        Task<bool> IncrementPredictionCountAsync(Guid modelId);

        // Get model performance stats
        Task<ModelPerformanceStats> GetModelPerformanceAsync(Guid modelId);
    }
}
