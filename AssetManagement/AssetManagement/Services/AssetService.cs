using AssetManagement.Models;
using AssetManagement.Models.DataTransferObject;
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
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Assets.db3");
                _dbConnection = new SQLiteAsyncConnection(dbPath);
                await _dbConnection.CreateTableAsync<Assets>();
            }
        }

        public async Task<Assets> GetAssetsById(int assetId)
        {
            await SetUpDb();
            Assets asset = await _dbConnection.Table<Assets>().Where(a => a.AssetId == assetId).FirstOrDefaultAsync();
            return asset;
        }

        public async Task<List<Assets>> GetAssetsList()
        {
            await SetUpDb();
            List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
            return records;
        }

        public async Task<List<AssetDocuments>> GetAssetDocumentsList(int assetId)
        {
            await SetUpDb();
            List<AssetDocuments> records = await _dbConnection.Table<AssetDocuments>().Where(d => d.AssetId == assetId).ToListAsync();
            return records;
        }
    }
}
