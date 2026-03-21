using ClosedXML.Excel;
using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using Pecuaria_Digital.ViewModels;
using Pecuaria_Digital.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing; // Necessário para cores (Color.Red, etc)
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq; // Necessário para .First(), .Skip()
using System.Windows.Forms;

namespace Pecuaria_Digital
{
    public partial class FrmMenuTabelas : Form
    {
        #region 1. Variáveis Globais, Enums e Estado
        // =================================================================================
        // ESTADO DO SISTEMA E VARIÁVEIS DE CONTROLE
        // =================================================================================

        // --- Variáveis Privadas ---
        private readonly DateCalculatorService _dateCalc = new DateCalculatorService();
        private readonly IArquivoRepository _csvRepository = new CsvRepository();
        private readonly IArquivoRepository _excelRepository = new ExcelRepository();
        private readonly ProtocoloViewModel _viewModel;
        private readonly EstatisticasService _estatisticasService = new EstatisticasService();
        // --- Dados do Ambiente ---
        private string _caminhoArquivoAberto = "";
        private string _diretorioDeTrabalhoAtual = "";
        private bool _sistemaEstaOcupado = false;

        // --- Controle de Fluxo (Travas) ---
        // Evita que o evento ValueChanged dispare recursivamente ao alterar data via código
        private bool _atualizandoDatasAutomaticamente = false;

        // Flags para respeitar a edição manual do usuário nas datas
        private bool _dataD8DefinidaManualmente = false;
        private bool _dataIATFDefinidaManualmente = false;
        private bool _dataDGDefinidaManualmente = false;

        // --- Sistema de Ajuda ---
        private Dictionary<Control, (string Titulo, string Descricao)> mapaAjuda;


        // --- Enumeração dos Filtros ---
        public enum TipoFiltro
        {
            TextoExato,
            TextoContem,
            MaiorQue,
            MenorQue,
            IgualNumerico
        }
        #endregion

        #region 2. Construtor e Inicialização (Load)
        // =================================================================================
        // INICIALIZAÇÃO DA TELA
        // =================================================================================
        public FrmMenuTabelas(ProtocoloViewModel viewModel)
        {
            _viewModel = viewModel;

            InitializeComponent();
            _cbResultadoDG.SelectedIndex = 0;
        }

        private void FrmMenuTabela_Load(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_viewModel.CaminhoPasta))
                    _diretorioDeTrabalhoAtual = _viewModel.CaminhoPasta;

                // 1. Configura as NomesColunasGrid da tabela (Grid)
                ConfigurarSuperTabela();

                // 2. Define dados iniciais (Simulação)
                _viewModel.Inicializar();
                AtualizarInterfaceGeral();

                this.Text = $"Pecuária Digital - {_viewModel.NomeFazenda} ({_viewModel.DataInicioInseminacao})";
                btnAvancar.BackColor = Color.White;

                // 3. Carrega dados do disco
                CarregarTabelaDoArquivo();

                // 4. Inicia os Filtros
                ConfigurarEventosDeFiltro();

                // 5. Inicia sistema de ajuda e foca no primeiro campo
                ConfigurarSistemaDeAjuda();

