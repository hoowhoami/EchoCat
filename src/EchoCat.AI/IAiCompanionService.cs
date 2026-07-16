namespace EchoCat.AI;

public interface IAiCompanionService
{
    Task<CompanionResponse> ReplyAsync(string userMessage, CancellationToken cancellationToken = default);
}
