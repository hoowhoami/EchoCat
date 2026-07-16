namespace EchoCat.AI;

public sealed record OpenAiCompanionOptions(
    string ApiKey,
    string Model,
    string BaseUrl,
    string Instructions)
{
    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
