using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SukonbuDiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private ulong _channelId;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            // 設定ファイルを読み込む
            var config = JObject.Parse(File.ReadAllText("config.json"));
            var token = config["BotToken"].ToString();
            _channelId = ulong.Parse(config["ChannelId"].ToString());

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // ボットが終了しないように待機
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
            // ユーザーがボイスチャンネルに入室したとき
            if (before.VoiceChannel == null && after.VoiceChannel != null)
            {
                if (_client.GetChannel(_channelId) is IMessageChannel channel)
                {
                    await channel.SendMessageAsync("@everyone " + user.Username + " has joined the voice channel!");
                }
            }
        }
    }
}
