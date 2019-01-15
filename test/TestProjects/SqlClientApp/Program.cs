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
            using (var client = new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Integrated Security=True"))
            {
                client.Open();
                return !string.IsNullOrEmpty(client.ServerVersion);
            }
        }
    }
}
