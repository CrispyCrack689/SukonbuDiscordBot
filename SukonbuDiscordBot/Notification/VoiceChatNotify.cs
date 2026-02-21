using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

using SukonbuDiscordBot.Utils;

namespace SukonbuDiscordBot.VoiceChat
{
    internal class VoiceChatNotify
    {
        private static readonly Dictionary<ulong, DateTime> m_voiceStartTimes = new Dictionary<ulong, DateTime>();

        /// <summary>
        /// ボイスチャンネルに入室したら通知
        /// </summary>
        /// <param name="client">クライアント</param>
        /// <param name="user">ユーザー名</param>
        /// <param name="before">直前のVC状態</param>
        /// <param name="after">直後のVC状態</param>
        /// <returns></returns>
        public static async Task UserVoiceStateUpdateAsync(DiscordSocketClient client, SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            var setting = JObject.Parse(File.ReadAllText(ExternalFiles.SETTINGS_FILE));
            // ボイス通知チャンネルのIDを取得
            var channelIdVoice = ulong.Parse(setting[ExternalFiles.CHANNEL_ID_VOICE].ToString());
            // 監視対象のボイスチャンネルを取得
            var channelIdWatchVoice = setting[ExternalFiles.CHANNEL_ID_WATCH_VOICE].ToObject<List<ulong>>();

            if (client.GetChannel(channelIdVoice) is IMessageChannel channel)
            {
                // ユーザーがボイスチャンネルに入室した
                if (before.VoiceChannel == null && after.VoiceChannel != null)
                {
                    // 対象のボイスチャンネルでなければ無視
                    if (!channelIdWatchVoice.Contains(after.VoiceChannel.Id))
                    {
                        return;
                    }
                    // すでにユーザーが通話中の場合は無視
                    // すでに通話中の場合は開始時間を更新しない
                    if (after.VoiceChannel.ConnectedUsers.Count > 1)
                    {
                        return;
                    }

                    // チャンネルIDをキーにして開始時間を保存
                    m_voiceStartTimes[after.VoiceChannel.Id] = DateTime.Now;

                    var userName = user.Username;
                    var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
                    var channelName = after.VoiceChannel.Name;
                    var startTime = m_voiceStartTimes[after.VoiceChannel.Id].ToString("yyyy/MM/dd HH:mm:ss");
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
                // ユーザーがボイスチャンネルから退出した
                else if (before.VoiceChannel != null && after.VoiceChannel == null)
                {
                    // 通話チャンネルにまだ人がいる場合は通知しない
                    if (before.VoiceChannel.ConnectedUsers.Count > 0)
                    {
                        return;
                    }
                    // 通話開始時間が取得できなかった
                    if (!m_voiceStartTimes.TryGetValue(before.VoiceChannel.Id, out var startTime))
                    {
                        await Log.Trace(LogSeverity.Info, $"Coudn't get chat start time: {before.VoiceChannel.Name}");
                        return;
                    }

                    // 開始時間の情報を削除
                    m_voiceStartTimes.Remove(before.VoiceChannel.Id);

                    // 2桁表示にする
                    var endTime = DateTime.Now;
                    var duration = endTime - startTime;
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

                    var channelName = before.VoiceChannel.Name;
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
}
