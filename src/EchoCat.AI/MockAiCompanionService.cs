namespace EchoCat.AI;

public sealed class MockAiCompanionService : IAiCompanionService
{
    public Task<CompanionResponse> ReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var text = string.IsNullOrWhiteSpace(userMessage)
            ? "你可以把想法丢给我，我会帮你接住。"
            : $"我听到啦：{userMessage.Trim()}。第一版我先用本地脑袋陪你，之后再接真正的 AI。";

        return Task.FromResult(new CompanionResponse(text));
    }
}
