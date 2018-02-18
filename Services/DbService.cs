using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Transifex.Backend.Services
{
    public interface IDbService
    {
        IDbConnection GetDbConnection();
    }

    public class DbService : IDbService
    {
        private static string ConfigurationPath => Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Db.sqlite";

        public IDbConnection GetDbConnection()
        {
            var conn = new SqliteConnection("Data Source=" + ConfigurationPath);
            conn.Open();
            return conn;
        }
    }
}