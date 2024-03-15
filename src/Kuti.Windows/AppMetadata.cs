namespace Kuti.Windows
{
    public class AppMetadata
    {
        public string ProductName { get; private set; }
        public string Company { get; private set; }

        public AppMetadata() : this(null, null)
        {
        }

        public AppMetadata(string? productName, string? company)
        {
            ProductName = productName ?? "Kuti";
            Company = company ?? "Codoxide.com";
        }
    }
}
