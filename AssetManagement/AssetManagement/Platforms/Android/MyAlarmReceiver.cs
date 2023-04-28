using Android.Content;
using AssetManagement.Models;
using Plugin.LocalNotification;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Platforms.Android
{
    [BroadcastReceiver]
    public class MyAlarmReceiver: BroadcastReceiver
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
        public async override void OnReceive(Context context, Intent intent)
        {
            await SetUpDb();
            List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
            List<MaturingAssets> assetsMaturingIn10Days = (from rec in records
                                                           where (rec.MaturityDate < DateTime.Now.AddDays(20) || rec.MaturityDate < DateTime.Now)
                                                           select new MaturingAssets
                                                           {
                                                               InvestmentEntity = rec.InvestmentEntity,
                                                               Amount = Convert.ToString(rec.Amount),
                                                               MaturityDate = rec.MaturityDate
                                                           }).OrderBy(o => o.MaturityDate).ToList();
            if(assetsMaturingIn10Days.Count > 0)
            {
                var request = new NotificationRequest
                {
                    NotificationId = 1000,
                    Title = "Asset Management",
                    Subtitle = DateTime.Now.ToString("dd/MM/yyyy"),
                    Description = "Assets Maturing Soon!!",
                    BadgeNumber = 42,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(5),
                        NotifyRepeatInterval = TimeSpan.FromDays(1)
                    }
                };
                await LocalNotificationCenter.Current.Show(request);
            }
        }
    }
}
