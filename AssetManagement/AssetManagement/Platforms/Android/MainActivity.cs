using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Java.Lang;
using SQLite;

namespace AssetManagement;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        //ScheduleAlarm();
    }
    //private void ScheduleAlarm()
    //{

    //    //Intent intent = new Intent(this, typeof(MyAlarmReceiver));
    //    //PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.Immutable);

    //    //AlarmManager alarmManager = (AlarmManager)GetSystemService(Context.AlarmService);
    //    //alarmManager.SetRepeating(AlarmType.RtcWakeup, 1000, AlarmManager.IntervalFifteenMinutes, pendingIntent);

    //        var intent = new Intent(this, typeof(MyAlarmReceiver));
    //        var pendingIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.Immutable);

    //        var alarmManager = (AlarmManager)GetSystemService(AlarmService);
    //        var interval = 1440 * 60 * 1000; // 24 hours in milliseconds

    //        alarmManager.SetRepeating(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis() + interval, interval, pendingIntent);

    //}

}
