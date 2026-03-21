using Pecuaria_Digital.Services;
using System.Collections.Generic;
using Pecuaria_Digital.Services;

namespace Pecuaria_Digital.Models
{
    public class FazendaData
    {
        public string Nome { get; set; } = "";
        public string Localizacao { get; set; } = "";
        public string CaminhoPasta { get; set; } = "";
        public string CaminhoImagem { get; set; } = "";
        public List<IatfResumo> Protocolos { get; set; } = new();
        public string CaminhoArquivo { get; set; } = string.Empty;
        public string DiretorioDeTrabalho { get; set; } = string.Empty;
    }

    public class IatfResumo
    {
        public int Id { get; set; }
        public string CaminhoArquivo { get; set; } = "";
        public string NomeArquivo => System.IO.Path.GetFileName(CaminhoArquivo);
        public string EstagioAtual { get; set; } = "";
        public bool Finalizado { get; set; }

        // Datas reais (média das fileiras preenchidas)
        public System.DateTime DataInicio { get; set; }  // D0 média
        public System.DateTime DataD8Media { get; set; }  // D8 média real
        public System.DateTime DataIATFMedia { get; set; } // IATF média real
        public System.DateTime DataFim { get; set; }  // DG média real

        // Datas estimadas (calculadas quando não há dados reais)
        public System.DateTime DataD8Estimada { get; set; }
        public System.DateTime DataIATFEstimada { get; set; }
        public System.DateTime DataFimEstimada { get; set; }

        // Flags indicando se a data exibida é estimativa
        public bool DataD8EhEstimativa { get; set; } = true;
        public bool DataIATFEhEstimativa { get; set; } = true;
        public bool DataFimEhEstimativa { get; set; } = true;

        // Propriedades de exibição (usa real se existir, senão estimada)
        public System.DateTime DataD8Exibida => DataD8EhEstimativa ? DataD8Estimada : DataD8Media;
        public System.DateTime DataIATFExibida => DataIATFEhEstimativa ? DataIATFEstimada : DataIATFMedia;
        public System.DateTime DataFimExibida => DataFimEhEstimativa ? DataFimEstimada : DataFim;

        public int NumeroDeVacas { get; set; }
        public double? SucessoEstimado { get; set; }   // IA
        public double? TaxaPrenhez { get; set; }   // calculada do CSV
        public bool EhEstimativa { get; set; } = true;

        // Status usa a data estimada da próxima etapa para protocolos em andamento
        public StatusIatf Status
        {
            get
            {
                if (Finalizado) return StatusIatf.Finalizado;
                if (EstagioAtual == "COMPLETA") return StatusIatf.Futuro; // completa tem status próprio

                // Usa a próxima data estimada pendente
                var proximaEstimada = DataD8EhEstimativa ? DataD8Exibida
                                    : DataIATFEhEstimativa ? DataIATFExibida
                                    : DataFimEhEstimativa ? DataFimExibida
                                    : (DateTime?)null;

                if (proximaEstimada == null || proximaEstimada == default(DateTime))
                    return StatusIatf.Futuro;

                var classificacao = DateCentralService.Classificar(proximaEstimada.Value);
                return classificacao switch
                {
                    DateCentralService.ClassificacaoData.EmDia => StatusIatf.EmDia,
                    DateCentralService.ClassificacaoData.DiaIdeal => StatusIatf.DiaIdeal,
                    DateCentralService.ClassificacaoData.Atrasado => StatusIatf.Atrasado,
                    _ => StatusIatf.Futuro
                };
            }
        }
    }

    public enum StatusIatf { Futuro, DiaIdeal, EmDia, Atrasado, Finalizado }
}