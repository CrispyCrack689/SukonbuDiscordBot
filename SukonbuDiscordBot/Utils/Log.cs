using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Utils
{
    internal static class Log
    {
        private static readonly string m_logFilePath = $"data/log/SukonbuDiscordBot_{DateTime.Now:yyyyMMdd}.log";
        private static readonly object m_logLock = new object();

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="severity">重要度</param>
        /// <param name="message">コンソールメッセージ</param>
        /// <returns></returns>
        public static Task Trace(LogSeverity severity, string message)
        {
            // コンソールに出力
            switch (severity)
            {
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            default:
                break;
            }
            Console.WriteLine(message.ToString());
            Console.ResetColor();

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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
