using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

	public class Transaction
	{
		[Key]
		public long Id { get; set; }

		public int Amount { get; set; }

		public required string Type { get; set; }
		
		public required string Description { get; set; }

		public int CustomerId { get; set; }
		public virtual Customer Customer { get; set; }

		public bool IsDebit() => Type == "d";
		public bool IsCredit() => Type == "c";

	}

	public class Customer
	{
		[Key]
		public int Id { get; set; }
        public int Limit { get; set; }
        public int Balance { get; set; }

		public int Debit(int amount)
		{
			if (Balance - amount < Limit * -1)
				throw new InvalidOperationException("Saldo insuficiente");

			Balance -= amount;
			return Balance;
		}

		public int Credit(int amount)
		{
			Balance += amount;
			return Balance;
		}
	}
}
