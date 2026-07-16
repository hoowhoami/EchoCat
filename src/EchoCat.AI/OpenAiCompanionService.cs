using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EchoCat.AI;

public sealed class OpenAiCompanionService : IAiCompanionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly OpenAiCompanionOptions _options;

    public OpenAiCompanionService(HttpClient http, OpenAiCompanionOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<CompanionResponse> ReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        if (!_options.HasApiKey)
        {
            return new CompanionResponse("请先在设置里填写 OpenAI API Key。", "setup");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildResponsesUri())
            {
                Content = JsonContent.Create(
                    new ResponsesRequest(_options.Model, _options.Instructions, userMessage),
                    options: JsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new CompanionResponse($"AI 请求暂时失败：{(int)response.StatusCode}。请检查模型名、额度或 API Key。", "error");
            }

            var payload = await response.Content.ReadFromJsonAsync<ResponsesApiResponse>(JsonOptions, cancellationToken);
            var text = payload?.ExtractText();
            return string.IsNullOrWhiteSpace(text)
                ? new CompanionResponse("我收到了回应，但没读到文字内容。", "error")
                : new CompanionResponse(text.Trim());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new CompanionResponse("我现在连不上 AI 服务，先陪你安静待一会儿。", "error");
        }
    }

    private Uri BuildResponsesUri()
    {
        var root = _options.BaseUrl.TrimEnd('/') + "/";
        return new Uri(new Uri(root), "responses");
    }

    private sealed record ResponsesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("instructions")] string Instructions,
        [property: JsonPropertyName("input")] string Input);

    private sealed record ResponsesApiResponse(
        [property: JsonPropertyName("output_text")] string? OutputText,
        [property: JsonPropertyName("output")] IReadOnlyList<ResponseOutputItem>? Output)
    {
        public string? ExtractText()
        {
            if (!string.IsNullOrWhiteSpace(OutputText))
            {
                return OutputText;
            }

            return Output?
                .SelectMany(item => item.Content ?? [])
                .Select(content => content.Text)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
        }
    }

    private sealed record ResponseOutputItem(
        [property: JsonPropertyName("content")] IReadOnlyList<ResponseContentItem>? Content);

    private sealed record ResponseContentItem(
        [property: JsonPropertyName("text")] string? Text);
}
