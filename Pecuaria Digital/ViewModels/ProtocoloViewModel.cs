using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using System;
using System.Windows.Forms;
using static Pecuaria_Digital.FrmMenuTabelas;

namespace Pecuaria_Digital.ViewModels
{
    // --- Enumeração dos Estágios ---
    public enum EstagioProtocolo
    {
        D0_Inicio,
        D8_Retirada,
        D10_IATF,
        DG_Diagnostico,
        Finalizado
    }

    /// <summary>
    /// Gerencia o estado do protocolo e coordena os serviços.
    /// O Form só chama métodos deste ViewModel — nunca manipula estado diretamente.
    /// </summary>
    public class ProtocoloViewModel
    {
        // --- Estado público (antes eram variáveis no Form) ---

        public string NomeFazenda { get; private set; } = string.Empty;
        public string DataInicioInseminacao { get; private set; } = string.Empty;
        public string CaminhoArquivoAberto { get; private set; } = string.Empty;

        public EstagioProtocolo EstagioAtual
        { get; private set; } = EstagioProtocolo.D0_Inicio;

        public EstagioProtocolo MaiorEstagioAlcancado
        { get; private set; } = EstagioProtocolo.D0_Inicio;

        public bool ModoTabelaCompleta { get; private set; }

        // --- Serviços injetados ---
        private readonly IArquivoRepository _repository;
        private readonly EstatisticasService _estatisticas;
        private readonly DateCalculatorService _dateCalc;

        public ProtocoloViewModel(
            IArquivoRepository repository,
            EstatisticasService estatisticas,
            DateCalculatorService dateCalc)
        {
            _repository = repository;
            _estatisticas = estatisticas;
            _dateCalc = dateCalc;
        }


        public void Inicializar()
        {
            EstagioAtual = EstagioProtocolo.D0_Inicio;
            MaiorEstagioAlcancado = EstagioProtocolo.D0_Inicio;
            AppLogger.Info("Protocolo inicializado");
        }
        /// <summary>Avança para o próximo estágio do protocolo.</summary>
        public bool TentarAvancar()
        {
            // Apenas muda o estágio — sem MessageBox
            EstagioAtual++;
            if (EstagioAtual > MaiorEstagioAlcancado)
                MaiorEstagioAlcancado = EstagioAtual;

            if (EstagioAtual == EstagioProtocolo.Finalizado)
                ModoTabelaCompleta = true;

            AppLogger.Acao("Estágio", $"Avançou para: {EstagioAtual}");
            return true;
        }

        public void Voltar()
        {
            if (EstagioAtual > EstagioProtocolo.D0_Inicio)
            {
                EstagioAtual--;
                AppLogger.Acao("Estágio", $"Voltou para: {EstagioAtual}");
            }
        }

        public void ConfigurarNovoLote(string nomeFazenda, string caminhoPasta,
    bool modoCompleto, string dataInicio = null)
        {
            NomeFazenda = nomeFazenda?.Trim() ?? string.Empty;
            DataInicioInseminacao = dataInicio ?? DateTime.Now.ToString("dd-MM-yyyy");
            ModoTabelaCompleta = modoCompleto;
            CaminhoArquivoAberto = string.Empty;

            if (dataInicio == null) 
            {
                EstagioAtual = EstagioProtocolo.D0_Inicio;
                MaiorEstagioAlcancado = modoCompleto
                    ? EstagioProtocolo.Finalizado
                    : EstagioProtocolo.D0_Inicio;
            }

            AppLogger.Acao("Lote configurado", $"Fazenda: {NomeFazenda}");
        }

        public void IrParaEstagio(EstagioProtocolo estagio)
        {
            if (estagio <= MaiorEstagioAlcancado)
            {
                EstagioAtual = estagio;
                AppLogger.Acao("Estágio", $"Navegou direto para: {estagio}");
            }
        }

        public void RestaurarEstagio(EstagioProtocolo estagio)
        {
            EstagioAtual = estagio;
            MaiorEstagioAlcancado = estagio;
            AppLogger.Acao("Estágio", $"Restaurado do arquivo: {estagio}");
        }
    }
}
