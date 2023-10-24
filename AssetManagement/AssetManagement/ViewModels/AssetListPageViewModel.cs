using AssetManagement.Models;
using AssetManagement.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Java.Lang;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.ViewModels
{
    public partial class AssetListPageViewModel : ObservableObject
    {
        private SQLiteAsyncConnection _dbConnection;
        public ObservableCollection<MaturingAssets> AssetDetails { get; set; } = new ObservableCollection<MaturingAssets>();
        public bool IsRefreshing { get; set; }
        public ObservableCollection<Assets> Assets { get; set; } = new();
        public Command RefreshCommand { get; set; }
        public Assets SelectedAsset { get; set; }
        public bool PaginationEnabled { get; set; } = true;

        private readonly IAssetService _assetService;
        public AssetListPageViewModel(IAssetService assetService)
        {
            //GetAssetsList();
            _assetService = assetService;
        }

        public async Task LoadAssets()
        {
            //var monkeys = await httpClient.GetFromJsonAsync<Monkey[]>("https://montemagno.com/monkeys.json");
            List<Assets> records = await _assetService.GetAssetsList();
            Assets.Clear();
            foreach (var record in records)
            {
                Assets.Add(record);
            }
        }

        //public string image { get; set; } = SelectedMonkey.Image;
        //public string location = SelectedMonkey.Location;
        public Assets GetSelectedRecordDetail()
        {

            Assets obj = new Assets()
            {
                AssetId = SelectedAsset.AssetId,
                InvestmentEntity = SelectedAsset.InvestmentEntity,
                Type = SelectedAsset.Type,
                Amount = SelectedAsset.Amount,
                InterestRate = SelectedAsset.InterestRate,
                InterestFrequency = SelectedAsset.InterestFrequency,
                Holder = SelectedAsset.Holder,
                StartDate = SelectedAsset.StartDate,
                MaturityDate = SelectedAsset.MaturityDate,
                AsOfDate = SelectedAsset.AsOfDate,
                Remarks = SelectedAsset.Remarks
            };

            return obj;
        }

        [RelayCommand]
        public async Task<decimal> GetAssetsList()
        {
            AssetDetails.Clear();
            decimal MaturingAssetsTotalValue = 0;
            List<Assets> records = await _assetService.GetAssetsList();
            List<MaturingAssets> assetsMaturingIn10Days = (from rec in records
                                                           where (rec.MaturityDate < DateTime.Now.AddDays(20) || rec.MaturityDate < DateTime.Now)
                                                           select new MaturingAssets
                                                           {
                                                               InvestmentEntity = rec.InvestmentEntity,
                                                               HolderName = rec.Holder,
                                                               Amount = Convert.ToString(rec.Amount),
                                                               MaturityDate = rec.MaturityDate
                                                           }).OrderBy(o => o.MaturityDate).ToList();
            if (assetsMaturingIn10Days?.Count > 0)
            {
                foreach (var asset in assetsMaturingIn10Days)
                {
                    if (asset.MaturityDate.ToString("dd-MM-yyyy") != "01-01-0001")
                    {
                        decimal amount = Convert.ToDecimal(asset.Amount);
                        MaturingAssetsTotalValue = MaturingAssetsTotalValue + amount;
                        asset.HolderName = asset.HolderName;
                        asset.Amount = string.Format(new CultureInfo("en-IN"), "{0:C0}", amount);
                        AssetDetails.Add(asset);
                    }
                }
            }

            return MaturingAssetsTotalValue;
        }

        public async Task<decimal> GetMaturingAssetsListByDaysLeft(int daysLeft)
        {
            AssetDetails.Clear();
            decimal MaturingAssetsTotalValue = 0;
            List<Assets> records = await _assetService.GetAssetsList();
            List<MaturingAssets> assetsMaturingIn10Days = (from rec in records
                                                           where (rec.MaturityDate < DateTime.Now.AddDays(daysLeft) || rec.MaturityDate < DateTime.Now)
                                                           select new MaturingAssets
                                                           {
                                                               InvestmentEntity = rec.InvestmentEntity,
                                                               HolderName = rec.Holder,
                                                               Amount = Convert.ToString(rec.Amount),
                                                               MaturityDate = rec.MaturityDate
                                                           }).OrderBy(o => o.MaturityDate).ToList();
            if (assetsMaturingIn10Days?.Count > 0)
            {
                foreach (var asset in assetsMaturingIn10Days)
                {
                    if (asset.MaturityDate.ToString("dd-MM-yyyy") != "01-01-0001")
                    {
                        decimal amount = Convert.ToDecimal(asset.Amount);
                        MaturingAssetsTotalValue = MaturingAssetsTotalValue + amount;
                        asset.HolderName = asset.HolderName;
                        asset.Amount = string.Format(new CultureInfo("en-IN"), "{0:C0}", amount);
                        AssetDetails.Add(asset);
                    }
                }
            }

            return MaturingAssetsTotalValue;
        }
    }
}
