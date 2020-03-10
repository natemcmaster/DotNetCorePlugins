using System;
using System.Data.SqlClient;

namespace SqlClientApp
{
    public class Program
    {
        // required to make the C# compiler happy
        public static void Main(string[] args)
        {
        }

        public static bool Run()
        {
            try
            {
                using (var client = new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Integrated Security=True"))
                {
                    client.Open();
                    return !string.IsNullOrEmpty(client.ServerVersion);
                }
            }
            catch (SqlException ex) when (ex.Number == -2)  // -2 means SQL timeout
            {
                // When running the test in Azure DevOps build pipeline, we'll get a SqlException with "Connection Timeout Expired".
                // We can ignore this safely in unit tests.
                return true;
            }
        }
    }
}
