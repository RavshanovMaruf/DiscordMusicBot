using DiscordTestBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;

namespace DiscordTestBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension interactivity { get; private set; }

        
        public async Task RunBotAsync(string[] args)
        {
            

            //var lavalink = 

            var json = string.Empty;

            //reads info from json file
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            //assigns var json to configJson in proper way
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            #region for lavalink
            var endPoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };
            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endPoint,
                SocketEndpoint = endPoint
            };
            #endregion

            var config = new DiscordConfiguration
            {
                Token = configJson.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            Client = new DiscordClient(config);//client with cfg

            Client.Ready += ClientReady;    //handle event when bot is activated

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });


            var commandsCfg = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,
            };
            Commands = Client.UseCommandsNext(commandsCfg);//commands with cfg
            Commands.RegisterCommands<CommandsClass>();
            Commands.RegisterCommands<RoleCommands>();
            Commands.RegisterCommands<LavalinkCommandsClass>();

            var lavalink = Client.UseLavalink();
            await Client.ConnectAsync();    //set bot active

            await lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);           //dont turn off bot, int inside brackets is seconds
        }

        private Task ClientReady(DiscordClient sender, ReadyEventArgs e)//event 
        {
            Console.WriteLine("Bot is Online");
            return Task.CompletedTask;
        }
    }
}
