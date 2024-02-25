using Api;
using Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApiDb>(options => {
	options.UseNpgsql(builder.Configuration.GetConnectionString("DB"));
		
	options.EnableServiceProviderCaching();

	if (env.IsDevelopment())
		options.EnableDetailedErrors();
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

	var customer = await accountService.GetCustomer(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	try
	{
		var account = await accountService.SaveOperation(transaction, customer);
		return Results.Ok(new { limite = account.Limit, saldo = account.Balance });
	}
	catch (InsufficientBalanceException)
	{
		return Results.UnprocessableEntity(new ErrorDto("Saldo insuficiente"));
	}
});

app.MapGet("/clientes/{id}/extrato", async (int id,	ApiDb db, AccountService accountService) =>
{
	var customer = await accountService.GetCustomer(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	var transactions = await db.Transactions
		.AsNoTracking()
		.Where(t => t.CustomerId == id)
		.ToArrayAsync();
	
	return Results.Ok(new
	{
		saldo = new { 
			total = customer.Balance, 
			limite = customer.Limit, 
			data_extrato = DateTime.UtcNow 
		},
		ultimas_transacoes = transactions
	});	
});

app.MapPost("/clear", async (ApiDb db) =>
{
	await db.Database.ExecuteSqlRawAsync("UPDATE customers SET balance = 0;");
	await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE transactions;");
	
	return Results.Ok(new { message = "done"});
});

app.Run();

public record ErrorDto(string Message);