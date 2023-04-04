using CthulhuBot.Log;
using Discord.Interactions;
using Discord.WebSocket;
using Discord;

namespace CthulhuBot.Modules
{
    public class IntroductionCheck : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private static Logger _logger;

        public IntroductionCheck(ConsoleLogger logger)
        {
            _logger = logger;
        }

        [SlashCommand("introcheck", "Check introduction posts")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task IntroCheck()
        {
            await _logger.Log(new LogMessage(LogSeverity.Info, "IntroductionCheck : IntroCheck", $"User: {Context.User.Username}, Command: IntroCheck", null));

            var embedElement = new EmbedBuilder().WithTitle("Introduction Check");

            foreach (SocketGuildUser User in Context.Guild.Users)
            {
                var Role = (User as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 971128285071310949);
                if (User.Roles.Contains(Role))
                {
                    embedElement.AddField(User.ToString(), User.Id);
                }
                else
                {
                    //Context.Channel.SendMessageAsync($"{User.ToString()} does not have the role");
                }
            }
            //if (message.Author is SocketGuildUser && ((SocketGuildUser)message.Author).Guild.Id != GuildID)
            //Context.Channel.SendMessageAsync(Context.Guild.MemberCount.ToString());
            await RespondAsync(embed: embedElement.Build());
        }
    }
}