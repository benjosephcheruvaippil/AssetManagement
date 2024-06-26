# AssetManagement-MobileApp
# Put this command in the terminal to create the apk file. (dotnet publish -c Release -f:net8.0-android)
# The apk file can be found on bin\Release\net7.0-android\

#Change the value in constants.cs file based on test and production environment before creating the release
#IncomeExpenseTest - this is testing collection
#IncomeExpenseFirestoreData - this is production collection


#Page Information
AppFlyoutPage.xaml, MenuPage.xaml - related to page layout
ExpensePage.xaml - the home page or where expense data is recorded
IncomePage.xaml - where income data is recorded
AssetPage.xaml - where pick file option is there and here we can see the asset details
IncomeExpenseReportsPage.xaml - report for income and expense
AssetReportPage.xaml - report for asset details
ManageUsersPage.xaml - page to add/update/delete owners
ManageCategoriesPage.xaml - page to add/update/delete income expense categories
SettingsPage.xaml - page where backup,restore database options are present

#Popup Page Information
AssetByCategoryPage.xaml, AssetsByHolder.xaml - popup page

#Sqlite database
datetime("Date"/10000000 - 62135596800, 'unixepoch') -> use this to convert big int date to readable date
select ((SELECT strftime('%s', '2022-10-13') + 62135596800)*10000000) as DatetimeInUnixFormat

#Keystore File Information
A copy of key.keystore is stored in Ben's google drive. This file has to be placed at solution(.sln) level file.