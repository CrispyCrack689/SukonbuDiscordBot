using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Discord;
using Discord.WebSocket;
using SukonbuDiscordBot.Manager;
using SukonbuDiscordBot.TextChat;
using SukonbuDiscordBot.Utils;
using SukonbuDiscordBot.VoiceChat;

/// <summary>
/// 外部ファイルパスをまとめる
/// </summary>
public static class ExternalFiles
{
    public const string TOKEN_FILE = "data/token.json";
    public const string SETTINGS_FILE = "data/settings.json";
    public const string BIRTHDAYS_FILE = "data/birthdays.json";

    public const string TOKEN = "BotToken";
    public const string APP_ID = "AppId";

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
    public const int  START_DELAY = 1000;
    public const int  DAILY_TASK_HOUR = 09;
    public const int  DAILY_TASK_MINUTE = 00;
}

namespace SukonbuDiscordBot
{
    internal class Program
    {
        private static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            // カレントディレクトリからファイル読み込み
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            // クライアント初期化
            ClientManager clientManager = ClientManager.GetInstance;
            clientManager.Initialize();
            // トークン初期化
            TokenManager tokenManager = TokenManager.GetInstance;
            tokenManager.Initialize();

            // イベントハンドラを設定
            DiscordSocketClient client = clientManager.GetClient;
            client.Log += (logMessage) => Log.Trace(logMessage.Severity, logMessage.ToString());
            client.Ready += TextChatCommand.Register;
            client.UserVoiceStateUpdated += (user, before, after) => VoiceChatNotify.UserVoiceStateUpdateAsync(client, user, before, after);
            client.SlashCommandExecuted += (command) => TextChatReply.SlashCommandHandler(command, client);
#if DEBUG
            client.MessageReceived += (messageDebug) => Internals.FileDownload(client, messageDebug);
#endif

            // ログイン
            await client.LoginAsync(TokenType.Bot, tokenManager.GetToken);
            await client.StartAsync();
            // 準備完了するまで待機
            await Task.Delay(Constants.START_DELAY);

            // 定期実行タスク
            JObject settingsFile = JObject.Parse(File.ReadAllText(ExternalFiles.SETTINGS_FILE));
            ulong channelIdInfo = ulong.Parse(settingsFile[ExternalFiles.CHANNEL_ID_INFO].ToString());
            Scheduler scheduler = new Scheduler();
            scheduler.ScheduleDailyTaskAsync(Constants.DAILY_TASK_HOUR, Constants.DAILY_TASK_MINUTE, async () => await Notification.BirthdayNotify.NotifyTodayIsMyBirthdayAsync(client, channelIdInfo));

            // ループさせる
            await Task.Delay(-1);
        }
    }
}
