using Pecuaria_Digital.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pecuaria_Digital.Services
{
    /// <summary>
    /// Centraliza toda a lógica de datas do protocolo IATF.
    /// Usado pelo FrmMenuTabelas e pelo IatfSummaryService.
    /// </summary>
    public class DateCentralService
    {
        // ── Enumeração de classificação ───────────────────────────────────────
        public enum ClassificacaoData { EmDia, DiaIdeal, Atrasado }

        // ── Resultado de um cálculo completo ─────────────────────────────────
        public class ResultadoDatas
        {
            // Médias reais (null = nenhuma fileira preenchida nessa etapa)
            public DateTime? MediaD0 { get; set; }
            public DateTime? MediaD8 { get; set; }
            public DateTime? MediaIATF { get; set; }
            public DateTime? MediaDG { get; set; }

            // Estimativas calculadas
            public DateTime? EstimativaD8 { get; set; }
            public DateTime? EstimativaIATF { get; set; }
            public DateTime? EstimativaDG { get; set; }

            // Flags: true = estimada, false = real
            public bool D8EhEstimativa { get; set; } = true;
            public bool IATFEhEstimativa { get; set; } = true;
            public bool DGEhEstimativa { get; set; } = true;

            // Datas de exibição (real se existir, senão estimativa)
            public DateTime? D0Exibida => MediaD0;
            public DateTime? D8Exibida => D8EhEstimativa ? EstimativaD8 : MediaD8;
            public DateTime? IATFExibida => IATFEhEstimativa ? EstimativaIATF : MediaIATF;
            public DateTime? DGExibida => DGEhEstimativa ? EstimativaDG : MediaDG;

            // Classificação da próxima etapa pendente
            public ClassificacaoData? ClassificacaoProximaEtapa { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÉTODO PRINCIPAL
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Recebe as listas de datas reais de cada etapa e retorna o resultado
        /// completo com médias, estimativas e classificações.
        /// </summary>
        public ResultadoDatas Calcular(
            IEnumerable<DateTime> datasD0,
            IEnumerable<DateTime> datasD8,
            IEnumerable<DateTime> datasIATF,
            IEnumerable<DateTime> datasDG)
        {
            var r = new ResultadoDatas();

            // 1. Médias reais
            r.MediaD0 = Media(datasD0);
            r.MediaD8 = Media(datasD8);
            r.MediaIATF = Media(datasIATF);
            r.MediaDG = Media(datasDG);

            // 2. Determina a data base = média da etapa mais avançada preenchida
            DateTime? base_ = r.MediaDG ?? r.MediaIATF ?? r.MediaD8 ?? r.MediaD0;

            if (base_ == null)
            {
                // Sem nenhum dado — sem estimativas
                return r;
            }

            // 3. Calcula estimativas a partir da etapa mais avançada disponível
            if (r.MediaDG.HasValue)
            {
                // DG real → nada a estimar além de D8/IATF se faltarem
                r.D8EhEstimativa = !r.MediaD8.HasValue;
                r.IATFEhEstimativa = !r.MediaIATF.HasValue;
                r.DGEhEstimativa = false;

                r.EstimativaD8 = r.MediaD0.HasValue
                    ? r.MediaD0.Value.AddDays(ProtocoloConstants.DiasD0ParaD8)
                    : (DateTime?)null;
                r.EstimativaIATF = r.MediaD8.HasValue
                    ? r.MediaD8.Value.AddDays(ProtocoloConstants.DiasD8ParaIATF)
                    : r.EstimativaD8.HasValue
                        ? r.EstimativaD8.Value.AddDays(ProtocoloConstants.DiasD8ParaIATF)
                        : (DateTime?)null;
                r.EstimativaDG = r.MediaDG.Value; // real
            }
            else if (r.MediaIATF.HasValue)
            {
                // IATF real → estima DG
                r.D8EhEstimativa = !r.MediaD8.HasValue;
                r.IATFEhEstimativa = false;
                r.DGEhEstimativa = true;

                r.EstimativaD8 = r.MediaD0.HasValue
                    ? r.MediaD0.Value.AddDays(ProtocoloConstants.DiasD0ParaD8)
                    : (DateTime?)null;
                r.EstimativaIATF = r.MediaIATF.Value; // real
                r.EstimativaDG = r.MediaIATF.Value
                    .AddDays(ProtocoloConstants.DiasIATFParaDG);
            }
            else if (r.MediaD8.HasValue)
            {
                // D8 real → estima IATF e DG
                r.D8EhEstimativa = false;
                r.IATFEhEstimativa = true;
                r.DGEhEstimativa = true;

                r.EstimativaD8 = r.MediaD8.Value; // real
                r.EstimativaIATF = r.MediaD8.Value
                    .AddDays(ProtocoloConstants.DiasD8ParaIATF);
                r.EstimativaDG = r.EstimativaIATF.Value
                    .AddDays(ProtocoloConstants.DiasIATFParaDG);
            }
            else
            {
                // Só D0 → estima tudo
                r.D8EhEstimativa = true;
                r.IATFEhEstimativa = true;
                r.DGEhEstimativa = true;

                r.EstimativaD8 = r.MediaD0.Value
                    .AddDays(ProtocoloConstants.DiasD0ParaD8);
                r.EstimativaIATF = r.EstimativaD8.Value
                    .AddDays(ProtocoloConstants.DiasD8ParaIATF);
                r.EstimativaDG = r.EstimativaIATF.Value
                    .AddDays(ProtocoloConstants.DiasIATFParaDG);
            }

            // 4. Classificação da próxima etapa pendente
            // (a primeira data ainda estimada)
            DateTime? proximaEstimada = null;
            if (r.D8EhEstimativa && r.EstimativaD8.HasValue)
                proximaEstimada = r.EstimativaD8;
            else if (r.IATFEhEstimativa && r.EstimativaIATF.HasValue)
                proximaEstimada = r.EstimativaIATF;
            else if (r.DGEhEstimativa && r.EstimativaDG.HasValue)
                proximaEstimada = r.EstimativaDG;

            if (proximaEstimada.HasValue)
                r.ClassificacaoProximaEtapa = Classificar(proximaEstimada.Value);

            return r;
        }

        // ─────────────────────────────────────────────────────────────────────
        // UTILITÁRIOS PÚBLICOS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Classifica uma data estimada em relação à data atual.</summary>
        public static ClassificacaoData Classificar(DateTime dataEstimada)
        {
            var hoje = DateTime.Now.Date;
            var prazo = dataEstimada.Date;

            if (prazo > hoje) return ClassificacaoData.EmDia;
            if (prazo == hoje) return ClassificacaoData.DiaIdeal;
            return ClassificacaoData.Atrasado;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVADOS
        // ─────────────────────────────────────────────────────────────────────

        private static DateTime? Media(IEnumerable<DateTime> datas)
        {
            var lista = datas?.Where(d => d != default).ToList();
            if (lista == null || lista.Count == 0) return null;
            return new DateTime((long)lista.Average(d => d.Ticks));
        }
    }
}