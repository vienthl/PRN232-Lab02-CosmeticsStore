# CLAUDE.md — Lab02: ASP.NET Core Web API với OData (CosmeticsStore)

## Tổng quan Solution

**File solution:** `Lab02.sln`
**Framework:** .NET 8.0

### Các project trong solution

| Project | Loại | Vai trò |
|---|---|---|
| `BusinessObjects` | Class Library | Các entity model (EF Core) |
| `DataAccessObjects` | Class Library | DbContext + các lớp DAO (truy cập DB trực tiếp) |
| `Repositories` | Class Library | Interface + implementation của Repository pattern |
| `Services` | Class Library | Interface + implementation chứa business logic |
| `WebAPI` | ASP.NET Core Web API | Controllers, Program.cs, cấu hình OData + JWT |

### Luồng phụ thuộc (một chiều, không vòng tròn)

```
WebAPI → Services → Repositories → DataAccessObjects → BusinessObjects
                                                    ↑
                                        (DbContext đặt ở đây)
```

`BusinessObjects` được tham chiếu bởi tất cả các layer. Tuyệt đối không để circular reference.

---

## Cơ sở dữ liệu: CosmeticsDB

### Các bảng

**SystemAccount**
- `AccountID` (int, PK, `ValueGeneratedNever()` — không tự tăng)
- `AccountPassword` (nvarchar 100)
- `EmailAddress` (nvarchar 100, unique index)
- `AccountNote` (nvarchar 240)
- `Role` (int) — 1=Administrator, 2=Manager, 3=Staff, 4=Member

**CosmeticCategory**
- `CategoryID` (nvarchar 30, PK)
- `CategoryName` (nvarchar 120)
- `UsagePurpose` (nvarchar 250)
- `FormulationType` (nvarchar 250)

**CosmeticInformation**
- `CosmeticID` (nvarchar 30, PK — sinh bằng `GenerateId()` = "PL" + 6 chữ số ngẫu nhiên)
- `CosmeticName` (nvarchar 160)
- `SkinType` (nvarchar 200)
- `ExpirationDate` (nvarchar 160)
- `CosmeticSize` (nvarchar 400)
- `DollarPrice` (decimal(18,0))
- `CategoryID` (nvarchar 30, FK → CosmeticCategory, cascade delete)

### Chuỗi kết nối
Key: `ConnectionStrings:DefaultConnectionString` trong `appsettings.json`
Giá trị: `Server=(local); Database=CosmeticsDB; Uid=sa; Pwd=1234567890;TrustServerCertificate=True`

---

## Quy tắc từng project

### BusinessObjects
- Chỉ chứa 3 entity class: `CosmeticCategory.cs`, `CosmeticInformation.cs`, `SystemAccount.cs`
- Không chứa logic — chỉ là các POCO khớp với schema DB trên
- Các NuGet đã cài sẵn: `EFCore.SqlServer 8.0.2`, `EFCore.Tools 8.0.2`, `Extensions.Configuration.Json 8.0.0`

### DataAccessObjects
- Chứa `CosmeticsDbContext.cs` (chuyển từ BusinessObjects sau khi scaffold)
- Chứa `CosmeticInformationDAO.cs` — singleton pattern, toàn bộ method là async:
  `GetAllCosmetics`, `GetAllCategories`, `AddCosmeticInformation`, `GetById`, `Update`, `Delete`, và private `GenerateId()`
- Chứa `SystemAccountDAO.cs` — singleton pattern, method `Login(email, password)`
- `CosmeticsDbContext.GetConnectionString()` đọc từ `appsettings.json` qua `ConfigurationBuilder` với `Directory.GetCurrentDirectory()` làm base path
- Phải tham chiếu project `BusinessObjects`

### Repositories
- Interface `ICosmeticInformationRepository`: `GetAllCosmetics`, `GetOne(id)`, `Add`, `Update`, `Delete`, `GetAllCategories`
- Interface `ISystemAccountRepository`: `Login(email, password)`
- `CosmeticInformationRepository` implements `ICosmeticInformationRepository` — gọi vào `CosmeticInformationDAO.Instance`
- `SystemAccountRepository` implements `ISystemAccountRepository` — gọi vào `SystemAccountDAO.Instance`
- Phải tham chiếu `BusinessObjects` và `DataAccessObjects`

### Services
- Interface `ICosmeticInformationService`: các method giống Repository
- Interface `ISystemAccountService`: `Login(email, password)`
- `CosmeticInformationService` — inject `ICosmeticInformationRepository` qua constructor, ủy thác toàn bộ
- `SystemAccountService` — inject `ISystemAccountRepository` qua constructor, ủy thác `Login`
- Phải tham chiếu `BusinessObjects` và `Repositories`

### WebAPI
- Đã tham chiếu `BusinessObjects` và `Services` (có sẵn trong .csproj)
- Các NuGet cần cài thêm:
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (phiên bản tương thích .NET 8, ví dụ 8.0.x)
  - `Microsoft.AspNetCore.OData` (9.2.0)
  - `Swashbuckle.AspNetCore` (7.3.0)

