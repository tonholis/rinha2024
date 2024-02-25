using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class ApiDb : DbContext
	{
		public ApiDb(DbContextOptions<ApiDb> options) : base(options) { }

		public DbSet<Transaction> Transactions => Set<Transaction>();
		public DbSet<Customer> Customers => Set<Customer>();

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
				.UseSnakeCaseNamingConvention();
		}

	}
}
