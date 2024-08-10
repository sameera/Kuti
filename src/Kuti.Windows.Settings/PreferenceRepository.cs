using Dapper;

namespace Kuti.Windows.Settings;

public interface IPreferencesRepository: IRepository
{
    string? GetPreference(string key);
    void SetPreference(string key, string value);
}

public class PreferencesRepository : IPreferencesRepository
{
    private IDatabase _db;

    public PreferencesRepository(IDatabase db)
    {
        _db = db;
    }

    public static string[] SetupScripts => [@"
                CREATE TABLE IF NOT EXISTS Preferences (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                )"];

    public string? GetPreference(string key)
    {
        using var connection = _db.GetOpenConnection();

        // Retrieve the value for the specified key from the preferences table
        var value = connection.QueryFirstOrDefault<string>(
            "SELECT Value FROM Preferences WHERE Key = @Key",
            new { Key = key });

        connection.Close();

        return value;
    }

    public void SetPreference(string key, string value)
    {
        using var connection = _db.GetOpenConnection();

        // Insert or update the preference in the preferences table
        connection.Execute(@"
                INSERT OR REPLACE INTO Preferences (Key, Value)
                VALUES (@Key, @Value)",
            new { Key = key, Value = value });

        connection.Close();
    }
}