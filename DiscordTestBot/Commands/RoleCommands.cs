using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordTestBot.MySql;

namespace DiscordTestBot.Commands
{
    public class RoleCommands : BaseCommandModule
    {
        [Command("role")]
        [Description("Gives written role for 50 gold")]
        //[RequireRoles(RoleCheckMode.Any, "Moderator")]
        public async Task AssignRole(CommandContext context, string text)
        {
            if (text != "Moderator")
            {
                Connection connection = new Connection();
                int gold = 0;
                var joinEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Role: {text}" + "\nPlease choose suitable option:" +
                    "\n 1. assign role \n 2. delete role from user",
                    Color = DiscordColor.Black
                };
                var rolesCollection = context.Guild.Roles.Values;
                var list = rolesCollection.ToList();
                DiscordRole role1 = null;
                foreach (var x in list)
                {
                    if (x.Name == text)
                    {
                        role1 = x;
                    }
                }
                if (role1 != null)
                {
                    var joinMessage = await context.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

                    var thumbsUpEmoji = DiscordEmoji.FromName(context.Client, ":+1:");
                    var thumbsDownEmoji = DiscordEmoji.FromName(context.Client, ":-1:");

                    await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
                    await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

                    var interactivity = context.Client.GetInteractivity();

                    var reactionResult = await interactivity.WaitForReactionAsync
                    (x => x.Message == joinMessage &&
                    x.User == context.User &&
                    (x.Emoji == thumbsUpEmoji || x.Emoji == thumbsDownEmoji)
                    ).ConfigureAwait(false);

                    if (reactionResult.Result.Emoji == thumbsUpEmoji)
                    {
                        var nickName = context.User.Username;  //works
                        gold = connection.GetAmountOfGold(nickName);
                        if (gold >= 50)
                        {
                            await context.Member.GrantRoleAsync(role1).ConfigureAwait(false);
                            connection.AssignRole(nickName);
                            await context.Channel.SendMessageAsync($"Role {text} successfully assigned").ConfigureAwait(false);
                            Console.WriteLine($"Role {text} successfully assigned");
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"Role {text} was not assigned, because you dont have enough gold").ConfigureAwait(false);
                        }

                    }
                    if (reactionResult.Result.Emoji == thumbsDownEmoji)
                    {
                        await context.Member.RevokeRoleAsync(role1).ConfigureAwait(false);
                        await context.Channel.SendMessageAsync($"Role {text} has successfully been deleted from user").ConfigureAwait(false);
                    }

                    await joinMessage.DeleteAsync().ConfigureAwait(true);
                }
                else
                {
                    await context.Channel.SendMessageAsync($"There is no role named: {text}").ConfigureAwait(false);
                }
            }
        }
    }
}
