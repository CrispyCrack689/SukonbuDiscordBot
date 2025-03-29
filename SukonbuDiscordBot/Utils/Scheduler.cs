using System;
using System.Threading;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Utils
{
    internal class Scheduler
    {
        private Timer m_timer;

        /// <summary>
        /// 毎日指定時刻にタスクを実行する
        /// </summary>
        /// <param name="hour">時</param>
        /// <param name="minute">分</param>
        /// <param name="task">実行するタスク</param>
        public void ScheduleDailyTask(int hour, int minute, Func<Task> task)
        {
            // 実行タイミングを設定
            var now = DateTime.Now;
            var firstRun = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, 0);
            if (now > firstRun)
            {
                // すでに過ぎていたら翌日にする
                firstRun = firstRun.AddDays(1);
            }

            // タイマーを設定
            var timeToGo = firstRun - now;
            m_timer = new Timer(async x =>
                {
                    // タスクを実行
                    await task.Invoke();
                    ScheduleDailyTask(hour, minute, task);
                }, 
                null,
                timeToGo,
                Timeout.InfiniteTimeSpan
            );
        }
    }
}
