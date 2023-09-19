# AssetManagement-MobileApp
# Put this command in the terminal to create the apk file. (dotnet publish -f net7.0-android -c Release)
# The apk file can be found on bin\Release\net7.0-android\

#Change the value in constants.cs file based on test and production environment before creating the release
#IncomeExpenseTest - this is testing collection
#IncomeExpenseFirestoreData - this is production collection


#Page information
AppFlyoutPage.xaml, MenuPage.xaml - related to page layout
ExpensePage.xaml - the home page or where expense data is recorded
IncomePage.xaml - where income data is recorded
AssetPage.xaml - where pick file option is there and here we can see the asset details
IncomeExpenseReportsPage.xaml - report for income and expense
AssetReportPage.xaml - report for asset details
AssetByCategoryPage.xaml, AssetsByHolder.xaml - popup page