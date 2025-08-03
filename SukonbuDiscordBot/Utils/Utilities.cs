using System;

namespace SukonbuDiscordBot.Utils
{
    public static class Utilities
    {
        /// <summary>
        /// 他のインスタンスを終了させる
        /// ※初期化時に呼び出してください
        /// </summary>
        public static void KillOtherInstances()
        {
            var current = System.Diagnostics.Process.GetCurrentProcess();
            var processes = System.Diagnostics.Process.GetProcessesByName(current.ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != current.Id)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }
    };
}
