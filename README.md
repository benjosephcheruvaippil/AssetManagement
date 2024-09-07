# AssetManagement-MobileApp
## Put this command in the terminal to create the apk file. (dotnet publish -c Release -f:net8.0-android)
## The apk file can be found on bin\Release\net8.0-android34.0\


Page Information:
1.AppFlyoutPage.xaml, MenuPage.xaml - related to page layout
2.ExpensePage.xaml - the home page or where expense data is recorded
3.IncomePage.xaml - where income data is recorded
4.AssetPage.xaml - where pick file option is there and here we can see the asset details
5.IncomeExpenseReportsPage.xaml - report for income and expense
6.AssetReportPage.xaml - report for asset details
7.ManageUsersPage.xaml - page to add/update/delete owners
8.ManageCategoriesPage.xaml - page to add/update/delete income expense categories
9.SettingsPage.xaml - page where backup,restore database options are present

#Popup Page Information
AssetByCategoryPage.xaml, AssetsByHolder.xaml - popup page

#Sqlite database
datetime("Date"/10000000 - 62135596800, 'unixepoch') -> use this to convert big int date to readable date
select ((SELECT strftime('%s', '2022-10-13') + 62135596800)*10000000) as DatetimeInUnixFormat

#Keystore File Information
A copy of key.keystore is stored in Ben's google drive. This file has to be placed at solution(.sln) level file.