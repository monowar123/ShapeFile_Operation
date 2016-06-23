using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpacialTest
{
    public class DbHandeler
    {
        public DataTable GetDataTable(string query)
        {
            DbConnection conObj = new DbConnection();
            DataTable dt = new DataTable();

            using (NpgsqlDataAdapter adp = new NpgsqlDataAdapter(query, conObj.GetConnectionString()))
            {
                adp.Fill(dt);
            }

            return dt;
        }

        public bool InsertData(string query, List<NpgsqlParameter> parameter)
        {
            int affectedRows = 0;
            DbConnection conObj = new DbConnection();

            using (NpgsqlConnection con = new NpgsqlConnection(conObj.GetConnectionString()))
            {
                con.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    foreach (NpgsqlParameter pr in parameter)
                    {
                        cmd.Parameters.Add(pr);
                    }
                    cmd.CommandTimeout = 1000;
                    affectedRows = cmd.ExecuteNonQuery();
                }

                con.Close();
            }

            if (affectedRows > 0)
                return true;

            return false;
        }
    }
}
