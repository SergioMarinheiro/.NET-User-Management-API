var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var validToken = builder.Configuration["Auth:Token"] ?? "demo-token-123";

var app = builder.Build();

// Middleware: catch unhandled exceptions and return a consistent JSON error payload.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal server error."
        });
    }
});

// Middleware: validate the bearer token from incoming requests and deny unauthorized access.
app.Use(async (context, next) =>
{
    var authorizationHeader = context.Request.Headers.Authorization.ToString();

    if (string.IsNullOrWhiteSpace(authorizationHeader) ||
        !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Unauthorized. A valid bearer token is required."
        });
        return;
    }

    var token = authorizationHeader["Bearer ".Length..].Trim();
    if (!string.Equals(token, validToken, StringComparison.Ordinal))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Unauthorized. Invalid token."
        });
        return;
    }

    await next();
});

// Middleware: log incoming HTTP requests and outgoing responses, including method, path, status code, and elapsed time.
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    var requestMethod = context.Request.Method;
    var requestPath = context.Request.Path;

    // Capture the response status after the next middleware runs.
    await next();

    var elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;
    var statusCode = context.Response.StatusCode;

    Console.WriteLine($"[{DateTime.UtcNow:O}] HTTP {requestMethod} {requestPath} -> {statusCode} in {elapsedMs} ms");
});



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/", () => "I am root");
app.MapGet("/throw", () =>
{
    throw new Exception("Test exception");
});
app.MapControllers();

app.Run();
