using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using SukonbuDiscordBot.TextChat;
using SukonbuDiscordBot.VoiceChat;
using SukonbuDiscordBot.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// 外部ファイルパスをまとめる
/// </summary>
public static class ExternalFiles
{
    public const string TOKEN_FILE = "data/token.json";
    public const string SETTINGS_FILE = "data/settings.json";
    public const string BIRTHDAYS_FILE = "data/birthdays.json";

    public const string TOKEN = "BotToken";

    public const string CHANNEL_ID_CHAT = "ChannelId_Chat";
    public const string CHANNEL_ID_VOICE = "ChannelId_Voice";
    public const string CHANNEL_ID_INFO = "ChannelId_Info";

    public const string CHANNEL_ID_WATCH_VOICE = "ChannelId_WatchVoice";

    public const string CUSTOM_WORKING_DIRECTORY = "Custom_WorkingDirectory";
    public const string CUSTOM_SCRIPT_PATH = "Custom_ScriptPath";
    public const string CUSTOM_VENV_PYTHON_PATH = "Custom_VenvPythonPath";
};

/// <summary>
/// 定数をまとめる
/// </summary>
public static class Constants
{
    public const int START_DELAY = 1000;
    public const int DAILY_TASK_HOUR = 09;
    public const int DAILY_TASK_MINUTE = 00;
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
            Utilities.KillOtherInstances();
            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds
                               | GatewayIntents.GuildMessages
                               | GatewayIntents.GuildVoiceStates
                               | GatewayIntents.MessageContent
                               | GatewayIntents.GuildMembers
            };
            m_client = new DiscordSocketClient(config);

            // カレントディレクトリからファイル読み込み
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // イベントハンドラを設定
            m_client.Log += TraceLog.Log;
            m_client.UserVoiceStateUpdated += (user, before, after) => VoiceChatNotify.UserVoiceStateUpdateAsync(m_client, user, before, after);
            m_client.MessageReceived += (message) => TextChatReply.ChatBotAsync(m_client, message);
            m_client.MessageReceived += (messageDebug) => Internals.GoFileDownload(m_client, messageDebug);

            // 設定ファイルを読み込む
            var tokenFile = JObject.Parse(File.ReadAllText(ExternalFiles.TOKEN_FILE));
            var token = tokenFile[ExternalFiles.TOKEN].ToString();

            // ログイン
            await m_client.LoginAsync(TokenType.Bot, token);
            await m_client.StartAsync();

            // 準備完了するまで待機
            await Task.Delay(Constants.START_DELAY);

            // 定期実行タスク
            var settingsFile = JObject.Parse(File.ReadAllText(ExternalFiles.SETTINGS_FILE));
            var channelIdInfo = ulong.Parse(settingsFile[ExternalFiles.CHANNEL_ID_INFO].ToString());
            var scheduler = new Scheduler();
            scheduler.ScheduleDailyTaskAsync(Constants.DAILY_TASK_HOUR, Constants.DAILY_TASK_MINUTE, async () => await Notification.BirthdayNotify.NotifyTodayIsMyBirthdayAsync(m_client, channelIdInfo));

            // ループさせる
            await Task.Delay(-1);
        }
    }
}
