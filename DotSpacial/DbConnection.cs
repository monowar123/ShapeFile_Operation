using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DotSpacialTest
{
    public class DbConnection
    {
        string host = "localhost";
        string port = "5432";
        string dbName = "PO02";
        string userId = "postgres";
        string password = "postgres";

        string connString = string.Empty;

        public DbConnection()
        {
            connString = string.Format("SERVER={0}; Port={1}; Database={2}; User id={3}; Password={4}; encoding=unicode;",
                                        host, port, dbName, userId, password);
        }

        public string GetConnectionString()
        {
            return connString;
        }

        public NpgsqlConnection GetConnection()
        {
            NpgsqlConnection con = new NpgsqlConnection(connString);
            try
            {
                con.Open();
            }
            catch
            {
                throw new Exception("Problem in connection string");
            }

            return con;
        }
    }
}
