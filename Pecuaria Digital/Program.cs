using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using Pecuaria_Digital.ViewModels;
using Pecuaria_Digital.Views;

namespace Pecuaria_Digital
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (s, ex) =>
            {
                AppLogger.Erro("ThreadException não tratada", ex.Exception);
                MessageBox.Show($"Erro inesperado:\n{ex.Exception.Message}\n\nDetalhes no log.",
                    "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                AppLogger.Erro("UnhandledException", ex.ExceptionObject as Exception);

            ApplicationConfiguration.Initialize();
            AppLogger.Info("Aplicativo iniciado");
            Application.Run(new FrmMenuFazendas());
            AppLogger.Info("Aplicativo encerrado");
        }
    }
}
