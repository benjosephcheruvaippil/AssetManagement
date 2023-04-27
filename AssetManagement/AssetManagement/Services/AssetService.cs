using AssetManagement.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Services
{
    public class AssetService : IAssetService
    {
        private SQLiteAsyncConnection _dbConnection;

        private async Task SetUpDb()
        {
            if (_dbConnection == null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
                _dbConnection = new SQLiteAsyncConnection(dbPath);
                await _dbConnection.CreateTableAsync<Assets>();
            }
        }
        public async Task<List<Assets>> GetAssetsList()
        {
            await SetUpDb();
            List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
            return records;
        }
    }
}
