using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Utils
{
    internal static class TraceLog
    {
        private static readonly string m_logFilePath = $"data/log/SukonbuDiscordBot_{DateTime.Now:yyyyMMdd}.log";
        private static readonly object m_logLock = new object();

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
                Directory.CreateDirectory(Path.GetDirectoryName(m_logFilePath));
                lock (m_logLock)
                {
                    File.AppendAllText(m_logFilePath, $"{DateTime.Now}: {message}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
