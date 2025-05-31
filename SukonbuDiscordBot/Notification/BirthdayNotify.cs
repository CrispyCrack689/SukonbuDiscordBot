using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Discord;
using Discord.WebSocket;
using System.Diagnostics;

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
            string filePath = NS_.ExternalFiles.BIRTHDAYS_FILE;
            var userBirthdays = await ReadUserBirthdaysFromJsonAsync(filePath);

            // 本日誕生日の人を絞り込む
            var today = DateTime.Today;
            var birthdaysToday = userBirthdays
                .Where(ub => ub.Birthday.Month == today.Month && ub.Birthday.Day == today.Day)
                .ToList();

            // チャネルを取得
            var channel = client.GetChannel(channelId) as SocketGuildChannel;
            Debug.Assert(channel != null);
            // ユーザーを取得
            //TODO:ボットしか取得できない　なぜ
            var userIdsInChannel = channel.Users
                .Select(u => u.Id.ToString())
                .ToHashSet();

            // ボットがいるチャネルの中に該当者がいれば祝福
            foreach (var userBirthday in birthdaysToday)
            {
                if (userIdsInChannel.Contains(userBirthday.UserId))
                {
                    var user = channel.Users.FirstOrDefault(u => u.Id.ToString() == userBirthday.UserId);
                    if (user != null)
                    {
                        if (client.GetChannel(channelId) is IMessageChannel channelSend)
                        {
                            await channelSend.SendMessageAsync($"Happy Birthday, {user.DisplayName}!");
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
