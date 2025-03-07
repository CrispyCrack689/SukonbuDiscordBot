using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

namespace SukonbuDiscordBot
{
    internal class Program
    {
        private DiscordSocketClient m_client;
        private ulong m_channelIdVoice;
        private ulong m_channelIdChat;
        private Dictionary<ulong, DateTime> m_voiceStartTimes = new Dictionary<ulong, DateTime>();

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
                var userName = user.Username;
                var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

                // ユーザーがボイスチャンネルに入室した
                if (before.VoiceChannel == null && after.VoiceChannel != null)
                {
                    m_voiceStartTimes[user.Id] = DateTime.Now;

                    var channelName = after.VoiceChannel.Name;
                    var startTime = m_voiceStartTimes[user.Id].ToString("yyyy/MM/dd HH:mm:ss");

                    var embed = new EmbedBuilder()
                        .WithTitle("通話開始")
                        .AddField("**`チャンネル`**", channelName, true)
                        .AddField("**`始めた人`**", userName + "さん", true)
                        .AddField("**`開始時間`**", startTime, true)
                        .WithColor(0xff8e8e)
                        .WithThumbnailUrl(avatarUrl)
                        .Build();

                    await channel.SendMessageAsync("@everyone", embed: embed);
                }
                else if (before.VoiceChannel != null && after.VoiceChannel == null)
                {
                    var endTime = DateTime.Now;
                    var endTimeString = endTime.ToString("yyyy/MM/dd HH:mm:ss");

                    if (m_voiceStartTimes.TryGetValue(user.Id, out var startTime))
                    {
                        var channelName = before.VoiceChannel.Name;
                        var duration = endTime - startTime;
                        m_voiceStartTimes.Remove(user.Id);

                        // 2桁表示にする
                        var durationHours = duration.Hours.ToString();
                        if (durationHours.Length <= 1)
                        {
                            durationHours = "0" + durationHours;
                        }
                        var durationMinutes = duration.Minutes.ToString();
                        if (durationMinutes.Length <= 1)
                        {
                            durationMinutes = "0" + durationMinutes;
                        }
                        var durationSeconds = duration.Seconds.ToString();
                        if (durationSeconds.Length <= 1)
                        {
                            durationSeconds = "0" + durationSeconds;
                        }

                        var embed = new EmbedBuilder()
                            .WithTitle("通話終了")
                            .AddField("**`チャンネル`**", channelName, true)
                            .AddField("**`通話時間`**", $"{durationHours}:{durationMinutes}:{durationSeconds}", true)
                            .WithColor(0x8e8eff)
                            .Build();

                        await channel.SendMessageAsync(embed: embed);
                    }
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
