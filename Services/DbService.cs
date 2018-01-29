using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

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