using Microsoft.EntityFrameworkCore;
namespace AshproSAASERP.Interface;

public class MasterDBContext : DbContext
{
    public MasterDBContext(DbContextOptions<MasterDBContext> options) : base(options)
    {

    }
    public DbSet<Models.Company.LoginCompany> LoginCompanies { get; set; }
    public DbSet<Models.Company.AdminUser> AdminUsers { get; set; }

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Models.Company.AdminUser>().HasData(
            new Models.Company.AdminUser
            {
                UserId = 1,
                UserName = "admin",
                Password = "ig79891",
                IsActive = true,
                Remarks = "Default admin user"
            }
        );

    }
}


