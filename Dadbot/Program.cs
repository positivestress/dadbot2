using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Discord;
using Discord.WebSocket;

namespace DotnetTestBot
{
    class Program
    {
        private static readonly string _token = ConfigurationManager.AppSettings["Token"];
        private static readonly string _dadbotToken = ConfigurationManager.AppSettings["DadbotToken"];
        private static List<IMessage> _welcomeMessages = new List<IMessage>();
        private static DiscordSocketClient client = new DiscordSocketClient();

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client.Log += Log;
            client.MessageReceived += MessageReceived;
            client.UserJoined += NewUser;
            client.UserLeft += DeleteWelcomeMessage;
            client.Ready += ClientReady;

            await client.LoginAsync(TokenType.Bot, _dadbotToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task ClientReady()
        {
            var channel = client.GetChannel(431145275118190623) as ISocketMessageChannel;
            await channel.SendMessageAsync("HELLO");
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content == ".help")
            {
                var test = await msg.Channel.SendMessageAsync("I can't do anything yet! Sorry!");
            }
            else
            {
                SocketRole whomst = (msg.Author as SocketGuildUser).Roles.SingleOrDefault(r => r.Name == "Whomst?");
                if(whomst != null && msg.Content.Length > 6)
                {
                    await (msg.Author as SocketGuildUser).RemoveRoleAsync(whomst);
                    await (msg as SocketUserMessage).AddReactionAsync(new Emoji("👋"));
                    await DeleteWelcomeMessage(msg.Author as SocketGuildUser);
                }
            }
        }

        private async Task NewUser(SocketGuildUser user)
        {
            var channel = user.Guild.Channels.Single(c => c.Name == "introductions") as ISocketMessageChannel;
            var rules = user.Guild.Channels.Single(c => c.Name == "rules") as ITextChannel;
            var role = user.Guild.Roles.Single(r => r.Name == "Whomst?");
            await user.AddRoleAsync(role);
            var welcomeMessage = await channel.SendMessageAsync($"Welcome, {user.Mention}! At the moment you can only post here, so please read the {rules.Mention} and then introduce yourself (name, age, pronouns, social media, whatever you might want people to know about you) to get access to the rest of the server!");
            _welcomeMessages.Add(welcomeMessage);
        }

        private async Task DeleteWelcomeMessage(SocketGuildUser user)
        {
            IMessage[] welcomes = _welcomeMessages.Where(m => m.MentionedUserIds.Contains(user.Id)).ToArray();
            foreach(IMessage welcome in welcomes)
            {
                await welcome.Channel.DeleteMessageAsync(welcome);
            }
        }
        
    }
}
