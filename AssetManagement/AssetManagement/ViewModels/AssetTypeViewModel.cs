using AssetManagement.Models;
using AssetManagement.Models.DataTransferObject;
using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.ViewModels
{
    public class AssetTypeViewModel : ObservableObject
    {
        private SQLiteAsyncConnection _dbConnection;

        // For CollectionView (paged)
        public ObservableCollection<AssetTypeDTO> AssetTypes { get; set; } = new();

        private async Task SetUpDb()
        {
            if (_dbConnection == null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
                _dbConnection = new SQLiteAsyncConnection(dbPath);
                await _dbConnection.CreateTableAsync<AssetTypeModel>();
            }
        }

        public async Task<List<AssetTypeDTO>> GetAllAssetTypes()
        {
            await SetUpDb();
            var models = await _dbConnection.Table<AssetTypeModel>().ToListAsync();

            return models.Select(m => new AssetTypeDTO
            {
                AssetTypeId = m.AssetTypeId,
                AssetTypeName = m.AssetTypeName,
                EnableMaturityDate = m.EnableMaturityDate,
                EnableAsOfDate = m.EnableAsOfDate,
                IncludeInNetWorth = m.IncludeInNetWorth,
                CategoryTag = m.CategoryTag,
                Description = m.Description
            }).ToList();
        }

        public void LoadPagedData(List<AssetTypeDTO> allItems, int pageNum, int pageSize)
        {
            AssetTypes.Clear();
            var pageData = allItems.Skip((pageNum - 1) * pageSize).Take(pageSize);
            foreach (var item in pageData)
                AssetTypes.Add(item);
        }
    }
}
