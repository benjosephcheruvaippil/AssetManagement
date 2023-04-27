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
        private readonly IAssetService _assetService;
        public AssetListPageViewModel(IAssetService assetService)
        {
            //GetAssetsList();
            _assetService= assetService;
        }

        [RelayCommand]
        public async void GetAssetsList()
        {
            AssetDetails.Clear();
            List<Assets> records = await _assetService.GetAssetsList();
            List<MaturingAssets> assetsMaturingIn10Days = (from rec in records
                                                   where (rec.MaturityDate < DateTime.Now.AddDays(20) || rec.MaturityDate < DateTime.Now)
                                                   select new MaturingAssets
                                                   {
                                                       InvestmentEntity = rec.InvestmentEntity,
                                                       Amount = Convert.ToString(rec.Amount),
                                                       MaturityDate = rec.MaturityDate
                                                   }).OrderBy(o => o.MaturityDate).ToList();
            //var studentList = await _studentService.GetStudentList();
            if (assetsMaturingIn10Days?.Count > 0)
            {
                //assetsMaturingIn10Days = assetsMaturingIn10Days.OrderBy(f => f.FullName).ToList();
                foreach (var asset in assetsMaturingIn10Days)
                {
                    //asset.Amount("#,#.##", new CultureInfo(0x0439));
                    decimal amount = Convert.ToDecimal(asset.Amount);
                    asset.Amount=amount.ToString("#,#.##", new CultureInfo(0x0439));
                    AssetDetails.Add(asset);
                }
                //StudentsListForSearch.Clear();
                //StudentsListForSearch.AddRange(studentList);
            }
        }
    }
}
