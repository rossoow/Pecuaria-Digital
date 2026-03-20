using System.IO;

namespace Pecuaria_Digital.Constants
{
    /// <summary>
    /// Lê configurações do arquivo appsettings.txt na pasta do executável.
    /// Formato: chave=valor (uma por linha)
    /// </summary>
    public static class AppSettings
    {
        
        private static readonly string _caminho = Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "appsettings.txt");

        private static readonly System.Collections.Generic.Dictionary<string, string> _config = Carregar();
        public const string NomeArquivoConfig = "config_pasta.txt";

        public static string ApiKeyAnthropic =>
            ObterValor("ApiKeyAnthropic", "");

        private static string ObterValor(string chave, string padrao)
        {
            _config.TryGetValue(chave, out string val);
            return string.IsNullOrWhiteSpace(val) ? padrao : val.Trim();
        }

        private static System.Collections.Generic.Dictionary<string, string> Carregar()
        {
            var dic = new System.Collections.Generic.Dictionary<string, string>(
                System.StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(_caminho)) return dic;
            try
            {
                foreach (var linha in File.ReadAllLines(_caminho))
                {
                    int idx = linha.IndexOf('=');
                    if (idx < 0) continue;
                    dic[linha[..idx].Trim()] = linha[(idx + 1)..];
                }
            }
            catch { }
            return dic;
        }
    }
}