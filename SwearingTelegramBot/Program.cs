var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", Bot.TelegramBot);

app.Run();