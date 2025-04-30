using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Utils
{
    internal static class TraceLog
    {
        private static readonly string LogFilePath = $"data/log/SukonbuDiscordBot_{DateTime.Now:yyyyMMdd}.log";

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="message">コンソールメッセージ</param>
        /// <returns></returns>
        public static Task Log(LogMessage message)
        {
            // コンソールに出力
            Console.WriteLine(message.ToString());

            // ファイルに追記
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));
                File.AppendAllText(LogFilePath, $"{DateTime.Now}: {message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
