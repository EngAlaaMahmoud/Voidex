builder.Services.AddHttpClient<IETATaxService, ETATaxClientService>(client =>
{
    // Base address optional, ETATaxClientService builds full URLs from config
    client.Timeout = TimeSpan.FromSeconds(30);
});