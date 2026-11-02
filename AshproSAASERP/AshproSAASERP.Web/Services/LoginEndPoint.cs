using AshproSAASERP.Models.Company;
using AshproSAASERP.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AshproSAASERP.Web.Services;

public static class LoginEndPoint
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        // ==============================
        // PRE-LOGIN (GET)
        // ==============================
        group.MapGet("/prelogin", async (
            [FromQuery] string tenant,
            [FromQuery] string username,
            [FromServices] ITenantRepository tenantRepository,
            [FromServices] IConfiguration config,
            [FromServices] ILoginRepo loginRepo) =>
        {
            Console.WriteLine($"[DEBUG] /prelogin called with tenant='{tenant}', username='{username}'");
            string? conn = null;
            LoginCompany? lCompany = null;
            DateTime? SubEndDate = null;
            bool IsExpired = false;
            bool IsTrial = false;

            var tenantType = config["TenantType"];
            bool isSingleTenant = string.Equals(tenantType, "single", StringComparison.OrdinalIgnoreCase);

            if (isSingleTenant)
            {
                var dbProvider = config["DbProvider"] ?? "MSSQL";
                conn = config[$"ConnectionStrings:{dbProvider}:Master"];
                Console.WriteLine($"[DEBUG] Single tenant mode. Using Master connection string: '{conn}'");
            }
            else if (tenant != "maui")
            {
                lCompany = await tenantRepository.GetLoginCompanyAsync(tenant);
                if (lCompany != null)
                {
                    Console.WriteLine($"[DEBUG] Resolved LoginCompany: CompanyCode='{lCompany.CompanyCode}', ConnectionString='{lCompany.ConnectionString}'");
                    SubEndDate = lCompany.SubscriptionEndingDate;
                    IsExpired = lCompany.SubscriptionEndingDate.HasValue && lCompany.SubscriptionEndingDate.Value < DateTime.UtcNow;
                    IsTrial = lCompany.SubscriptionType == "Trial";
                }
                else
                {
                    Console.WriteLine($"[DEBUG] LoginCompany not found for tenant='{tenant}'");
                }
                conn = lCompany?.ConnectionString;
            }
            else
            {
                conn = config.GetConnectionString("Master")!;
                Console.WriteLine($"[DEBUG] Using Master connection string: '{conn}'");
            }

            if (string.IsNullOrWhiteSpace(conn))
            {
                Console.WriteLine($"[DEBUG] Connection string is null or empty. Returning BadRequest.");
                return Results.BadRequest("Invalid company code.");
            }

            try
            {
                var branches = await loginRepo.GetBranchesForUserAsync(username, conn);
                Console.WriteLine($"[DEBUG] GetBranchesForUserAsync found {branches?.Count ?? 0} branches");

                if (branches == null || !branches.Any())
                {
                    branches = await loginRepo.GetAllLoginBranchesAsync(conn);
                    Console.WriteLine($"[DEBUG] GetAllLoginBranchesAsync found {branches?.Count ?? 0} branches");
                }

                var fyears = await loginRepo.GetOpenFinancialYearsAsync(conn);
                Console.WriteLine($"[DEBUG] GetOpenFinancialYearsAsync found {fyears?.Count ?? 0} financial years");

                return Results.Ok(new PreLoginDatas(branches, fyears, SubEndDate, IsExpired, IsTrial));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in prelogin query: {ex}");
                return Results.Problem(ex.Message);
            }
        });

        // ==============================
        // LOGIN (POST)
        // ==============================
        group.MapPost("/login", async (
            [FromBody] LoginRequest req,
            [FromServices] ITenantRepository tenantRepository,
            [FromServices] IJwtTokenService jwtService,
            [FromServices] ICompanyCreationRepo repo,
            [FromServices] IAdminUserRepo auserRepo,
            [FromServices] IConfiguration config,
            [FromServices] ILoginRepo loginRepo) =>
        {
            if (string.IsNullOrWhiteSpace(req.EmailOrUserName))
                return Results.BadRequest("Email/username required.");

            var parts = req.EmailOrUserName.Split('@');
            var tenantType = config["TenantType"];
            bool isSingleTenant = string.Equals(tenantType, "single", StringComparison.OrdinalIgnoreCase);

            // ===== COMPANY USER LOGIN =====
            if (parts.Length == 2 || req.IsMaui || isSingleTenant)
            {
                var username = (req.IsMaui || parts.Length == 1) ? req.EmailOrUserName : parts[0];
                var tenantCode = req.IsMaui ? "maui" : (parts.Length == 2 ? parts[1].ToUpperInvariant() : "DEFAULT");

                string? conn;

                if (isSingleTenant)
                {
                    var dbProvider = config["DbProvider"] ?? "MSSQL";
                    conn = config[$"ConnectionStrings:{dbProvider}:Master"];
                }
                else if (!req.IsMaui)
                    conn = await tenantRepository.GetConnectionStringAsync(tenantCode);
                else
                    conn = config.GetConnectionString("Master");

                if (!string.IsNullOrWhiteSpace(conn))
                {
                    var user = await loginRepo.GetUserByCredentialsAsync(
                        username,
                        req.Password,
                        conn);

                    if (user != null)
                    {
                        var token = jwtService.GenerateToken(username, tenantCode);
                        return Results.Ok(new LoginResponse(1, token, true));
                    }
                }
            }

            // ===== ADMIN LOGIN =====
            var adminUser = await auserRepo.GetForLoginAsync(
                req.EmailOrUserName,
                req.Password);

            if (adminUser != null)
            {
                var token = jwtService.GenerateToken(req.EmailOrUserName, "79891");
                return Results.Ok(new LoginResponse(1, token, true, true));
            }

            return Results.Unauthorized();
        });

        return group;
    }
}
