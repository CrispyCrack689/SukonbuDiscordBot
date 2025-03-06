using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SukonbuDiscordBot
{
    internal class Program
    {
        private DiscordSocketClient m_client;
        private ulong m_channelIdVoice;
        private ulong m_channelIdChat;

        private static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            // 初期化
            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildVoiceStates | GatewayIntents.MessageContent
            };
            m_client = new DiscordSocketClient(config);

            // イベントハンドラを設定
            m_client.Log += Log;
            m_client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            m_client.MessageReceived += ChatBotAsync;

            // 設定ファイルを読み込む
            var setting = JObject.Parse(File.ReadAllText("data/settings.json"));
            var token = setting["BotToken"]?.ToString();
            m_channelIdVoice = ulong.Parse(setting["ChannelId_Voice"]?.ToString() ?? throw new InvalidOperationException());
            m_channelIdChat = ulong.Parse(setting["ChannelId_Chat"]?.ToString() ?? throw new InvalidOperationException());

            // ログイン
            await m_client.LoginAsync(TokenType.Bot, token);
            await m_client.StartAsync();

            // 準備完了するまで待機
            await Task.Delay(1000);

            // ループさせる
            await Task.Delay(-1);
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="message">コンソールメッセージ</param>
        /// <returns></returns>
        private static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// ボイスチャンネルに入室したら通知
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="before">直前のVC状態</param>
        /// <param name="after">直後のVC状態</param>
        /// <returns></returns>
        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (m_client.GetChannel(m_channelIdVoice) is IMessageChannel channel)
            {
                // ユーザーがボイスチャンネルに入室した
                if (before.VoiceChannel == null && after.VoiceChannel != null)
                {
                    await channel.SendMessageAsync(user.Username + " has joined the voice channel!");
                }
                // ユーザーがボイスチャンネルから退出した
                else if (before.VoiceChannel != null && after.VoiceChannel == null)
                {
                    await channel.SendMessageAsync(user.Username + " has left the voice channel!");
                }
            }
        }

        /// <summary>
        /// チャットボット
        /// </summary>
        /// <param name="message">受信メッセージ</param>
        /// <returns></returns>
        private async Task ChatBotAsync(SocketMessage message)
        {
            // ボット自身のメッセージは無視
            // 特定チャンネル以外は無視
            if (message.Author.IsBot) return;
            if (message.Channel.Id != m_channelIdChat) return;

            if (m_client.GetChannel(m_channelIdChat) is IMessageChannel channel)
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
