using DiscordTestBot.MySql;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using MySqlConnector;
using System.Data;
using System.Linq;

namespace DiscordTestBot.Commands
{
    public class CommandsClass : BaseCommandModule
    {
        Connection connection = new Connection();
        [Command("HI")]
        [Description("Says hello")]
        public async Task Ping(CommandContext context, string text)
        {
            await context.Channel.SendMessageAsync("HELLO").ConfigureAwait(false);
        }
        [Command("roll")]
        [Description("Random number between 0-100")]
        public async Task RollRandom(CommandContext context)
        {
            Random rdm = new Random();
            await context.Channel.SendMessageAsync("1-100: " + rdm.Next(0, 101).ToString()).ConfigureAwait(false);
        }
        [Command("gold")]
        [Description("Shows amount of money you have")]
        public async Task GetAmountOfGold(CommandContext context)
        {
            string nickName = context.Message.Author.Username;
            int gold;
            gold = connection.GetAmountOfGold(nickName);
            Console.WriteLine(gold);
            if (gold == 0)
            {
                connection.CreateNewUser(nickName);
                gold = connection.GetAmountOfGold(nickName);
                Console.WriteLine("Gold is zero");
            }
            await context.Channel.SendMessageAsync("Gold: "
            + gold.ToString() + "\n" + nickName).ConfigureAwait(false);
        }
        [Command("goldFromUserName")]
        [Description("Shows written user's amount of gold")]
        public async Task GetAmountOfGoldByUserName(CommandContext context, string nickName)
        {
            int gold;
            gold = connection.GetAmountOfGold(nickName);
            Console.WriteLine(gold);
            if (gold == 0)
            {
                connection.CreateNewUser(nickName);
                gold = connection.GetAmountOfGold(nickName);
                Console.WriteLine("Gold is zero");
            }
            await context.Channel.SendMessageAsync("Gold: "
            + gold.ToString() + "\n" + nickName).ConfigureAwait(false);
        }

        [Command("addGold")]
        [Description("Adds written amount of money to you")]
        [RequireRoles(RoleCheckMode.Any, "Moderator")]
        public async Task AddGold(CommandContext context, string text)
        {
            int gold = Convert.ToInt32(text);
            string nickName = context.Message.Author.Username;
            connection.AddGold(nickName, gold);
            gold = connection.GetAmountOfGold(nickName);
            await context.Channel.SendMessageAsync("Gold: " 
            + gold.ToString() + "\n" + nickName).ConfigureAwait(false);
        }
        [Command("addGoldTo")]
        [Description("Adds 100 gold to written person")]
        [RequireRoles(RoleCheckMode.Any, "Moderator")]
        public async Task AddGoldTo(CommandContext context, string nickName)
        {
            int gold;
            gold = connection.GetAmountOfGold(nickName);
            Console.WriteLine(gold);
            if (gold == 0)
            {
                connection.CreateNewUser(nickName);
                gold = connection.GetAmountOfGold(nickName);
                Console.WriteLine("Gold is zero");
            }
            else
            {
                gold = 100;
                connection.AddGold(nickName, gold);
                gold = connection.GetAmountOfGold(nickName);
                await context.Channel.SendMessageAsync("Gold: "
                + gold.ToString() + "\n" + nickName).ConfigureAwait(false);
            }
        }
        [Command("whoIsPidr")]
        public async Task DefinePidor(CommandContext context)
        {
            string nickName = context.Message.Author.Username;
            var members = context.Guild.Members.Values;
            var list = members.ToList();

            Random rnd = new Random();
            int randomNumber = rnd.Next(0, list.Count);
            foreach (var user in list)
            {
                await context.RespondAsync(randomNumber.ToString());
            }
        }
    }
}
