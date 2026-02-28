using System;
using System.Collections.Generic;
using System.Text;

namespace Pecuaria_Digital.Logging
{
    /// <summary>
    /// Logger simplificado que registra eventos em arquivo rotativo diário.
    /// Arquivo fica em: %AppData%\PecuariaDigital\logs\app_YYYY-MM-DD.log
    /// </summary>
    public static class AppLogger
    {
        private static readonly string _pastaLog;
        private static readonly object _lock = new object();

        static AppLogger()
        {
            _pastaLog = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PecuariaDigital", "logs");

            Directory.CreateDirectory(_pastaLog);
        }

        private static string ArquivoHoje =>
            Path.Combine(_pastaLog, $"app_{DateTime.Now:yyyy-MM-dd}.log");

        public static void Info(string mensagem) => Escrever("INFO ", mensagem);
        public static void Aviso(string mensagem) => Escrever("AVISO", mensagem);
        public static void Acao(string acao, string detalhe) =>
            Escrever("AÇÃO ", $"{acao}: {detalhe}");

        public static void Erro(string mensagem, Exception ex = null)
        {
            Escrever("ERRO ", mensagem);
            if (ex != null)
            {
                Escrever("STACK", ex.ToString());
            }
        }

        private static void Escrever(string nivel, string texto)
        {
            lock (_lock)
            {
                try
                {
                    string linha = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{nivel}]  {texto}";
                    File.AppendAllText(ArquivoHoje, linha + Environment.NewLine);
                }
                catch { /* Logger nunca pode travar o sistema */ }
            }
        }

        /// <summary>Abre a pasta de logs no Windows Explorer.</summary>
        public static void AbrirPastaLogs()
        {
            if (Directory.Exists(_pastaLog))
                System.Diagnostics.Process.Start("explorer.exe", _pastaLog);
        }
    }

}
