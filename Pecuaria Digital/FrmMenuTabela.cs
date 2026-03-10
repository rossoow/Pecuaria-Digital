using ClosedXML.Excel;
using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using Pecuaria_Digital.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing; // Necessário para cores (Color.Red, etc)
using System.Globalization;
using System.IO;
using System.Linq; // Necessário para .First(), .Skip()
using System.Windows.Forms;

namespace Pecuaria_Digital
{
    public partial class FrmMenuTabela : Form
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

        // --- Enumeração dos Estágios ---
        public enum EstagioProtocolo
        {
            D0_Inicio,
            D8_Retirada,
            D10_IATF,
            DG_Diagnostico,
            Finalizado
        }

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
        public FrmMenuTabela(ProtocoloViewModel viewModel)
        {
            _viewModel = viewModel;
            
            InitializeComponent();
            _cbResultadoDG.SelectedIndex = 0;
        }

        private void FrmMenuTabela_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Configura as colunas da tabela (Grid)
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
            var (valido, mensagem) = ValidacaoService.ValidarInsercaoAnimal(_numero.Text, _categoria.Text, _raca.Text);
            if (!valido)
            {
                MessageBox.Show(mensagem, "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Valida ECC se preenchido
            var (eccValido, eccMensagem) = ValidacaoService.ValidarEcc(_ecc.Text);
            if (!eccValido)
            {
                MessageBox.Show(eccMensagem, "ECC Inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ecc.Focus();
                return;
            }

            string idDigitado = _numero.Text.Trim().ToUpper();
            DataGridViewRow linhaAlvo = null;
            bool ehNovaLinha = false;

            // 1. Busca TODAS as linhas que têm esse mesmo brinco
            List<DataGridViewRow> linhasEncontradas = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in tabela.Rows)
            {
                if (row.IsNewRow) continue;
                string idTabela = row.Cells[Colunas.Id].Value?.ToString().Trim().ToUpper() ?? "";
                if (idTabela == idDigitado)
                {
                    linhasEncontradas.Add(row);
                }
            }

            // 2. Lógica de Decisão (D0 vs D8 em diante)
            if (linhasEncontradas.Count == 0)
            {
                // O brinco não existe na tabela, sempre cria um novo
                ehNovaLinha = true;
            }
            else
            {
                // O brinco já existe! Vamos ver em qual etapa estamos:
                if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio)
                {
                    // No D0: Pergunta se quer duplicar ou atualizar
                    var resp = MessageBox.Show($"O brinco '{idDigitado}' já existe na tabela.\n\nDeseja adicionar um NOVO animal com este mesmo brinco?\n\n[SIM] = Cria um brinco repetido\n[NÃO] = Atualiza os dados do existente", "Brinco Repetido", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (resp == DialogResult.Cancel) return; // Cancela a inserção

                    if (resp == DialogResult.Yes)
                    {
                        ehNovaLinha = true; // Permite duplicar
                    }
                    else
                    {
                        // Quer atualizar. Se tiver mais de um, abre a janela de escolha
                        if (linhasEncontradas.Count > 1)
                        {
                            linhaAlvo = EscolherLinhaDuplicada(linhasEncontradas);
                            if (linhaAlvo == null) return; // Fechou a janela sem escolher
                        }
                        else
                        {
                            linhaAlvo = linhasEncontradas[0];
                        }
                    }
                }
                else
                {
                    // Do D8 em diante: Nunca duplica. Apenas atualiza.
                    if (linhasEncontradas.Count > 1)
                    {
                        // Abre a janela para ele escolher QUAL das vacas repetidas ele está manejando agora
                        linhaAlvo = EscolherLinhaDuplicada(linhasEncontradas);
                        if (linhaAlvo == null) return; // Fechou a janela sem escolher
                    }
                    else
                    {
                        linhaAlvo = linhasEncontradas[0];
                    }
                }
            }

            // 3. Se for nova linha, cria na tabela
            if (ehNovaLinha)
            {
                int index = tabela.Rows.Add();
                linhaAlvo = tabela.Rows[index];
                linhaAlvo.Cells[Colunas.Id].Value = idDigitado;
            }

            // =========================================================================
            // 3. SALVA OS DADOS COM PROTEÇÃO DE ETAPA
            // (Só salva a etapa se estiver nela OU no modo completo, para não apagar dados escondidos)
            // =========================================================================

            // --- BLOCO D0 ---
            if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio || _viewModel.ModoTabelaCompleta || ehNovaLinha)
            {
                AprenderNovaOpcao(Colunas.Categoria, _categoria.Text, _categoria);
                AprenderNovaOpcao(Colunas.Raca, _raca.Text, _raca);
                AprenderNovaOpcao(Colunas.Lote, _lote.Text, _lote);

                // Só atualiza se o campo não estiver vazio (Segurança extra)
                if (!string.IsNullOrWhiteSpace(_categoria.Text)) linhaAlvo.Cells[Colunas.Categoria].Value = _categoria.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_raca.Text)) linhaAlvo.Cells[Colunas.Raca].Value = _raca.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_ecc.Text)) linhaAlvo.Cells[Colunas.Ecc].Value = _ecc.Text;
                if (!string.IsNullOrWhiteSpace(_lote.Text)) linhaAlvo.Cells[Colunas.Lote].Value = _lote.Text;

                // Data e Obs salvamos sempre que estivermos nesta etapa
                linhaAlvo.Cells[Colunas.DataD0].Value = _dtpD0.Value.ToString("dd/MM/yyyy HH:mm");
            }

            // Observação é global, mas para não apagar sem querer, só salvamos se tiver texto escrito
            if (!string.IsNullOrWhiteSpace(_observacoes.Text))
            {
                linhaAlvo.Cells[Colunas.Obs].Value = _observacoes.Text;
            }

            // --- BLOCO D8 ---
            if (_viewModel.EstagioAtual == EstagioProtocolo.D8_Retirada || _viewModel.ModoTabelaCompleta)
            {
                linhaAlvo.Cells[Colunas.PerdeuImplante].Value = _chkPerdeuImplante.Checked;
                linhaAlvo.Cells[Colunas.Ecg].Value = _chkEcg.Checked;
                DateTime dataD8 = _dataD8DefinidaManualmente ? _dtpD8.Value : DateTime.Now;

                linhaAlvo.Cells[Colunas.DataD8].Value = dataD8.ToString(Protocolo.FormatoDataHora);
            }

            // --- BLOCO IATF ---
            if (_viewModel.EstagioAtual == EstagioProtocolo.D10_IATF || _viewModel.ModoTabelaCompleta)
            {
                AprenderNovaOpcao(Colunas.Touro, _touro.Text, _touro);
                AprenderNovaOpcao(Colunas.Inseminador, _inseminador.Text, _inseminador);

                if (!string.IsNullOrWhiteSpace(_touro.Text)) linhaAlvo.Cells[Colunas.Touro].Value = _touro.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_inseminador.Text)) linhaAlvo.Cells[Colunas.Inseminador].Value = _inseminador.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_txtEscoreCio.Text)) linhaAlvo.Cells[Colunas.EscoreCio].Value = _txtEscoreCio.Text;

                linhaAlvo.Cells[Colunas.Gnrh].Value = _chkGnrh.Checked;
                DateTime dataIATF = _dataIATFDefinidaManualmente ? _dtpIATF.Value : DateTime.Now;
                linhaAlvo.Cells[Colunas.DataIATF].Value = dataIATF.ToString(Protocolo.FormatoDataHora);
            }

            // --- BLOCO DG ---
            if (_viewModel.EstagioAtual == EstagioProtocolo.DG_Diagnostico || _viewModel.EstagioAtual == EstagioProtocolo.Finalizado || _viewModel.ModoTabelaCompleta)
            {
                AprenderNovaOpcao(Colunas.Ovario, _txtOvario.Text, _txtOvario);
                AprenderNovaOpcao(Colunas.Destino, _txtDestino.Text, _txtDestino);
                AprenderNovaOpcao(Colunas.ResultadoDG, _cbResultadoDG.Text, _cbResultadoDG);

                if (!string.IsNullOrWhiteSpace(_cbResultadoDG.Text)) linhaAlvo.Cells[Colunas.ResultadoDG].Value = _cbResultadoDG.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_txtOvario.Text)) linhaAlvo.Cells[Colunas.Ovario].Value = _txtOvario.Text.ToUpper();
                if (!string.IsNullOrWhiteSpace(_txtDestino.Text)) linhaAlvo.Cells[Colunas.Destino].Value = _txtDestino.Text.ToUpper();

                DateTime dataDG = _dataDGDefinidaManualmente ? _dtpDG.Value : DateTime.Now;
                linhaAlvo.Cells[Colunas.DataDG].Value = dataDG.ToString(Protocolo.FormatoDataSimples);
            }

            // =========================================================================
            // 4. LIMPEZA INTELIGENTE
            // =========================================================================
            if (ehNovaLinha)
            {
                AtualizarMemoriaNumeros();

                _numero.Clear();
                _observacoes.Clear();

                // Reseta calculadora se for o começo de um novo animal
                if (_viewModel.ModoTabelaCompleta || _viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio)
                {
                    ResetarCalculadoraDatas();
                }

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
                    if (_viewModel.EstagioAtual == EstagioProtocolo.D0_Inicio)
                    {
                        _ecc.Clear();
                        LimparControles(pnlD8);
                        LimparControles(pnlIATF); _txtEscoreCio.Clear();
                        LimparControles(pnlDG);
                    }
                    else if (_viewModel.EstagioAtual == EstagioProtocolo.D8_Retirada)
                    {
                        LimparControles(pnlD8);
                    }
                    else if (_viewModel.EstagioAtual == EstagioProtocolo.D10_IATF)
                    {
                        _txtEscoreCio.Clear();
                    }
                    else if (_viewModel.EstagioAtual == EstagioProtocolo.DG_Diagnostico)
                    {
                        LimparControles(pnlDG);
                    }
                }
            }
            else
            {
                MessageBox.Show($"Dados do animal {idDigitado} atualizados!");
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
            _sistemaEstaOcupado = true;
            try
            {
                if (_viewModel.TentarAvancar())
                {
                    SalvarTabelaNoArquivo();
                    AtualizarInterfaceGeral();
                }
            }
            finally { _sistemaEstaOcupado = false; }
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
        }

        private void AtualizarVisualizador(EstagioProtocolo estagio)
        {
            foreach (DataGridViewColumn col in tabela.Columns) col.Visible = false;

            // --- REGRA: Se for Modo Completo, considera que estamos no fim para fins de VISIBILIDADE ---
            // Isso faz todas as colunas aparecerem na tabela, mas o painel de inserção continua respeitando o 'estagio' atual.
            bool mostrarTudo = _viewModel.ModoTabelaCompleta;

            // Mostra colunas baseadas no estágio OU se for modo completo
            MostrarColuna(Colunas.Id, true); // Sempre visível
            MostrarColuna(Colunas.Categoria, true);
            MostrarColuna(Colunas.Raca, true);

            if (estagio >= EstagioProtocolo.D0_Inicio || mostrarTudo)
            {
                MostrarColuna(Colunas.Ecc, true);
                MostrarColuna(Colunas.Lote, true);
                MostrarColuna(Colunas.DataD0, true);
            }
            if (estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo)
            {
                MostrarColuna(Colunas.PerdeuImplante, estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo);
                MostrarColuna(Colunas.Ecg, estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo);
                MostrarColuna(Colunas.DataD8, estagio >= EstagioProtocolo.D8_Retirada || mostrarTudo);
            }
            if (estagio >= EstagioProtocolo.D10_IATF || mostrarTudo)
            {
                bool editavel = (estagio == EstagioProtocolo.D10_IATF) || mostrarTudo;
                MostrarColuna(Colunas.EscoreCio, editavel);
                MostrarColuna(Colunas.Gnrh, editavel);
                MostrarColuna(Colunas.Touro, editavel);
                MostrarColuna(Colunas.Inseminador, editavel);
                MostrarColuna(Colunas.DataIATF, editavel);
            }
            if (estagio >= EstagioProtocolo.DG_Diagnostico || mostrarTudo)
            {
                bool editavel = (estagio == EstagioProtocolo.DG_Diagnostico) || mostrarTudo;
                MostrarColuna(Colunas.ResultadoDG, editavel);
                MostrarColuna(Colunas.Ovario, editavel);
                MostrarColuna(Colunas.Destino, editavel);
                MostrarColuna(Colunas.DataDG, editavel);
            }

            MostrarColuna(Colunas.Obs, true);
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
            AdicionarColunaTexto(Colunas.Id, "Nº Brinco", 80, true);
            AdicionarColunaCombo(Colunas.Categoria, "Categoria", 100, new string[] { });
            AdicionarColunaCombo(Colunas.Raca, "Raça", 100, new string[] { });

            // Grupo 2: D0
            AdicionarColunaTexto(Colunas.Ecc, "ECC (D0)", 60, true);
            AdicionarColunaTexto(Colunas.Lote, "Lote Trat.", 100);
            AdicionarColunaTexto(Colunas.DataD0, "Data D0", 110);

            // Grupo 3: D8
            AdicionarColunaCheck(Colunas.PerdeuImplante, "Perdeu Implante?", 100);
            AdicionarColunaCheck(Colunas.Ecg, "Usou eCG?", 70);
            AdicionarColunaTexto(Colunas.DataD8, "Data D8", 110);

            // Grupo 4: IATF
            AdicionarColunaTexto(Colunas.EscoreCio, "Escore Cio", 70, true);
            AdicionarColunaCheck(Colunas.Gnrh, "Indutor (GnRH)?", 100);
            AdicionarColunaTexto(Colunas.DataIATF, "Data IATF", 110);
            AdicionarColunaCombo(Colunas.Touro, "Touro", 120, new string[] { });
            AdicionarColunaCombo(Colunas.Inseminador, "Inseminador", 120, new string[] { });

            // Grupo 5: DG
            AdicionarColunaCombo(Colunas.ResultadoDG, "Resultado DG", 100, new string[] { "VAZIA", "PRENHA" });
            AdicionarColunaCombo(Colunas.Ovario, "Est. Ovariana", 120, new string[] { });
            AdicionarColunaCombo(Colunas.Destino, "Destino Final", 120, new string[] { });
            AdicionarColunaTexto(Colunas.DataDG, "Data DG", 90);

            AdicionarColunaTexto(Colunas.Obs, "Observações", 200);

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
                string id = linha.Cells[Colunas.Id].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(id)) listaBrincos.Add(id);
            }
            _numero.AutoCompleteCustomSource = listaBrincos;
        }

        // Funções para criar colunas
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

        private void CalcularEstatisticas()
        {
            double totalAptas = 0;
            double totalInseminadas = 0;
            double totalPrenhas = 0;
            double totalDoses = 0;

            foreach (DataGridViewRow linha in tabela.Rows)
            {
                if (linha.IsNewRow || !linha.Visible) continue;

                totalAptas++;
                string touro = linha.Cells[Colunas.Touro].Value?.ToString();
                string inseminador = linha.Cells[Colunas.Inseminador].Value?.ToString();
                bool foiInseminada = !string.IsNullOrWhiteSpace(touro) || !string.IsNullOrWhiteSpace(inseminador);

                if (foiInseminada) { totalInseminadas++; totalDoses++; }

                string resultadoDG = linha.Cells[Colunas.ResultadoDG].Value?.ToString().ToUpper() ?? "";
                if (resultadoDG.Contains("PRENHA") || resultadoDG == "TRUE") totalPrenhas++;
            }

            // Taxa de Concepção
            double taxaConcepcao = (totalInseminadas > 0) ? (totalPrenhas / totalInseminadas) * 100 : 0;
            lblStatConcepcao.Text = (totalInseminadas > 0) ? $"{taxaConcepcao:F1}%" : "-";
            lblStatConcepcao.ForeColor = (taxaConcepcao > 50) ? Color.Green : (totalInseminadas > 0 ? Color.Red : Color.Gray);

            // Taxa de Serviço
            double taxaServico = (totalAptas > 0) ? (totalInseminadas / totalAptas) * 100 : 0;
            lblStatServico.Text = (totalAptas > 0) ? $"{taxaServico:F1}%" : "-";
            lblStatServico.ForeColor = (taxaServico > 60) ? Color.Green : Color.Orange;

            // Taxa de Prenhez
            double taxaPrenhez = (totalAptas > 0) ? (totalPrenhas / totalAptas) * 100 : 0;
            lblStatPrenhez.Text = (totalAptas > 0) ? $"{taxaPrenhez:F1}%" : "-";
            lblStatPrenhez.ForeColor = (taxaPrenhez >= 40) ? Color.Green : Color.Red;

            // Índice de Inseminação
            if (totalPrenhas > 0)
            {
                double indiceInseminacao = totalDoses / totalPrenhas;
                lblStatDoses.Text = $"{indiceInseminacao:F2}";
                if (indiceInseminacao <= 1.5) lblStatDoses.ForeColor = Color.Green;
                else if (indiceInseminacao <= 2.0) lblStatDoses.ForeColor = Color.Orange;
                else lblStatDoses.ForeColor = Color.Red;
            }
            else lblStatDoses.Text = "-";

            lblStatTotal.Text = totalAptas.ToString();
        }

        private void CalcularTodasMedias()
        {
            long ticksD0 = 0, ticksD8 = 0, ticksIATF = 0, ticksDG = 0;
            int countD0 = 0, countD8 = 0, countIATF = 0, countDG = 0;

            foreach (DataGridViewRow linha in tabela.Rows)
            {
                if (linha.IsNewRow || !linha.Visible) continue;
                if (DateTime.TryParse(linha.Cells[Colunas.DataD0].Value?.ToString(), out DateTime d0)) { ticksD0 += d0.Ticks; countD0++; }
                if (DateTime.TryParse(linha.Cells[Colunas.DataD8].Value?.ToString(), out DateTime d8)) { ticksD8 += d8.Ticks; countD8++; }
                if (DateTime.TryParse(linha.Cells[Colunas.DataIATF].Value?.ToString(), out DateTime iatf)) { ticksIATF += iatf.Ticks; countIATF++; }
                if (DateTime.TryParse(linha.Cells[Colunas.DataDG].Value?.ToString(), out DateTime dg)) { ticksDG += dg.Ticks; countDG++; }
            }

            // Médias reais de cada etapa (null se não há dados)
            DateTime? mediaD0 = countD0 > 0 ? new DateTime(ticksD0 / countD0) : (DateTime?)null;
            DateTime? mediaD8 = countD8 > 0 ? new DateTime(ticksD8 / countD8) : (DateTime?)null;
            DateTime? mediaIATF = countIATF > 0 ? new DateTime(ticksIATF / countIATF) : (DateTime?)null;

            // =========================================================
            // PREVISÕES: cada etapa usa a base mais recente disponível
            // Prioridade: dado real da etapa anterior > calcular da anterior
            // =========================================================

            // Previsão D8: baseada na média real do D0 da tabela
            DateTime? previsaoD8 = mediaD0.HasValue
                ? _dateCalc.CalcularD8(mediaD0.Value)
                : (DateTime?)null;

            // Previsão IATF:
            // — Se já há dados de D8 na tabela → usa média real do D8
            // — Senão, se há dados de D0 → usa a previsão do D8
            DateTime? previsaoIATF = mediaD8.HasValue
                ? _dateCalc.CalcularIATF(mediaD8.Value)
                : previsaoD8.HasValue
                    ? _dateCalc.CalcularIATF(previsaoD8.Value)
                    : (DateTime?)null;

            // Previsão DG:
            // — Se já há dados de IATF na tabela → usa média real do IATF
            // — Senão, se há previsão de IATF → usa ela
            DateTime? previsaoDG = mediaIATF.HasValue
                ? _dateCalc.CalcularDG(mediaIATF.Value)
                : previsaoIATF.HasValue
                    ? _dateCalc.CalcularDG(previsaoIATF.Value)
                    : (DateTime?)null;

            AtualizarLabelMedia(lblMediaD0, "D0", ticksD0, countD0, EstagioProtocolo.D0_Inicio, null);
            AtualizarLabelMedia(lblMediaD8, "D8", ticksD8, countD8, EstagioProtocolo.D8_Retirada, previsaoD8);
            AtualizarLabelMedia(lblMediaIATF, "IATF", ticksIATF, countIATF, EstagioProtocolo.D10_IATF, previsaoIATF);
            AtualizarLabelMedia(lblMediaDG, "DG", ticksDG, countDG, EstagioProtocolo.DG_Diagnostico, previsaoDG);
        }

        private void AtualizarLabelMedia(Label lbl, string titulo, long totalTicks, int count,
    EstagioProtocolo estagioDaLabel, DateTime? previsao)
        {
            if (count > 0)
            {
                // Tem dados reais na tabela — mostra a média
                DateTime dataMedia = new DateTime(totalTicks / count);
                lbl.Text = $"{titulo}: {dataMedia:dd/MM HH:mm}";

                if (estagioDaLabel <= _viewModel.EstagioAtual)
                    lbl.ForeColor = Color.Black;
                else
                {
                    DateTime hoje = DateTime.Now.Date;
                    if (dataMedia.Date < hoje) lbl.ForeColor = Color.Red;
                    else if (dataMedia.Date == hoje) lbl.ForeColor = Color.OrangeRed;
                    else lbl.ForeColor = Color.Green;
                }
            }
            else if (previsao.HasValue)
            {
                // Sem dados ainda — mostra a previsão do DatePicker
                lbl.Text = $"{titulo}: {previsao.Value:dd/MM HH:mm} (prev.)";
                lbl.ForeColor = Color.Blue;
            }
            else
            {
                lbl.Text = $"{titulo}: -";
                lbl.ForeColor = Color.Gray;
            }
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
                        if (!(linha.Cells[Colunas.Raca].Value?.ToString() ?? "").Equals(_cmbFiltroRaca.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                    // Categoria
                    if (deveAparecer && _cmbFiltroCategoria.SelectedIndex > 0 && _cmbFiltroCategoria.Text != "(Todos)")
                        if (!(linha.Cells[Colunas.Categoria].Value?.ToString() ?? "").Equals(_cmbFiltroCategoria.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                    // ECC (Valor Numérico)
                    if (deveAparecer && !string.IsNullOrWhiteSpace(_txtFiltroValorECC.Text))
                    {
                        string eccTexto = linha.Cells[Colunas.Ecc].Value?.ToString() ?? "0";
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
                        if (_chkFiltroPerdeuImplante.Checked && !Convert.ToBoolean(linha.Cells[Colunas.PerdeuImplante].Value)) deveAparecer = false;

                        // Apenas quem usou eCG
                        if (_chkFiltroEcg.Checked && !Convert.ToBoolean(linha.Cells[Colunas.Ecg].Value)) deveAparecer = false;
                    }

                    // =================================================================
                    // 3. FILTROS DA IATF (Inseminação)
                    // =================================================================
                    if (deveAparecer)
                    {
                        // Touro
                        if (_cmbFiltroTouro.SelectedIndex > 0 && _cmbFiltroTouro.Text != "(Todos)")
                            if (!(linha.Cells[Colunas.Touro].Value?.ToString() ?? "").Equals(_cmbFiltroTouro.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                        // Inseminador
                        if (_cmbFiltroInseminador.SelectedIndex > 0 && _cmbFiltroInseminador.Text != "(Todos)")
                            if (!(linha.Cells[Colunas.Inseminador].Value?.ToString() ?? "").Equals(_cmbFiltroInseminador.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;

                        // Escore de Cio (Valor Numérico)
                        if (!string.IsNullOrWhiteSpace(_txtFiltroValorCio.Text))
                        {
                            string cioTexto = linha.Cells[Colunas.EscoreCio].Value?.ToString() ?? "0";
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
                            string res = linha.Cells[Colunas.ResultadoDG].Value?.ToString().ToUpper() ?? "";
                            bool ehPrenha = res.Contains("PRENHA") || res == "TRUE";

                            if (_rbFiltroPrenha.Checked && !ehPrenha) deveAparecer = false;
                            if (_rbFiltroVazia.Checked && ehPrenha) deveAparecer = false;
                        }

                        // Destino Final
                        if (_cmbFiltroDestino.SelectedIndex > 0 && _cmbFiltroDestino.Text != "(Todos)")
                            if (!(linha.Cells[Colunas.Destino].Value?.ToString() ?? "").Equals(_cmbFiltroDestino.Text, StringComparison.OrdinalIgnoreCase)) deveAparecer = false;
                    }

                    // Aplica a visibilidade calculada
                    linha.Visible = deveAparecer;
                }

                // Recalcula totais com base nas linhas visíveis
                CalcularEstatisticas();
                CalcularTodasMedias();
            }
            catch (IOException ex)
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
            if (op.Contains("<")) AplicarFiltro(Colunas.Ecc, val, TipoFiltro.MenorQue);
            else if (op.Contains(">")) AplicarFiltro(Colunas.Ecc, val, TipoFiltro.MaiorQue);
            else AplicarFiltro(Colunas.Ecc, val, TipoFiltro.IgualNumerico);
        }

        private void AplicarFiltroCio(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtFiltroValorCio.Text)) { btnLimparFiltros_Click(null, null); return; }
            string op = _cmbOperadorCio.Text;
            string val = _txtFiltroValorCio.Text;
            if (op.Contains("<")) AplicarFiltro(Colunas.EscoreCio, val, TipoFiltro.MenorQue);
            else if (op.Contains(">")) AplicarFiltro(Colunas.EscoreCio, val, TipoFiltro.MaiorQue);
            else AplicarFiltro(Colunas.EscoreCio, val, TipoFiltro.IgualNumerico);
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
            catch (IOException ex)
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
            int faseAtual = (int)_viewModel.EstagioAtual;
            pnlFiltroD0.Visible = (faseAtual >= (int)EstagioProtocolo.D0_Inicio);
            pnlFiltroD8.Visible = (faseAtual >= (int)EstagioProtocolo.D8_Retirada);
            pnlFiltroIATF.Visible = (faseAtual >= (int)EstagioProtocolo.D10_IATF);
            pnlFiltroDG.Visible = (faseAtual >= (int)EstagioProtocolo.DG_Diagnostico);

            if (pnlFiltroD0.Visible)
            {
                if (_cmbOperadorECC.Items.Count == 0) { _cmbOperadorECC.Items.AddRange(new string[] { "< (Menor)", "> (Maior)", "= (Igual)" }); _cmbOperadorECC.SelectedIndex = 0; }
                PreencherComboFiltro(_cmbFiltroRaca, Colunas.Raca);
                PreencherComboFiltro(_cmbFiltroCategoria, Colunas.Categoria);
            }
            if (pnlFiltroIATF.Visible)
            {
                if (_cmbOperadorCio.Items.Count == 0) { _cmbOperadorCio.Items.AddRange(new string[] { "> (Maior)", "< (Menor)", "= (Igual)" }); _cmbOperadorCio.SelectedIndex = 0; }
                PreencherComboFiltro(_cmbFiltroInseminador, Colunas.Inseminador);
                PreencherComboFiltro(_cmbFiltroTouro, Colunas.Touro);
            }
            if (pnlFiltroDG.Visible) PreencherComboFiltro(_cmbFiltroDestino, Colunas.Destino);
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
        }

        // Métodos curtos que chamam o preenchimento na hora do clique
        private void AtualizarComboRaca(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroRaca, Colunas.Raca);
        private void AtualizarComboCategoria(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroCategoria, Colunas.Categoria);
        private void AtualizarComboTouro(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroTouro, Colunas.Touro);
        private void AtualizarComboInseminador(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroInseminador, Colunas.Inseminador);
        private void AtualizarComboDestino(object sender, EventArgs e) => PreencherComboFiltro(_cmbFiltroDestino, Colunas.Destino);
        #endregion

        #region 7. Arquivos (Importar, Exportar, Gerenciar)
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

        // Botão Importar
        private void btnImportar_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Arquivos de Dados (*.xlsx; *.csv)|*.xlsx;*.csv";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var confirmacao = MessageBox.Show(
                "Deseja LIMPAR a tabela atual antes de importar?\n\n" +
                "Sim = Apaga tudo e traz o arquivo.\nNão = Mantém os atuais e adiciona no final.",
                "Importação", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (confirmacao == DialogResult.Cancel) return;
            if (confirmacao == DialogResult.Yes)
            {
                tabela.Rows.Clear();
                btnLimparFiltros_Click(null, null);
            }

            // ← uma única chamada que carrega, atualiza título e metadados
            CarregarArquivoEspecifico(ofd.FileName);

            MessageBox.Show("Importação realizada com sucesso!",
                "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Botão Gerenciador (Janela Separada)
        private void btnAbrirGerenciador_Click(object sender, EventArgs e)
        {
            using (FrmGerenciadorArquivos janela = new FrmGerenciadorArquivos())
            {
                if (janela.ShowDialog() == DialogResult.OK)
                {
                    if (janela.CriarNovo)
                    {
                        // ATUALIZADO: Agora passa o booleano do Modo Completo
                        ExecutarNovoLote(janela.NomeFazendaDigitado, janela.CaminhoPastaSelecionada, janela.ModoTabelaCompleta);
                    }
                    else if (!string.IsNullOrEmpty(janela.CaminhoArquivoRetorno))
                    {
                        CarregarArquivoEspecifico(janela.CaminhoArquivoRetorno);
                    }
                }
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
                // Pega o caminho oficial da pasta "Meus Documentos" do Windows
                string pastaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Cria uma pasta com o nome do seu app lá dentro
                _diretorioDeTrabalhoAtual = Path.Combine(pastaDocumentos, "PecuariaDigital_Dados");
            }

            if (!Directory.Exists(_diretorioDeTrabalhoAtual))
                Directory.CreateDirectory(_diretorioDeTrabalhoAtual);

            string nomeArquivo = GerarNomeArquivo(".csv");
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
            CarregarArquivoEspecifico(ObterCaminhoCompleto());
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
                _viewModel.ConfigurarNovoLote(_viewModel.NomeFazenda, _diretorioDeTrabalhoAtual,lote.ModoTabelaCompleta, dataInicio); // ← passa a data do arquivo

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
                    Id = G(Colunas.Id),
                    Categoria = G(Colunas.Categoria),
                    Raca = G(Colunas.Raca),
                    Ecc = G(Colunas.Ecc),
                    Lote = G(Colunas.Lote),
                    Observacoes = G(Colunas.Obs),
                    PerdeuImplante = B(Colunas.PerdeuImplante),
                    UsouEcg = B(Colunas.Ecg),
                    EscoreCio = G(Colunas.EscoreCio),
                    UsouGnrh = B(Colunas.Gnrh),
                    Touro = G(Colunas.Touro),
                    Inseminador = G(Colunas.Inseminador),
                    ResultadoDG = G(Colunas.ResultadoDG),
                    Ovario = G(Colunas.Ovario),
                    Destino = G(Colunas.Destino),
                };

                // Datas com TryParse seguro
                if (DateTime.TryParse(G(Colunas.DataD0), out var d0)) animal.DataD0 = d0;
                if (DateTime.TryParse(G(Colunas.DataD8), out var d8)) animal.DataD8 = d8;
                if (DateTime.TryParse(G(Colunas.DataIATF), out var iatf)) animal.DataIATF = iatf;
                if (DateTime.TryParse(G(Colunas.DataDG), out var dg)) animal.DataDG = dg;

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
            tabela.Rows.Clear();

            foreach (var animal in lote.Animais)
            {
                int index = tabela.Rows.Add();
                var linha = tabela.Rows[index];

                void S(string col, object val) { if (val != null) linha.Cells[col].Value = val; }

                S(Colunas.Id, animal.Id);
                S(Colunas.Categoria, animal.Categoria);
                S(Colunas.Raca, animal.Raca);
                S(Colunas.Ecc, animal.Ecc);
                S(Colunas.Lote, animal.Lote);
                S(Colunas.Obs, animal.Observacoes);
                S(Colunas.PerdeuImplante, animal.PerdeuImplante);
                S(Colunas.Ecg, animal.UsouEcg);
                S(Colunas.EscoreCio, animal.EscoreCio);
                S(Colunas.Gnrh, animal.UsouGnrh);
                S(Colunas.Touro, animal.Touro);
                S(Colunas.Inseminador, animal.Inseminador);
                S(Colunas.ResultadoDG, animal.ResultadoDG);
                S(Colunas.Ovario, animal.Ovario);
                S(Colunas.Destino, animal.Destino);

                if (animal.DataD0.HasValue)
                    S(Colunas.DataD0, animal.DataD0.Value.ToString(Protocolo.FormatoDataHora));
                if (animal.DataD8.HasValue)
                    S(Colunas.DataD8, animal.DataD8.Value.ToString(Protocolo.FormatoDataHora));
                if (animal.DataIATF.HasValue)
                    S(Colunas.DataIATF, animal.DataIATF.Value.ToString(Protocolo.FormatoDataHora));
                if (animal.DataDG.HasValue)
                    S(Colunas.DataDG, animal.DataDG.Value.ToString(Protocolo.FormatoDataSimples));

                // Reensina os combos com os valores carregados
                AprenderNovaOpcao(Colunas.Raca, animal.Raca, _raca);
                AprenderNovaOpcao(Colunas.Categoria, animal.Categoria, _categoria);
                AprenderNovaOpcao(Colunas.Touro, animal.Touro, _touro);
                AprenderNovaOpcao(Colunas.Inseminador, animal.Inseminador, _inseminador);
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
            if (nomeColuna == Colunas.DataD0 && _viewModel.EstagioAtual <= EstagioProtocolo.D0_Inicio)
            {
                deveAnalisar = true;
                // Se tem Raça ou Categoria, considera D0 iniciado
                if (!string.IsNullOrWhiteSpace(linha.Cells[Colunas.Raca].Value?.ToString())) dadosPreenchidos = true;
            }
            else if (nomeColuna == Colunas.DataD8 && _viewModel.EstagioAtual <= EstagioProtocolo.D8_Retirada)
            {
                deveAnalisar = true;
                // Se marcou 'Perdeu Implante' OU 'eCG', considera D8 feito
                bool perdeu = Convert.ToBoolean(linha.Cells[Colunas.PerdeuImplante].Value);
                bool ecg = Convert.ToBoolean(linha.Cells[Colunas.Ecg].Value);
                // OU se já avançou para IATF (tem dados de cio)
                bool temIATF = !string.IsNullOrWhiteSpace(linha.Cells[Colunas.EscoreCio].Value?.ToString());

                if (perdeu || ecg || temIATF) dadosPreenchidos = true;
            }
            else if (nomeColuna == Colunas.DataIATF && _viewModel.EstagioAtual <= EstagioProtocolo.D10_IATF)
            {
                deveAnalisar = true;
                // Se tem Touro, Inseminador ou Escore de Cio
                string touro = linha.Cells[Colunas.Touro].Value?.ToString();
                string cio = linha.Cells[Colunas.EscoreCio].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(touro) || !string.IsNullOrWhiteSpace(cio)) dadosPreenchidos = true;
            }
            else if (nomeColuna == Colunas.DataDG && _viewModel.EstagioAtual <= EstagioProtocolo.DG_Diagnostico)
            {
                deveAnalisar = true;
                // Se tem Resultado DG
                if (!string.IsNullOrWhiteSpace(linha.Cells[Colunas.ResultadoDG].Value?.ToString())) dadosPreenchidos = true;
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

        // Ordenação de Colunas
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
        #endregion

        private DataGridViewRow EscolherLinhaDuplicada(List<DataGridViewRow> linhas)
        {
            using (Form formEscolha = new Form())
            {
                formEscolha.Text = "Animais Duplicados";
                formEscolha.Size = new Size(550, 300); // Janela um pouco mais larga
                formEscolha.StartPosition = FormStartPosition.CenterParent;
                formEscolha.FormBorderStyle = FormBorderStyle.FixedDialog;
                formEscolha.MaximizeBox = false;
                formEscolha.MinimizeBox = false;

                // 1. Cria o Botão (Fundo)
                Button btnOk = new Button();
                btnOk.Text = "Confirmar Escolha";
                btnOk.Dock = DockStyle.Bottom;
                btnOk.Height = 45;
                btnOk.BackColor = Color.LightGreen;
                btnOk.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btnOk.FlatStyle = FlatStyle.Flat;

                // 2. Cria o Texto (Topo)
                Label lbl = new Label();
                lbl.Text = "Foram encontrados múltiplos animais com este mesmo brinco.\nSelecione na lista abaixo qual você deseja atualizar agora:";
                lbl.Dock = DockStyle.Top;
                lbl.Height = 50;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                // 3. Cria a Lista blindada contra bugs visuais
                ListBox lst = new ListBox();
                lst.Dock = DockStyle.Fill;
                lst.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                lst.ForeColor = Color.Black; // Força a letra a ser preta
                lst.BackColor = Color.White; // Força o fundo a ser branco
                lst.IntegralHeight = false;

                // Preenche a lista com os animais repetidos
                foreach (var row in linhas)
                {
                    string raca = row.Cells[Colunas.Raca].Value?.ToString() ?? "-";
                    string cat = row.Cells[Colunas.Categoria].Value?.ToString() ?? "-";
                    string lote = row.Cells[Colunas.Lote].Value?.ToString() ?? "-";

                    lst.Items.Add($"Linha {row.Index + 1} | Raça: {raca} | Categoria: {cat} | Lote: {lote}");
                }

                // 4. Ordem de adição crucial para o layout não esconder a lista
                formEscolha.Controls.Add(lst);
                formEscolha.Controls.Add(lbl);
                formEscolha.Controls.Add(btnOk);
                lst.BringToFront(); // Puxa a lista para a frente de tudo

                btnOk.Click += (s, e) => { formEscolha.DialogResult = DialogResult.OK; };

                // Se o usuário clicou OK e selecionou algum item, retorna a linha correspondente
                if (formEscolha.ShowDialog() == DialogResult.OK && lst.SelectedIndex != -1)
                {
                    return linhas[lst.SelectedIndex];
                }
                return null; // Retorna nulo se fechou a janela sem escolher
            }
        }
    }

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
            else if (_nomeColuna == Colunas.Id)
            {
                int.TryParse(valor1, out int n1); int.TryParse(valor2, out int n2);
                resultado = n1.CompareTo(n2);
            }
            else if (_nomeColuna == Colunas.Ecc || _nomeColuna == Colunas.EscoreCio)
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