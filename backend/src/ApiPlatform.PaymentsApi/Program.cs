WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();

var payments = new List<PaymentDto>
{
    new(1, 150.00m, "BRL", "Approved"),
    new(2, 89.90m, "BRL", "Pending"),
    new(3, 320.50m, "BRL", "Refunded"),
};

app.MapGet("/payments", () => Results.Ok(payments));

app.MapGet("/payments/{id:int}", (int id) =>
{
    PaymentDto? payment = payments.FirstOrDefault(p => p.Id == id);
    return payment is not null ? Results.Ok(payment) : Results.NotFound();
});

app.MapPost("/payments", (CreatePaymentRequest request) =>
{
    int newId = payments.Count > 0 ? payments.Max(p => p.Id) + 1 : 1;
    var payment = new PaymentDto(newId, request.Amount, request.Currency, "Pending");
    payments.Add(payment);
    return Results.Created($"/payments/{payment.Id}", payment);
});

app.Run();

public sealed record PaymentDto(int Id, decimal Amount, string Currency, string Status);

public sealed record CreatePaymentRequest(decimal Amount, string Currency);
