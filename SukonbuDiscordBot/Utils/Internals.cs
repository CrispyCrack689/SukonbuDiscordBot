using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SukonbuDiscordBot.Utils
{
#if DEBUG
    internal class Internals
    {
        private static readonly string goFileUrlPrefix = "https://gofile.io/d/";

        public static async Task GoFileDownload(DiscordSocketClient client, SocketMessage message)
        {
            // テキストチャンネルのIDを取得
            var setting = JObject.Parse(File.ReadAllText(NS_.ExternalFiles.SETTINGS_FILE));
            var channelIdChat = ulong.Parse(setting[NS_.ExternalFiles.CHANNEL_ID_CHAT].ToString());

            // ボット自身のメッセージは無視
            // 特定チャンネル以外は無視
            if (message.Author.IsBot) return;
            if (message.Channel.Id != channelIdChat) return;

            if (message.Content.StartsWith(goFileUrlPrefix))
            {
                var url = message.Content.Trim();
                var settingsFile = JObject.Parse(File.ReadAllText(NS_.ExternalFiles.SETTINGS_FILE));

                try
                {
                    // gofile-downloader.pyでダウンロードする
                    var psi = new ProcessStartInfo
                    {
                        FileName = settingsFile[NS_.ExternalFiles.GOFILE_VENV_PYTHON_PATH].ToString(),
                        Arguments = $"\"{settingsFile[NS_.ExternalFiles.GOFILE_SCRIPT_PATH]}\" \"{url}\"",
                        WorkingDirectory = settingsFile[NS_.ExternalFiles.GOFILE_WORKING_DIRECTORY].ToString(),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            if (client.GetChannel(channelIdChat) is IMessageChannel channel)
                            {
                                await channel.SendMessageAsync("Download success: " + message.Content);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            await TraceLog.Log(new LogMessage(LogSeverity.Error, "Trace", $"ERROR: {error}"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    await TraceLog.Log(new LogMessage(LogSeverity.Error, "Trace", $"ERROR: {ex.Message}"));
                    if (client.GetChannel(channelIdChat) is IMessageChannel channel)
                    {
                        await channel.SendMessageAsync("Download failed: " + message.Content);
                    }
                }
            }
        }
    }
#endif
}
