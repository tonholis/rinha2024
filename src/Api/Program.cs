using Api;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApiDb>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("DB")));

builder.Services.AddScoped<IValidator<Transaction>, TransactionValidator>();

var app = builder.Build();

app.MapPost("/clientes/{id}/transacoes", async (int id, 
	[FromBody] Transaction transaction,
	IValidator<Transaction> validator,
	ApiDb db) =>
{
	var customer = await db.Customers.FindAsync(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	var validationResult = await validator.ValidateAsync(transaction);
	if (!validationResult.IsValid)
		return Results.BadRequest(validationResult.Errors);

	try
	{
		if (transaction.IsDebit())
			customer.Debit(transaction.Amount);
		else
			customer.Credit(transaction.Amount);

		transaction.CustomerId = id;
		transaction.CreatedAt = DateTime.UtcNow;
		db.Transactions.Add(transaction);
		await db.SaveChangesAsync();

		return Results.Ok(new { limite = customer.Limit, saldo = customer.Balance });
	}
	catch (InvalidOperationException)
	{
		return Results.UnprocessableEntity(new ErrorDto("Saldo insuficiente"));
	}
});

app.MapGet("/clientes/{id}/extrato", async (int id,	ApiDb db) =>
{
	var customer = await db.Customers.FindAsync(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	var transactions = await db.Transactions.Where(t => t.CustomerId == id).ToArrayAsync();
	return Results.Ok(new
	{
		saldo = new { total = customer.Balance, limite = customer.Limit, data_extrato = DateTime.UtcNow },
		ultimas_transacoes = transactions
	});	
});

app.MapPost("/clear", async (ApiDb db) =>
{
	await db.Database.ExecuteSqlRawAsync("UPDATE customers SET balance = 0;TRUNCATE TABLE transactions;");
	return Results.Ok(new { message = "done"});
});

app.Run();

public record ErrorDto(string Message);