# Database Setup — কেন প্রজেক্ট আগে লোড হচ্ছিল না

আগের migration ফাইলগুলোতে সমস্যা ছিল:
- একটা migration (`AddUserProfileImage`) এর সাথের `.resx` snapshot ফাইলটা মিসিং ছিল
- আরেকটা migration (`InitialCreate`) একই কলাম (`Users.ImagePath`) আবার যোগ করার চেষ্টা করছিল
- Brand, Category, Customer, Purchase, Sale, Warehouse ইত্যাদি অনেক নতুন model/table এর জন্য
  কোনো migration-ই ছিল না — মানে DB আর কোডের model সম্পূর্ণ আলাদা হয়ে গিয়েছিল

সব পুরনো/ভাঙা migration ফাইল মুছে ফেলা হয়েছে এবং EF এখন
**Automatic Migrations** ব্যবহার করবে (`Migrations/Configuration.cs` →
`AutomaticMigrationsEnabled = true`), তাই আলাদা করে কিছু করা ছাড়াই EF
নিজেই পুরো schema বানিয়ে/আপডেট করে নেবে।

## Option A — EF কে নিজে থেকে করতে দাও (সহজ, recommended)
1. `Web.config` এ `connectionStrings` → `defaultconnection` এ আপনার আসল
   connection string বসান।
2. প্রজেক্ট রান করুন (F5), অথবা Package Manager Console এ:
   ```
   Update-Database
   ```
3. EF নিজেই সব table বানাবে (Warehouses, Categories, Brands, Units,
   Suppliers, Customers, Products, Purchases, Sales, StockTransfers,
   Salaries, Modules, RolePermissions, Orders, OrderItems ইত্যাদি) এবং
   `Seed()` মেথড রান করবে — যা Modules লিস্ট, প্রতিটা role এর জন্য
   default RolePermission, একটা default Unit ("Piece") আর একটা default
   Warehouse ("Main Store") বানিয়ে দেবে।

## Option B — SQL script ম্যানুয়ালি রান করা
`2026-09-18_full_schema.sql` ফাইলটা SSMS এ খুলে আপনার ডাটাবেসে (Web.config
এ যেটার নাম আছে) রান করুন। প্রতিটা `CREATE TABLE`/`ALTER TABLE`
`IF NOT EXISTS` দিয়ে wrap করা, তাই একাধিকবার রান করলেও সমস্যা নেই।

এই অপশন ব্যবহার করলে `Migrations/Configuration.cs` এ
`AutomaticMigrationsEnabled = false` করে দিন — নাহলে EF নিজের মতো করে
আবার migrate করার চেষ্টা করতে পারে schema এর সাথে conflict করে। সেক্ষেত্রে,
Package Manager Console এ একবার এই কমান্ড চালান:
```
Enable-Migrations -EnableAutomaticMigrations:$false
Add-Migration InitialCreate -IgnoreChanges
Update-Database
```
এটা EF কে বলে দেবে "schema আগে থেকেই ঠিক আছে, নতুন করে বানানোর দরকার নেই।"

## কোনটা বেছে নেবেন?
- **নতুন/খালি ডাটাবেস** → Option A সবচেয়ে সহজ।
- **আগে থেকেই SQL script একবার রান করেছেন, বা schema পুরোপুরি নিজের
  নিয়ন্ত্রণে রাখতে চান** → Option B, তারপর উপরের `-IgnoreChanges` স্টেপ।

## প্রথম Admin ইউজার তৈরি
সবচেয়ে সহজ উপায়: `/Account/Register` দিয়ে একটা account বানান (এটা
default ভাবে "Customer" role এ যাবে), তারপর SSMS এ গিয়ে সেই ইউজারের
`RoleName` কলাম ম্যানুয়ালি `'Admin'` করে দিন:
```sql
UPDATE dbo.Users SET RoleName = 'Admin' WHERE Email = 'your@email.com';
```
