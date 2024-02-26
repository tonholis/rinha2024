using Api.Data;
using Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Api
{
	public class AccountService
	{
		private readonly ApiDbContext _db;

		public AccountService(ApiDbContext db)
		{
			_db = db;
		}

		public async Task<Customer> SaveOperationAsync(Transaction t, int customerId)
		{	
			var result = (await _db.Database
				.SqlQuery<AddTransactionResult>($"SELECT * FROM add_transaction({customerId}, {t.Amount}, {t.Type}, {t.Description})")
				.ToListAsync())[0];				

			if (result.Error == 1)
				throw new CustomerNotFoundException();

			if (result.Error == 2)
				throw new InsufficientBalanceException();

			return result;		
		}

		public async Task<Customer?> GetCustomer(int customerId)
		{
			if (customerId < 1 || customerId > 5)
				return null;

			return await _db.Customers.FindAsync(customerId);
		}
	}

	public class CustomerNotFoundException : Exception { }

	public class InsufficientBalanceException : Exception { }

	public class AddTransactionResult : Customer
	{
		public int Error { get; set; }
	}


}
