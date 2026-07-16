using EchoCat.AI;

namespace EchoCat.Desktop.Services;

public sealed class SettingsBackedAiCompanionService : IAiCompanionService
{
    private readonly HttpClient _http = new();
    private readonly MockAiCompanionService _mock = new();
    private readonly ILocalSettingsService _settings;

    public SettingsBackedAiCompanionService(ILocalSettingsService settings)
    {
        _settings = settings;
    }

    public async Task<CompanionResponse> ReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var settings = await _settings.LoadAsync(cancellationToken);
        var ai = settings.Ai;

        if (ai.Provider == AiProviderIds.Mock)
        {
            return await _mock.ReplyAsync(userMessage, cancellationToken);
        }

        if (!IsOpenAiComplete(ai))
        {
            return new CompanionResponse("请先在设置里完整填写 OpenAI endpoint、model 和 API Key。", "setup");
        }

        var instructions = string.IsNullOrWhiteSpace(ai.Instructions)
            ? AiSettingsDefaults.Instructions
            : ai.Instructions.Trim();

        return await new OpenAiCompanionService(
            _http,
            new OpenAiCompanionOptions(
                ApiKey: ai.ApiKey.Trim(),
                Model: ai.Model.Trim(),
                BaseUrl: ai.Endpoint.Trim(),
                Instructions: instructions))
            .ReplyAsync(userMessage, cancellationToken);
    }

    private static bool IsOpenAiComplete(AiSettings ai)
    {
        return !string.IsNullOrWhiteSpace(ai.Endpoint)
            && !string.IsNullOrWhiteSpace(ai.Model)
            && !string.IsNullOrWhiteSpace(ai.ApiKey);
    }
}
