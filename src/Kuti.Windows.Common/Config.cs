using System.Reflection;

namespace Kuti.Windows.Common;

public static class Config
{
    /// <summary>
    /// Name of this Product.
    /// </summary>
    public static string ProductName { get; private set; }

    public static string ExecutableName { get; private set; }

    public static string Developer { get; private set; }

    private static readonly object _syncLock = new();
    private static bool _wasSet = false;

    static Config()
    {
        var assembly = Assembly.GetEntryAssembly();
        Developer = assembly?.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Codoxide.com";
        ProductName = assembly?.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Kuti";
        ExecutableName = assembly?.GetName().Name ?? "Kuti.Windows";
    }

    public static void SetMetadata(string name, string developer)
    {
        if (_wasSet) throw new InvalidOperationException("Product metadata can only bey set once.");
            
        lock (_syncLock)
        {
            if (!_wasSet)
            {
                ProductName = name;
                Developer = developer;
                _wasSet = true;
            }
            else
            {
                throw new InvalidOperationException("Product metadata can only be set once.");
            }
        }
    }
}
