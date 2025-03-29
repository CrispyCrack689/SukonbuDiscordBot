using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Discord;
using Discord.WebSocket;

namespace SukonbuDiscordBot.Notification
{
    internal abstract class BirthdayNotify
    {
        public class UserBirthday
        {
            public string UserId { get; set; }
            public DateTime Birthday { get; set; }
        }

        public static async Task NotifyTodayIsMyBirthdayAsync(DiscordSocketClient client)
        {
            string filePath = NS_.ExternalFiles.BIRTHDAYS_FILE;
            var userBirthdays = await ReadUserBirthdaysFromJsonAsync(filePath);

            var today = DateTime.Today;
            var birthdaysToday = userBirthdays.Where(ub => ub.Birthday.Month == today.Month && ub.Birthday.Day == today.Day);

            foreach (var userBirthday in birthdaysToday)
            {
                var user = client.GetUser(ulong.Parse(userBirthday.UserId));
                if (user != null)
                {
                    await user.SendMessageAsync("Happy Birthday!");
                }
            }
        }

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
