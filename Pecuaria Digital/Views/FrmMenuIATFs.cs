using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using Pecuaria_Digital.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace Pecuaria_Digital.Views
{
    /// <summary>
    /// TELA 2 — Lista de protocolos (IATFs) de uma fazenda.
    /// Navegação: FrmMenuFazendas → FrmMenuIATFs → FrmMenuTabelas
    /// </summary>
    public partial class FrmMenuIATFs : Form
    {
        // ─── Modelo de estatística ────────────────────────────────────────────────────
        private record ItemEstatistica(string Titulo, string Valor, Color? CorValor = null);

        // ─── Renderizador genérico ────────────────────────────────────────────────────
        private void RenderizarEstatisticas(FlowLayoutPanel painel,
            IEnumerable<ItemEstatistica> itens)
        {
            painel.SuspendLayout();
            painel.Controls.Clear();

            foreach (var item in itens)
            {
                var linha = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    Margin = new Padding(0)
                };

                linha.Controls.Add(new Label
                {
                    Text = item.Titulo + ":",
                    Font = new Font(painel.Font, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(4, 4, 2, 0)
                });

                linha.Controls.Add(new Label
                {
                    Text = item.Valor,
                    AutoSize = true,
                    ForeColor = item.CorValor ?? Color.Black,
                    Margin = new Padding(0, 4, 8, 0)
                });

                painel.Controls.Add(linha);
            }

            painel.ResumeLayout();
        }
        // =====================================================================
        // 1. CAMPOS DA CLASSE
        // =====================================================================

        private readonly IatfListViewModel _vm;
        private readonly CsvRepository _csvRepo;
        private CancellationTokenSource _cts;

        // Mapa botão → filtro — preenchido em ConfigurarBotoesFiltro()
        private readonly Dictionary<Button, FiltroIatf> _mapaFiltros = new();

        // Cores do semáforo
        private static readonly Color CorFuturo = Color.FromArgb(198, 239, 206);
        private static readonly Color CorDiaIdeal = Color.FromArgb(255, 235, 156);
        private static readonly Color CorEmDia = Color.FromArgb(189, 215, 238);
        private static readonly Color CorAtrasado = Color.FromArgb(255, 199, 206);
        private static readonly Color CorFinalizado = Color.FromArgb(230, 230, 230);

        // =====================================================================
        // 2. CONSTRUTOR
        // =====================================================================

        public FrmMenuIATFs(FazendaData fazenda, string apiKey = "")
        {
            InitializeComponent();

            _csvRepo = new CsvRepository();
            _vm = new IatfListViewModel(
                           new IatfSummaryService(),
                           new AiEstimativaService(apiKey),
                           new FazendaRepository());

            _vm.Carregar(fazenda);
            _vm.EstimativaCalculada += OnEstimativaCalculada;

            this.Text = $"IATFs — {fazenda.Nome}";
        }

        // =====================================================================
        // 3. LOAD E CLOSE
        // =====================================================================

        private void FrmMenuIATFs_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarGrid();
                ConfigurarBotoesFiltro();

                PopularGrid();
                AtualizarEstatisticasFazenda();
                LimparDetalheProtocolo();

                _cts = new CancellationTokenSource();
                _vm.IniciarEstimativasAsync(_csvRepo, _cts.Token);

                AppLogger.Info($"FrmMenuIATFs carregado: {_vm.Fazenda.Nome}");
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs_Load", ex);
                MessageBox.Show($"Erro ao carregar protocolos:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmMenuIATFs_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        // =====================================================================
        // 4. CONFIGURAÇÃO DO GRID
        // =====================================================================

        private void ConfigurarGrid()
        {
            gridProtocolos.AutoGenerateColumns = false;
            gridProtocolos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProtocolos.MultiSelect = false;
            gridProtocolos.ReadOnly = true;
            gridProtocolos.AllowUserToAddRows = false;
            gridProtocolos.RowHeadersVisible = false;
            gridProtocolos.BackgroundColor = Color.White;
            gridProtocolos.BorderStyle = BorderStyle.None;
            gridProtocolos.ColumnHeadersDefaultCellStyle.Font =
                new Font(gridProtocolos.Font, FontStyle.Bold);

            gridProtocolos.Columns.Clear();
            gridProtocolos.CellFormatting += Grid_CellFormatting;  // ← ADICIONAR

            AdicionarColuna("colIdProtocolo", "ID", 50);
            AdicionarColuna("colEtapa", "Etapa Atual", 110);
            AdicionarColuna("colDataInicio", "Data Início", 100);
            AdicionarColuna("colDataD8", "Data D8", 100);  // ← NOVO
            AdicionarColuna("colDataIATF", "Data IATF", 100);  // ← NOVO
            AdicionarColuna("colDataFim", "Data Fim", 100);
            AdicionarColuna("colNumVacas", "Nº Vacas", 80);
            AdicionarColuna("colSucesso", "Prenhez (%)", 100);  // ← nome atualizado
            AdicionarColuna("colArquivo", "Arquivo", 200);
        }

        private void AdicionarColuna(string nome, string titulo, int largura)
        {
            gridProtocolos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nome,
                HeaderText = titulo,
                Width = largura,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                ReadOnly = true
            });
        }

        // =====================================================================
        // 5. POPULAR O GRID
        // =====================================================================

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (gridProtocolos.Rows[e.RowIndex].Tag is not IatfResumo p) return;

            string col = gridProtocolos.Columns[e.ColumnIndex].Name;

            // DataInicio (D0) → sempre preto, sem formatação especial
            if (col == "colDataInicio")
            {
                e.CellStyle.ForeColor = Color.Black;
                e.CellStyle.SelectionForeColor = Color.Black;
                e.CellStyle.Font = new Font(gridProtocolos.Font, FontStyle.Regular);
                return;
            }

            // Mapeia coluna → (data exibida, é estimativa)
            bool ehEstimativa;
            DateTime dataExibida;

            switch (col)
            {
                case "colDataD8":
                    ehEstimativa = p.DataD8EhEstimativa;
                    dataExibida = p.DataD8Exibida;
                    break;
                case "colDataIATF":
                    ehEstimativa = p.DataIATFEhEstimativa;
                    dataExibida = p.DataIATFExibida;
                    break;
                case "colDataFim":
                    ehEstimativa = p.DataFimEhEstimativa;
                    dataExibida = p.DataFimExibida;
                    break;
                default:
                    return;
            }

            if (dataExibida == default) return;

            if (!ehEstimativa)
            {
                // Real → preto normal
                e.CellStyle.ForeColor = Color.Black;
                e.CellStyle.SelectionForeColor = Color.Black;
                e.CellStyle.Font = new Font(gridProtocolos.Font, FontStyle.Regular);
                return;
            }

            // Estimativa → negrito + cor por classificação
            e.CellStyle.Font = new Font(gridProtocolos.Font, FontStyle.Bold);

            var classificacao = DateCentralService.Classificar(dataExibida);
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = classificacao switch
            {
                DateCentralService.ClassificacaoData.EmDia => Color.RoyalBlue,
                DateCentralService.ClassificacaoData.DiaIdeal => Color.DarkGoldenrod,
                DateCentralService.ClassificacaoData.Atrasado => Color.Red,
                _ => Color.Gray
            };
        }

        private void PopularGrid()
        {
            gridProtocolos.Rows.Clear();

            foreach (var p in _vm.ProtocolosFiltrados)
            {
                int idx = gridProtocolos.Rows.Add(
                    p.Id.ToString("D2"),
                    p.EstagioAtual,
                    p.DataInicio != default ? p.DataInicio.ToString("dd/MM/yyyy") : "-",
                    FormatarData(p.DataD8Exibida, p.DataD8EhEstimativa),    // D8
                    FormatarData(p.DataIATFExibida, p.DataIATFEhEstimativa),  // IATF
                    FormatarData(p.DataFimExibida, p.DataFimEhEstimativa),   // Fim
                    p.NumeroDeVacas > 0 ? p.NumeroDeVacas.ToString() : "-",
                    FormatarPrenhez(p),
                    p.NomeArquivo
                );

                gridProtocolos.Rows[idx].Tag = p;
                gridProtocolos.Rows[idx].DefaultCellStyle.BackColor = CorDoStatus(p.Status);
            }
        }
        private static string FormatarData(DateTime data, bool ehEstimativa)
        {
            if (data == default) return "-";
            string texto = data.ToString("dd/MM/yyyy");
            return ehEstimativa ? $"{texto} *" : texto;
        }
        private static string FormatarPrenhez(IatfResumo p)
        {
            if (p.TaxaPrenhez.HasValue)
                return $"{p.TaxaPrenhez:F1}%";
            if (p.SucessoEstimado.HasValue)
                return $"{p.SucessoEstimado:F1}% (IA)";
            return p.Finalizado ? "-" : "…";
        }

        private static string FormatarSucesso(IatfResumo p)
        {
            if (!p.SucessoEstimado.HasValue) return p.Finalizado ? "-" : "…";
            return $"{p.SucessoEstimado:F1}%{(p.EhEstimativa ? " *" : "")}";
        }

        private static Color CorDoStatus(StatusIatf status) => status switch
        {
            StatusIatf.Futuro => Color.FromArgb(198, 239, 206),  // verde claro
            StatusIatf.DiaIdeal => Color.FromArgb(255, 235, 156),  // amarelo
            StatusIatf.EmDia => Color.FromArgb(198, 239, 206),  // verde claro
            StatusIatf.Atrasado => Color.FromArgb(255, 199, 206),  // vermelho claro
            StatusIatf.Finalizado => Color.FromArgb(230, 230, 230),  // cinza
            _ => Color.White
        };

        // =====================================================================
        // 6. EVENTOS DO GRID
        // =====================================================================

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            AtualizarDetalheProtocolo(ObterProtocoloSelecionado());
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) AbrirProtocolo();
        }

        private void Grid_ColumnHeaderMouseClick(object sender,
            DataGridViewCellMouseEventArgs e)
        {
            var col = gridProtocolos.Columns[e.ColumnIndex];

            var direcao = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending
                ? SortOrder.Descending : SortOrder.Ascending;

            foreach (DataGridViewColumn c in gridProtocolos.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            col.HeaderCell.SortGlyphDirection = direcao;
            OrdenarGrid(col.Name, direcao);
        }

        private void OrdenarGrid(string nomeColuna, SortOrder direcao)
        {
            var lista = _vm.ProtocolosFiltrados.ToList();

            lista = nomeColuna switch
            {
                "colIdProtocolo" => direcao == SortOrder.Ascending
                    ? lista.OrderBy(p => p.Id).ToList()
                    : lista.OrderByDescending(p => p.Id).ToList(),

                "colDataInicio" => direcao == SortOrder.Ascending
                    ? lista.OrderBy(p => p.DataInicio).ToList()
                    : lista.OrderByDescending(p => p.DataInicio).ToList(),

                "colDataFim" => direcao == SortOrder.Ascending
                    ? lista.OrderBy(p => p.DataFim).ToList()
                    : lista.OrderByDescending(p => p.DataFim).ToList(),

                "colNumVacas" => direcao == SortOrder.Ascending
                    ? lista.OrderBy(p => p.NumeroDeVacas).ToList()
                    : lista.OrderByDescending(p => p.NumeroDeVacas).ToList(),

                "colSucesso" => direcao == SortOrder.Ascending
                    ? lista.OrderBy(p => p.SucessoEstimado ?? -1).ToList()
                    : lista.OrderByDescending(p => p.SucessoEstimado ?? -1).ToList(),

                _ => lista.OrderBy(p => p.EstagioAtual).ToList()
            };

            gridProtocolos.Rows.Clear();
            foreach (var p in lista)
            {
                int idx = gridProtocolos.Rows.Add(
                    p.Id.ToString("D2"),
                    p.EstagioAtual,
                    p.DataInicio != default ? p.DataInicio.ToString("dd/MM/yyyy") : "-",
                    p.DataFim != default ? p.DataFim.ToString("dd/MM/yyyy") : "-",
                    p.NumeroDeVacas > 0 ? p.NumeroDeVacas.ToString() : "-",
                    FormatarSucesso(p),
                    p.NomeArquivo
                );
                gridProtocolos.Rows[idx].Tag = p;
                gridProtocolos.Rows[idx].DefaultCellStyle.BackColor = CorDoStatus(p.Status);
            }
        }

        // =====================================================================
        // 7. BOTÕES DE FILTRO
        // =====================================================================

        private void ConfigurarBotoesFiltro()
        {
            _mapaFiltros[btnFiltroTodos] = FiltroIatf.Todos;
            _mapaFiltros[btnFiltroFinalizado] = FiltroIatf.Finalizado;
            _mapaFiltros[btnFiltroDiaIdeal] = FiltroIatf.DiaIdeal;
            _mapaFiltros[btnFiltroAtrasado] = FiltroIatf.Atrasado;
            _mapaFiltros[btnFiltroEmDia] = FiltroIatf.EmDia;

            DestcarBotaoFiltro(btnFiltroTodos);
        }

        private void BtnFiltro_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (!_mapaFiltros.TryGetValue(btn, out var filtro)) return;

            _vm.AplicarFiltro(filtro);
            PopularGrid();
            LimparDetalheProtocolo();
            DestcarBotaoFiltro(btn);
        }

        private void DestcarBotaoFiltro(Button ativo)
        {
            foreach (var btn in _mapaFiltros.Keys)
            {
                btn.BackColor = SystemColors.Control;
                btn.ForeColor = Color.Black;
                btn.Font = new Font(btn.Font, FontStyle.Regular);
            }
            ativo.BackColor = Color.FromArgb(0, 120, 215);
            ativo.ForeColor = Color.White;
            ativo.Font = new Font(ativo.Font, FontStyle.Bold);
        }

        // =====================================================================
        // 8. PAINEL DE DETALHE DO PROTOCOLO SELECIONADO
        // =====================================================================

        private void AtualizarDetalheProtocolo(IatfResumo p)
        {
            if (p == null) { LimparDetalheProtocolo(); return; }

            Color corStatus = p.Status switch
            {
                StatusIatf.Futuro => Color.DarkGreen,
                StatusIatf.DiaIdeal => Color.DarkGoldenrod,
                StatusIatf.EmDia => Color.SteelBlue,
                StatusIatf.Atrasado => Color.Red,
                StatusIatf.Finalizado => Color.Gray,
                _ => Color.Black
            };

            // ↓ Adicione ou remova itens aqui livremente
            var itens = new List<ItemEstatistica>
    {
        new("Etapa",   p.EstagioAtual),
        new("Vacas",   p.NumeroDeVacas > 0 ? p.NumeroDeVacas.ToString() : "-"),
        new("Sucesso", FormatarSucesso(p)),
        new("Status",  TraduzirStatus(p.Status), corStatus),
        new("Início",  p.DataInicio != default ? p.DataInicio.ToString("dd/MM/yyyy") : "-"),
        new("Fim",     p.DataFim    != default ? p.DataFim.ToString("dd/MM/yyyy")    : "-"),
        new("Arquivo", p.NomeArquivo, Color.Gray),
    };

            RenderizarEstatisticas(flpEstatProtocolo, itens);
            flpEstatProtocolo.Visible = true;
        }

        private void LimparDetalheProtocolo()
        {
            flpEstatProtocolo.Controls.Clear();
            flpEstatProtocolo.Visible = false;
        }

        private static string TraduzirStatus(StatusIatf s) => s switch
        {
            StatusIatf.Futuro => "Dia ideal ainda vai chegar",
            StatusIatf.DiaIdeal => "Hoje é o dia ideal!",
            StatusIatf.EmDia => "Em dia",
            StatusIatf.Atrasado => "Atrasado",
            StatusIatf.Finalizado => "Finalizado",
            _ => s.ToString()
        };

        // =====================================================================
        // 9. ESTATÍSTICAS DA FAZENDA
        // =====================================================================

        private void AtualizarEstatisticasFazenda()
        {
            var (totalIatfs, eficacia) = _vm.EstatisticasFazenda();

            int atrasados = _vm.TodosProtocolos.Count(p => p.Status == StatusIatf.Atrasado);
            int diaIdeal = _vm.TodosProtocolos.Count(p => p.Status == StatusIatf.DiaIdeal);
            int finalizados = _vm.TodosProtocolos.Count(p => p.Finalizado);
            int emAndamento = _vm.TodosProtocolos.Count(p => !p.Finalizado);

            // ↓ Adicione ou remova itens aqui livremente
            var itens = new List<ItemEstatistica>
    {
        new("Local",        _vm.Fazenda.Localizacao.Length > 0
                            ? _vm.Fazenda.Localizacao : "Não informada"),
        new("Total IATFs",  totalIatfs.ToString()),
        new("Finalizados",  finalizados.ToString()),
        new("Em andamento", emAndamento.ToString()),
        new("Dia ideal",    diaIdeal.ToString(),
            diaIdeal > 0 ? Color.DarkGoldenrod : Color.Black),
        new("Atrasados",    atrasados.ToString(),
            atrasados > 0 ? Color.Red : Color.DarkGreen),
        new("Eficácia",     eficacia > 0 ? $"{eficacia:F1}%" : "Sem dados",
            eficacia >= 40 ? Color.DarkGreen
            : eficacia > 0 ? Color.DarkOrange
            : Color.Gray),
    };

            RenderizarEstatisticas(flpEstatFazenda, itens);
        }

        // =====================================================================
        // 10. BOTÕES DE AÇÃO
        // =====================================================================

        private void btnAbrir_Click(object sender, EventArgs e) => AbrirProtocolo();
        private void btnExcluir_Click(object sender, EventArgs e) => ExcluirProtocolo();
        private void btnCriar_Click(object sender, EventArgs e) => CriarProtocolo();

        private void AbrirProtocolo()
        {
            var p = ObterProtocoloSelecionado();
            if (p == null) { MsgSelecione(); return; }

            try
            {
                var viewModel = CriarViewModelParaProtocolo(p);
                using var tela = new FrmMenuTabelas(viewModel);
                tela.ShowDialog(this);

                RecarregarAposNavegacao();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs.AbrirProtocolo", ex);
                MessageBox.Show($"Erro ao abrir protocolo:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExcluirProtocolo()
        {
            var p = ObterProtocoloSelecionado();
            if (p == null) { MsgSelecione(); return; }

            if (MessageBox.Show(
                    $"Excluir '{p.NomeArquivo}'?\n\nEsta ação não pode ser desfeita.",
                    "Confirmar exclusão",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                _vm.ExcluirProtocolo(p);
                RecarregarAposNavegacao();
                AppLogger.Info($"Protocolo excluído: {p.NomeArquivo}");
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs.ExcluirProtocolo", ex);
                MessageBox.Show($"Erro ao excluir:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CriarProtocolo()
        {
            using var dlgModo = new FrmEscolhaModo();
            if (dlgModo.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var viewModel = new ProtocoloViewModel(
                    new CsvRepository(), new EstatisticasService(), new DateCalculatorService());

                viewModel.ConfigurarNovoLote(
                    _vm.Fazenda.Nome,
                    _vm.Fazenda.CaminhoPasta,
                    modoCompleto: dlgModo.ModoCompleto);

                using var tela = new FrmMenuTabelas(viewModel);
                tela.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs.CriarProtocolo", ex);
                MessageBox.Show($"Erro ao criar protocolo:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // ← SEMPRE recarrega, independente de como o form fechou
                RecarregarAposNavegacao();
            }
        }

        // =====================================================================
        // 11. CALLBACK DA IA (thread-safe)
        // =====================================================================

        private void OnEstimativaCalculada(IatfResumo resumo)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { Invoke(() => OnEstimativaCalculada(resumo)); return; }

            // Atualiza apenas a célula de sucesso da linha correspondente
            foreach (DataGridViewRow row in gridProtocolos.Rows)
            {
                if (row.Tag is IatfResumo p && p.Id == resumo.Id)
                {
                    row.Cells["colSucesso"].Value = FormatarSucesso(resumo);
                    row.Cells["colSucesso"].Style.ForeColor = Color.FromArgb(0, 112, 192);
                    row.Cells["colSucesso"].Style.Font =
                        new Font(gridProtocolos.Font, FontStyle.Bold);
                    break;
                }
            }

            // Atualiza o detalhe se o protocolo atualizado está selecionado
            if (ObterProtocoloSelecionado()?.Id == resumo.Id)
                AtualizarDetalheProtocolo(resumo);
        }

        // =====================================================================
        // 12. UTILITÁRIOS PRIVADOS
        // =====================================================================

        private IatfResumo ObterProtocoloSelecionado() =>
            gridProtocolos.CurrentRow?.Tag as IatfResumo;

        private ProtocoloViewModel CriarViewModelParaProtocolo(IatfResumo p)
        {
            var vm = new ProtocoloViewModel(
                new CsvRepository(),
                new EstatisticasService(),
                new DateCalculatorService());

            vm.ConfigurarNovoLote(
                _vm.Fazenda.Nome,
                System.IO.Path.GetDirectoryName(p.CaminhoArquivo),
                modoCompleto: p.Finalizado,
                dataInicio: p.DataInicio != default
                            ? p.DataInicio.ToString("dd-MM-yyyy") : null);

            vm.DefinirArquivoExistente(p.CaminhoArquivo);

            return vm;
        }

        private void RecarregarAposNavegacao()
        {
            _vm.Carregar(_vm.Fazenda);
            PopularGrid();
            AtualizarEstatisticasFazenda();
            LimparDetalheProtocolo();
        }

        private static void MsgSelecione() =>
            MessageBox.Show("Selecione um protocolo na lista.",
                "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnImportar_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Importar tabela para esta fazenda",
                Filter = "Tabelas (*.csv;*.xlsx)|*.csv;*.xlsx"
            };

            // Abre direto na pasta da fazenda selecionada
            if (Directory.Exists(_vm.Fazenda.CaminhoPasta))
                ofd.InitialDirectory = _vm.Fazenda.CaminhoPasta;

            if (ofd.ShowDialog() != DialogResult.OK) return;

            // Pergunta se importa para a pasta da fazenda ou mantém no local original
            var destino = MessageBox.Show(
                $"Copiar o arquivo para a pasta da fazenda '{_vm.Fazenda.Nome}'?\n\n" +
                "Sim = Copia para a pasta da fazenda (recomendado).\n" +
                "Não = Usa o arquivo no local original.",
                "Destino da importação",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (destino == DialogResult.Cancel) return;

            try
            {
                string caminhoFinal = ofd.FileName;

                if (destino == DialogResult.Yes)
                {
                    string nomeArquivo = Path.GetFileName(ofd.FileName);
                    caminhoFinal = Path.Combine(_vm.Fazenda.CaminhoPasta, nomeArquivo);

                    if (File.Exists(caminhoFinal))
                    {
                        var sobrescrever = MessageBox.Show(
                            $"Já existe um arquivo com o nome '{nomeArquivo}' nesta pasta.\n\nSobrescrever?",
                            "Arquivo existente",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (sobrescrever != DialogResult.Yes) return;
                    }

                    File.Copy(ofd.FileName, caminhoFinal, overwrite: true);
                }

                AppLogger.Acao("Importar", caminhoFinal);
                MessageBox.Show("Arquivo importado com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RecarregarAposNavegacao();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs.btnImportar_Click", ex);
                MessageBox.Show($"Erro ao importar:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            var p = ObterProtocoloSelecionado();
            if (p == null) { MsgSelecione(); return; }

            using var sfd = new SaveFileDialog
            {
                Title = "Exportar protocolo",
                Filter = "Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv",
                FileName = Path.GetFileNameWithoutExtension(p.NomeArquivo),
                InitialDirectory = _vm.Fazenda.CaminhoPasta
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Carrega o lote original e salva no novo caminho
                var csvRepo = new CsvRepository();
                var excelRepo = new ExcelRepository();

                IArquivoRepository origem = Path.GetExtension(p.CaminhoArquivo)
                    .Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                    ? excelRepo : csvRepo;

                IArquivoRepository destino = Path.GetExtension(sfd.FileName)
                    .Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                    ? excelRepo : csvRepo;

                var lote = origem.Carregar(p.CaminhoArquivo);
                destino.Salvar(lote, sfd.FileName);

                AppLogger.Acao("Exportar", sfd.FileName);
                MessageBox.Show("Exportação concluída!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs.btnExportar_Click", ex);
                MessageBox.Show($"Erro ao exportar:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            var p = ObterProtocoloSelecionado();
            if (p == null) { MsgSelecione(); return; }

            ImprimirProtocolo(p);
        }

        private void ImprimirProtocolo(IatfResumo p)
        {
            try
            {
                var csvRepo = new CsvRepository();
                var excelRepo = new ExcelRepository();
                IArquivoRepository repo = Path.GetExtension(p.CaminhoArquivo)
                    .Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                    ? excelRepo : csvRepo;

                var lote = repo.Carregar(p.CaminhoArquivo);
                var animais = lote.Animais;

                // Definição das colunas a imprimir
                var colunas = new (string Nome, int Largura)[]
                {
            ("Brinco",       55),
            ("Categoria",    75),
            ("Raça",         70),
            ("ECC",          35),
            ("Lote",         60),
            ("D0",           85),
            ("D8",           85),
            ("IATF",         85),
            ("DG",           70),
            ("Resultado",    65),
            ("Ovário",       65),
            ("Destino",      65),
            ("Observações", 100),
                };

                int indiceLinha = 0;

                using var pd = new PrintDocument();
                pd.DocumentName = p.NomeArquivo;
                pd.DefaultPageSettings.Landscape = true; // paisagem para caber todas as colunas

                pd.PrintPage += (s, ev) =>
                {
                    float x = ev.MarginBounds.Left;
                    float y = ev.MarginBounds.Top;
                    float largura = ev.MarginBounds.Width;
                    var g = ev.Graphics;

                    var fTitulo = new Font("Segoe UI", 11, FontStyle.Bold);
                    var fCabecalho = new Font("Segoe UI", 7, FontStyle.Bold);
                    var fDados = new Font("Segoe UI", 7);
                    float altLinha = fDados.GetHeight(g) + 3;

                    if (indiceLinha == 0)
                    {
                        // Título
                        g.DrawString($"Pecuária Digital — {_vm.Fazenda.Nome}",
                            fTitulo, Brushes.Black, x, y);
                        y += fTitulo.GetHeight(g) + 3;

                        g.DrawString(
                            $"Protocolo: {p.NomeArquivo}   " +
                            $"Etapa: {p.EstagioAtual}   " +
                            $"Vacas: {p.NumeroDeVacas}   " +
                            $"Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}",
                            fDados, Brushes.DimGray, x, y);
                        y += fDados.GetHeight(g) + 8;

                        g.DrawLine(Pens.Black, x, y, x + largura, y);
                        y += 4;

                        // Cabeçalho das colunas
                        float cx = x;
                        foreach (var (nome, larg) in colunas)
                        {
                            g.DrawString(nome, fCabecalho, Brushes.Black,
                                new RectangleF(cx, y, larg, altLinha));
                            cx += larg;
                        }
                        y += altLinha + 2;
                        g.DrawLine(Pens.Gray, x, y, x + largura, y);
                        y += 2;
                    }

                    while (indiceLinha < animais.Count)
                    {
                        if (y + altLinha > ev.MarginBounds.Bottom)
                        {
                            ev.HasMorePages = true;
                            return;
                        }

                        var a = animais[indiceLinha];
                        var valores = new string[]
                        {
                    a.Id,
                    a.Categoria,
                    a.Raca,
                    a.Ecc,
                    a.Lote,
                    a.DataD0?.ToString("dd/MM/yy HH:mm")   ?? "",
                    a.DataD8?.ToString("dd/MM/yy HH:mm")   ?? "",
                    a.DataIATF?.ToString("dd/MM/yy HH:mm") ?? "",
                    a.DataDG?.ToString("dd/MM/yy")         ?? "",
                    a.ResultadoDG,
                    a.Ovario,
                    a.Destino,
                    a.Observacoes
                        };

                        float cx = x;
                        for (int i = 0; i < valores.Length; i++)
                        {
                            g.DrawString(valores[i], fDados, Brushes.Black,
                                new RectangleF(cx, y, colunas[i].Largura, altLinha));
                            cx += colunas[i].Largura;
                        }

                        // Linha zebrada suave
                        if (indiceLinha % 2 == 0)
                            g.FillRectangle(new SolidBrush(Color.FromArgb(15, 0, 0, 0)),
                                x, y, largura, altLinha);

                        y += altLinha;
                        indiceLinha++;
                    }

                    ev.HasMorePages = false;
                };

                using var ppd = new PrintPreviewDialog
                {
                    Document = pd,
                    Width = 1000,
                    Height = 700
                };
                ppd.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuIATFs.ImprimirProtocolo", ex);
                MessageBox.Show($"Erro ao imprimir:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}