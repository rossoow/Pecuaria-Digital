using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using Pecuaria_Digital.ViewModels;
using Pecuaria_Digital.Logging;

namespace Pecuaria_Digital
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            

            // --- Montagem das dependências (Composition Root) ---
            var csvRepo = new CsvRepository();
            var stats = new EstatisticasService();
            var dateCalc = new DateCalculatorService();
            var viewModel = new ProtocoloViewModel(csvRepo, stats, dateCalc);

            AppLogger.Info("Aplicativo iniciado");

            Application.Run(new FrmMenuTabela(viewModel));
        }
    }
}
