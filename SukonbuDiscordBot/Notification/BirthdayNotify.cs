using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SukonbuDiscordBot.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Notification
{
    internal abstract class BirthdayNotify
    {
        public class UserBirthday
        {
            public string UserId { get; set; }
            public DateTime Birthday { get; set; }
        }

        /// <summary>
        /// 本日誕生日のユーザーを通知
        /// </summary>
        /// <param name="client">クライアント</param>
        /// <param name="channelId">チャネルID</param>
        /// <returns></returns>
        public static async Task NotifyTodayIsMyBirthdayAsync(DiscordSocketClient client, ulong channelId)
        {
            // 誕生日リスト取得
            string filePath = ExternalFiles.BIRTHDAYS_FILE;
            var userBirthdays = await ReadUserBirthdaysFromJsonAsync(filePath);
            Assert.IsNotNull(userBirthdays, nameof(userBirthdays));
            // 本日誕生日の人を絞り込む
            var today = DateTime.Today;
            var birthdaysToday = userBirthdays
                .Where(ub => ub.Birthday.Month == today.Month && ub.Birthday.Day == today.Day)
                .ToList();

            // チャネルを取得
            var channel = client.GetChannel(channelId) as SocketGuildChannel;
            Assert.IsNotNull(channel, nameof(channel));
            // ユーザーを取得
            var guild = channel.Guild;
            Assert.IsNotNull(guild, nameof(guild));
            await guild.DownloadUsersAsync();

            // ボットがいるチャネルの中に該当者がいれば祝福
            foreach (var userBirthday in birthdaysToday)
            {
                if (ulong.TryParse(userBirthday.UserId, out var userId))
                {
                    var user = guild.GetUser(userId);
                    if (user != null)
                    {
                        if (client.GetChannel(channelId) is IMessageChannel channelSend)
                        {
                            await channelSend.SendMessageAsync($"@everyone\n本日は{MentionUtils.MentionUser(user.Id)}さんの誕生日です、おめでとう！！");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 誕生日リストを読み込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns></returns>
        private static async Task<List<UserBirthday>> ReadUserBirthdaysFromJsonAsync(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<UserBirthday>>(json);
            }
        }
    }
}
