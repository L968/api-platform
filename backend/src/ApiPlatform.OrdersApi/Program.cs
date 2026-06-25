WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();

var orders = new List<OrderDto>
{
    new(1, "Widget A", 2, "Pending"),
    new(2, "Widget B", 1, "Shipped"),
    new(3, "Widget C", 5, "Delivered"),
};

app.MapGet("/orders", () => Results.Ok(orders));

app.MapGet("/orders/{id:int}", (int id) =>
{
    OrderDto? order = orders.FirstOrDefault(o => o.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

app.MapPost("/orders", (CreateOrderRequest request) =>
{
    int newId = orders.Count > 0 ? orders.Max(o => o.Id) + 1 : 1;
    var order = new OrderDto(newId, request.Item, request.Quantity, "Pending");
    orders.Add(order);
    return Results.Created($"/orders/{order.Id}", order);
});

app.Run();

sealed record OrderDto(int Id, string Item, int Quantity, string Status);

sealed record CreateOrderRequest(string Item, int Quantity);
