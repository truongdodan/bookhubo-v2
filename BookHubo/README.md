# BookHubo - Hướng dẫn Setup

## Yêu cầu

- .NET core
- PostgreSQL

## Cài đặt

### 1. Tạo database

```bash
# Tạo database mới tên bookhubodb
createdb -U postgres bookhubodb

# Import schema
psql -U postgres -d bookhubodb -f database_schema.sql
```

Hoặc dùng pgAdmin: tạo database `bookhubodb` rồi chạy file `database_schema.sql`

### 2. Sửa connection string

Mở `BookHubo/appsettings.json`, sửa dòng:

```json
"DefaultConnection": "Host=localhost;Database=bookhubodb;Username=postgres;Password=YOUR_PASSWORD"
```

Thay `YOUR_PASSWORD` bằng password PostgreSQL của bạn.

### 3. Chạy app

```bash
cd BookHubo
dotnet run
``

## Tài khoản mẫu

- **Admin**: admin@bookhubo.com / admin123
- **User**: user@example.com / user123
```
