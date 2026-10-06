# StoreApp API (.NET 8 Clean Architecture)

StoreApp هو نظام Backend لمتجر تجاري مبني باستخدام أحدث معايير هندسة البرمجيات النظيفة (Clean Architecture) وفصل المسؤوليات.

## 🚀 التقنيات المستخدمة
* **.NET 8** (ASP.NET Core Web API)
* **Entity Framework Core** (ORM & SQL Server)
* **FluentValidation** (التحقق من صحة البيانات DTOs)
* **LINQ & Repository Pattern** (إدارة البيانات وتجريد قاعدة البيانات)

---

## 📂 هيكلية المشروع (Architecture)
تم تقسيم الحل إلى الطبقات الرئيسية التالية:
1. **Store.Domain**: يضم الكيانات الأساسية (Entities) مثل `Product` و `Category`.
2. **StoreApp.Application**: يضم خدمات المنطق البرمجي (Services)، واجهات المستودعات (Interfaces)، الـ DTOs، وقواعد التحقق (Validators)، والاستثناءات المخصصة (`NotFoundException`, `ConflictException`).
3. **StoreApp.Infrastructure**: يضم سياق قاعدة البيانات (`ApplicationDbContext`)، تطبيق الـ Repositories، وبيانات التهيئة الأولية (`DbSeeder`).
4. **StoreApp.Api**: طبقة الـ Controllers ومعالجة الأخطاء المركزية (Global Error Handling Middleware).

---

## ⚙️ إعداد وتشغيل المشروع

### 1. المتطلبات الأساسية
* تثبيت [.NET 8 SDK](https://dotnet.microsoft.com/)
* تثبيت **SQL Server** (أو LocalDB)

### 2. إعداد الـ Connection String
تأكدي من ضبط إعدادات الاتصال بقاعدة البيانات في ملف `appsettings.json`:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=StoreInventoryDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}

