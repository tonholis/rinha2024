using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
		[JsonIgnore]
		public long Id { get; set; }

		[JsonPropertyName("valor")]
		public int Amount { get; set; }

		[JsonPropertyName("tipo")]
		public required string Type { get; set; }

		[JsonPropertyName("descricao")]
		public required string Description { get; set; }

		[JsonPropertyName("realizada_em")]
		public DateTime CreatedAt { get; set; }

		[JsonIgnore]
		public int CustomerId { get; set; }

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
