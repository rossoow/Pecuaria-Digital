using System.Collections.Generic;

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

                // Tabelas completas nunca são Finalizado — usam estimativa
                var dataRef = DataFimExibida;
                if (dataRef == default) return StatusIatf.Futuro;

                var hoje = System.DateTime.Now.Date;
                var prazo = dataRef.Date;

                if (prazo < hoje) return StatusIatf.Atrasado;
                if (prazo == hoje) return StatusIatf.DiaIdeal;
                if (prazo <= hoje.AddDays(2)) return StatusIatf.EmDia;
                return StatusIatf.Futuro;
            }
        }
    }

    public enum StatusIatf { Futuro, DiaIdeal, EmDia, Atrasado, Finalizado }
}