                if (mapaAjuda.ContainsKey(_numero))
                {
                    lblAjudaTitulo.Text = mapaAjuda[_numero].Titulo;
                    lblAjudaDescricao.Text = mapaAjuda[_numero].Descricao;
                }
                this.ActiveControl = _numero;
                ResetarCalculadoraDatas();
                AppLogger.Info($"Sistema iniciado. Versão: 1.0.0");
            }
            catch (IOException ex)
            {
                AppLogger.Erro("Falha ao iniciar arquivo", ex);
                MessageBox.Show(
                    "Não foi possível abrir. O arquivo pode estar aberto em outro programa.\n\n" +
                    "Detalhes registrados no log.",
                    "Erro ao Abrir Arquivo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }
        #endregion

        #region 3. Ações Principais (Inserir, Excluir, Navegar)
        // =================================================================================
        // BOTÕES DE AÇÃO (CRUD E NAVEGAÇÃO)
        // =================================================================================

        private void inserir_Click(object sender, EventArgs e)
        {
            // ── Validações ────────────────────────────────────────────────────────────
            var (valido, mensagem) = ValidacaoService.ValidarInsercaoAnimal(
                _numero.Text, _categoria.Text, _raca.Text);
            if (!valido)
            {
                MessageBox.Show(mensagem, "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (eccValido, eccMensagem) = ValidacaoService.ValidarEcc(_ecc.Text);
            if (!eccValido)
            {
                MessageBox.Show(eccMensagem, "ECC Inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ecc.Focus();
                return;
            }

            // ── Busca linhas com este ID ──────────────────────────────────────────────
            string idDigitado = _numero.Text.Trim().ToUpper();
            DataGridViewRow linhaAlvo = null;
            bool ehNovaLinha = true;

            var linhasComEsteId = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in tabela.Rows)
            {
                if (row.IsNewRow) continue;
                string idTabela = row.Cells[NomesColunasGrid.Id].Value
                    ?.ToString().Trim().ToUpper() ?? "";
                if (idTabela == idDigitado)
                    linhasComEsteId.Add(row);
            }

            // ── Decide o que fazer com o ID ───────────────────────────────────────────
            if (linhasComEsteId.Count == 0)
            {
                // ID completamente novo — cria linha
                ehNovaLinha = true;
            }
            else if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio
                     || _viewModel.ModoTabelaCompleta)
            {
                // D0 / Modo completo — pergunta o que fazer
                var escolha = MessageBox.Show(
                    $"O brinco '{idDigitado}' já existe na tabela.\n\n" +
                    "Sim       → Atualizar o(s) animal(is) existente(s)\n" +
                    "Não       → Criar uma nova linha com o mesmo ID\n" +
                    "Cancelar → Voltar sem fazer nada",
                    "Brinco já cadastrado",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (escolha == DialogResult.Cancel) return;

                if (escolha == DialogResult.No)
                {
                    // Usuário quer nova linha mesmo com ID repetido
                    ehNovaLinha = true;
                }
                else
                {
                    // Usuário quer atualizar — se houver múltiplos, escolhe qual
                    if (linhasComEsteId.Count == 1)
                    {
                        linhaAlvo = linhasComEsteId[0];
                        ehNovaLinha = false;
                    }
                    else
                    {
                        using var dlg = new FrmSelecionarAnimal(idDigitado, linhasComEsteId);
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        linhaAlvo = dlg.LinhaSelecionada;
                        ehNovaLinha = false;
                    }
                }
            }
            else
            {
                // D8 / IATF / DG — atualiza; se múltiplos, seleciona qual
                if (linhasComEsteId.Count == 1)
                {
                    linhaAlvo = linhasComEsteId[0];
                    ehNovaLinha = false;
                }
                else
                {
                    using var dlg = new FrmSelecionarAnimal(idDigitado, linhasComEsteId);
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    linhaAlvo = dlg.LinhaSelecionada;
                    ehNovaLinha = false;
                }
            }

            // Cria a linha se necessário
            if (ehNovaLinha)
            {
                int idx = tabela.Rows.Add();
                linhaAlvo = tabela.Rows[idx];
                linhaAlvo.Cells[NomesColunasGrid.Id].Value = idDigitado;
            }

            // ── Salva dados com proteção de etapa ────────────────────────────────────
            // BLOCO D0
            if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio
                || _viewModel.ModoTabelaCompleta || ehNovaLinha)
            {
                AprenderNovaOpcao(NomesColunasGrid.Categoria, _categoria.Text, _categoria);
                AprenderNovaOpcao(NomesColunasGrid.Raca, _raca.Text, _raca);
                AprenderNovaOpcao(NomesColunasGrid.Lote, _lote.Text, _lote);

                if (!string.IsNullOrWhiteSpace(_categoria.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Categoria].Value = _categoria.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_raca.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Raca].Value = _raca.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_ecc.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Ecc].Value = _ecc.Text;
                if (!string.IsNullOrWhiteSpace(_lote.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Lote].Value = _lote.Text;

                linhaAlvo.Cells[NomesColunasGrid.DataD0].Value =
                    _dtpD0.Value.ToString("dd/MM/yyyy HH:mm");
            }

            // Observações — global, só salva se preenchido
            if (!string.IsNullOrWhiteSpace(_observacoes.Text))
                linhaAlvo.Cells[NomesColunasGrid.Obs].Value = _observacoes.Text;

            // BLOCO D8
            if (_viewModel.EstagioAtual == EstagioProtocolo.D8_Retirada
             || _viewModel.ModoTabelaCompleta)
            {
                linhaAlvo.Cells[NomesColunasGrid.PerdeuImplante].Value = _chkPerdeuImplante.Checked;
                linhaAlvo.Cells[NomesColunasGrid.Ecg].Value = _chkEcg.Checked;

                // ✅ No modo completo, SEMPRE usa o valor do DatePicker — o usuário o preencheu
                DateTime dataD8 = _dtpD8.Value;

                linhaAlvo.Cells[NomesColunasGrid.DataD8].Value =
                    dataD8.ToString(ProtocoloConstants.FormatoDataHora);
            }

            // BLOCO IATF
            if (_viewModel.EstagioAtual == EstagioProtocolo.D10_IATF
                || _viewModel.ModoTabelaCompleta)
            {
                AprenderNovaOpcao(NomesColunasGrid.Touro, _touro.Text, _touro);
                AprenderNovaOpcao(NomesColunasGrid.Inseminador, _inseminador.Text, _inseminador);

                if (!string.IsNullOrWhiteSpace(_touro.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Touro].Value = _touro.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_inseminador.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Inseminador].Value = _inseminador.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_txtEscoreCio.Text))
                    linhaAlvo.Cells[NomesColunasGrid.EscoreCio].Value = _txtEscoreCio.Text;

                linhaAlvo.Cells[NomesColunasGrid.Gnrh].Value = _chkGnrh.Checked;
                DateTime dataIATF = _dtpIATF.Value;

                linhaAlvo.Cells[NomesColunasGrid.DataIATF].Value =
                    dataIATF.ToString(ProtocoloConstants.FormatoDataHora);
            }

            // BLOCO DG
            if (_viewModel.EstagioAtual == EstagioProtocolo.DG_Diagnostico
                || _viewModel.EstagioAtual == EstagioProtocolo.Finalizado
                || _viewModel.ModoTabelaCompleta)
            {
                AprenderNovaOpcao(NomesColunasGrid.Ovario, _txtOvario.Text, _txtOvario);
                AprenderNovaOpcao(NomesColunasGrid.Destino, _txtDestino.Text, _txtDestino);
                AprenderNovaOpcao(NomesColunasGrid.ResultadoDG, _cbResultadoDG.Text, _cbResultadoDG);

                if (!string.IsNullOrWhiteSpace(_cbResultadoDG.Text))
                    linhaAlvo.Cells[NomesColunasGrid.ResultadoDG].Value = _cbResultadoDG.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_txtOvario.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Ovario].Value = _txtOvario.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_txtDestino.Text))
                    linhaAlvo.Cells[NomesColunasGrid.Destino].Value = _txtDestino.Text.ToUpper();

                DateTime dataDG = _dtpDG.Value;

                linhaAlvo.Cells[NomesColunasGrid.DataDG].Value =
                    dataDG.ToString(ProtocoloConstants.FormatoDataSimples);
            }

            // ── Limpeza inteligente ───────────────────────────────────────────────────
            if (ehNovaLinha)
            {
                AtualizarMemoriaNumeros();
                _numero.Clear();

                if (_viewModel.ModoTabelaCompleta
                    || _viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio)
                    ResetarCalculadoraDatas();
            }
            else
            {
                MessageBox.Show($"Animal {idDigitado} atualizado com sucesso!");
            }

            // ── Limpeza dos campos da etapa — SEMPRE, nova linha ou atualização ──────────
            _observacoes.Clear();

            if (_viewModel.ModoTabelaCompleta)
            {
                _ecc.Clear();
                _chkPerdeuImplante.Checked = false;
                _chkEcg.Checked = false;
                _txtEscoreCio.Clear();
                _chkGnrh.Checked = false;
                _txtOvario.Text = "";
                _txtDestino.Text = "";
            }
            else
            {
                switch (_viewModel.EstagioAtual)
                {
                    case EstagioProtocolo.D0_Inicio:
                        _ecc.Clear();
                        LimparControles(pnlD8);
                        LimparControles(pnlIATF);
                        _txtEscoreCio.Clear();
                        LimparControles(pnlDG);
                        break;

                    case EstagioProtocolo.D8_Retirada:
                        _chkPerdeuImplante.Checked = false;
                        _chkEcg.Checked = false;
                        break;

                    case EstagioProtocolo.D10_IATF:
                        _txtEscoreCio.Clear();
                        _chkGnrh.Checked = false;
                        break;

                    case EstagioProtocolo.DG_Diagnostico:
                        LimparControles(pnlDG);
                        break;
                }
            }

            _numero.Focus();
            CalcularEstatisticas();
            SalvarTabelaNoArquivo();
            CalcularTodasMedias();
        }

        // Função auxiliar necessária dentro da classe (caso não tenha)
        private void LimparControles(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox) ((TextBox)c).Clear();
                if (c is CheckBox) ((CheckBox)c).Checked = false;
                if (c is ComboBox) ((ComboBox)c).Text = "";
            }
        }
        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (tabela.CurrentRow != null && !tabela.CurrentRow.IsNewRow)
            {
                string numeroBoi = tabela.CurrentRow.Cells[0].Value.ToString();
                var confirmacao = MessageBox.Show(
                    $"Tem certeza que deseja excluir o boi Nº {numeroBoi}?",
                    "Excluir Registro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmacao == DialogResult.Yes)
                {
                    tabela.Rows.Remove(tabela.CurrentRow);
                    SalvarTabelaNoArquivo();
                    MessageBox.Show("Registro excluído com sucesso!");
                }
            }
            else
            {
                MessageBox.Show("Por favor, clique na linha que deseja excluir.");
            }
        }

        // --- Navegação ---

        private void btnAvancar_Click(object sender, EventArgs e)
        {
            if (_viewModel.EstagioAtual == EstagioProtocolo.DG_Diagnostico)
            {
                if (MessageBox.Show("Deseja concluir?",
                    "Concluir Protocolo",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            }
            _viewModel.TentarAvancar(); // ViewModel apenas muda estado, sem dialog
            SalvarTabelaNoArquivo();
            AtualizarInterfaceGeral();
        }
        private void btnVoltar_Click(object sender, EventArgs e)
        {
            _sistemaEstaOcupado = true;
            try
            {
                _viewModel.Voltar();
                AtualizarInterfaceGeral();
            }
            finally { _sistemaEstaOcupado = false; }
        }
        private void btnConcluir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "Deseja concluir e finalizar esta tabela?",
                    "Concluir Tabela Completa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;

            _viewModel.TentarAvancar(); // avança para Finalizado
            SalvarTabelaNoArquivo();
            AtualizarInterfaceGeral();

            MessageBox.Show("Tabela finalizada com sucesso!",
                "Concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btnImprimir_Click(object sender, EventArgs e)
        {
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

            var animais = ExtrairLoteDataDaTabela().Animais;
            int indiceLinha = 0;

            using var pd = new PrintDocument();
            pd.DocumentName = _viewModel.NomeFazenda;
            pd.DefaultPageSettings.Landscape = true;

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
                    g.DrawString(
                        $"Pecuária Digital — {_viewModel.NomeFazenda} ({_viewModel.DataInicioInseminacao})",
                        fTitulo, Brushes.Black, x, y);
                    y += fTitulo.GetHeight(g) + 3;

                    g.DrawString(
                        $"Etapa: {_viewModel.EstagioAtual}   " +
                        $"Animais: {animais.Count}   " +
                        $"Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}",
                        fDados, Brushes.DimGray, x, y);
                    y += fDados.GetHeight(g) + 8;

                    g.DrawLine(Pens.Black, x, y, x + largura, y);
                    y += 4;

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

                    if (indiceLinha % 2 == 0)
                        g.FillRectangle(new SolidBrush(Color.FromArgb(12, 0, 0, 0)),
                            x, y, largura, altLinha);

                    float cx = x;
                    for (int i = 0; i < valores.Length; i++)
                    {
                        g.DrawString(valores[i], fDados, Brushes.Black,
                            new RectangleF(cx, y, colunas[i].Largura, altLinha));
                        cx += colunas[i].Largura;
                    }

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

        // Atalhos Superiores
        private void btnTesteD0_Click(object sender, EventArgs e) { _viewModel.IrParaEstagio(EstagioProtocolo.D0_Inicio); AtualizarInterfaceGeral(); }
        private void btnTesteD8_Click(object sender, EventArgs e) { _viewModel.IrParaEstagio(EstagioProtocolo.D8_Retirada); AtualizarInterfaceGeral(); }
        private void btnTesteIATF_Click(object sender, EventArgs e) { _viewModel.IrParaEstagio(EstagioProtocolo.D10_IATF); AtualizarInterfaceGeral(); }
        private void btnTesteDG_Click(object sender, EventArgs e) { _viewModel.IrParaEstagio(EstagioProtocolo.DG_Diagnostico); AtualizarInterfaceGeral(); }
        #endregion

        #region 4. Lógica de Interface e Negócio (Helpers)
        // =================================================================================
        // FUNÇÕES AUXILIARES DA UI E CÁLCULOS
        // =================================================================================

        private void AtualizarInterfaceGeral()
        {
            // --- NOVO: Esconde a navegação de etapas se for Modo Completo ---
            if (grpEtapas != null)
                grpEtapas.Visible = !_viewModel.ModoTabelaCompleta;

            AtualizarVisualizador(_viewModel.EstagioAtual);
            AtualizarPainelInsercao(_viewModel.EstagioAtual);

            // Botões de navegação (Voltar/Avançar) só fazem sentido no modo Etapas
            btnVoltar.Enabled = !_viewModel.ModoTabelaCompleta && (_viewModel.EstagioAtual != EstagioProtocolo.D0_Inicio);
            btnAvancar.Enabled = !_viewModel.ModoTabelaCompleta && (_viewModel.EstagioAtual != EstagioProtocolo.DG_Diagnostico);

            ConfigurarBotoesNavegacao();
            ConfigurarBotoesSuperiores();
            AtualizarPainelDeFiltros();
            ConfigurarEventosDeFiltro();

            if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio || _viewModel.ModoTabelaCompleta)
                _numero.AutoCompleteMode = AutoCompleteMode.None;
            else
            {
                _numero.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _numero.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }

            CalcularEstatisticas();
            CalcularTodasMedias();

            // Foca no primeiro campo disponível
            if (_numero.Visible && _numero.Enabled) this.ActiveControl = _numero;

            // Botão concluir só aparece no modo tabela completa e se ainda não estivermos finalizados
            btnConcluir.Visible = _viewModel.ModoTabelaCompleta
                     && !_viewModel.EstagioAtual.Equals(EstagioProtocolo.Finalizado);
            btnConcluir.BackColor = Color.ForestGreen;
            btnConcluir.ForeColor = Color.White;
        }

        private void AtualizarVisualizador(EstagioProtocolo estagio)
        {
            foreach (DataGridViewColumn col in tabela.Columns) col.Visible = false;

            // --- REGRA: Se for Modo Completo, considera que estamos no fim para fins de VISIBILIDADE ---
            // Isso faz todas as NomesColunasGrid aparecerem na tabela, mas o painel de inserção continua respeitando o 'estagio' atual.
            bool mostrarTudo = _viewModel.ModoTabelaCompleta;

            // Mostra NomesColunasGrid baseadas no estágio OU se for modo completo
            MostrarColuna(NomesColunasGrid.Id, true); // Sempre visível
            MostrarColuna(NomesColunasGrid.Categoria, true);
            MostrarColuna(NomesColunasGrid.Raca, true);

            if (estagio >= EstagioProtocolo.D0_Inicio || mostrarTudo)
            {
                MostrarColuna(NomesColunasGrid.Ecc, true);
                MostrarColuna(NomesColunasGrid.Lote, true);
                MostrarColuna(NomesColunasGrid.DataD0, true);
            }
            if (estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo)
            {
                MostrarColuna(NomesColunasGrid.PerdeuImplante, estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo);
                MostrarColuna(NomesColunasGrid.Ecg, estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo);
                MostrarColuna(NomesColunasGrid.DataD8, estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo);
            }
            if (estagio >= EstagioProtocolo.D10_IATF || mostrarTudo)
            {
                bool editavel = (estagio == EstagioProtocolo.D10_IATF) || mostrarTudo;
                MostrarColuna(NomesColunasGrid.EscoreCio, editavel);
                MostrarColuna(NomesColunasGrid.Gnrh, editavel);
                MostrarColuna(NomesColunasGrid.Touro, editavel);
                MostrarColuna(NomesColunasGrid.Inseminador, editavel);
                MostrarColuna(NomesColunasGrid.DataIATF, editavel);
            }
            if (estagio >= EstagioProtocolo.DG_Diagnostico || mostrarTudo)
            {
                bool editavel = (estagio == EstagioProtocolo.DG_Diagnostico) || mostrarTudo;
                MostrarColuna(NomesColunasGrid.ResultadoDG, editavel);
                MostrarColuna(NomesColunasGrid.Ovario, editavel);
                MostrarColuna(NomesColunasGrid.Destino, editavel);
                MostrarColuna(NomesColunasGrid.DataDG, editavel);
            }

            MostrarColuna(NomesColunasGrid.Obs, true);
            tabela.Refresh();
        }

        private void MostrarColuna(string nomeColuna, bool editavel)
        {
            if (tabela.Columns.Contains(nomeColuna))
            {
                tabela.Columns[nomeColuna].Visible = true;
                if (editavel)
                    tabela.Columns[nomeColuna].DefaultCellStyle.Font = new Font(tabela.Font, FontStyle.Bold);
            }
        }

        private void AtualizarPainelInsercao(EstagioProtocolo estagio)
        {
            if (pnlContainerInsercao == null) return;

            // 1. Congela a tela (Layout Stop)
            pnlContainerInsercao.SuspendLayout();

            try
            {
                // Garante que a OBS e os BOTÕES estejam dentro do container
                if (pnlObs.Parent != pnlContainerInsercao) pnlObs.Parent = pnlContainerInsercao;

                // SE VOCÊ CRIOU O pnlBotoes, DESCOMENTE A LINHA ABAIXO:
                // if (pnlBotoes.Parent != pnlContainerInsercao) pnlBotoes.Parent = pnlContainerInsercao;

                // Lista de controles para limpar
                // (Adicione pnlBotoes na lista se tiver criado ele)
                Control[] todos = { pnlD0, pnlD8, pnlIATF, pnlDG, pnlObs /*, pnlBotoes*/ };

                // 2. Reseta configurações (Solta tudo)
                foreach (var c in todos)
                {
                    if (c == null) continue;
                    c.Visible = false;
                    c.Dock = DockStyle.None;
                }

                // 3. Define qual painel de dados vamos mostrar
                Control painelDados = null;

                if (_viewModel.ModoTabelaCompleta)
                {
                    // No modo completo, o "painel de dados" são TODOS eles juntos
                    // Mas para simplificar a ordem, vamos tratar a visibilidade individualmente abaixo
                    painelDados = null;
                    pnlContainerInsercao.AutoScroll = true;
                }
                else
                {
                    switch (estagio)
                    {
                        case EstagioProtocolo.D0_Inicio: painelDados = pnlD0; break;
                        case EstagioProtocolo.D8_Retirada: painelDados = pnlD8; break;
                        case EstagioProtocolo.D10_IATF: painelDados = pnlIATF; break;
                        case EstagioProtocolo.DG_Diagnostico: painelDados = pnlDG; break;
                    }
                }

                // =========================================================
                // LÓGICA DE ORDENAÇÃO (O SEGREDO ESTÁ AQUI)
                // Para ficar: [DADOS] -> [OBS] -> [BOTÕES]
                // Chamamos BringToFront na ordem INVERSA (de baixo para cima)
                // =========================================================

                // --- 1. CONFIGURA OS BOTÕES (FUNDO / BAIXO) ---
                // Se você criou o painel pnlBotoes, use o bloco abaixo:
                /*
                if (pnlBotoes != null)
                {
                    pnlBotoes.Visible = true;
                    pnlBotoes.Dock = DockStyle.Top;
                    pnlBotoes.BringToFront(); // Coloca na pilha, mas será empurrado para baixo pelos próximos
                }
                */

                // --- 2. CONFIGURA A OBSERVAÇÃO (MEIO) ---
                pnlObs.Visible = true;
                pnlObs.Dock = DockStyle.Bottom;
                pnlObs.BringToFront(); // Fica acima dos botões

                // --- 3. CONFIGURA OS DADOS (TOPO) ---
                if (_viewModel.ModoTabelaCompleta)
                {
                    pnlContainerInsercao.AutoScroll = true;


                    // Ordem visual: D0 (Topo) > D8 > IATF > DG (Fundo dos dados)
                    // Ordem de código: DG -> IATF -> D8 -> D0

                    pnlDG.Visible = true; pnlDG.Dock = DockStyle.Top; pnlDG.BringToFront();
                    pnlIATF.Visible = true; pnlIATF.Dock = DockStyle.Top; pnlIATF.BringToFront();
                    pnlD8.Visible = true; pnlD8.Dock = DockStyle.Top; pnlD8.BringToFront();
                    pnlD0.Visible = true; pnlD0.Dock = DockStyle.Top; pnlD0.BringToFront();

                    // O D0 agora é o topo absoluto, empurrando Obs e Botões para baixo
                }
                else if (painelDados != null)
                {
                    pnlContainerInsercao.AutoScroll = true;

                    painelDados.Visible = true;
                    painelDados.Dock = DockStyle.Top;
                    painelDados.BringToFront(); // Fica no topo absoluto
                }

                // Garante o foco no campo certo
                if (estagio == EstagioProtocolo.D0_Inicio) ConfigurarOrdemFoco(_numero, _raca);
                // ... outros focos ...
            }
            finally
            {
                pnlContainerInsercao.ResumeLayout(true);
            }
        }

        private void ConfigurarSuperTabela()
        {
            tabela.Columns.Clear();
            tabela.AutoGenerateColumns = false;

            // Grupo 1: Identidade
            AdicionarColunaTexto(NomesColunasGrid.Id, "Nº Brinco", 80, true);
            AdicionarColunaCombo(NomesColunasGrid.Categoria, "Categoria", 100, new string[] { });
            AdicionarColunaCombo(NomesColunasGrid.Raca, "Raça", 100, new string[] { });

            // Grupo 2: D0
            AdicionarColunaTexto(NomesColunasGrid.Ecc, "ECC (D0)", 60, true);
            AdicionarColunaTexto(NomesColunasGrid.Lote, "Lote Trat.", 100);
            AdicionarColunaTexto(NomesColunasGrid.DataD0, "Data D0", 110);

            // Grupo 3: D8
            AdicionarColunaCheck(NomesColunasGrid.PerdeuImplante, "Perdeu Implante?", 100);
            AdicionarColunaCheck(NomesColunasGrid.Ecg, "Usou eCG?", 70);
            AdicionarColunaTexto(NomesColunasGrid.DataD8, "Data D8", 110);

            // Grupo 4: IATF
            AdicionarColunaTexto(NomesColunasGrid.EscoreCio, "Escore Cio", 70, true);
            AdicionarColunaCheck(NomesColunasGrid.Gnrh, "Indutor (GnRH)?", 100);
            AdicionarColunaTexto(NomesColunasGrid.DataIATF, "Data IATF", 110);
            AdicionarColunaCombo(NomesColunasGrid.Touro, "Touro", 120, new string[] { });
            AdicionarColunaCombo(NomesColunasGrid.Inseminador, "Inseminador", 120, new string[] { });

            // Grupo 5: DG
            AdicionarColunaCombo(NomesColunasGrid.ResultadoDG, "Resultado DG", 100, new string[] { "VAZIA", "PRENHA" });
            AdicionarColunaCombo(NomesColunasGrid.Ovario, "Est. Ovariana", 120, new string[] { });
            AdicionarColunaCombo(NomesColunasGrid.Destino, "Destino Final", 120, new string[] { });
            AdicionarColunaTexto(NomesColunasGrid.DataDG, "Data DG", 90);

            AdicionarColunaTexto(NomesColunasGrid.Obs, "Observações", 200);

            tabela.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (tabela.CurrentCell is DataGridViewCheckBoxCell)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!tabela.IsDisposed)
                            tabela.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    }));
                }
            };

            // Desativa ordenação automática
            foreach (DataGridViewColumn coluna in tabela.Columns)
                coluna.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        private void AprenderNovaOpcao(string nomeColunaGrid, string valor, ComboBox inputInsercao = null)
        {
            if (string.IsNullOrWhiteSpace(valor)) return;
            valor = valor.Trim().ToUpper();

            if (tabela.Columns.Contains(nomeColunaGrid))
            {
                var colunaCombo = tabela.Columns[nomeColunaGrid] as DataGridViewComboBoxColumn;
                if (colunaCombo != null && !colunaCombo.Items.Contains(valor))
                    colunaCombo.Items.Add(valor);
            }

            if (inputInsercao != null && !inputInsercao.Items.Contains(valor))
                inputInsercao.Items.Add(valor);
        }

        private void LimparCamposDoPainelAtivo()
        {
            void LimparControles(Control parent)
            {
                foreach (Control c in parent.Controls)
                {
                    if (c is TextBox) ((TextBox)c).Clear();
                    if (c is CheckBox) ((CheckBox)c).Checked = false;
                }
            }
            if (_viewModel.ModoTabelaCompleta)
            {
                LimparControles(pnlD0);
                LimparControles(pnlD8);
                LimparControles(pnlIATF);
                LimparControles(pnlDG);
                _txtEscoreCio.Clear();
            }
            else if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio) LimparControles(pnlD0);
            else if (_viewModel.EstagioAtual == EstagioProtocolo.D8_Retirada) LimparControles(pnlD8);
            else if (_viewModel.EstagioAtual == EstagioProtocolo.D10_IATF) { LimparControles(pnlIATF); _txtEscoreCio.Clear(); }
            else if (_viewModel.EstagioAtual == EstagioProtocolo.DG_Diagnostico) LimparControles(pnlDG);

            _dataD8DefinidaManualmente = false;
            _dataIATFDefinidaManualmente = false;
            _dataDGDefinidaManualmente = false;
            _observacoes.Clear();
        }

        private void AtualizarMemoriaNumeros()
        {
            _numero.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _numero.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection listaBrincos = new AutoCompleteStringCollection();

            foreach (DataGridViewRow linha in tabela.Rows)
            {
                string id = linha.Cells[NomesColunasGrid.Id].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(id)) listaBrincos.Add(id);
            }
            _numero.AutoCompleteCustomSource = listaBrincos;
        }

        // Funções para criar NomesColunasGrid
        private void AdicionarColunaCombo(string nome, string titulo, int largura, string[] opcoes)
        {
            var col = new DataGridViewComboBoxColumn { Name = nome, HeaderText = titulo, Width = largura, FlatStyle = FlatStyle.Popup };
            col.Items.AddRange(opcoes);
            tabela.Columns.Add(col);
        }
        private void AdicionarColunaTexto(string nome, string titulo, int largura, bool ehNumero = false)
        {
            var col = new DataGridViewTextBoxColumn { Name = nome, HeaderText = titulo, Width = largura };
            if (ehNumero) col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            tabela.Columns.Add(col);
        }
        private void AdicionarColunaCheck(string nome, string titulo, int largura)
        {
            var col = new DataGridViewCheckBoxColumn { Name = nome, HeaderText = titulo, Width = largura };
            tabela.Columns.Add(col);
        }

        // Funções de Botões Visuais
        private void AtualizarEstadoBotao(Button btn, EstagioProtocolo estagioDoBotao)
        {
            if (estagioDoBotao == _viewModel.MaiorEstagioAlcancado)
            {
                if (_viewModel.EstagioAtual == _viewModel.MaiorEstagioAlcancado) { btn.BackColor = Color.ForestGreen; btn.ForeColor = Color.White; btn.Enabled = false; }
                else { btn.BackColor = Color.LightGreen; btn.ForeColor = Color.Black; btn.Enabled = true; }
            }
            else if (estagioDoBotao == _viewModel.EstagioAtual) { btn.BackColor = Color.DarkGray; btn.ForeColor = Color.White; btn.Enabled = false; }
            else if (estagioDoBotao < _viewModel.MaiorEstagioAlcancado) { btn.BackColor = Color.White; btn.ForeColor = Color.Black; btn.Enabled = true; }
            else { btn.BackColor = Color.FromArgb(240, 240, 240); btn.ForeColor = Color.Gray; btn.Enabled = false; }

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Silver;
        }

        private void ConfigurarBotoesNavegacao()
        {
            // Lógica do Botão Voltar (Permanece igual)
            btnVoltar.Enabled = !_viewModel.ModoTabelaCompleta && (_viewModel.EstagioAtual != EstagioProtocolo.D0_Inicio);

            // Lógica do Botão Avançar / Concluir
            if (_viewModel.ModoTabelaCompleta)
            {
                // Se já finalizou, desabilita
                btnAvancar.Text = "Concluído";
                btnAvancar.Enabled = false;
                btnAvancar.BackColor = SystemColors.Control;
                btnAvancar.ForeColor = Color.Gray;
            }
            else if (_viewModel.EstagioAtual == EstagioProtocolo.DG_Diagnostico)
            {
                // --- MUDANÇA AQUI: No DG, o botão vira "Concluir" ---
                btnAvancar.Text = "Concluir ✓";
                btnAvancar.Enabled = true; // Garante que esteja habilitado
                btnAvancar.BackColor = Color.ForestGreen; // Destaque visual (Verde)
                btnAvancar.ForeColor = Color.White;
            }
            else
            {
                // Comportamento normal (D0, D8, IATF)
                btnAvancar.Text = "Avançar ▶";
                btnAvancar.Enabled = true;
                btnAvancar.BackColor = SystemColors.Control; // Cor padrão do Windows
                btnAvancar.ForeColor = Color.Black;
            }
        }

        private void ConfigurarBotoesSuperiores()
        {
            AtualizarEstadoBotao(btnTesteD0, EstagioProtocolo.D0_Inicio);
            AtualizarEstadoBotao(btnTesteD8, EstagioProtocolo.D8_Retirada);
            AtualizarEstadoBotao(btnTesteIATF, EstagioProtocolo.D10_IATF);
            AtualizarEstadoBotao(btnTesteDG, EstagioProtocolo.DG_Diagnostico);
        }

        private void ConfigurarOrdemFoco(Control controleAnterior, Control proximoControle)
        {
            proximoControle.TabIndex = controleAnterior.TabIndex + 1;
        }

        private string TraduzirEstagio(EstagioProtocolo estagio)
        {
            switch (estagio)
            {
                case EstagioProtocolo.D0_Inicio: return "Início (D0)";
                case EstagioProtocolo.D8_Retirada: return "Retirada de Implante (D8)";
                case EstagioProtocolo.D10_IATF: return "Inseminação (IATF)";
                case EstagioProtocolo.DG_Diagnostico: return "Diagnóstico (DG)";
                default: return "";
            }
        }
        #endregion

        #region 5. Cálculos Matemáticos e Datas
        // =================================================================================
        // LÓGICA MATEMÁTICA E DATAS
        // =================================================================================

        /// <summary>
        /// Lê as linhas VISÍVEIS da grade e converte em lista de Animal.
        /// Usado para calcular estatísticas respeitando filtros ativos.
        /// </summary>
        private List<Animal> ObterAnimaisVisiveis()
        {
            var lista = new List<Animal>();

            foreach (DataGridViewRow linha in tabela.Rows)
            {
                if (linha.IsNewRow || !linha.Visible) continue;

                string G(string col) =>
                    linha.Cells[col].Value?.ToString() ?? string.Empty;

                bool B(string col)
                {
                    var val = linha.Cells[col].Value;
                    return val != null && Convert.ToBoolean(val);
                }

                lista.Add(new Animal
                {
                    Id = G(NomesColunasGrid.Id),
                    Touro = G(NomesColunasGrid.Touro),
                    Inseminador = G(NomesColunasGrid.Inseminador),
                    ResultadoDG = G(NomesColunasGrid.ResultadoDG),
                    Ecc = G(NomesColunasGrid.Ecc),
                    EscoreCio = G(NomesColunasGrid.EscoreCio),
                    PerdeuImplante = B(NomesColunasGrid.PerdeuImplante),
                    UsouEcg = B(NomesColunasGrid.Ecg),
                    UsouGnrh = B(NomesColunasGrid.Gnrh),
                });
            }

            return lista;
        }

        /// <summary>
        /// Delega o cálculo ao EstatisticasService e atualiza os labels de resultado.
        /// </summary>
        private void CalcularEstatisticas()
        {
            var animais = ObterAnimaisVisiveis();
            _estatisticasService.Calcular(animais);

            // ── Textos ────────────────────────────────────────────────────────
            lblStatConcepcao.Text = _estatisticasService.TotalInseminadas > 0
                ? $"{_estatisticasService.TaxaConcepcao:F1}%"
                : "-";

            lblStatServico.Text = _estatisticasService.TotalAnimais > 0
                ? $"{_estatisticasService.TaxaServico:F1}%"
                : "-";

            lblStatPrenhez.Text = _estatisticasService.TotalAnimais > 0
                ? $"{_estatisticasService.TaxaPrenhez:F1}%"
                : "-";

            lblStatDoses.Text = _estatisticasService.TotalPrenhas > 0
                ? $"{_estatisticasService.IndiceInseminacao:F2}"
                : "-";

            lblStatTotal.Text = _estatisticasService.TotalAnimais.ToString();

            // ── Cores (usando as metas centralizadas em ProtocoloConstants) ───
            lblStatConcepcao.ForeColor = _estatisticasService.TotalInseminadas == 0
                ? Color.Gray
                : _estatisticasService.TaxaConcepcao >= ProtocoloConstants.MetaTaxaConcepcao
                    ? Color.Green
                    : Color.Red;

            lblStatServico.ForeColor = _estatisticasService.TotalAnimais == 0
                ? Color.Gray
                : _estatisticasService.TaxaServico >= ProtocoloConstants.MetaTaxaServico
                    ? Color.Green
                    : Color.Orange;

            lblStatPrenhez.ForeColor = _estatisticasService.TotalAnimais == 0
                ? Color.Gray
                : _estatisticasService.TaxaPrenhez >= ProtocoloConstants.MetaTaxaPrenhez
                    ? Color.Green
                    : Color.Red;

            lblStatDoses.ForeColor = _estatisticasService.TotalPrenhas == 0
                ? Color.Gray
                : _estatisticasService.IndiceInseminacao <= ProtocoloConstants.MetaIndiceInsemBom
                    ? Color.Green
                    : _estatisticasService.IndiceInseminacao <= ProtocoloConstants.MetaIndiceInsemMedio
                        ? Color.Orange
                        : Color.Red;

            lblStatTotal.ForeColor = Color.Black;
        }

        private void CalcularTodasMedias()
        {
            // Coleta datas reais da tabela
            var datasD0 = new List<DateTime>();
            var datasD8 = new List<DateTime>();
            var datasIATF = new List<DateTime>();
            var datasDG = new List<DateTime>();

            foreach (DataGridViewRow linha in tabela.Rows)
            {
                if (linha.IsNewRow || !linha.Visible) continue;

                if (DateTime.TryParse(linha.Cells[NomesColunasGrid.DataD0].Value?.ToString(),
                    out DateTime d0)) datasD0.Add(d0);
                if (DateTime.TryParse(linha.Cells[NomesColunasGrid.DataD8].Value?.ToString(),
                    out DateTime d8)) datasD8.Add(d8);
                if (DateTime.TryParse(linha.Cells[NomesColunasGrid.DataIATF].Value?.ToString(),
                    out DateTime iatf)) datasIATF.Add(iatf);
                if (DateTime.TryParse(linha.Cells[NomesColunasGrid.DataDG].Value?.ToString(),
                    out DateTime dg)) datasDG.Add(dg);
            }

            // ← SERVIÇO CENTRALIZADO faz todo o cálculo
            var svc = new DateCentralService();
            var datas = svc.Calcular(datasD0, datasD8, datasIATF, datasDG);

            // Atualiza os labels do painel Datas
            AtualizarLabelMedia(lblMediaD0, "D0", datas.D0Exibida, ehEstimativa: false);
            AtualizarLabelMedia(lblMediaD8, "D8", datas.D8Exibida, datas.D8EhEstimativa);
            AtualizarLabelMedia(lblMediaIATF, "IATF", datas.IATFExibida, datas.IATFEhEstimativa);
            AtualizarLabelMedia(lblMediaDG, "DG", datas.DGExibida, datas.DGEhEstimativa);
        }

        private void AtualizarLabelMedia(Label lbl, string titulo,
            DateTime? data, bool ehEstimativa)
        {
            if (!data.HasValue || data == default(DateTime))
            {
                lbl.Text = $"{titulo}: -";
                lbl.ForeColor = Color.Gray;
                lbl.Font = new Font(lbl.Font, FontStyle.Regular);
                return;
            }

            if (!ehEstimativa)
            {
                // Data real → preto, sem sufixo
                lbl.Text = $"{titulo}: {data.Value:dd/MM HH:mm}";
                lbl.ForeColor = Color.Black;
                lbl.Font = new Font(lbl.Font, FontStyle.Regular);
                return;
            }

            // Data estimada → colorida conforme classificação
            var classificacao = DateCentralService.Classificar(data.Value);

            lbl.Text = $"{titulo}: {data.Value:dd/MM HH:mm} *";
            lbl.Font = new Font(lbl.Font, FontStyle.Regular);

            lbl.ForeColor = classificacao switch
            {
                DateCentralService.ClassificacaoData.EmDia => Color.Green,
                DateCentralService.ClassificacaoData.DiaIdeal => Color.DarkGoldenrod,
                DateCentralService.ClassificacaoData.Atrasado => Color.Red,
                _ => Color.Gray
            };
        }

        // Lógica de Datas nos DatePickers
        private void AtualizarDatasAutomaticas()
        {
            if (_atualizandoDatasAutomaticamente) return;
            _atualizandoDatasAutomaticamente = true;
            try
            {
                if (!_dataD8DefinidaManualmente)
                    _dtpD8.Value = _dateCalc.CalcularD8(_dtpD0.Value);

                if (!_dataIATFDefinidaManualmente)
                    _dtpIATF.Value = _dateCalc.CalcularIATF(_dtpD8.Value);

                if (!_dataDGDefinidaManualmente)
                    _dtpDG.Value = _dateCalc.CalcularDG(_dtpIATF.Value);
            }
            finally { _atualizandoDatasAutomaticamente = false; }
        }


        private void ResetarCalculadoraDatas()
        {
            // Destrava todas as flags manuais
            _dataD8DefinidaManualmente = false;
            _dataIATFDefinidaManualmente = false;
            _dataDGDefinidaManualmente = false;

            // Reseta as datas baseadas no dia atual (ou na última data usada, se preferir)
            // Aqui forçamos um recálculo limpo a partir de agora
            _dtpD0.Value = DateTime.Now;

            // Opcional: Se quiser forçar o recálculo imediato após resetar
            AtualizarDatasAutomaticas();
        }

        // Eventos DatePicker
        private void _dtpD0_ValueChanged(object sender, EventArgs e)
        {
            if (_sistemaEstaOcupado) return;
            // D0 sempre comanda o recálculo geral
            AtualizarDatasAutomaticas();
        }

        private void _dtpD8_ValueChanged(object sender, EventArgs e)
        {
            if (_sistemaEstaOcupado) return;

            // Só marca como manual se NÃO for o robô calculando
            if (!_atualizandoDatasAutomaticamente)
            {
                _dataD8DefinidaManualmente = true;
                // Se mudou D8 na mão, recalcula IATF e DG a partir daqui
                AtualizarDatasAutomaticas();
            }
        }

        private void _dtpIATF_ValueChanged(object sender, EventArgs e)
        {
            if (_sistemaEstaOcupado) return;

            if (!_atualizandoDatasAutomaticamente)
            {
                _dataIATFDefinidaManualmente = true;
                // Se mudou IATF na mão, recalcula DG a partir daqui
                AtualizarDatasAutomaticas();
            }
        }

        private void _dtpDG_ValueChanged(object sender, EventArgs e)
        {
            if (_sistemaEstaOcupado) return;

            if (!_atualizandoDatasAutomaticamente) _dataDGDefinidaManualmente = true;
        }
        #endregion

        #region 6. Filtros e Busca
        // =================================================================================
        // FILTROS
        // =================================================================================

        private void ConfigurarEnterNosFiltros()
        {
            // Ordem dos controles de filtro por estágio
            // Será reconstruída dinamicamente conforme o estágio/modo
            var sequencia = new List<Control>();

            if (pnlFiltroD0.Visible)
            {
                sequencia.Add(_cmbFiltroRaca);
                sequencia.Add(_cmbFiltroCategoria);
                sequencia.Add(_cmbOperadorECC);
                sequencia.Add(_txtFiltroValorECC);
            }
            if (pnlFiltroD8.Visible)
            {
                sequencia.Add(_chkFiltroPerdeuImplante);
                sequencia.Add(_chkFiltroEcg);
            }
            if (pnlFiltroIATF.Visible)
            {
                sequencia.Add(_cmbFiltroInseminador);
                sequencia.Add(_cmbFiltroTouro);
                sequencia.Add(_cmbOperadorCio);
                sequencia.Add(_txtFiltroValorCio);
            }
            if (pnlFiltroDG.Visible)
            {
                sequencia.Add(_rbFiltroTodos);
                sequencia.Add(_rbFiltroPrenha);
                sequencia.Add(_rbFiltroVazia);
                sequencia.Add(_cmbFiltroDestino);
            }

            // Remove handlers anteriores e adiciona novos
            foreach (var ctrl in sequencia)
            {
                ctrl.KeyDown -= FiltroEnter_KeyDown;
                ctrl.KeyDown += FiltroEnter_KeyDown;
                ctrl.Tag = sequencia; // guarda a sequência no Tag
            }
        }

        private void FiltroEnter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            if (sender is Control ctrl && ctrl.Tag is List<Control> seq)
            {
                int idx = seq.IndexOf(ctrl);
                if (idx >= 0 && idx < seq.Count - 1)
                    seq[idx + 1].Focus();   // vai para o próximo
                else
                    btnFiltrar.PerformClick(); // último campo → filtra
            }
        }
        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            // 1. TRAVA O SISTEMA (Evita que as datas mudem sozinhas)
            _sistemaEstaOcupado = true;
            tabela.SuspendLayout();

            try
            {
                foreach (DataGridViewRow linha in tabela.Rows)
                {
                    if (linha.IsNewRow) continue;
                    bool deveAparecer = true;

                    // =================================================================
                    // 1. FILTROS DO D0 (Início)
                    // =================================================================

                    // Raça
                    if (_cmbFiltroRaca.SelectedIndex > 0 && _cmbFiltroRaca.Text != "(Todos)")
                        if (!(linha.Cells[NomesColunasGrid.Raca].Value?.ToString() ?? "").Equals(_cmbFiltroRaca.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                    // Categoria
                    if (deveAparecer && _cmbFiltroCategoria.SelectedIndex > 0 && _cmbFiltroCategoria.Text != "(Todos)")
                        if (!(linha.Cells[NomesColunasGrid.Categoria].Value?.ToString() ?? "").Equals(_cmbFiltroCategoria.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                    // ECC (Valor Numérico)
                    if (deveAparecer && !string.IsNullOrWhiteSpace(_txtFiltroValorECC.Text))
                    {
                        string eccTexto = linha.Cells[NomesColunasGrid.Ecc].Value?.ToString() ?? "0";
                        double.TryParse(eccTexto.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double eccValor);
                        double.TryParse(_txtFiltroValorECC.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double valorBusca);
                        string op = _cmbOperadorECC.Text;

                        if (op.Contains("<") && !(eccValor < valorBusca)) deveAparecer = false;
                        else if (op.Contains(">") && !(eccValor > valorBusca)) deveAparecer = false;
                        else if (op.Contains("=") && !(Math.Abs(eccValor - valorBusca) < 0.001)) deveAparecer = false;
                    }

                    // =================================================================
                    // 2. FILTROS DO D8 (Retirada)
                    // =================================================================
                    if (deveAparecer)
                    {
                        // Apenas quem perdeu implante
                        if (_chkFiltroPerdeuImplante.Checked && !Convert.ToBoolean(linha.Cells[NomesColunasGrid.PerdeuImplante].Value)) deveAparecer = false;

                        // Apenas quem usou eCG
                        if (_chkFiltroEcg.Checked && !Convert.ToBoolean(linha.Cells[NomesColunasGrid.Ecg].Value)) deveAparecer = false;
                    }

                    // =================================================================
                    // 3. FILTROS DA IATF (Inseminação)
                    // =================================================================
                    if (deveAparecer)
                    {
                        // Touro
                        if (_cmbFiltroTouro.SelectedIndex > 0 && _cmbFiltroTouro.Text != "(Todos)")
                            if (!(linha.Cells[NomesColunasGrid.Touro].Value?.ToString() ?? "").Equals(_cmbFiltroTouro.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                        // Inseminador
                        if (_cmbFiltroInseminador.SelectedIndex > 0 && _cmbFiltroInseminador.Text != "(Todos)")
                            if (!(linha.Cells[NomesColunasGrid.Inseminador].Value?.ToString() ?? "").Equals(_cmbFiltroInseminador.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                        // Escore de Cio (Valor Numérico)
                        if (!string.IsNullOrWhiteSpace(_txtFiltroValorCio.Text))
                        {
                            string cioTexto = linha.Cells[NomesColunasGrid.EscoreCio].Value?.ToString() ?? "0";
                            double.TryParse(cioTexto.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double cioValor);
                            double.TryParse(_txtFiltroValorCio.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double buscaCio);
                            string op = _cmbOperadorCio.Text;

                            if (op.Contains("<") && !(cioValor < buscaCio)) deveAparecer = false;
                            else if (op.Contains(">") && !(cioValor > buscaCio)) deveAparecer = false;
                            else if (op.Contains("=") && !(Math.Abs(cioValor - buscaCio) < 0.001)) deveAparecer = false;
                        }
                    }

                    // =================================================================
                    // 4. FILTROS DO DG (Diagnóstico)
                    // =================================================================
                    if (deveAparecer)
                    {
                        // Prenha vs Vazia
                        if (!_rbFiltroTodos.Checked)
                        {
                            string res = linha.Cells[NomesColunasGrid.ResultadoDG].Value?.ToString().ToUpper() ?? "";
                            bool ehPrenha = res.Contains("PRENHA") || res == "TRUE";

                            if (_rbFiltroPrenha.Checked && !ehPrenha) deveAparecer = false;
                            if (_rbFiltroVazia.Checked && ehPrenha) deveAparecer = false;
                        }

                        // Destino Final
                        if (_cmbFiltroDestino.SelectedIndex > 0 && _cmbFiltroDestino.Text != "(Todos)")
                            if (!(linha.Cells[NomesColunasGrid.Destino].Value?.ToString() ?? "").Equals(_cmbFiltroDestino.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;
                    }

                    // Aplica a visibilidade calculada
                    linha.Visible = deveAparecer;
                }

                // Recalcula totais com base nas linhas visíveis
                CalcularEstatisticas();
                CalcularTodasMedias();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("Falha ao filtrar dados", ex);
                MessageBox.Show(
                    "Não foi possível filtrar.\n\n" +
                    "Detalhes registrados no log.",
                    "Erro ao Filtrar a Tabela",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            finally
            {
                tabela.ResumeLayout();

                // 2. DESTRAVA O SISTEMA (Importante!)
                _sistemaEstaOcupado = false;
            }
        }

        private void btnLimparFiltros_Click(object sender, EventArgs e)
        {
            tabela.SuspendLayout();
            foreach (DataGridViewRow linha in tabela.Rows) if (!linha.IsNewRow) linha.Visible = true;

            // Resetar Campos Visuais
            _cmbFiltroRaca.SelectedIndex = -1; if (_cmbFiltroRaca.Items.Count > 0) _cmbFiltroRaca.SelectedIndex = 0;
            _cmbFiltroCategoria.SelectedIndex = -1; if (_cmbFiltroCategoria.Items.Count > 0) _cmbFiltroCategoria.SelectedIndex = 0;
            _txtFiltroValorECC.Clear();
            _chkFiltroPerdeuImplante.Checked = false; _chkFiltroEcg.Checked = false;
            _cmbFiltroTouro.SelectedIndex = -1; if (_cmbFiltroTouro.Items.Count > 0) _cmbFiltroTouro.SelectedIndex = 0;
            _cmbFiltroInseminador.SelectedIndex = -1; if (_cmbFiltroInseminador.Items.Count > 0) _cmbFiltroInseminador.SelectedIndex = 0;
            _txtFiltroValorCio.Clear();
            _rbFiltroTodos.Checked = true;
            _cmbFiltroDestino.SelectedIndex = -1; if (_cmbFiltroDestino.Items.Count > 0) _cmbFiltroDestino.SelectedIndex = 0;

            tabela.ResumeLayout();
        }

        private void AplicarFiltroECC(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtFiltroValorECC.Text)) { btnLimparFiltros_Click(null, null); return; }
            string op = _cmbOperadorECC.Text;
            string val = _txtFiltroValorECC.Text;
            if (op.Contains("<")) AplicarFiltro(NomesColunasGrid.Ecc, val, TipoFiltro.MenorQue);
            else if (op.Contains(">")) AplicarFiltro(NomesColunasGrid.Ecc, val, TipoFiltro.MaiorQue);
            else AplicarFiltro(NomesColunasGrid.Ecc, val, TipoFiltro.IgualNumerico);
        }

        private void AplicarFiltroCio(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtFiltroValorCio.Text)) { btnLimparFiltros_Click(null, null); return; }
            string op = _cmbOperadorCio.Text;
            string val = _txtFiltroValorCio.Text;
            if (op.Contains("<")) AplicarFiltro(NomesColunasGrid.EscoreCio, val, TipoFiltro.MenorQue);
            else if (op.Contains(">")) AplicarFiltro(NomesColunasGrid.EscoreCio, val, TipoFiltro.MaiorQue);
            else AplicarFiltro(NomesColunasGrid.EscoreCio, val, TipoFiltro.IgualNumerico);
        }

        private void AplicarFiltro(string nomeColuna, string valorBusca, TipoFiltro tipo)
        {
            if (!tabela.Columns.Contains(nomeColuna)) return;
            tabela.SuspendLayout();
            double.TryParse(valorBusca.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numeroBusca);

            try
            {
                foreach (DataGridViewRow linha in tabela.Rows)
                {
                    if (linha.IsNewRow) continue;
                    bool mostrar = false;
                    string valorCelulaTexto = linha.Cells[nomeColuna].Value?.ToString() ?? "";
                    double.TryParse(valorCelulaTexto.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numeroCelula);

                    switch (tipo)
                    {
                        case TipoFiltro.TextoExato: mostrar = valorCelulaTexto.Equals(valorBusca, StringComparison.OrdinalIgnoreCase); break;
                        case TipoFiltro.TextoContem: mostrar = valorCelulaTexto.ToUpper().Contains(valorBusca.ToUpper()); break;
                        case TipoFiltro.MaiorQue: mostrar = numeroCelula > numeroBusca; break;
                        case TipoFiltro.MenorQue: mostrar = numeroCelula < numeroBusca; break;
                        case TipoFiltro.IgualNumerico: mostrar = Math.Abs(numeroCelula - numeroBusca) < 0.001; break;
                    }
                    linha.Visible = mostrar;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Erro("Falha ao filtrar dados", ex);
                MessageBox.Show(
                    "Não foi possível filtrar.\n\n" +
                    "Detalhes registrados no log.",
                    "Erro ao Filtrar Tabela",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally { tabela.ResumeLayout(); }
        }

        private void AtualizarPainelDeFiltros()
        {
            // No modo Tabela Completa todos os filtros ficam sempre visíveis.
            // No modo Por Etapas, libera conforme o estágio alcançado.
            bool modoCompleto = _viewModel.ModoTabelaCompleta;
            int faseMaxima = (int)_viewModel.MaiorEstagioAlcancado;

            pnlFiltroD0.Visible = modoCompleto || faseMaxima >= (int)EstagioProtocolo.D0_Inicio;
            pnlFiltroD8.Visible = modoCompleto || faseMaxima >= (int)EstagioProtocolo.D8_Retirada;
            pnlFiltroIATF.Visible = modoCompleto || faseMaxima >= (int)EstagioProtocolo.D10_IATF;
            pnlFiltroDG.Visible = modoCompleto || faseMaxima >= (int)EstagioProtocolo.DG_Diagnostico;

            if (pnlFiltroD0.Visible)
            {
                if (_cmbOperadorECC.Items.Count == 0)
                {
                    _cmbOperadorECC.Items.AddRange(new[] { "< (Menor)", "> (Maior)", "= (Igual)" });
                    _cmbOperadorECC.SelectedIndex = 0;
                }
                PreencherComboFiltro(_cmbFiltroRaca, NomesColunasGrid.Raca);
                PreencherComboFiltro(_cmbFiltroCategoria, NomesColunasGrid.Categoria);
            }

            if (pnlFiltroIATF.Visible)
            {
                if (_cmbOperadorCio.Items.Count == 0)
                {
                    _cmbOperadorCio.Items.AddRange(new[] { "> (Maior)", "< (Menor)", "= (Igual)" });
                    _cmbOperadorCio.SelectedIndex = 0;
                }
                PreencherComboFiltro(_cmbFiltroInseminador, NomesColunasGrid.Inseminador);
                PreencherComboFiltro(_cmbFiltroTouro, NomesColunasGrid.Touro);
            }

            if (pnlFiltroDG.Visible)
                PreencherComboFiltro(_cmbFiltroDestino, NomesColunasGrid.Destino);
        }

        private void PreencherComboFiltro(ComboBox cmb, string nomeColuna)
        {
            if (!tabela.Columns.Contains(nomeColuna)) return;

            // 1. Guarda a seleção atual para não perder o filtro se a lista atualizar
            string selecaoAnterior = cmb.Text;
            if (string.IsNullOrEmpty(selecaoAnterior)) selecaoAnterior = "(Todos)";

            // 2. Limpa e reinicia
            cmb.Items.Clear();
            cmb.Items.Add("(Todos)");

            HashSet<string> itensUnicos = new HashSet<string>();

            // 3. Varre a tabela buscando valores únicos
            foreach (DataGridViewRow linha in tabela.Rows)
            {
                if (linha.IsNewRow) continue;

                // Pega o valor da célula
                string valor = linha.Cells[nomeColuna].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(valor))
                {
                    itensUnicos.Add(valor.ToUpper().Trim());
                }
            }

            // 4. Ordena e adiciona ao ComboBox
            List<string> listaOrdenada = itensUnicos.ToList();
            listaOrdenada.Sort(); // Deixa em ordem alfabética (opcional, mas recomendado)

            foreach (string item in listaOrdenada)
            {
                cmb.Items.Add(item);
            }

            // 5. Restaura a seleção anterior
            if (cmb.Items.Contains(selecaoAnterior))
            {
                cmb.Text = selecaoAnterior;
            }
            else
            {
                cmb.SelectedIndex = 0; // Volta para (Todos) se o item sumiu
            }
        }

        private void ConfigurarEventosDeFiltro()
        {
            // Remove eventos anteriores para não duplicar (segurança)
            _cmbFiltroRaca.DropDown -= AtualizarComboRaca;
            _cmbFiltroCategoria.DropDown -= AtualizarComboCategoria;
            _cmbFiltroTouro.DropDown -= AtualizarComboTouro;
            _cmbFiltroInseminador.DropDown -= AtualizarComboInseminador;
            _cmbFiltroDestino.DropDown -= AtualizarComboDestino;

            // Adiciona os eventos
            _cmbFiltroRaca.DropDown += AtualizarComboRaca;
            _cmbFiltroCategoria.DropDown += AtualizarComboCategoria;
            _cmbFiltroTouro.DropDown += AtualizarComboTouro;
            _cmbFiltroInseminador.DropDown += AtualizarComboInseminador;
            _cmbFiltroDestino.DropDown += AtualizarComboDestino;
            ConfigurarEnterNosFiltros();
        }

        // Métodos curtos que chamam o preenchimento na hora do clique
        private void AtualizarComboRaca(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroRaca, NomesColunasGrid.Raca);
        private void AtualizarComboCategoria(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroCategoria, NomesColunasGrid.Categoria);
        private void AtualizarComboTouro(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroTouro, NomesColunasGrid.Touro);
        private void AtualizarComboInseminador(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroInseminador, NomesColunasGrid.Inseminador);
        private void AtualizarComboDestino(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroDestino, NomesColunasGrid.Destino);
        #endregion

        #region 7. Arquivos (Importar, Salvar)
        // =================================================================================
        // MANIPULAÇÃO DE ARQUIVOS
        // =================================================================================

        // Botão Exportar
        private void btnExportar_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Arquivo Excel (*.xlsx)|*.xlsx|Arquivo CSV (*.csv)|*.csv";
            sfd.FileName = GerarNomeArquivo(".xlsx");
            sfd.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Dados");

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var lote = ExtrairLoteDataDaTabela();
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                IArquivoRepository repo = ext == ".xlsx" ? _excelRepository : _csvRepository;

                repo.Salvar(lote, sfd.FileName);

                AppLogger.Acao("Exportar", sfd.FileName);
                MessageBox.Show("Exportação concluída com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException ex)
            {
                AppLogger.Erro("Falha ao exportar arquivo", ex);
                MessageBox.Show(
                    "Não foi possível exportar. O arquivo pode estar aberto em outro programa.\n\n" +
                    "Detalhes registrados no log.",
                    "Erro ao Exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                SalvarTabelaNoArquivo();
                AppLogger.Acao("Salvar", _caminhoArquivoAberto);

                // Fecha o form com OK — FrmMenuIATFs detecta e recarrega a lista
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("btnSalvar_Click", ex);
                MessageBox.Show($"Erro ao salvar:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Adicione o parâmetro 'caminhoPasta'
        // Adicione o bool modoCompleto
        private void ExecutarNovoLote(string NomeFazendaInput, string caminhoPasta, bool modoCompleto)
        {
            if (tabela.Rows.Count > 1)
            {
                var resp = MessageBox.Show("Deseja limpar a tela atual?", "Novo", MessageBoxButtons.YesNo);
                if (resp == DialogResult.No) return;
            }

            tabela.Rows.Clear();
            btnLimparFiltros_Click(null, null);

            _viewModel.ConfigurarNovoLote(NomeFazendaInput, caminhoPasta, modoCompleto);

            _caminhoArquivoAberto = string.Empty;
            _diretorioDeTrabalhoAtual = caminhoPasta;
            _dataD8DefinidaManualmente = false;
            _dataIATFDefinidaManualmente = false;
            _dataDGDefinidaManualmente = false;

            this.Text = $"Pecuária Digital - {_viewModel.NomeFazenda} ({_viewModel.DataInicioInseminacao})";

            AtualizarInterfaceGeral();
            SalvarTabelaNoArquivo();

            MessageBox.Show($"Arquivo criado em:\n{caminhoPasta}\nModo: " +
                (_viewModel.ModoTabelaCompleta ? "Completo" : "Por Etapas"));
        }

        // Métodos de IO Internos
        // COLE ISTO NO LUGAR:
        private string ObterCaminhoCompleto()
        {
            if (string.IsNullOrEmpty(_diretorioDeTrabalhoAtual))
            {
                _diretorioDeTrabalhoAtual = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "PecuariaDigital_Dados");
            }

            if (!Directory.Exists(_diretorioDeTrabalhoAtual))
                Directory.CreateDirectory(_diretorioDeTrabalhoAtual);

            string nomeFazenda = _viewModel.NomeFazenda.Replace(" ", "_");
            string data = _viewModel.DataInicioInseminacao.Replace("/", "-");

            // ← Adiciona timestamp para garantir nome único por sessão
            string timestamp = DateTime.Now.ToString("HHmmss");
            string nomeArquivo = $"IATF_{nomeFazenda}_{data}_{timestamp}.csv";

            return Path.Combine(_diretorioDeTrabalhoAtual, nomeArquivo);
        }

        private void SalvarTabelaNoArquivo()
        {
            // Define o caminho (mesma lógica de antes)
            if (string.IsNullOrEmpty(_caminhoArquivoAberto))
                _caminhoArquivoAberto = ObterCaminhoCompleto();

            try
            {
                var lote = ExtrairLoteDataDaTabela();

                // Escolhe o repositório pela extensão do arquivo
                string ext = Path.GetExtension(_caminhoArquivoAberto).ToLower();
                IArquivoRepository repo = ext == ".xlsx" ? _excelRepository : _csvRepository;

                repo.Salvar(lote, _caminhoArquivoAberto);
            }
            catch (Exception ex)
            {
                AppLogger.Erro("Falha ao salvar arquivo", ex);
                MessageBox.Show(
                    "Não foi possível salvar.\n\nDetalhes registrados no log.",
                    "Erro ao Salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CarregarTabelaDoArquivo()
        {
            string caminho = _viewModel.CaminhoArquivoAberto;

            // Só carrega se o arquivo realmente existir no disco
            // Se for tabela nova, CaminhoArquivoAberto está vazio — não carrega nada
            if (!string.IsNullOrWhiteSpace(caminho) && File.Exists(caminho))
                CarregarArquivoEspecifico(caminho);
        }

        private void CarregarArquivoEspecifico(string caminho)
        {
            if (!File.Exists(caminho)) return;

            _caminhoArquivoAberto = caminho;
            _diretorioDeTrabalhoAtual = Path.GetDirectoryName(caminho);

            try
            {
                btnLimparFiltros_Click(null, null);
                _dataD8DefinidaManualmente = false;
                _dataIATFDefinidaManualmente = false;
                _dataDGDefinidaManualmente = false;

                string ext = Path.GetExtension(caminho).ToLower();
                IArquivoRepository repo = ext == ".xlsx" ? _excelRepository : _csvRepository;

                LoteData lote = repo.Carregar(caminho);

                // Restaura o estado do protocolo via ViewModel
                if (Enum.TryParse(lote.MaiorEstagioAlcancado, out EstagioProtocolo estagioRecuperado))
                    _viewModel.RestaurarEstagio(estagioRecuperado);

                // Recupera nome da fazenda — LoteData ou extrai do nome do arquivo
                string nomeFazenda;
                if (!string.IsNullOrWhiteSpace(lote.NomeFazenda))
                {
                    nomeFazenda = lote.NomeFazenda;
                }
                else
                {
                    string[] partes = Path.GetFileNameWithoutExtension(caminho).Split('_');
                    nomeFazenda = partes.Length >= 2 ? $"{partes[0]}_{partes[1]}" : partes[0];
                }

                // Recupera data — LoteData ou extrai do nome do arquivo
                string dataInicio;
                if (!string.IsNullOrWhiteSpace(lote.DataInicioInseminacao))
                {
                    dataInicio = lote.DataInicioInseminacao;
                }
                else
                {
                    string[] partes = Path.GetFileNameWithoutExtension(caminho).Split('_');
                    dataInicio = partes.Length >= 3 ? partes[2] : DateTime.Now.ToString("dd-MM-yyyy");
                }

                // Atualiza o ViewModel com os dados recuperados
                _viewModel.ConfigurarNovoLote(_viewModel.NomeFazenda, _diretorioDeTrabalhoAtual, lote.ModoTabelaCompleta, dataInicio); // ← passa a data do arquivo

                // Atualiza o título da janela
                this.Text = $"Pecuária Digital - {_viewModel.NomeFazenda} ({_viewModel.DataInicioInseminacao})";

                PopularTabelaDoLoteData(lote);
                AtualizarMemoriaNumeros();
                CalcularEstatisticas();
                CalcularTodasMedias();
                AtualizarInterfaceGeral();

                // Regrava o arquivo para salvar os metadados atualizados
                SalvarTabelaNoArquivo();

                AppLogger.Acao("Arquivo carregado", caminho);
            }
            catch (Exception ex)
            {
                AppLogger.Erro("Falha ao carregar arquivo", ex);
                MessageBox.Show(
                    "Não foi possível carregar o arquivo.\n\nDetalhes registrados no log.",
                    "Erro ao Carregar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Exportação / Importação Específica
        private string GerarNomeArquivo(string extensao)
        {
            // Formato: "Fazenda_Data_ESTAGIO.extensao"
            // Exemplo: "Fazenda_Rio_Verde_20-01-2026_D8_Retirada.csv"

            // Proteção: Se a extensão não vier com ponto, adiciona
            if (!extensao.StartsWith(".")) extensao = "." + extensao;

            string nomeLimpoFazenda = _viewModel.NomeFazenda.Replace(" ", "_");
            return $"{nomeLimpoFazenda}_{_viewModel.DataInicioInseminacao}_{_viewModel.MaiorEstagioAlcancado}{extensao}";
        }

        // Conversão DataGridView ↔ LoteData
        /// <summary>
        /// Lê todas as linhas da tabela visual e monta um LoteData para salvar.
        /// </summary>
        private LoteData ExtrairLoteDataDaTabela()
        {
            var lote = new LoteData
            {
                NomeFazenda = _viewModel.NomeFazenda,
                DataInicioInseminacao = _viewModel.DataInicioInseminacao,
                MaiorEstagioAlcancado = _viewModel.MaiorEstagioAlcancado.ToString(),
                ModoTabelaCompleta = _viewModel.ModoTabelaCompleta,
            };

            foreach (DataGridViewRow linha in tabela.Rows)
            {
                if (linha.IsNewRow) continue;

                string G(string col) => linha.Cells[col].Value?.ToString() ?? string.Empty;
                bool B(string col) => Convert.ToBoolean(linha.Cells[col].Value);

                var animal = new Animal
                {
                    Id = G(NomesColunasGrid.Id),
                    Categoria = G(NomesColunasGrid.Categoria),
                    Raca = G(NomesColunasGrid.Raca),
                    Ecc = G(NomesColunasGrid.Ecc),
                    Lote = G(NomesColunasGrid.Lote),
                    Observacoes = G(NomesColunasGrid.Obs),
                    PerdeuImplante = B(NomesColunasGrid.PerdeuImplante),
                    UsouEcg = B(NomesColunasGrid.Ecg),
                    EscoreCio = G(NomesColunasGrid.EscoreCio),
                    UsouGnrh = B(NomesColunasGrid.Gnrh),
                    Touro = G(NomesColunasGrid.Touro),
                    Inseminador = G(NomesColunasGrid.Inseminador),
                    ResultadoDG = G(NomesColunasGrid.ResultadoDG),
                    Ovario = G(NomesColunasGrid.Ovario),
                    Destino = G(NomesColunasGrid.Destino),
                };

                // Datas com TryParse seguro
                if (DateTime.TryParse(G(NomesColunasGrid.DataD0), out var d0)) animal.DataD0 = d0;
                if (DateTime.TryParse(G(NomesColunasGrid.DataD8), out var d8)) animal.DataD8 = d8;
                if (DateTime.TryParse(G(NomesColunasGrid.DataIATF), out var iatf)) animal.DataIATF = iatf;
                if (DateTime.TryParse(G(NomesColunasGrid.DataDG), out var dg)) animal.DataDG = dg;

                if (!string.IsNullOrWhiteSpace(animal.Id))
                    lote.Animais.Add(animal);
            }

            return lote;
        }

        /// <summary>
        /// Recebe um LoteData carregado e popula a tabela visual.
        /// </summary>
        private void PopularTabelaDoLoteData(LoteData lote)
        {
            // ← Ativa a trava antes de popular — impede CellValueChanged durante carga
            _sistemaEstaOcupado = true;
            tabela.SuspendLayout();

            try
            {
                tabela.Rows.Clear();

                foreach (var animal in lote.Animais)
                {
                    int index = tabela.Rows.Add();
                    var linha = tabela.Rows[index];

                    void S(string col, object val)
                    { if (val != null) linha.Cells[col].Value = val; }

                    S(NomesColunasGrid.Id, animal.Id);
                    S(NomesColunasGrid.Categoria, animal.Categoria);
                    S(NomesColunasGrid.Raca, animal.Raca);
                    S(NomesColunasGrid.Ecc, animal.Ecc);
                    S(NomesColunasGrid.Lote, animal.Lote);
                    S(NomesColunasGrid.Obs, animal.Observacoes);
                    S(NomesColunasGrid.PerdeuImplante, animal.PerdeuImplante);
                    S(NomesColunasGrid.Ecg, animal.UsouEcg);
                    S(NomesColunasGrid.EscoreCio, animal.EscoreCio);
                    S(NomesColunasGrid.Gnrh, animal.UsouGnrh);
                    S(NomesColunasGrid.Touro, animal.Touro);
                    S(NomesColunasGrid.Inseminador, animal.Inseminador);
                    S(NomesColunasGrid.ResultadoDG, animal.ResultadoDG);
                    S(NomesColunasGrid.Ovario, animal.Ovario);
                    S(NomesColunasGrid.Destino, animal.Destino);

                    if (animal.DataD0.HasValue)
                        S(NomesColunasGrid.DataD0,
                            animal.DataD0.Value.ToString(ProtocoloConstants.FormatoDataHora));
                    if (animal.DataD8.HasValue)
                        S(NomesColunasGrid.DataD8,
                            animal.DataD8.Value.ToString(ProtocoloConstants.FormatoDataHora));
                    if (animal.DataIATF.HasValue)
                        S(NomesColunasGrid.DataIATF,
                            animal.DataIATF.Value.ToString(ProtocoloConstants.FormatoDataHora));
                    if (animal.DataDG.HasValue)
                        S(NomesColunasGrid.DataDG,
                            animal.DataDG.Value.ToString(ProtocoloConstants.FormatoDataSimples));

                    AprenderNovaOpcao(NomesColunasGrid.Raca, animal.Raca, _raca);
                    AprenderNovaOpcao(NomesColunasGrid.Categoria, animal.Categoria, _categoria);
                    AprenderNovaOpcao(NomesColunasGrid.Touro, animal.Touro, _touro);
                    AprenderNovaOpcao(NomesColunasGrid.Inseminador, animal.Inseminador, _inseminador);
                }
            }
            finally
            {
                _sistemaEstaOcupado = false;  // ← Destrava após popular
                tabela.ResumeLayout();
            }
        }
        #endregion

        #region 8. Eventos de Interface (Teclado, Mouse, Ajuda)
        // =================================================================================
        // EVENTOS DE INTERAÇÃO DO USUÁRIO
        // =================================================================================

        // Teclado (Atalhos, Enter)
        private void _numero_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendKeys.Send("{TAB}"); } }
        private void _prenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendKeys.Send("{TAB}"); }
            if (e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.S) { e.SuppressKeyPress = true; if (_cbResultadoDG.SelectedIndex == -1) _cbResultadoDG.SelectedIndex = 0; else if (_cbResultadoDG.SelectedIndex < _cbResultadoDG.Items.Count - 1) _cbResultadoDG.SelectedIndex++; }
            else if (e.KeyCode == Keys.NumPad8 || e.KeyCode == Keys.W) { e.SuppressKeyPress = true; if (_cbResultadoDG.SelectedIndex == -1) _cbResultadoDG.SelectedIndex = 0; else if (_cbResultadoDG.SelectedIndex > 0) _cbResultadoDG.SelectedIndex--; }
        }
        private void _observacoes_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; inserir.PerformClick(); } }
        private void _textBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendKeys.Send("{TAB}"); } }

        private void _comboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
            {
                ComboBox caixa = (ComboBox)sender;
                if (caixa.Items.Contains(caixa.Text))
                {
                    if (MessageBox.Show("Deseja esquecer a opção '" + caixa.Text + "' para sempre?", "Limpar Memória", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        caixa.Items.Remove(caixa.Text);
                        caixa.Text = "";
                        MessageBox.Show("Item removido!");
                    }
                }
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendKeys.Send("{TAB}"); }
        }

        private void _checkBox_KeyDown(object sender, KeyEventArgs e)
        {
            CheckBox caixa = (CheckBox)sender;
            if (e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1) { e.SuppressKeyPress = true; caixa.Checked = true; SendKeys.Send("{TAB}"); }
            else if (e.KeyCode == Keys.NumPad0 || e.KeyCode == Keys.D0) { e.SuppressKeyPress = true; caixa.Checked = false; SendKeys.Send("{TAB}"); }
            else if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendKeys.Send("{TAB}"); }
        }

        private void label26_Click(object sender, EventArgs e) { } // Placeholder

        // Sistema de Ajuda
        private void ConfigurarSistemaDeAjuda()
        {
            mapaAjuda = new Dictionary<Control, (string, string)>();
            AdicionarAjuda(_numero, "Número do Brinco", "Digite a identificação única do animal (ID).\nUse ENTER para pular para o próximo campo.");
            AdicionarAjuda(_observacoes, "Observações Gerais", "Anote detalhes de saúde, manejo ou alertas importantes sobre este animal.");
            AdicionarAjuda(_raca, "Raça Predominante", "Informe a raça base do animal (Ex: NELORE, ANGUS).\nO sistema aprende novas raças automaticamente.");
            AdicionarAjuda(_categoria, "Categoria Animal", "Classificação reprodutiva (Ex: NULÍPARA, MULTÍPARA).\nIsso define o protocolo hormonal.");
            AdicionarAjuda(_ecc, "Escore de Condição Corporal (ECC)", "Avaliação visual de gordura (Escala 1 a 5).\nUse ponto ou vírgula (ex: 3.5).");
            AdicionarAjuda(_lote, "Lote de Tratamento", "Grupo de manejo onde o animal se encontra.");
            AdicionarAjuda(_chkPerdeuImplante, "Queda de Implante", "Marque APENAS se o animal perdeu o dispositivo intravaginal durante o protocolo.");
            AdicionarAjuda(_chkEcg, "Aplicação de eCG", "Marque se foi administrado eCG (Gonadotrofina Cor iônica Equina) neste animal.");
            AdicionarAjuda(_txtEscoreCio, "Escore de Cio", "Intensidade da tinta reveladora (ex: 1 = Sem tinta, 3 = Muita tinta).\nAceita valores numéricos ou texto.");
            AdicionarAjuda(_touro, "Touro / Sêmen", "Nome ou código do touro utilizado na inseminação.");
            AdicionarAjuda(_chkGnrh, "Indutor de Ovulação (GnRH)", "Marque esta opção se foi aplicado o hormônio GnRH no momento da inseminação para garantir a ovulação.");
            AdicionarAjuda(_inseminador, "Inseminador", "Nome do profissional que realizou o procedimento.");
            AdicionarAjuda(_cbResultadoDG, "Diagnóstico de Gestação", "Resultado do ultrassom. Selecione PRENHA ou VAZIA.");
            AdicionarAjuda(_txtOvario, "Estrutura Ovariana", "Situação do ovário (Ex: CL, FOLICULO, CISTO).\nImportante para animais vazios.");
            AdicionarAjuda(_txtDestino, "Destino Final", "Para onde o animal vai após o diagnóstico (Ex: DESCARTE, VENDA, RESINCRONIZAÇÃO).");

            foreach (var controle in mapaAjuda.Keys)
            {
                controle.Enter -= MostrarAjuda_Enter;
                controle.Enter += MostrarAjuda_Enter;
            }
        }

        private void AdicionarAjuda(Control ctrl, string titulo, string desc) { if (ctrl != null && !mapaAjuda.ContainsKey(ctrl)) mapaAjuda.Add(ctrl, (titulo, desc)); }
        private void MostrarAjuda_Enter(object sender, EventArgs e) { Control c = sender as Control; if (c != null && mapaAjuda.ContainsKey(c)) { var info = mapaAjuda[c]; lblAjudaTitulo.Text = info.Titulo; lblAjudaDescricao.Text = info.Descricao; } }
        private void LimparAjuda(object sender, EventArgs e) { lblAjudaTitulo.Text = "Aguardando seleção..."; lblAjudaDescricao.Text = "Clique em um campo para ver detalhes."; }

        private void MostrarAjudaEstatistica(object sender, EventArgs e)
        {
            Control controle = sender as Control; if (controle == null) return;
            string titulo = "Estatística", explicacao = "Passe o mouse sobre os indicadores para ver detalhes.";

            if (controle.Name == "lblStatConcepcao") { titulo = "Taxa de Concepção (TC)"; explicacao = "Mede a competência do Inseminador e a fertilidade do Sêmen.\nFórmula: (Vacas Prenhas ÷ Vacas Inseminadas)."; }
            else if (controle.Name == "lblStatServico") { titulo = "Taxa de Serviço (TS)"; explicacao = "Mede o aproveitamento do lote.\nFórmula: (Vacas Inseminadas ÷ Total de Vacas Aptas)."; }
            else if (controle.Name == "lblStatPrenhez") { titulo = "Taxa de Prenhez (TP)"; explicacao = "O indicador financeiro principal.\nFórmula: (Vacas Prenhas ÷ Total do Lote)."; }
            else if (controle.Name == "lblStatDoses") { titulo = "Índice de Inseminação (II)"; explicacao = "Custo-Benefício: Quantas doses gastas para fazer 1 bezerro.\nMeta Ideal: Menor que 1.5."; }
            else if (controle.Name == "lblStatTotal") { titulo = "Total de Animais"; explicacao = "Quantidade de animais considerados nos cálculos atuais."; }

            lblAjudaTitulo.Text = titulo; lblAjudaDescricao.Text = explicacao;
        }
        private void LimparAjudaEstatistica(object sender, EventArgs e) { lblAjudaTitulo.Text = "Ajuda Contextual"; lblAjudaDescricao.Text = "Passe o mouse sobre um campo ou estatística para ver dicas."; }

        // Formatação de Cores na Tabela
        private void tabela_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            string nomeColuna = tabela.Columns[e.ColumnIndex].Name;

            bool deveAnalisar = false;
            bool dadosPreenchidos = false;
            DataGridViewRow linha = tabela.Rows[e.RowIndex];

            // Lógica para identificar se a etapa tem dados
            if (nomeColuna == NomesColunasGrid.DataD0)
            {
                e.CellStyle.ForeColor = Color.Black;
                e.CellStyle.SelectionForeColor = Color.Black;
                e.CellStyle.Font = new Font(tabela.Font, FontStyle.Regular);
                return; // sai do evento sem aplicar semáforo
            }
            else if (nomeColuna == NomesColunasGrid.DataD8 && _viewModel.EstagioAtual <= EstagioProtocolo.D8_Retirada)
            {
                deveAnalisar = true;
                // Se marcou 'Perdeu Implante' OU 'eCG', considera D8 feito
                bool perdeu = Convert.ToBoolean(linha.Cells[NomesColunasGrid.PerdeuImplante].Value);
                bool ecg = Convert.ToBoolean(linha.Cells[NomesColunasGrid.Ecg].Value);
                // OU se já avançou para IATF (tem dados de cio)
                bool temIATF = !string.IsNullOrWhiteSpace(linha.Cells[NomesColunasGrid.EscoreCio].Value?.ToString());

                if (perdeu || ecg || temIATF) dadosPreenchidos = true;
            }
            else if (nomeColuna == NomesColunasGrid.DataIATF && _viewModel.EstagioAtual <= EstagioProtocolo.D10_IATF)
            {
                deveAnalisar = true;
                // Se tem Touro, Inseminador ou Escore de Cio
                string touro = linha.Cells[NomesColunasGrid.Touro].Value?.ToString();
                string cio = linha.Cells[NomesColunasGrid.EscoreCio].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(touro) || !string.IsNullOrWhiteSpace(cio)) dadosPreenchidos = true;
            }
            else if (nomeColuna == NomesColunasGrid.DataDG && _viewModel.EstagioAtual <= EstagioProtocolo.DG_Diagnostico)
            {
                deveAnalisar = true;
                // Se tem Resultado DG
                if (!string.IsNullOrWhiteSpace(linha.Cells[NomesColunasGrid.ResultadoDG].Value?.ToString())) dadosPreenchidos = true;
            }

            // Aplica as cores
            if (deveAnalisar)
            {
                if (dadosPreenchidos)
                {
                    // PRETO: Etapa já tem dados preenchidos
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionForeColor = Color.Black;
                    e.CellStyle.Font = new Font(tabela.Font, FontStyle.Regular);
                }
                else if (DateTime.TryParse(e.Value.ToString(), out DateTime dataAgendada))
                {
                    // SEMÁFORO: Pendente
                    DateTime hoje = DateTime.Now.Date;
                    if (dataAgendada.Date == hoje)
                    {
                        e.CellStyle.ForeColor = Color.OrangeRed; // Hoje!
                        e.CellStyle.SelectionForeColor = Color.OrangeRed;
                        e.CellStyle.Font = new Font(tabela.Font, FontStyle.Bold);
                    }
                    else if (dataAgendada.Date < hoje)
                    {
                        e.CellStyle.ForeColor = Color.Red; // Atrasado
                        e.CellStyle.SelectionForeColor = Color.Red;
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Green; // Futuro
                        e.CellStyle.SelectionForeColor = Color.LightGreen;
                    }
                }
            }
            else if (nomeColuna.Contains("Data"))
            {
                // Etapas passadas ficam pretas por padrão
                e.CellStyle.ForeColor = Color.Black;
            }
        }

        // Ordenação de NomesColunasGrid
        private void tabela_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn colunaClicada = tabela.Columns[e.ColumnIndex];
            SortOrder direcaoDestino = SortOrder.Ascending;
            if (colunaClicada.HeaderCell.SortGlyphDirection == SortOrder.Ascending) direcaoDestino = SortOrder.Descending;

            foreach (DataGridViewColumn col in tabela.Columns) col.HeaderCell.SortGlyphDirection = SortOrder.None;
            tabela.Sort(new ComparadorPersonalizado(e.ColumnIndex, colunaClicada.Name, direcaoDestino));
            colunaClicada.HeaderCell.SortGlyphDirection = direcaoDestino;
        }

        // Erro para corrigir o erro de tipo de dados
        private void tabela_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Este evento captura o erro "Valor inválido" do ComboBox e o ignora silenciosamente.
            // Isso evita que a janela de erro apareça para o usuário.
            e.ThrowException = false;
            e.Cancel = false;
        }

        // Edição de tabela
        private void tabela_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var linha = tabela.Rows[e.RowIndex];
            string col = tabela.Columns[e.ColumnIndex].Name;
            string agora = DateTime.Now.ToString(ProtocoloConstants.FormatoDataHora);

            // Se editou campo do D8 → preenche DataD8 com hoje (se ainda vazia)
            bool ehCampoD8 = col == NomesColunasGrid.PerdeuImplante
                          || col == NomesColunasGrid.Ecg;

            // Se editou campo do IATF → preenche DataIATF
            bool ehCampoIATF = col == NomesColunasGrid.EscoreCio
                            || col == NomesColunasGrid.Gnrh
                            || col == NomesColunasGrid.Touro
                            || col == NomesColunasGrid.Inseminador;

            // Se editou campo do DG → preenche DataDG
            bool ehCampoDG = col == NomesColunasGrid.ResultadoDG
                          || col == NomesColunasGrid.Ovario
                          || col == NomesColunasGrid.Destino;

            if (ehCampoD8)
            {
                if (string.IsNullOrWhiteSpace(linha.Cells[NomesColunasGrid.DataD8].Value?.ToString()))
                    linha.Cells[NomesColunasGrid.DataD8].Value = agora;
            }
            else if (ehCampoIATF)
            {
                if (string.IsNullOrWhiteSpace(linha.Cells[NomesColunasGrid.DataIATF].Value?.ToString()))
                    linha.Cells[NomesColunasGrid.DataIATF].Value = agora;
            }
            else if (ehCampoDG)
            {
                if (string.IsNullOrWhiteSpace(linha.Cells[NomesColunasGrid.DataDG].Value?.ToString()))
                    linha.Cells[NomesColunasGrid.DataDG].Value =
                        DateTime.Now.ToString(ProtocoloConstants.FormatoDataSimples);
            }

            SalvarTabelaNoArquivo();
        }

        private bool _processandoCellValueChanged = false;

        private void tabela_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_processandoCellValueChanged) return;
            if (_sistemaEstaOcupado) return;          
            if (e.RowIndex < 0) return;

            _processandoCellValueChanged = true;
            try
            {
                var linha = tabela.Rows[e.RowIndex];
                string col = tabela.Columns[e.ColumnIndex].Name;
                string agora = DateTime.Now.ToString(ProtocoloConstants.FormatoDataHora);
                string agoraSimples = DateTime.Now.ToString(ProtocoloConstants.FormatoDataSimples);

                bool ehCampoD8 = col == NomesColunasGrid.PerdeuImplante
                                || col == NomesColunasGrid.Ecg;
                bool ehCampoIATF = col == NomesColunasGrid.EscoreCio
                                || col == NomesColunasGrid.Gnrh
                                || col == NomesColunasGrid.Touro
                                || col == NomesColunasGrid.Inseminador;
                bool ehCampoDG = col == NomesColunasGrid.ResultadoDG
                                || col == NomesColunasGrid.Ovario
                                || col == NomesColunasGrid.Destino;

                if (ehCampoD8)
                {
                    if (string.IsNullOrWhiteSpace(
                            linha.Cells[NomesColunasGrid.DataD8].Value?.ToString()))
                        linha.Cells[NomesColunasGrid.DataD8].Value = agora;
                }
                else if (ehCampoIATF)
                {
                    if (string.IsNullOrWhiteSpace(
                            linha.Cells[NomesColunasGrid.DataIATF].Value?.ToString()))
                        linha.Cells[NomesColunasGrid.DataIATF].Value = agora;
                }
                else if (ehCampoDG)
                {
                    if (string.IsNullOrWhiteSpace(
                            linha.Cells[NomesColunasGrid.DataDG].Value?.ToString()))
                        linha.Cells[NomesColunasGrid.DataDG].Value = agoraSimples;
                }
                else return;

                SalvarTabelaNoArquivo();
            }
            finally
            {
                _processandoCellValueChanged = false;
            }
        }
    }
        #endregion





    // =================================================================================
    // CLASSES EXTERNAS
    // =================================================================================
    public class ComparadorPersonalizado : IComparer
    {
        private int _colunaIndex;
        private SortOrder _direcao;
        private string _nomeColuna;

        public ComparadorPersonalizado(int colunaIndex, string nomeColuna, SortOrder direcao)
        {
            _colunaIndex = colunaIndex; _direcao = direcao; _nomeColuna = nomeColuna;
        }

        public int Compare(object x, object y)
        {
            DataGridViewRow linha1 = (DataGridViewRow)x; DataGridViewRow linha2 = (DataGridViewRow)y;
            string valor1 = linha1.Cells[_colunaIndex].Value?.ToString() ?? "";
            string valor2 = linha2.Cells[_colunaIndex].Value?.ToString() ?? "";
            int resultado = 0;

            if (_nomeColuna.Contains("Data"))
            {
                DateTime.TryParse(valor1, out DateTime d1); DateTime.TryParse(valor2, out DateTime d2);
                resultado = DateTime.Compare(d1, d2);
            }
            else if (_nomeColuna == NomesColunasGrid.Id)
            {
                int.TryParse(valor1, out int n1); int.TryParse(valor2, out int n2);
                resultado = n1.CompareTo(n2);
            }
            else if (_nomeColuna == NomesColunasGrid.Ecc || _nomeColuna == NomesColunasGrid.EscoreCio)
            {
                double.TryParse(valor1.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num1);
                double.TryParse(valor2.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num2);
                resultado = num1.CompareTo(num2);
            }
            else resultado = string.Compare(valor1, valor2, true);

            if (_direcao == SortOrder.Descending) resultado *= -1;
            return resultado;
        }
    }
}