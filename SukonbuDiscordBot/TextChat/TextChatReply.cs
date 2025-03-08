using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace SukonbuDiscordBot.TextChat
{
    internal abstract class TextChatReply
    {
        /// <summary>
        /// チャットボット
        /// </summary>
        /// <param name="client">クライアント</param>
        /// <param name="message">受信メッセージ</param>
        /// <returns></returns>
        public static async Task ChatBotAsync(DiscordSocketClient client, SocketMessage message)
        {
            // テキストチャンネルのIDを取得
            var setting = JObject.Parse(File.ReadAllText(NS_.Settings.SETTINGS_FILE));
            var channelIdChat = ulong.Parse(setting["ChannelId_Chat"].ToString());

            // ボット自身のメッセージは無視
            // 特定チャンネル以外は無視
            if (message.Author.IsBot) return;
            if (message.Channel.Id != channelIdChat) return;

            if (client.GetChannel(channelIdChat) is IMessageChannel channel)
            {
                switch (message.Content)
                {
                    // コマンド一覧
                    case "help":
                        await channel.SendMessageAsync(
                            "・members birthday\n" +
                            "・すこんぶ"
                        );
                        break;
                    // メンバーの誕生日
                    case "members birthday":
                    {
                        var birthday = JObject.Parse(File.ReadAllText("data/birthdays.json"));
                        var birthdayResponse = birthday["Birthdays"];

                        Debug.Assert(birthdayResponse != null, nameof(birthdayResponse) + " != null");
                        await channel.SendMessageAsync(birthdayResponse.ToString());
                        break;
                    }
                }
            }
        }
    }
}
