using System;

namespace DiscordTestBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RunBotAsync(args).GetAwaiter().GetResult(); //go run downside code, even though func is running async
        }
    }
}
