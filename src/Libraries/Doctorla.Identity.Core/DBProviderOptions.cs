namespace Doctorla.Identity.Core
{
    public class DBProviderOptions
    {
        public string DbSchema { get; set; } = "dbo";

        public string ConnectionString { get; set; }
    }
}
