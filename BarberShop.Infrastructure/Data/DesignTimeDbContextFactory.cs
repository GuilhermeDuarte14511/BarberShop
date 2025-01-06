using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BarberShop.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BarbeariaContext>
    {
        public BarbeariaContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BarbeariaContext>();
            optionsBuilder.UseSqlServer(
                "Server=tcp:gui14511.database.windows.net,1433;Initial Catalog=BarberShopDb;Persist Security Info=False;User ID=gui14511;Password=Kratos14511@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
            );

            return new BarbeariaContext(optionsBuilder.Options);
        }
    }
}
