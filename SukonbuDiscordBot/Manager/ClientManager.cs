using System;
using Discord;
using Discord.WebSocket;
using SukonbuDiscordBot.Utils;

namespace SukonbuDiscordBot.Manager
{
    /// <summary>
    /// クライアントを管理するシングルトンクラス
    /// </summary>
    public sealed class ClientManager
    {
        private static readonly Lazy<ClientManager> s_instance = new Lazy<ClientManager>(() => new ClientManager());
        private DiscordSocketClient m_client;
        private bool m_isInitialized;

        /// <summary>
        /// シングルトンインスタンスを取得
        /// </summary>
        public static ClientManager GetInstance => s_instance.Value;

        /// <summary>
        /// クライアントを取得
        /// </summary>
        public DiscordSocketClient GetClient
        {
            get
            {
                if (!m_isInitialized)
                {
                    throw new InvalidOperationException("ClientManager is not initialized.");
                }
                return m_client;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ClientManager()
        {
            m_isInitialized = false;
        }

        /// <summary>
        /// トークン情報を初期化
        /// </summary>
        public void Initialize()
        {
            try
            {
                // 前のインスタンスをキル
                Utilities.KillOtherInstances();
                // クライアント初期化
                var config = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    GatewayIntents = GatewayIntents.Guilds
                                   | GatewayIntents.GuildMessages
                                   | GatewayIntents.GuildVoiceStates
                                   | GatewayIntents.MessageContent
                                   | GatewayIntents.GuildMembers
                };
                m_client = new DiscordSocketClient(config);
                Assert.IsNotNull(m_client);

                m_isInitialized = true;
                Log.Trace(LogSeverity.Info, "ClientManager initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Trace(LogSeverity.Error, $"ClientManager initialize failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初期化状態を確認
        /// </summary>
        public bool IsInitialized => m_isInitialized;
    }
}
