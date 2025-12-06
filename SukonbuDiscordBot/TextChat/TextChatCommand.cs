using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using SukonbuDiscordBot.Manager;
using SukonbuDiscordBot.Utils;

namespace SukonbuDiscordBot.TextChat
{
    internal class TextChatCommand
    {
        public static async Task Register()
        {
            var command = new SlashCommandBuilder();
            command.WithName("members_birthday");
            command.WithDescription("メンバーの誕生日一覧");

            try
            {
                DiscordSocketClient client = ClientManager.GetInstance.GetClient;
                await client.CreateGlobalApplicationCommandAsync(command.Build());
            }
            catch (HttpException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                await Log.Trace(LogSeverity.Error, json);
            }
        }
    }
}
