using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Discord;
using SukonbuDiscordBot.Utils;
using System.Reflection;

namespace SukonbuDiscordBot.Manager
{
    /// <summary>
    /// トークン情報を管理するシングルトンクラス
    /// </summary>
    public sealed class TokenManager
    {
        private static readonly Lazy<TokenManager> s_instance = new Lazy<TokenManager>(() => new TokenManager());
        private string m_botToken;
        private long m_appId;
        private bool m_isInitialized;

        /// <summary>
        /// シングルトンインスタンスを取得
        /// </summary>
        public static TokenManager GetInstance => s_instance.Value;

        /// <summary>
        /// Botトークンを取得
        /// </summary>
        public string GetToken
        {
            get
            {
                if (!m_isInitialized)
                {
                    throw new InvalidOperationException("TokenManager is not initialized.");
                }
                return m_botToken;
            }
        }

        /// <summary>
        /// ボットIDを取得
        /// </summary>
        public long GetAppID
        {
            get
            {
                if (!m_isInitialized)
                {
                    throw new InvalidOperationException("TokenManager is not initialized.");
                }
                return m_appId;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private TokenManager()
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
                JObject tokenFile = JObject.Parse(File.ReadAllText(ExternalFiles.TOKEN_FILE));
                m_botToken = tokenFile[ExternalFiles.TOKEN].ToString();
                m_appId = tokenFile[ExternalFiles.APP_ID].ToObject<long>();

                m_isInitialized = true;
                Log.Trace(LogSeverity.Info, "TokenManager initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Trace(LogSeverity.Error, $"TokenManager initialize failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初期化状態を確認
        /// </summary>
        public bool IsInitialized => m_isInitialized;
    }
}