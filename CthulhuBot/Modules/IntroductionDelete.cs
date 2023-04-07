using CthulhuBot.Log;
using Discord.Interactions;
using Discord.WebSocket;
using Discord;

namespace CthulhuBot.Modules
{
    public class IntroductionDelete : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private static Logger _logger;

        public IntroductionDelete(ConsoleLogger logger)
        {
            _logger = logger;
        }

        [SlashCommand("introdelete", "Delete old introduction posts")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task IntroDelete()
        {
            await _logger.Log(new LogMessage(LogSeverity.Info, "IntroductionDelete : IntroDelete", $"User: {Context.User.Username}, Command: IntroDelete", null));

            

            // Cthulhu's List #player-introduction channel <#722903689790095421>
            ulong channelId = 722903689790095421;
            var channel = await Context.Client.GetChannelAsync(channelId);
            var textChannel = channel as IMessageChannel;
            var messages = await textChannel.GetMessagesAsync(1000).FlattenAsync();

            //var channelMsgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            //foreach (var message in channelMsgs)
            foreach(var message in messages)
            {
                /*
                if (message.Author.Id == Context.Client.CurrentUser.Id)
                {
                    //Console.WriteLine($"{message.Author.Username} is me.");
                }
                
                if (!message.Author.IsBot)
                {
                    //Console.WriteLine($"{message.Author.Username} is not a bot.");
                }
                */

                if (message.Author is not SocketGuildUser socketGuildUser)
                {
                    //var embedElement = new EmbedBuilder().WithTitle("Player Introductions Flagged for Deletion");

                    await Context.Channel.SendMessageAsync($"{message.Author.Username} is no longer in the server.  Deleting introduction.");
                    //embedElement.AddField($"Player: {message.Author.ToString()}", $"Message Id: {message.Id.ToString()}");
                    await textChannel.DeleteMessageAsync(message.Id);
                    //Context.Channel.SendMessageAsync($"Deleteing {message.Id}");

                    //await RespondAsync(embed: embedElement.Build());
                }
            }

            // await RespondAsync(embed: embedElement.Build());

        }
    }
}