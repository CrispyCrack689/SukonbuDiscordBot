using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using SukonbuDiscordBot.Utils;

namespace SukonbuDiscordBot.VoiceChat
{
    internal abstract class VoiceChatNotify
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
            var setting = JObject.Parse(File.ReadAllText(NS_.ExternalFiles.SETTINGS_FILE));
            // ボイス通知チャンネルのIDを取得
            var channelIdVoice = ulong.Parse(setting[NS_.ExternalFiles.CHANNEL_ID_VOICE].ToString());
            // 監視対象のボイスチャンネルを取得
            var channelIdWatchVoice = setting[NS_.ExternalFiles.CHANNEL_ID_WATCH_VOICE].ToObject<List<ulong>>();

            if (client.GetChannel(channelIdVoice) is IMessageChannel channel)
            {
                var userName = user.Username;
                var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

                if (before.VoiceChannel == null && after.VoiceChannel != null)
                {
                    // ユーザーがボイスチャンネルに入室した

                    // 対象のボイスチャンネルでなければ無視
                    if (!channelIdWatchVoice.Contains(after.VoiceChannel.Id))
                    {
                        return;
                    }

                    // すでにユーザーが通話中の場合は無視
                    if (after.VoiceChannel.ConnectedUsers.Count > 1)
                    {
                        return;
                    }

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
                    // ユーザーがボイスチャンネルから退出した

                    var endTime = DateTime.Now;

                    if (!m_voiceStartTimes.TryGetValue(user.Id, out var startTime))
                    {
                        // 通話開始時間が取得できなかった
                        await TraceLog.Log(new LogMessage(LogSeverity.Info, "Trace", $"Coudn't get chat start time: {before.VoiceChannel.Name}"));
                        return;
                    }

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
}
