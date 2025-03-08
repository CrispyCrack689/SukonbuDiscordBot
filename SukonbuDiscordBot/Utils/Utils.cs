using Discord;
using System;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Utils
{
    internal static class TraceLog
    {
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="message">コンソールメッセージ</param>
        /// <returns></returns>
        public static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}
