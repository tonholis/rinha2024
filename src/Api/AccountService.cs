using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api
{

    public class AccountService
	{
		private readonly ApiDb _db;

		public AccountService(ApiDb db)
		{
			_db = db;
		}

		public async Task<Customer> SaveOperation(Transaction transaction, Customer customer)
		{
			if (transaction.IsDebit())
			{
				if (customer.Balance - transaction.Amount < customer.Limit * -1)
					throw new InsufficientBalanceException();

				customer.Balance -= transaction.Amount;
			}
			else
				customer.Balance += transaction.Amount;

			transaction.CustomerId = customer.Id;
			_db.Transactions.Add(transaction);

			await SaveChanges();

			return customer;
		}

		private async Task SaveChanges()
		{
			var saved = false;
			while (!saved)
			{
				try
				{	
					await _db.SaveChangesAsync();
					saved = true;
				}
				catch (DbUpdateConcurrencyException ex)
				{
					foreach (var entry in ex.Entries)
					{
						if (entry.Entity is Customer)
						{
							var databaseValues = await entry.GetDatabaseValuesAsync();
							entry.OriginalValues.SetValues(databaseValues);
						}
						else
						{
							throw new NotSupportedException(
								"Don't know how to handle concurrency conflicts for "
								+ entry.Metadata.Name);
						}
					}
				}
			}
		}

		public async Task<Customer?> GetCustomer(int customerId)
		{
			if (customerId < 1 || customerId > 5)
				return null;

			return await _db.Customers.FindAsync(customerId);
		}
	}

	public class CustomerNotFoundException : Exception { }
}
