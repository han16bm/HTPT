       90
       91 ## Wave 1 Step D sản phẩm
       92 - `API/netcore/netcore.Entities/Persistence/AppDbContext.cs` — thin DbContext, 18 DbSet, `ApplyConfigurationsFromAssembly`, default schema
          `FSHOP_DB`
       93 - `API/netcore/netcore.Entities/Persistence/Configurations/*.cs` — 18 file: Role, User, CustomerProfile, CustomerAddress, Category, Product
          , ProductImage, InventoryTransaction, Promotion, PromotionProduct, ShoppingCart, CartItem, Order, OrderItem, Payment, BlogCategory, BlogPos
          t, ContactMessage
       94 - `API/netcore/netcore.Entities/Extensions/EntityServicesExtensions.cs` — `AddEntityServices` extension (namespace `netcore.Entities.Extens
          ions`)
       95 - `API/netcore/netcore.Entities/Repositories/UnitOfWork.cs` — now uses `AppDbContext`
       96 - `API/netcore/netcore.Entities/Repositories/GenericRepository.cs` — now uses `AppDbContext`
       97
       98 ## Entities partials (KHÔNG chỉnh — chỉ để tham chiếu nav props)
       99 - `API/netcore/netcore.Entities/Entities/UserExtensions.cs` — IsAdmin, CreatedBy, UpdatedBy, UpdatedAt2, ResetToken, ResetTokenExp, Role, C
          ustomerProfile nav
      100 - `API/netcore/netcore.Entities/Entities/RoleExtensions.cs` — Users ICollection
      101 - `API/netcore/netcore.Entities/Entities/NavigationProperties.cs` — partials cho Product, InventoryTransaction, CartItem, OrderItem, Order.
          OrderItems, BlogPost
      102
      103 ## Tech debt Wave 2 target
      104 - `API/Services/API.Admin/Services/AdminServices.cs` — 4 class trong 1 file (DashboardService, SalesService, CustomerAdminService, ReportSe
          rvice)
      105 - `API/Services/API.Content/Services/ContentServices.cs` — BlogService + ContactService
      106 - `API/Services/API.Orders/Services/PaymentPromotionService.cs` — merged 2 responsibilities
      107 - 6 controllers có warning CS8625 null literal (BlogController:68, ProductsController:73, InventoryController:38, CategoriesController:65,
          PaymentPromotionController:77, CartController:66)
      108 - `API/Services/API.Orders/Services/OrderService.cs:209` — CS8601 nullable
      109
      110 ## Plan
      111 - `Plan/00_OVERVIEW.md`, `Plan/01_ARCHITECTURE.md`, `Plan/02_PROJECT_STRUCTURE.md`, `Plan/04_BACKEND_PLAN.md`, `Plan/08_FEATURES_ROADMAP.md
          `, `Plan/12_ADMIN_MODULES.md` — đã đọc
      112 - Plan 03, 05, 06, 07, 09, 10, 11 — chưa đọc full, đọc khi Wave 2
      113
      114 ## Solution
      115 - `API/API.sln` — chứa netcore.Commons, netcore.Entities, API.{Auth,Products,Orders,Admin,Content}, FishShop.Gateway. KHÔNG chứa legacy.
      116 - 5 Program.cs đã thêm `using netcore.Entities.Extensions;` thay cho `using netcore.Entities;`
