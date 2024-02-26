using Api;
using Api.Data;
using Api.Data.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApiDbContext>(options => {
	options.UseNpgsql(builder.Configuration.GetConnectionString("DB"));		
});

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<IValidator<Transaction>, TransactionValidator>();

var app = builder.Build();

app.MapPost("/clientes/{id}/transacoes", async (int id, 
	[FromBody] Transaction transaction,
	IValidator<Transaction> validator,
	AccountService accountService) =>
{
	var validationResult = await validator.ValidateAsync(transaction);
	if (!validationResult.IsValid)
		return Results.UnprocessableEntity(validationResult.Errors);

	try
	{
		var customer = await accountService.SaveOperationAsync(transaction, id);
		return Results.Ok(new { limite = customer.AccountLimit, saldo = customer.Balance });
	}
	catch (InsufficientBalanceException)
	{
		return Results.UnprocessableEntity(new ErrorDto("Saldo insuficiente."));
	}
	catch (CustomerNotFoundException)
	{
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));
	}
});

app.MapGet("/clientes/{id}/extrato", async (int id,	ApiDbContext db, AccountService accountService) =>
{
	var customer = await accountService.GetCustomer(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	var transactions = await db.Transactions
		.AsNoTracking()
		.Where(t => t.CustomerId == id)
		.OrderByDescending(t => t.CreatedAt)
		.Take(10)
		.ToListAsync();
	
	return Results.Ok(new
	{
		saldo = new { 
			total = customer.Balance, 
			limite = customer.AccountLimit, 
			data_extrato = DateTime.UtcNow 
		},
		ultimas_transacoes = transactions
	});	
});

app.MapPost("/reset", async (ApiDbContext db) =>
{
	await db.Database.ExecuteSqlRawAsync("UPDATE customers SET balance = 0;");
	await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE transactions;");
	
	return Results.Ok(new { message = "done"});
});

app.Run();

public record ErrorDto(string Message);