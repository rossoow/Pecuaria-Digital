using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pecuaria_Digital.Services
{
    public class IatfSummaryService
    {
        // Nomes exatos do cabeçalho do CsvRepository
        private const string CabecalhoId = "Nº Brinco";
        private const string CabecalhoDataD0 = "DataD0";
        private const string CabecalhoDataD8 = "DataD8";
        private const string CabecalhoDataIATF = "DataIATF";
        private const string CabecalhoDataDG = "DataDG";
        private const string CabecalhoResultado = "ResultadoDG";

        public List<IatfResumo> GerarResumos(string caminhoPasta)
        {
            var repo = new FazendaRepository();
            var arquivos = repo.ListarArquivosProtocolo(caminhoPasta);
            var lista = new List<IatfResumo>();

            for (int i = 0; i < arquivos.Count; i++)
            {
                try
                {
                    var r = LerResumo(arquivos[i]);
                    r.Id = i + 1;
                    lista.Add(r);
                }
                catch (Exception ex)
                {
                    AppLogger.Aviso($"Resumo ignorado ({arquivos[i]}): {ex.Message}");
                }
            }
            return lista;
        }

        private IatfResumo LerResumo(string caminho)
        {
            var resumo = new IatfResumo { CaminhoArquivo = caminho };
            var linhas = LerPrimeiras(caminho, 200); // mais linhas para pegar todos os dados

            // Linha de metadados
            string meta = linhas.FirstOrDefault(l =>
                l.TrimStart().StartsWith(ProtocoloConstants.PrefixoEstagio,
                    StringComparison.OrdinalIgnoreCase)) ?? "";
            ParsearMeta(meta, resumo);

            // Linha do cabeçalho
            int cab = Array.FindIndex(linhas, l =>
                l.Contains(CabecalhoId, StringComparison.OrdinalIgnoreCase));

            if (cab < 0) return resumo;

            resumo.NumeroDeVacas = linhas.Skip(cab + 1)
                .Count(l => !string.IsNullOrWhiteSpace(l));

            // Índices das colunas
            int colD0 = IndiceColuna(linhas[cab], CabecalhoDataD0);
            int colD8 = IndiceColuna(linhas[cab], CabecalhoDataD8);
            int colIATF = IndiceColuna(linhas[cab], CabecalhoDataIATF);
            int colDG = IndiceColuna(linhas[cab], CabecalhoDataDG);
            int colResult = IndiceColuna(linhas[cab], CabecalhoResultado);

            // Médias reais de cada coluna
            var datasD0 = ExtrairDatas(linhas, cab, colD0).ToList();
            var datasD8 = ExtrairDatas(linhas, cab, colD8).ToList();
            var datasIATF = ExtrairDatas(linhas, cab, colIATF).ToList();
            var datasDG = ExtrairDatas(linhas, cab, colDG).ToList();

            resumo.DataInicio = datasD0.Any()
                ? new DateTime((long)datasD0.Average(d => d.Ticks)) : default;
            resumo.DataD8Media = datasD8.Any()
                ? new DateTime((long)datasD8.Average(d => d.Ticks)) : default;
            resumo.DataIATFMedia = datasIATF.Any()
                ? new DateTime((long)datasIATF.Average(d => d.Ticks)) : default;
            resumo.DataFim = datasDG.Any()
                ? new DateTime((long)datasDG.Average(d => d.Ticks)) : default;

            // ── Calcula estimativas pela etapa mais avançada ──────────────────
            // Base para estimativa = média da etapa mais avançada com dados
            DateTime baseEstimativa = default;

            if (resumo.DataIATFMedia != default)
            {
                // Tem IATF real → estima DG a partir do IATF
                baseEstimativa = resumo.DataIATFMedia;
                resumo.DataD8EhEstimativa = resumo.DataD8Media == default;
                resumo.DataIATFEhEstimativa = false;
                resumo.DataFimEhEstimativa = resumo.DataFim == default;

                resumo.DataD8Estimada = resumo.DataInicio != default
                    ? resumo.DataInicio.AddDays(ProtocoloConstants.DiasD0ParaD8)
                    : default;
                resumo.DataIATFEstimada = resumo.DataIATFMedia; // real
                resumo.DataFimEstimada = baseEstimativa.AddDays(ProtocoloConstants.DiasIATFParaDG);
            }
            else if (resumo.DataD8Media != default)
            {
                // Tem D8 real → estima IATF e DG a partir do D8
                baseEstimativa = resumo.DataD8Media;
                resumo.DataD8EhEstimativa = false;
                resumo.DataIATFEhEstimativa = true;
                resumo.DataFimEhEstimativa = resumo.DataFim == default;

                resumo.DataD8Estimada = resumo.DataD8Media; // real
                resumo.DataIATFEstimada = baseEstimativa.AddDays(ProtocoloConstants.DiasD8ParaIATF);
                resumo.DataFimEstimada = resumo.DataIATFEstimada.AddDays(ProtocoloConstants.DiasIATFParaDG);
            }
            else if (resumo.DataInicio != default)
            {
                // Só tem D0 → estima tudo a partir do D0
                baseEstimativa = resumo.DataInicio;
                resumo.DataD8EhEstimativa = true;
                resumo.DataIATFEhEstimativa = true;
                resumo.DataFimEhEstimativa = resumo.DataFim == default;

                resumo.DataD8Estimada = baseEstimativa.AddDays(ProtocoloConstants.DiasD0ParaD8);
                resumo.DataIATFEstimada = resumo.DataD8Estimada.AddDays(ProtocoloConstants.DiasD8ParaIATF);
                resumo.DataFimEstimada = resumo.DataIATFEstimada.AddDays(ProtocoloConstants.DiasIATFParaDG);
            }

            // Se DataFim real existe, marca como não estimativa
            if (resumo.DataFim != default)
            {
                resumo.DataFimEhEstimativa = false;
                resumo.DataFimEstimada = resumo.DataFim;
            }

            // ── Taxa de Prenhez ───────────────────────────────────────────────
            if (colResult >= 0 && resumo.NumeroDeVacas > 0)
            {
                var resultados = linhas.Skip(cab + 1)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l =>
                    {
                        var cols = l.Split(';');
                        return colResult < cols.Length ? cols[colResult].Trim().ToUpper() : "";
                    })
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToList();

                if (resultados.Any())
                {
                    int prenhas = resultados.Count(r => r.Contains("PRENHA"));
                    int total = resultados.Count;
                    resumo.TaxaPrenhez = total > 0
                        ? Math.Round((double)prenhas / total * 100, 1)
                        : (double?)null;
                }
            }

            return resumo;
        }

        private static void ParsearMeta(string meta, IatfResumo r)
        {
            if (string.IsNullOrWhiteSpace(meta)) return;

            var partes = meta.Split(ProtocoloConstants.SeparadorCsv);

            var segEstagio = partes[0].Split(':');
            string estagio = segEstagio.Length > 1 ? segEstagio[1].Trim() : "";

            bool modoCompleto = partes.Length > 1 &&
                partes[1].Contains(ProtocoloConstants.ModoCompleto,
                    StringComparison.OrdinalIgnoreCase);

            if (modoCompleto)
            {
                // COMPLETA nunca é Finalizado — tem status próprio baseado nas datas
                r.EstagioAtual = estagio.Equals("Finalizado",
                    StringComparison.OrdinalIgnoreCase)
                    ? "FINALIZADO"
                    : "COMPLETA";
            }
            else
            {
                r.EstagioAtual = estagio switch
                {
                    "D0_Inicio" => "D0",
                    "D8_Retirada" => "D8",
                    "D10_IATF" => "IATF",
                    "DG_Diagnostico" => "DG",
                    "Finalizado" => "FINALIZADO",
                    _ => estagio
                };
            }

            // Só é Finalizado se o estágio for "Finalizado" E não for COMPLETA em andamento
            r.Finalizado = estagio.Equals("Finalizado",
                StringComparison.OrdinalIgnoreCase);
        }

        private static int IndiceColuna(string cabecalho, string nome)
        {
            var cols = cabecalho.Split(';');
            return Array.FindIndex(cols, c =>
                c.Trim().Equals(nome, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<DateTime> ExtrairDatas(
            string[] linhas, int cab, int col)
        {
            if (col < 0) yield break;
            for (int i = cab + 1; i < linhas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(linhas[i])) continue;
                var c = linhas[i].Split(';');
                if (col < c.Length && DateTime.TryParse(c[col], out DateTime d))
                    yield return d;
            }
        }

        private static string[] LerPrimeiras(string caminho, int max)
        {
            var lista = new List<string>();
            try
            {
                using var sr = new StreamReader(caminho, Encoding.UTF8, true);
                while (!sr.EndOfStream && lista.Count < max)
                    lista.Add(sr.ReadLine() ?? "");
            }
            catch { }
            return lista.ToArray();
        }
    }
}