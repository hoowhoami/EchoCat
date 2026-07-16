namespace EchoCat.Desktop.Services;

public interface ILocalSettingsService
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);

    Task<PetWindowPlacement?> LoadPlacementAsync(CancellationToken cancellationToken = default);

    Task SavePlacementAsync(PetWindowPlacement placement, CancellationToken cancellationToken = default);

    Task SaveActiveSkinAsync(string skinId, CancellationToken cancellationToken = default);

    Task SaveTopmostAsync(bool isTopmost, CancellationToken cancellationToken = default);
}
