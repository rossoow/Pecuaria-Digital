using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pecuaria_Digital.ViewModels
{
    public enum FiltroIatf { Todos, Finalizado, DiaIdeal, Atrasado, EmDia }

    public class IatfListViewModel
    {
        private readonly IatfSummaryService _summary;
        private readonly AiEstimativaService _ai;
        private readonly FazendaRepository _repo;

        public FazendaData Fazenda { get; private set; }
        public List<IatfResumo> TodosProtocolos { get; private set; } = new();
        public FiltroIatf FiltroAtivo { get; private set; } = FiltroIatf.Todos;

        public IEnumerable<IatfResumo> ProtocolosFiltrados =>
            FiltroAtivo == FiltroIatf.Todos ? TodosProtocolos
            : TodosProtocolos.Where(p => MatchFiltro(p, FiltroAtivo));

        // O Form assina este evento para atualizar a UI quando a IA responder
        public event Action<IatfResumo> EstimativaCalculada;

        public IatfListViewModel(IatfSummaryService s, AiEstimativaService ai, FazendaRepository r)
        { _summary = s; _ai = ai; _repo = r; }

        public void Carregar(FazendaData fazenda)
        {
            Fazenda = fazenda;
            TodosProtocolos = _summary.GerarResumos(fazenda.CaminhoPasta);
        }

        public void AplicarFiltro(FiltroIatf f) => FiltroAtivo = f;

        public void ExcluirProtocolo(IatfResumo r)
        {
            try
            {
                System.IO.File.Delete(r.CaminhoArquivo);
                TodosProtocolos.Remove(r);
                AppLogger.Info($"Protocolo excluído: {r.NomeArquivo}");
            }
            catch (Exception ex)
            {
                AppLogger.Erro("ExcluirProtocolo", ex);
                throw; // re-lança para o Form exibir o MessageBox
            }
        }

        public void IniciarEstimativasAsync(CsvRepository csvRepo, CancellationToken ct = default)
        {
            foreach (var resumo in TodosProtocolos.Where(p => !p.Finalizado))
            {
                var r = resumo;
                Task.Run(async () =>
                {
                    try
                    {
                        if (ct.IsCancellationRequested) return;
                        var lote = csvRepo.Carregar(r.CaminhoArquivo);
                        var animais = lote.Animais;
                        double? taxa = await _ai.EstimarSucessoAsync(animais, r.EstagioAtual);
                        if (taxa.HasValue && !ct.IsCancellationRequested)
                        {
                            r.SucessoEstimado = taxa;
                            r.EhEstimativa = true;
                            EstimativaCalculada?.Invoke(r);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Aviso($"Estimativa falhou ({r.NomeArquivo}): {ex.Message}");
                    }
                }, ct);
            }
        }

        public (int totalIatfs, double eficaciaMedia) EstatisticasFazenda()
        {
            var fin = TodosProtocolos.Where(p => p.Finalizado).ToList();
            return (TodosProtocolos.Count,
                    fin.Any() ? fin.Average(p => p.SucessoEstimado ?? 0) : 0);
        }

        private static bool MatchFiltro(IatfResumo p, FiltroIatf f) => f switch
        {
            // COMPLETA em andamento NÃO entra no Finalizado
            FiltroIatf.Finalizado => p.Status == StatusIatf.Finalizado
                                  && p.EstagioAtual != "COMPLETA",

            FiltroIatf.DiaIdeal => p.Status == StatusIatf.DiaIdeal,
            FiltroIatf.Atrasado => p.Status == StatusIatf.Atrasado,
            FiltroIatf.EmDia => p.Status is StatusIatf.EmDia or StatusIatf.Futuro,
            _ => true
        };
    }
}