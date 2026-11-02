using System.Net;
using System.Web;

namespace ZakiSAASERP.Web.Services
{
    public sealed class GoogleTrans()
    {
        #region Properties
        /// <summary>
        /// Gets the supported languages.
        /// </summary>
        public static IEnumerable<string> Languages
        {
            get
            {
                GoogleTrans.EnsureInitialized();
                return GoogleTrans._languageModeMap.Keys.OrderBy(p => p);
            }
        }
        /// <summary>
        /// Gets the time taken to perform the translation.
        /// </summary>
        public TimeSpan TranslationTime
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the url used to speak the translation.
        /// </summary>
        /// <value>The url used to speak the translation.</value>
        public string TranslationSpeechUrl
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error
        {
            get;
            private set;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Translates the specified source text.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="sourceLanguage">The source language.</param>
        /// <param name="targetLanguage">The target language.</param>
        /// <returns>The translation.</returns>
        public string Translate(string sourceText, string sourceLanguage = "en", string targetLanguage = "ar")
        {
            try
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={HttpUtility.UrlEncode(sourceText)}";
                var webClient = new WebClient
                {
                    Encoding = System.Text.Encoding.UTF8
                };
                webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");
                var result = webClient.DownloadString(url);
                try
                {
                    result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                    return result;
                }
                catch
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<string> Translate(HttpClient http, string sourceText, string sourceLanguage = "en", string targetLanguage = "ar")
        {
            try
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={System.Net.WebUtility.UrlEncode(sourceText)}";

                // Add User-Agent to mimic browser request to avoid 403
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                var response = await http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                // Response format: [[["Translated","Source",null,null,1]],...]
                using var doc = System.Text.Json.JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstArray = root[0];
                    if (firstArray.ValueKind == System.Text.Json.JsonValueKind.Array && firstArray.GetArrayLength() > 0)
                    {
                        var translationItem = firstArray[0];
                        if (translationItem.ValueKind == System.Text.Json.JsonValueKind.Array && translationItem.GetArrayLength() > 0)
                        {
                            return translationItem[0].GetString() ?? "Error";
                        }
                    }
                }
                return "Error";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Converts a language to its identifier.
        /// </summary>
        /// <param name="language">The language."</param>
        /// <returns>The identifier or <see cref="string.Empty"/> if none.</returns>
        private static string LanguageEnumToIdentifier
            (string language)
        {
            string mode = string.Empty;
            GoogleTrans.EnsureInitialized();
            GoogleTrans._languageModeMap.TryGetValue(language, out mode);
            return mode;
        }
        /// <summary>
        /// Ensures the GoogleTrans.has been initialized.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (GoogleTrans._languageModeMap == null)
            {
                GoogleTrans._languageModeMap = new Dictionary<string, string>();
                GoogleTrans._languageModeMap.Add("Afrikaans", "af");
                GoogleTrans._languageModeMap.Add("Albanian", "sq");
                GoogleTrans._languageModeMap.Add("Arabic", "ar");
                GoogleTrans._languageModeMap.Add("Armenian", "hy");
                GoogleTrans._languageModeMap.Add("Azerbaijani", "az");
                GoogleTrans._languageModeMap.Add("Basque", "eu");
                GoogleTrans._languageModeMap.Add("Belarusian", "be");
                GoogleTrans._languageModeMap.Add("Bengali", "bn");
                GoogleTrans._languageModeMap.Add("Bulgarian", "bg");
                GoogleTrans._languageModeMap.Add("Catalan", "ca");
                GoogleTrans._languageModeMap.Add("Chinese", "zh-CN");
                GoogleTrans._languageModeMap.Add("Croatian", "hr");
                GoogleTrans._languageModeMap.Add("Czech", "cs");
                GoogleTrans._languageModeMap.Add("Danish", "da");
                GoogleTrans._languageModeMap.Add("Dutch", "nl");
                GoogleTrans._languageModeMap.Add("English", "en");
                GoogleTrans._languageModeMap.Add("Esperanto", "eo");
                GoogleTrans._languageModeMap.Add("Estonian", "et");
                GoogleTrans._languageModeMap.Add("Filipino", "tl");
                GoogleTrans._languageModeMap.Add("Finnish", "fi");
                GoogleTrans._languageModeMap.Add("French", "fr");
                GoogleTrans._languageModeMap.Add("Galician", "gl");
                GoogleTrans._languageModeMap.Add("German", "de");
                GoogleTrans._languageModeMap.Add("Georgian", "ka");
                GoogleTrans._languageModeMap.Add("Greek", "el");
                GoogleTrans._languageModeMap.Add("Haitian Creole", "ht");
                GoogleTrans._languageModeMap.Add("Hebrew", "iw");
                GoogleTrans._languageModeMap.Add("Hindi", "hi");
                GoogleTrans._languageModeMap.Add("Hungarian", "hu");
                GoogleTrans._languageModeMap.Add("Icelandic", "is");
                GoogleTrans._languageModeMap.Add("Indonesian", "id");
                GoogleTrans._languageModeMap.Add("Irish", "ga");
                GoogleTrans._languageModeMap.Add("Italian", "it");
                GoogleTrans._languageModeMap.Add("Japanese", "ja");
                GoogleTrans._languageModeMap.Add("Korean", "ko");
                GoogleTrans._languageModeMap.Add("Lao", "lo");
                GoogleTrans._languageModeMap.Add("Latin", "la");
                GoogleTrans._languageModeMap.Add("Latvian", "lv");
                GoogleTrans._languageModeMap.Add("Lithuanian", "lt");
                GoogleTrans._languageModeMap.Add("Macedonian", "mk");
                GoogleTrans._languageModeMap.Add("Malay", "ms");
                GoogleTrans._languageModeMap.Add("Maltese", "mt");
                GoogleTrans._languageModeMap.Add("Norwegian", "no");
                GoogleTrans._languageModeMap.Add("Persian", "fa");
                GoogleTrans._languageModeMap.Add("Polish", "pl");
                GoogleTrans._languageModeMap.Add("Portuguese", "pt");
                GoogleTrans._languageModeMap.Add("Romanian", "ro");
                GoogleTrans._languageModeMap.Add("Russian", "ru");
                GoogleTrans._languageModeMap.Add("Serbian", "sr");
                GoogleTrans._languageModeMap.Add("Slovak", "sk");
                GoogleTrans._languageModeMap.Add("Slovenian", "sl");
                GoogleTrans._languageModeMap.Add("Spanish", "es");
                GoogleTrans._languageModeMap.Add("Swahili", "sw");
                GoogleTrans._languageModeMap.Add("Swedish", "sv");
                GoogleTrans._languageModeMap.Add("Tamil", "ta");
                GoogleTrans._languageModeMap.Add("Telugu", "te");
                GoogleTrans._languageModeMap.Add("Thai", "th");
                GoogleTrans._languageModeMap.Add("Turkish", "tr");
                GoogleTrans._languageModeMap.Add("Ukrainian", "uk");
                GoogleTrans._languageModeMap.Add("Urdu", "ur");
                GoogleTrans._languageModeMap.Add("Vietnamese", "vi");
                GoogleTrans._languageModeMap.Add("Welsh", "cy");
                GoogleTrans._languageModeMap.Add("Yiddish", "yi");
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The language to translation mode map.
        /// </summary>
        private static Dictionary<string, string> _languageModeMap;
        #endregion
    }
    public class TranslateResult
    {
        public string translatedText { get; set; }
    }

}


