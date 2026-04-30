# Scaffolding DB
```powershell
dotnet ef dbcontext scaffold "Server=localhost;Database=FishShop_User;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Entities -n netcore.Entities.Entities -f --no-build -c AppDbContext --context-dir Persistence --context-namespace netcore.Entities.Persistence
```
