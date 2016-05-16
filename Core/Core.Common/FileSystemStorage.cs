using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AFT.RegoV2.Core.Common
{
    public interface IFileStorage
    {
        void Save(string fileName, byte[] content);
        byte[] Get(string fileName);
    }

    public class FileSystemStorage : IFileStorage
    {
        private object thisLock = new object();
        private readonly string ConnStr = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        public void Save(string fileName, byte[] content)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (var txn = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("INSERT INTO Documents(file_stream, name) VALUES (@file_stream, @name)", conn, txn))
                    {
                        cmd.Parameters.Add("@file_stream", SqlDbType.VarBinary).Value = content;
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = fileName;
                        cmd.ExecuteNonQuery();
                    }

                    txn.Commit();
                }

                conn.Close();
            }
        }

        public byte[] Get(string fileName)
        {
            byte[] result = {};

            lock (thisLock)
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    var cmd =
                        new SqlCommand("SELECT file_stream FROM Documents WITH (READCOMMITTEDLOCK) WHERE name = @name",
                            conn);
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = fileName;

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                        result = (byte[]) reader["file_stream"];
                }
            }

            return result;
        }
    }
}
