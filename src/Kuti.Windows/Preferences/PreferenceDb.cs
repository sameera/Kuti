using System.Data.SQLite;
using Dapper;

namespace Kuti.Windows.Preferences;

public interface IPreferencesDb
{
    string? GetPreference(string key);
    void SetPreference(string key, string value);
}

public class PreferencesDb : IPreferencesDb
{
    private readonly string _connectionString;

    public PreferencesDb(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
        EnsureDatabaseCreated();
    }

    private void EnsureDatabaseCreated()
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        // Create the preferences table if it does not exist
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Preferences (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                )");

        connection.Close();
    }

    public string? GetPreference(string key)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        // Retrieve the value for the specified key from the preferences table
        var value = connection.QueryFirstOrDefault<string>(
            "SELECT Value FROM Preferences WHERE Key = @Key",
            new { Key = key });

        connection.Close();

        return value;
    }

    public void SetPreference(string key, string value)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        // Insert or update the preference in the preferences table
        connection.Execute(@"
                INSERT OR REPLACE INTO Preferences (Key, Value)
                VALUES (@Key, @Value)",
            new { Key = key, Value = value });

        connection.Close();
    }
}