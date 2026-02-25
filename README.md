# AssetManagement-MobileApp

A mobile application for managing **Assets, Income, and Expenses** with reporting, category management, and database backup/restore functionality.

---

## Features

* Track **Income and Expenses**
* Manage **Assets and Attach Files**
* Generate **Income & Expense Reports**
* Generate **Asset Reports**
* Manage **Owners (Users)**
* Manage **Categories**
* Database **Backup & Restore**
* Simple and organized mobile UI

---

## Technology Stack

* .NET (Android)
* SQLite Database
* XAML UI
* C#

---

## Build APK

Run the following command in the terminal:

```bash
dotnet publish -c Release -f:net10.0-android36.0
```

After the build completes, the APK file will be available at:

```
bin/Release/net8.0-android34.0/
```

---

## Project Structure

### Layout Pages

These pages control the main application layout and navigation.

* `AppFlyoutPage.xaml`
* `MenuPage.xaml`

---

### Core Application Pages

#### ExpensePage.xaml

* Main home page of the application
* Record and manage expense entries

#### IncomePage.xaml

* Record and manage income entries

#### AssetPage.xaml

* Upload and attach files
* View asset details

---

### Reports

#### IncomeExpenseReportsPage.xaml

* Displays reports related to income and expenses

#### AssetReportPage.xaml

* Displays reports related to assets

---

### Management Pages

#### ManageUsersPage.xaml

* Add owners
* Update owner details
* Delete owners

#### ManageCategoriesPage.xaml

* Add categories
* Update categories
* Delete categories

---

### Settings

#### SettingsPage.xaml

Includes:

* Database Backup
* Database Restore

---

## Popup Pages

Used for quick filtering and data display.

* `AssetByCategoryPage.xaml`
* `AssetsByHolder.xaml`

---

## Database Information (SQLite)

### Convert BigInt Date to Readable Date

```sql
datetime("Date"/10000000 - 62135596800, 'unixepoch')
```

### Convert Date to BigInt Format

```sql
SELECT ((SELECT strftime('%s', '2022-10-13') + 62135596800) * 10000000) AS DatetimeInUnixFormat;
```

---

## Keystore Setup

A copy of the `key.keystore` file is stored in **Ben's Google Drive**.

### Setup Steps

1. Download `key.keystore` from the shared drive.
2. Place the file at the **solution (.sln) level directory**.


## Author

Project maintained by Ben Joseph.

