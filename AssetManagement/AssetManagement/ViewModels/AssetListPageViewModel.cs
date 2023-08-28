using AssetManagement.Models;
using AssetManagement.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.ViewModels
{
    public partial class AssetListPageViewModel: ObservableObject
    {
        private SQLiteAsyncConnection _dbConnection;
        public ObservableCollection<MaturingAssets> AssetDetails { get; set; } = new ObservableCollection<MaturingAssets>();
        //public List<MaturingAssets> AssetDetails { get; set; }
        private readonly IAssetService _assetService;
        public AssetListPageViewModel(IAssetService assetService)
        {
            //GetAssetsList();
            _assetService= assetService;
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
                                                               HolderName=rec.Holder,
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
