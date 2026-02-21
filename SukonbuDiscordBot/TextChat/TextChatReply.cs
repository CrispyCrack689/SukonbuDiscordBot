using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace SukonbuDiscordBot.TextChat
{
    internal class TextChatReply
    {
        // Add a parameter for DiscordSocketClient to the handler
        public static async Task SlashCommandHandler(SocketSlashCommand command, DiscordSocketClient client)
        {
            switch (command.Data.Name)
            {
            case "members_birthday":
                JArray birthday = JArray.Parse(File.ReadAllText(ExternalFiles.BIRTHDAYS_FILE));
                StringBuilder birthdayList = new StringBuilder();
                birthdayList.AppendLine("**誕生日一覧**\n");
                
                foreach (JToken item in birthday)
                {
                    string userIdStr = item["UserId"]?.ToString();
                    string birthdayStr = item["Birthday"]?.ToString();
                    if (ulong.TryParse(userIdStr, out ulong userId) && DateTime.TryParse(birthdayStr, out DateTime birthdayDate))
                    {
                        // REST APIで直接ユーザー情報を取得
                        string userName;
                        {
                            try
                            {
                                var restUser = await client.Rest.GetUserAsync(userId);
                                userName = restUser?.Username ?? $"ユーザーID: {userId}";
                            }
                            catch
                            {
                                userName = $"ユーザーID: {userId}";
                            }
                        }
                        string formattedBirthday = birthdayDate.ToString("MM月dd日");
                        birthdayList.AppendLine($"🎂 **{userName}**: {formattedBirthday}");
                    }
                }
                await command.RespondAsync(birthdayList.ToString());
                break;
            default:
                break;
            }
        }
    }
}