#### appsettings.json phải có
```json
"ConnectionStrings": {
  "DefaultConnectionString": "Server=(local); Database=CosmeticsDB; Uid=sa; Pwd=1234567890;TrustServerCertificate=True"
},
"Jwt": {
  "SecretKey": "SecretKeySecretKeySecretKeySecretKeySecretKeySecretKeySecretKeySecretKeySecretKeySecretKeySecretKey",
  "Issuer": "FU Lab Issuer",
  "Audience": "FU Lab Audience"
}
```

#### DTOs
`AccountDTO.cs` (namespace WebAPI):
- `AccountRequestDTO` — `Email`, `Password`
- `AccountResponseDTO` — `Token`, `Role`, `AccountId`

#### Program.cs phải cấu hình
1. Tạo `ODataConventionModelBuilder` với 2 entity set: `CosmeticInformations`, `CosmeticCategories`
2. `AddControllers()` với `AddJsonOptions` (IgnoreCycles, Never) + `AddOData(Select/Filter/OrderBy/Expand/Count/MaxTop=null)` route prefix = `"odata"`
3. Đăng ký 4 cặp interface→implementation bằng `AddScoped`
4. `AddEndpointsApiExplorer()`
5. JWT Bearer authentication: đọc key từ `configuration["JWT:SecretKey/Issuer/Audience"]`, đặt `ValidateIssuer=false`, `ValidateAudience=false`, `ValidateLifetime=false`
6. Swagger với security definition Bearer
7. Các Authorization policy:
   - `"AdminOnly"` — claim "Role" == "1"
   - `"AdminOrStaffOrMember"` — claim "Role" thuộc {"1", "3", "4"}
8. Thứ tự middleware: `UseRouting()` → `UseSwagger/SwaggerUI` (chỉ khi dev) → `UseAuthorization()` → `MapControllers()`

#### Controllers

**`SystemAccountsController`** (`[Route("api/[controller]")]`)
- `[HttpPost("Login")]` — xác thực tài khoản, tạo JWT với 3 claim (`ClaimTypes.Email`, `"Role"`, `"AccountId"`), trả `AccountResponseDTO`; 401 nếu không tìm thấy

**`CosmeticInformationsController`** kế thừa `ODataController` (không đặt `[Route]` ở class)
- `GET /api/CosmeticInformations` — `[EnableQuery]` + policy `AdminOrStaffOrMember`
- `GET /api/CosmeticCategories` — policy `AdminOrStaffOrMember`
- `POST /api/CosmeticInformations` — policy `AdminOnly`
- `PUT /api/CosmeticInformations/{id}` — policy `AdminOnly`, gán `cosmeticInformation.CosmeticId = id` trước khi gọi Update
- `DELETE /api/CosmeticInformations/{id}` — policy `AdminOnly`
- `GET /api/CosmeticInformations/{id}` — policy `AdminOrStaffOrMember`

---

## Quy tắc phân quyền

| Role | Giá trị | Quyền |
|---|---|---|
| Administrator | 1 | Toàn quyền CRUD + tìm kiếm |
| Manager | 2 | Không có quyền gì |
| Staff | 3 | Chỉ đọc / tìm kiếm |
| Member | 4 | Chỉ đọc / tìm kiếm |

---

## Quy ước lập trình

- Tất cả các method trong DAO và controller đều là `async Task<T>`
- Xử lý lỗi: trong DAO bắt exception rồi `throw new Exception(ex.Message)`; trong controller trả `StatusCode(400, $"{ex.Message}")`
- Không đặt `[Route("api/[controller]")]` trên `CosmeticInformationsController` — mỗi action tự khai báo route
- JSON: `ReferenceHandler.IgnoreCycles` để tránh lỗi vòng tham chiếu từ navigation property
- `CosmeticsDbContext` dùng `GetConnectionString()` đọc `appsettings.json` — file này phải có mặt trong thư mục output của WebAPI
- Không thêm comment thừa vào code; chỉ thêm khi logic thực sự khó hiểu

---

## Thứ tự thực hiện (theo Activity trong đề bài)

1. **BusinessObjects** — viết 3 entity class
2. **DataAccessObjects** — viết `CosmeticsDbContext` + 2 DAO
3. **Repositories** — viết interface + implementation
4. **Services** — viết interface + implementation
5. **WebAPI** — cấu hình `appsettings.json`, cài NuGet, viết `AccountDTO.cs`, `Program.cs`, 2 controller
6. Kiểm tra build toàn bộ solution — sửa nếu thiếu project reference

---

## Kiểm tra project reference

- `DataAccessObjects.csproj` → tham chiếu `BusinessObjects`
- `Repositories.csproj` → tham chiếu `BusinessObjects` + `DataAccessObjects`
- `Services.csproj` → tham chiếu `BusinessObjects` + `Repositories`
- `WebAPI.csproj` → tham chiếu `BusinessObjects` + `Services` (đã có sẵn)
