using Api;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApiDb>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("ApiDb")));


builder.Services.AddScoped<IValidator<Transaction>, TransactionValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapPost("/clientes/{id}/transacoes", async (int id, 
	[FromBody] Transaction transaction,
	IValidator<Transaction> validator,
	ApiDb db) =>
{
	var result = await validator.ValidateAsync(transaction);
	if (!result.IsValid)
		return Results.BadRequest(result.Errors);

	var customer = await db.Customers.FindAsync(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	try
	{
		if (transaction.IsDebit())
			customer.Debit(transaction.Amount);
		else
			customer.Credit(transaction.Amount);

		transaction.CustomerId = id;
		db.Transactions.Add(transaction);
		await db.SaveChangesAsync();

		return Results.Ok(customer);
	}
	catch (InvalidOperationException)
	{
		return Results.UnprocessableEntity(new ErrorDto("Saldo insuficiente"));
	}
})
.WithName("Transacoes")
.WithOpenApi();

app.MapGet("/clientes/{id}/extrato", async (int id,	ApiDb db) =>
{
	var customer = await db.Customers.FindAsync(id);
	if (customer is null)
		return Results.NotFound(new ErrorDto("Cliente nao encontrado"));

	var transactions = await db.Transactions.Where(t => t.CustomerId == id).ToArrayAsync();
	return Results.Ok(new ExtratoDto(customer, transactions));
	
})
.WithName("Extrato")
.WithOpenApi();

app.Run();


public record ErrorDto(string Message);

public record BalanceDto(int Total, DateTime DataExtrato, int Limite);
public class ExtratoDto
{
    public ExtratoDto(Customer customer, Transaction[] transactions)
    {
		Saldo = new BalanceDto(customer.Balance, DateTime.UtcNow, customer.Limit);
		UltimasTransacoes = transactions;
    }

    public BalanceDto Saldo { get; }
	public Transaction[] UltimasTransacoes { get; }
}