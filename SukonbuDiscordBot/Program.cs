using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SukonbuDiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private ulong _channelIdVoice;
        private ulong _channelIdChat;

        static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // 初期化
            DiscordSocketConfig config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildVoiceStates | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(config);

            // イベントハンドラを設定
            _client.Log += Log;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.MessageReceived += ChatBotAsync;

            // 設定ファイルを読み込む
            var setting = JObject.Parse(File.ReadAllText("data/_settings.json"));
            var token = setting["BotToken"].ToString();
            _channelIdVoice = ulong.Parse(setting["ChannelId_Voice"].ToString());
            _channelIdChat = ulong.Parse(setting["ChannelId_Chat"].ToString());

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // 準備完了するまで待機
            await Task.Delay(1000);

            // ループさせる
            await Task.Delay(-1);
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// ボイスチャンネルに入室したら通知
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            // 新規入室時/退出時以外は無視
            if (before.VoiceChannel != null && after.VoiceChannel == null) return;

            // 正しいチャンネルか確認
            if (_client.GetChannel(_channelIdVoice) is IMessageChannel channel)
            {
                //await channel.SendMessageAsync("@everyone " + user.Username + " has joined the voice channel!");
                await channel.SendMessageAsync(user.Username + " has joined the voice channel!");
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
            if (message.Channel.Id != _channelIdChat) return;

            // 正しいチャンネルか確認
            if (_client.GetChannel(_channelIdChat) is IMessageChannel channel)
            {
                // ヘルプ
                if (message.Content == "help")
                {
                    await channel.SendMessageAsync("・members birthday\n・すこんぶ");
                }

                // メンバーの誕生日
                if (message.Content == "members birthday")
                {
                    var birthday = JObject.Parse(File.ReadAllText("data/_birthdays.json"));
                    var birthdayResponse = birthday["Birthdays"];

                    await channel.SendMessageAsync(birthdayResponse.ToString());
                }

                if (message.Content == "ping")
                {
                    await channel.SendMessageAsync("pong");
                }

                if (message.Content == "hello")
                {
                    await channel.SendMessageAsync("world");
                }

                if (message.Content == "すこんぶ")
                {
                    await channel.SendMessageAsync("すっぱい");
                }
            }
        }
    }
}
