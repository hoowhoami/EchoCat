using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace EchoCat.Desktop.Services;

public sealed class SqliteLocalSettingsService : ILocalSettingsService
{
    private readonly string _connectionString;

    public SqliteLocalSettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var root = Path.Combine(appData, "EchoCat");
        Directory.CreateDirectory(root);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(root, "settings.db")
        }.ToString();
    }

    public async Task<PetWindowPlacement?> LoadPlacementAsync(CancellationToken cancellationToken = default)
    {
        return (await LoadAsync(cancellationToken)).WindowPlacement;
    }

    public async Task SavePlacementAsync(PetWindowPlacement placement, CancellationToken cancellationToken = default)
    {
        var settings = await LoadAsync(cancellationToken);
        await SaveAsync(settings with { WindowPlacement = placement }, cancellationToken);
    }

    public async Task SaveActiveSkinAsync(string skinId, CancellationToken cancellationToken = default)
    {
        var settings = await LoadAsync(cancellationToken);
        await SaveAsync(settings with { ActiveSkinId = skinId }, cancellationToken);
    }

    public async Task SaveTopmostAsync(bool isTopmost, CancellationToken cancellationToken = default)
    {
        var settings = await LoadAsync(cancellationToken);
        await SaveAsync(settings with { IsTopmost = isTopmost }, cancellationToken);
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var values = await ReadAllAsync(connection, cancellationToken);

        return new AppSettings(
            WindowPlacement: ReadPlacement(values),
            ActiveSkinId: ReadString(values, "active_skin_id"),
            IsTopmost: ReadBool(values, "is_topmost", defaultValue: true),
            ShowWelcomeBubble: ReadBool(values, "show_welcome_bubble", defaultValue: true),
            StartHidden: ReadBool(values, "start_hidden", defaultValue: false),
            Ai: new AiSettings(
                Provider: NormalizeProvider(ReadString(values, "ai_provider")),
                Endpoint: ReadString(values, "ai_endpoint"),
                Model: ReadString(values, "ai_model"),
                ApiKey: ReadString(values, "ai_api_key"),
                Instructions: ReadString(values, "ai_instructions")));
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await UpsertAsync(connection, transaction, "window_x", settings.WindowPlacement?.X.ToString(), cancellationToken);
        await UpsertAsync(connection, transaction, "window_y", settings.WindowPlacement?.Y.ToString(), cancellationToken);
        await UpsertAsync(connection, transaction, "active_skin_id", settings.ActiveSkinId, cancellationToken);
        await UpsertAsync(connection, transaction, "is_topmost", WriteBool(settings.IsTopmost), cancellationToken);
        await UpsertAsync(connection, transaction, "show_welcome_bubble", WriteBool(settings.ShowWelcomeBubble), cancellationToken);
        await UpsertAsync(connection, transaction, "start_hidden", WriteBool(settings.StartHidden), cancellationToken);
        await UpsertAsync(connection, transaction, "ai_provider", NormalizeProvider(settings.Ai.Provider), cancellationToken);
        await UpsertAsync(connection, transaction, "ai_endpoint", settings.Ai.Endpoint, cancellationToken);
        await UpsertAsync(connection, transaction, "ai_model", settings.Ai.Model, cancellationToken);
        await UpsertAsync(connection, transaction, "ai_api_key", settings.Ai.ApiKey, cancellationToken);
        await UpsertAsync(connection, transaction, "ai_instructions", settings.Ai.Instructions, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        return connection;
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS app_settings (
                key TEXT PRIMARY KEY NOT NULL,
                value TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, string>> ReadAllAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT key, value FROM app_settings;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            values[reader.GetString(0)] = reader.GetString(1);
        }

        return values;
    }

    private static async Task UpsertAsync(
        SqliteConnection connection,
        DbTransaction transaction,
        string key,
        string? value,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = (SqliteTransaction)transaction;
        command.CommandText = """
            INSERT INTO app_settings(key, value)
            VALUES ($key, $value)
            ON CONFLICT(key) DO UPDATE SET value = excluded.value;
            """;
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value ?? string.Empty);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static PetWindowPlacement? ReadPlacement(IReadOnlyDictionary<string, string> values)
    {
        return int.TryParse(ReadString(values, "window_x"), out var x)
            && int.TryParse(ReadString(values, "window_y"), out var y)
                ? new PetWindowPlacement(x, y)
                : null;
    }

    private static string ReadString(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static bool ReadBool(IReadOnlyDictionary<string, string> values, string key, bool defaultValue)
    {
        return values.TryGetValue(key, out var value)
            ? value == "1"
            : defaultValue;
    }

    private static string WriteBool(bool value)
    {
        return value ? "1" : "0";
    }

    private static string NormalizeProvider(string? provider)
    {
        return provider?.Trim().ToLowerInvariant() switch
        {
            AiProviderIds.OpenAi => AiProviderIds.OpenAi,
            _ => AiProviderIds.Mock
        };
    }
}
