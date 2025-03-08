using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

using SukonbuDiscordBot.Utils;
using SukonbuDiscordBot.TextChat;
using SukonbuDiscordBot.VoiceChat;

namespace NS_
{
    public static class ExternalFiles
    {
        public const string SETTINGS_FILE = "data/settings.json";
        public const string BIRTHDAYS_FILE = "data/birthdays.json";

        public const string TOKEN = "BotToken";

        public const string CHANNEL_ID_CHAT = "ChannelId_Chat";
        public const string CHANNEL_ID_VOICE = "ChannelId_Voice";
    }
}

namespace SukonbuDiscordBot
{
    internal class Program
    {
        private DiscordSocketClient m_client;

        private static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            // 初期化
            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildVoiceStates | GatewayIntents.MessageContent
            };
            m_client = new DiscordSocketClient(config);

            // イベントハンドラを設定
            m_client.Log += TraceLog.Log;
            m_client.UserVoiceStateUpdated += (user, before, after) => VoiceChatNotify.UserVoiceStateUpdateAsync(m_client, user, before, after);
            m_client.MessageReceived += (message) => TextChatReply.ChatBotAsync(m_client, message);

            // 設定ファイルを読み込む
            var setting = JObject.Parse(File.ReadAllText(NS_.ExternalFiles.SETTINGS_FILE));
            var token = setting[NS_.ExternalFiles.TOKEN].ToString();

            // ログイン
            await m_client.LoginAsync(TokenType.Bot, token);
            await m_client.StartAsync();

            // 準備完了するまで待機
            await Task.Delay(1000);

            // ループさせる
            await Task.Delay(-1);
        }
    }
}
