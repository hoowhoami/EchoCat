namespace EchoCat.Desktop.Services;

public static class AiProviderIds
{
    public const string Mock = "mock";

    public const string OpenAi = "openai";
}

public static class AiSettingsDefaults
{
    public const string Instructions = "你是 EchoCat，一只软萌、克制、会陪伴用户的桌面猫。用简短、温柔、自然的中文回答，避免长篇说教。";
}

public sealed record AiSettings(
    string Provider = AiProviderIds.Mock,
    string Endpoint = "",
    string Model = "",
    string ApiKey = "",
    string Instructions = "");

public sealed record AppSettings(
    PetWindowPlacement? WindowPlacement = null,
    string? ActiveSkinId = null,
    bool IsTopmost = true,
    bool ShowWelcomeBubble = true,
    bool StartHidden = false,
    AiSettings Ai = null!)
{
    public AppSettings()
        : this(null, null, true, true, false, new AiSettings())
    {
    }
}
