namespace Pecuaria_Digital.Constants
{
    /// <summary>
    /// Configurações globais do aplicativo.
    /// Centralize aqui todos os valores fixos para evitar magic strings/numbers.
    /// </summary>
    public static class AppSettings
    {
        // --- Aplicativo ---
        public const string Versao = "1.0.0";
        public const string NomeAplicacao = "Pecuária Digital";
        public const string PrefixoNomeFazenda = "Fazenda_";
        public const string NomePastaDefault = "Dados";
        public const string NomeArquivoConfig = "config_pasta.txt";
        public const string ExtensaoPadrao = ".csv";

        // --- Metas Zootécnicas (usadas nas cores das estatísticas) ---
        public const double MetaTaxaConcepcao = 50.0;
        public const double MetaTaxaServico = 60.0;
        public const double MetaTaxaPrenhez = 40.0;
        public const double MetaIndiceIdeal = 1.5;
        public const double MetaIndiceBom = 2.0;
    }
}
