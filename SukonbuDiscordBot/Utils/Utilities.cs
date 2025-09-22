using System;
using System.Collections.Concurrent;

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
                        // プロセスのパスを取得
                        string currentPath = current.MainModule.FileName;
                        string otherPath = null;
                        try
                        {
                            otherPath = process.MainModule.FileName;
                        }
                        catch
                        {
                            continue;
                        }

                        // 同じexeか
                        if (!string.Equals(currentPath, otherPath, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // なるべくそっ閉じする
                        if (process.CloseMainWindow())
                        {
                            if (process.WaitForExit(5000))
                            {
                                continue;
                            }
                        }

                        // 無理ならしかたなし
                        process.Kill();
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Log.Trace(Discord.LogSeverity.Error, $"An error occurred while trying to terminate process (ID: {process.Id}, Name: {process.ProcessName}): {ex.Message}").Wait();
                    }
                }
            }
        }
    };
}
