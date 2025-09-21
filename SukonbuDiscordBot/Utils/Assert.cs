using System;
using System.Diagnostics;

namespace SukonbuDiscordBot.Utils
{
    /// <summary>
    /// アサート
    /// </summary>
    internal static class Assert
    {
        /// <summary>
        /// 条件がfalseの場合、エラーログを出力し例外をスローします
        /// </summary>
        /// <param name="condition">検証する条件</param>
        /// <param name="message">エラーメッセージ</param>
        /// <exception cref="InvalidOperationException">条件がfalseの場合</exception>
        public static void IsTrue(bool condition, string message = "アサーションに失敗しました")
        {
            if (!condition)
            {
                var errorMessage = $"Assert失敗: {message}";
                Log.Trace(Discord.LogSeverity.Error, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// オブジェクトがnullでないことを検証します
        /// </summary>
        /// <param name="obj">検証するオブジェクト</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <exception cref="ArgumentNullException">オブジェクトがnullの場合</exception>
        public static void IsNotNull(object obj, string parameterName = "parameter")
        {
            if (obj == null)
            {
                var errorMessage = $"Assert失敗: {parameterName} がnullです";
                Log.Trace(Discord.LogSeverity.Error, errorMessage);
                throw new ArgumentNullException(parameterName, errorMessage);
            }
        }

        /// <summary>
        /// 文字列がnullまたは空でないことを検証します
        /// </summary>
        /// <param name="value">検証する文字列</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <exception cref="ArgumentException">文字列がnullまたは空の場合</exception>
        public static void IsNotNullOrEmpty(string value, string parameterName = "parameter")
        {
            if (string.IsNullOrEmpty(value))
            {
                var errorMessage = $"Assert失敗: {parameterName} がnullまたは空です";
                Log.Trace(Discord.LogSeverity.Error, errorMessage);
                throw new ArgumentException(errorMessage, parameterName);
            }
        }

        /// <summary>
        /// 文字列がnull、空、または空白文字のみでないことを検証します
        /// </summary>
        /// <param name="value">検証する文字列</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <exception cref="ArgumentException">文字列がnull、空、または空白文字のみの場合</exception>
        public static void IsNotNullOrWhiteSpace(string value, string parameterName = "parameter")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var errorMessage = $"Assert失敗: {parameterName} がnull、空、または空白文字のみです";
                Log.Trace(Discord.LogSeverity.Error, errorMessage);
                throw new ArgumentException(errorMessage, parameterName);
            }
        }

        /// <summary>
        /// 数値が指定した範囲内にあることを検証します
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <exception cref="ArgumentOutOfRangeException">値が範囲外の場合</exception>
        public static void IsInRange(int value, int min, int max, string parameterName = "parameter")
        {
            if (value < min || value > max)
            {
                var errorMessage = $"Assert失敗: {parameterName} ({value}) が範囲 [{min}, {max}] 外です";
                Log.Trace(Discord.LogSeverity.Error, errorMessage);
                throw new ArgumentOutOfRangeException(parameterName, value, errorMessage);
            }
        }

        /// <summary>
        /// デバッグビルド時のみアサートを実行します
        /// </summary>
        /// <param name="condition">検証する条件</param>
        /// <param name="message">エラーメッセージ</param>
        [Conditional("DEBUG")]
        public static void DebugOnly(bool condition, string message = "デバッグアサーションに失敗しました")
        {
            if (!condition)
            {
                var errorMessage = $"Debug Assert失敗: {message}";
                Log.Trace(Discord.LogSeverity.Warning, errorMessage);
                Debug.Assert(condition, errorMessage);
            }
        }
    }
}
