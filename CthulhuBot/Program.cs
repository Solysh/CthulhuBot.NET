using Discord.Interactions;
using Discord.WebSocket;
using CthulhuBot.Log;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.Yaml;
using CthulhuBot.Modules;
using Discord.Commands;
using Discord;
using System.Net.Sockets;

namespace CthulhuBot
{
    public class program
    {
        private DiscordSocketClient _client;

        // Program entry point
        public static Task Main(string[] args) => new program().MainAsync();

        public async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
            // this will be used more later on
            .SetBasePath(AppContext.BaseDirectory)
            // I chose using YML files for my config data as I am familiar with them
            .AddYamlFile("config.yml")
            .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
            services
            // Add the configuration to the registered services
            .AddSingleton(config)
            // Add the DiscordSocketClient, along with specifying the GatewayIntents and user caching
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
                LogLevel = Discord.LogSeverity.Debug
            }))
            // Adding console logging
            .AddTransient<ConsoleLogger>()
            // Used for slash commands and their registration with Discord
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            // Required to subscribe to the various client events used in conjunction with Interactions
            .AddSingleton<InteractionHandler>())
            .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var commands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Subscribe to client log events
            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);
            // Subscribe to slash command log events
            commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);
            // Subscribe to channel messages
            _client.MessageReceived += OnMessageReceived;
            // Subscribe to guild member updates
            _client.GuildMemberUpdated += OnMemberUpdated;
            _client.Ready += async () =>
            {
                // If running the bot with DEBUG flag, register all commands to guild specified in config
                if (IsDebug())
                    // Id of the test guild can be provided from the Configuration object
                    await commands.RegisterCommandsToGuildAsync(UInt64.Parse(config["testGuild"]), true);
                else
                    // If not debug, register commands globally
                    await commands.RegisterCommandsGloballyAsync(true);
            };

            await _client.LoginAsync(Discord.TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnMemberUpdated(Cacheable<SocketGuildUser, ulong> cachedUser, SocketGuildUser guild)
        {
            ulong guildId = 722900872006205524;
            ulong channelGeneralId = 722903613328064542;

            var user = await cachedUser.GetOrDownloadAsync().ConfigureAwait(false);

            Console.WriteLine("OnMemberUpdated Called!");

            if (!user.Roles.Any(role => role.Name == "Verified").Equals(guild.Roles.Any(role => role.Name == "Verified")))
            {
                // await _client.GetGuild(guildId).GetTextChannel(channelGeneralId).SendMessageAsync($"Please welcome our newest member {user.Mention} to Cthulhu's List Gaming Board!");
                Console.WriteLine("User Now Verified!");
            }

        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            // Channel Id of #general
            ulong guildId = 722900872006205524;
            ulong channelPlayerIntroId = 722903689790095421;
            ulong channelGeneralId = 722903613328064542;
            // ulong removedRoles = 929107169083801600;
            ulong addedRoles = 1091762487180918864;

            int messageLength = socketMessage.Content.Length;

            // We only want messages sent by real users 
            if (!(socketMessage is SocketUserMessage message))
                return;

            // This message handler would be called infinitely
            if (message.Author.Id == _client.CurrentUser.Id)
                return;            

            // Only answer if the channel is #player-introductions and message is over 40 characters
            if (socketMessage.Channel.Id == channelPlayerIntroId && messageLength > 40)
            {
                var user = (IGuildUser)message.Author;

                await socketMessage.Channel.SendMessageAsync($"Nice to meet you, {message.Author.Mention}!", messageReference: new Discord.MessageReference(message.Id));
                await _client.GetGuild(guildId).GetTextChannel(channelGeneralId).SendMessageAsync($"Hey everyone! Please welcome {socketMessage.Author.Mention} to Cthulhu's List Gaming Board!  You can learn more about them over at https://discord.com/channels/{guildId}/{socketMessage.Channel.Id}/{socketMessage.Id}");

                //await user.RemoveRoleAsync(removedRoles);
                await user.AddRoleAsync(addedRoles);
            }
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}