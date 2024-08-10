using Dapper;

namespace Kuti.Windows.Settings.Pages.PinnedApps;

public interface IPinnedAppsRepository: IRepository
{
    Guid? GetPinnedDesktop(string process);
    void SavePin(PinnableProcess process, PinnableDesktop desktop);

    IEnumerable<PinnableDesktop> GetSavedPins();
}

public class PinnedAppsRepository : IPinnedAppsRepository
{
    private readonly IDatabase _db;

    public static string[] SetupScripts => [
        @"CREATE TABLE IF NOT EXISTS PinnedApps (
            processPath TEXT PRIMARY KEY,
            processName TEXT not null,
            desktopId TEXT not null
        );",
        @"CREATE INDEX IF NOT EXISTS idx_desktopId ON PinnedApps (desktopId);",
        @"CREATE TABLE IF NOT EXISTS Desktops (
            id TEXT not null PRIMARY KEY,
            name TEXT not null
        );"
    ];

    public Guid? GetPinnedDesktop(string processPath)
    {
        using var connection = _db.GetOpenConnection();

        var desktop = connection.QueryFirstOrDefault<string>(
            "SELECT desktopId FROM PinnedApps WHERE processPath=@processPath",
            new { processPath });

        connection.Close();

        return (Guid.TryParse(desktop, out Guid desktopId)) ? desktopId : null;
    }

    public void SavePin(PinnableProcess process, PinnableDesktop desktop)
    {
        using var connection = _db.GetOpenConnection();
        using var txn = connection.BeginTransaction();

        string desktopId = desktop.Id.ToString("N");

        // Command to insert or ignore into Desktops
        var desktopsCmd = connection.CreateCommand();
        desktopsCmd.CommandText = "INSERT OR IGNORE INTO Desktops (id, name) VALUES (@desktopId, @desktopName)";
        desktopsCmd.Transaction = txn;

        desktopsCmd.SetParameter("@desktopId", desktopId);
        desktopsCmd.SetParameter("@desktopName", desktop.Name);
        desktopsCmd.ExecuteNonQuery();

        // Command to insert or replace into PinnedApps
        var pinnedAppsCmd = connection.CreateCommand();
        pinnedAppsCmd.CommandText = "INSERT OR REPLACE INTO PinnedApps (processPath, processName, desktopId) VALUES (@processPath, @processName, @desktopId)";
        pinnedAppsCmd.Transaction = txn;

        pinnedAppsCmd.SetParameter("@processPath", process.Path);
        pinnedAppsCmd.SetParameter("@processName", process.Name);
        pinnedAppsCmd.SetParameter("@desktopId", desktopId);
        pinnedAppsCmd.ExecuteNonQuery();

        txn.Commit();
    }


    public IEnumerable<PinnableDesktop> GetSavedPins()
    {
        var processByDesktopId = new Dictionary<string, PinnableDesktop>();

        using var conn = _db.GetOpenConnection();
        using var reader = conn.ExecuteReader(@"SELECT d.id, d.name, p.processPath, p.processName
                                                FROM PinnedApps p
                                                JOIN Desktops d ON p.desktopId = d.id");

        while (reader.Read())
        {
            string desktopId = reader.GetString(0);
            string desktopName = reader.GetString(1);
            string processPath = reader.GetString(2);
            string processName = reader.GetString(3);

            if (!processByDesktopId.ContainsKey(desktopId))
            {
                if (Guid.TryParse(desktopId, out Guid desktopGuid))
                {
                    processByDesktopId[desktopId] = new PinnableDesktop(desktopName, desktopGuid);
                }
                else
                {
                    continue;
                }
            }

            processByDesktopId[desktopId].Processes.Add(new PinnableProcess(processName, processPath));
        }

        reader.Close();

        return processByDesktopId.Values;
    }

    public PinnedAppsRepository(IDatabase db)
    {
        _db = db;
    }
}
