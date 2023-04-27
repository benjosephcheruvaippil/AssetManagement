using AssetManagement.Models;
using AssetManagement.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.ViewModels
{
    public partial class AssetListPageViewModel: ObservableObject
    {
        private SQLiteAsyncConnection _dbConnection;
        public ObservableCollection<Assets> AssetDetails { get; set; } = new ObservableCollection<Assets>();
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
            List<Assets> assetsMaturingIn10Days = (from rec in records
                                                   where (rec.MaturityDate < DateTime.Now.AddDays(60))
                                                   select new Assets
                                                   {
                                                       InvestmentEntity = rec.InvestmentEntity,
                                                       Amount = rec.Amount,
                                                       MaturityDate = rec.MaturityDate
                                                   }).ToList();
            //var studentList = await _studentService.GetStudentList();
            if (assetsMaturingIn10Days?.Count > 0)
            {
                //assetsMaturingIn10Days = assetsMaturingIn10Days.OrderBy(f => f.FullName).ToList();
                foreach (var asset in assetsMaturingIn10Days)
                {
                    AssetDetails.Add(asset);
                }
                //StudentsListForSearch.Clear();
                //StudentsListForSearch.AddRange(studentList);
            }
        }
    }
}
