using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

static class Bot
{
    private static DateTimeOffset startTime;
    public static void TelegramBot()
    {
        ITelegramBotClient client = new TelegramBotClient(BotToken.Token);
        startTime = DateTimeOffset.Now;
        client.StartReceiving(UpdateHandler, ErrorHandler);
    }

    private static async Task UpdateHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        if (update.Type is not UpdateType.Message || update.Message is null || update.Message.From is null)
        {
            return;
        }

        if (update.Message.Date < startTime)
        {
            return;
        }
        
        if (update.Type is UpdateType.Message  && update.Message?.From?.IsBot is false)
        {
            try
            {
                await client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    await Processing.Reduplicate(update.Message.Text),
                    cancellationToken: ct);
            }
            catch (ArgumentException e)
            {
                await client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "хуйню какую-то спизданул",
                    cancellationToken: ct);
                await SendLogs("logs.txt",
                    new
                    {
                        e.Message,
                        update
                    });
            }
            catch (FormatException e)
            {
                await client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "отъебись))",
                    cancellationToken: ct);
                await SendLogs("logs.txt",
                    new
                    {
                        e.Message,
                        update
                    });
            }
            catch (Exception e)
            {
                await client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "иди нахуй",
                    cancellationToken: ct);
                await SendLogs("logs.txt",
                    new
                    {
                        e.Message,
                        update
                    });
            }
        }
    }

    private static async Task ErrorHandler(
        ITelegramBotClient client,
        Exception ex,
        CancellationToken ct)
    {
        await SendLogs("logs.txt", ex);
    }
    
    private static async Task SendLogs(string path, object obj)
    {
        await using (var sw = new StreamWriter(path, true))
        {
            await sw.WriteLineAsync(JsonSerializer.Serialize(obj));
            await sw.WriteLineAsync();
        }
    }
}