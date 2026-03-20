using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using Pecuaria_Digital.Repositories;
using Pecuaria_Digital.Services;
using Pecuaria_Digital.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Pecuaria_Digital.Views
{
    /// <summary>
    /// TELA 1 — Seleção de Fazenda.
    /// Exibe as fazendas cadastradas como cards clicáveis.
    /// Navegação: FrmMenuFazendas → FrmMenuIATFs → FrmMenuTabelas
    /// </summary>
    public partial class FrmMenuFazendas : Form
    {
        // ─── Dependências ─────────────────────────────────────────────────────
        private readonly FazendaRepository _repo = new();

        // ─── Estado ───────────────────────────────────────────────────────────
        private List<FazendaData> _fazendas = new();
        private FazendaData _selecionada = null;

        // ─── Pasta raiz ───────────────────────────────────────────────────────
        private readonly string _pastaRaiz;
        private readonly string _arquivoConfig;

        // ─── Cores dos cards ──────────────────────────────────────────────────
        private static readonly Color CorCardNormal = Color.White;
        private static readonly Color CorCardHover = Color.FromArgb(229, 243, 255);
        private static readonly Color CorCardSelecionado = Color.FromArgb(204, 232, 255);
        private static readonly Color CorBordaSelecionado = Color.FromArgb(0, 120, 215);

        // ─── Construtor ───────────────────────────────────────────────────────

        public FrmMenuFazendas()
        {
            InitializeComponent();

            _arquivoConfig = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "config_pasta.txt");
            _pastaRaiz = CarregarPastaRaiz();
            GarantirPastaExiste(_pastaRaiz);
        }

        // ─── Load ─────────────────────────────────────────────────────────────

        private void FrmMenuFazendas_Load(object sender, EventArgs e)
        {
            try
            {
                CarregarFazendas();
                AppLogger.Info("FrmMenuFazendas carregado.");
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuFazendas_Load", ex);
                MessageBox.Show($"Erro ao carregar fazendas:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Carregar e renderizar ────────────────────────────────────────────

        private void CarregarFazendas()
        {
            _fazendas = _repo.ListarTodas(_pastaRaiz);
            _selecionada = null;

            RenderizarCards();
            AtualizarBotoes();
            AtualizarEstatisticas();
        }

        private void RenderizarCards()
        {
            flpFazendas.SuspendLayout();
            flpFazendas.Controls.Clear();

            if (_fazendas.Count == 0)
            {
                var lblVazio = new Label
                {
                    Text = "Nenhuma fazenda cadastrada.\nClique em \"+ Criar\" para adicionar.",
                    AutoSize = false,
                    Size = new Size(300, 60),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 10f),
                    Margin = new Padding(20)
                };
                flpFazendas.Controls.Add(lblVazio);
            }
            else
            {
                foreach (var fazenda in _fazendas)
                    flpFazendas.Controls.Add(CriarCard(fazenda));
            }

            flpFazendas.ResumeLayout();
        }

        // ─── Criação de card ──────────────────────────────────────────────────

        private Panel CriarCard(FazendaData fazenda)
        {
            // Container principal do card
            var card = new Panel
            {
                Width = 130,
                Height = 170,
                Margin = new Padding(8),
                Cursor = Cursors.Hand,
                Tag = fazenda,
                BackColor = CorCardNormal,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Ícone (quadrado cinza — pode ser substituído por PictureBox)
            var pic = new PictureBox
            {
                Width = 90,
                Height = 90,
                Left = 20,
                Top = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(160, 160, 160),
                Tag = fazenda,
                Cursor = Cursors.Hand
            };

            // Letra inicial da fazenda no centro do ícone
            var lblIniciai = new Label
            {
                Text = fazenda.Nome.Length > 0
                            ? fazenda.Nome[0].ToString().ToUpper() : "F",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Color.White,
                Tag = fazenda,
                Cursor = Cursors.Hand
            };
            if (!string.IsNullOrWhiteSpace(fazenda.CaminhoImagem)
            && File.Exists(fazenda.CaminhoImagem))
            {
                try
                {
                    pic.Image = new Bitmap(fazenda.CaminhoImagem);
                    pic.BackColor = Color.White;
                }
                catch { /* mantém letra inicial */ }
            }

            if (pic.Image == null)
            {
                var lblInicial = new Label
                {
                    Text = fazenda.Nome.Length > 0
                                ? fazenda.Nome[0].ToString().ToUpper() : "F",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Tag = fazenda,
                    Cursor = Cursors.Hand
                };
                pic.Controls.Add(lblInicial);
            }

            card.Controls.Add(pic);

            // Nome da fazenda abaixo do ícone
            var lblNome = new Label
            {
                Text = fazenda.Nome,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 128,
                Height = 36,
                Top = 106,
                Left = 0,
                Font = new Font("Segoe UI", 8.5f),
                Tag = fazenda,
                Cursor = Cursors.Hand
            };

            // Contagem de protocolos
            var arquivos = _repo.ListarArquivosProtocolo(fazenda.CaminhoPasta);
            var lblCount = new Label
            {
                Text = $"{arquivos.Count} IATF(s)",
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 128,
                Height = 18,
                Top = 144,
                Left = 0,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.Gray,
                Tag = fazenda,
                Cursor = Cursors.Hand
            };

            card.Controls.Add(lblNome);
            card.Controls.Add(lblCount);

            // Registra eventos em todos os controles do card
            foreach (Control ctrl in ObterTodosControles(card))
            {
                ctrl.Click += Card_Click;
                ctrl.DoubleClick += Card_DoubleClick;
                ctrl.MouseEnter += Card_MouseEnter;
                ctrl.MouseLeave += Card_MouseLeave;
            }
            // Inclui o próprio card
            card.Click += Card_Click;
            card.DoubleClick += Card_DoubleClick;
            card.MouseEnter += Card_MouseEnter;
            card.MouseLeave += Card_MouseLeave;

            return card;
        }

        // ─── Eventos dos cards ────────────────────────────────────────────────

        private void Card_Click(object sender, EventArgs e)
        {
            var fazenda = ExtrairFazendaDoSender(sender);
            if (fazenda == null) return;

            _selecionada = fazenda;
            AtualizarDestaqueTodosCards();
            AtualizarEstatisticas();
            AtualizarBotoes();
        }

        private void Card_DoubleClick(object sender, EventArgs e)
        {
            Card_Click(sender, e);
            AbrirFazenda();
        }

        private void Card_MouseEnter(object sender, EventArgs e)
        {
            var fazenda = ExtrairFazendaDoSender(sender);
            if (fazenda == null || fazenda == _selecionada) return;
            AplicarCorCard(fazenda, CorCardHover, Color.Transparent);
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            var fazenda = ExtrairFazendaDoSender(sender);
            if (fazenda == null || fazenda == _selecionada) return;
            AplicarCorCard(fazenda, CorCardNormal, Color.Transparent);
        }

        // ─── Botões ───────────────────────────────────────────────────────────

        private void btnAbrir_Click(object sender, EventArgs e) => AbrirFazenda();
        private void btnCriar_Click(object sender, EventArgs e) => CriarFazenda();
        private void btnExcluir_Click(object sender, EventArgs e) => ExcluirFazenda();

        private void AbrirFazenda()
        {
            if (_selecionada == null) { MsgSelecioneItem(); return; }
            try
            {
                string apiKey = AppSettings.ApiKeyAnthropic;
                using var tela = new FrmMenuIATFs(_selecionada, apiKey);
                tela.ShowDialog(this);

                // Recarrega ao voltar (pode ter novos arquivos)
                CarregarFazendas();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuFazendas.AbrirFazenda", ex);
                MessageBox.Show($"Erro ao abrir fazenda:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CriarFazenda()
        {
            using var dlg = new FrmNovaFazenda();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Valida nome
            var (valido, msg) = ValidacaoService.ValidarNomeFazenda(dlg.NomeFazenda);
            if (!valido)
            {
                MessageBox.Show(msg, "Nome inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nomePasta = dlg.NomeFazenda.Trim().Replace(" ", "_");
            string caminho = Path.Combine(_pastaRaiz, nomePasta);

            // Verifica duplicata
            if (Directory.Exists(caminho))
            {
                MessageBox.Show($"Já existe uma fazenda com o nome '{dlg.NomeFazenda}'.",
                    "Nome duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fazenda = new FazendaData
            {
                Nome = dlg.NomeFazenda.Trim(),
                Localizacao = dlg.Localizacao.Trim(),
                CaminhoPasta = caminho
            };

            try
            {
                _repo.Salvar(fazenda);
                AppLogger.Info($"Fazenda criada: {fazenda.Nome}");
                CarregarFazendas();

                // Seleciona automaticamente a fazenda recém-criada
                _selecionada = _fazendas.FirstOrDefault(f => f.Nome == fazenda.Nome);
                AtualizarDestaqueTodosCards();
                AtualizarBotoes();
                AtualizarEstatisticas();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuFazendas.CriarFazenda", ex);
                MessageBox.Show($"Erro ao criar fazenda:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExcluirFazenda()
        {
            if (_selecionada == null) { MsgSelecioneItem(); return; }

            var confirmacao = MessageBox.Show(
                $"Excluir a fazenda '{_selecionada.Nome}'?\n\n" +
                "ATENÇÃO: Todos os protocolos (arquivos CSV/XLSX) serão apagados permanentemente.",
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacao != DialogResult.Yes) return;

            try
            {
                _repo.Excluir(_selecionada);
                _selecionada = null;
                CarregarFazendas();
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuFazendas.ExcluirFazenda", ex);
                MessageBox.Show($"Erro ao excluir:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Painel de estatísticas (rodapé esquerdo) ─────────────────────────

        private void AtualizarEstatisticas()
        {
            flpEstatisticas.SuspendLayout();
            flpEstatisticas.Controls.Clear();

            if (_selecionada == null)
            {
                AdicionarItemEstat("Selecione uma fazenda para ver os detalhes.",
                    Color.Gray, negrito: false);
                flpEstatisticas.ResumeLayout();
                return;
            }

            var arquivos = _repo.ListarArquivosProtocolo(_selecionada.CaminhoPasta);
            int totalIatfs = arquivos.Count;

            AdicionarItemEstat($"📍  {_selecionada.Localizacao}", Color.Black);
            AdicionarItemEstat($"📋  IATFs cadastrados: {totalIatfs}", Color.Black);
            AdicionarItemEstat($"📁  {_selecionada.CaminhoPasta}",
                Color.Gray, negrito: false, fontSize: 7.5f);

            flpEstatisticas.ResumeLayout();
        }

        /// <summary>
        /// Cria e adiciona um label de estatística ao FlowLayoutPanel.
        /// Para adicionar ou remover estatísticas, basta incluir/remover
        /// uma chamada a este método em AtualizarEstatisticas().
        /// </summary>
        private void AdicionarItemEstat(string texto, Color cor,
            bool negrito = true, float fontSize = 9f)
        {
            flpEstatisticas.Controls.Add(new Label
            {
                Text = texto,
                AutoSize = true,
                ForeColor = cor,
                Font = new Font("Segoe UI", fontSize,
                                negrito ? FontStyle.Bold : FontStyle.Regular),
                Margin = new Padding(2, 3, 10, 0)
            });
        }

        // ─── Destaque visual dos cards ────────────────────────────────────────

        private void AtualizarDestaqueTodosCards()
        {
            foreach (Control ctrl in flpFazendas.Controls)
            {
                if (ctrl is not Panel card) continue;
                var fazenda = card.Tag as FazendaData;
                if (fazenda == null) continue;

                bool ativo = fazenda == _selecionada;
                AplicarCorCard(fazenda,
                    ativo ? CorCardSelecionado : CorCardNormal,
                    ativo ? CorBordaSelecionado : Color.Transparent);
            }
        }

        private void AplicarCorCard(FazendaData fazenda, Color fundo, Color borda)
        {
            foreach (Control ctrl in flpFazendas.Controls)
            {
                if (ctrl is not Panel card || card.Tag != fazenda) continue;

                card.BackColor = fundo;
                card.BorderStyle = borda != Color.Transparent
                    ? BorderStyle.FixedSingle
                    : BorderStyle.FixedSingle;

                // Aplica a cor nos filhos também
                foreach (Control filho in ObterTodosControles(card))
                    filho.BackColor = fundo;
            }
        }

        // ─── Utilitários ──────────────────────────────────────────────────────

        private void AtualizarBotoes()
        {
            bool tem = _selecionada != null;
            btnAbrir.Enabled = tem;
            btnExcluir.Enabled = tem;
            btnEditar.Enabled = tem;
        }

        private string CarregarPastaRaiz()
        {
            try
            {
                if (File.Exists(_arquivoConfig))
                {
                    string p = File.ReadAllText(_arquivoConfig).Trim();
                    if (Directory.Exists(p)) return p;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Aviso($"Não leu config_pasta.txt: {ex.Message}");
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dados");
        }

        private static void GarantirPastaExiste(string pasta)
        {
            if (!Directory.Exists(pasta))
                try { Directory.CreateDirectory(pasta); }
                catch (Exception ex)
                { AppLogger.Aviso($"Não criou pasta raiz: {ex.Message}"); }
        }

        private static FazendaData ExtrairFazendaDoSender(object sender) =>
            (sender as Control)?.Tag as FazendaData;

        private static IEnumerable<Control> ObterTodosControles(Control pai)
        {
            foreach (Control filho in pai.Controls)
            {
                yield return filho;
                foreach (var neto in ObterTodosControles(filho))
                    yield return neto;
            }
        }

        private static void MsgSelecioneItem() =>
            MessageBox.Show("Selecione uma fazenda primeiro.",
                "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (_selecionada == null) { MsgSelecioneItem(); return; }

            using var dlg = new FrmEditarFazenda(_selecionada);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var (valido, msg) = ValidacaoService.ValidarNomeFazenda(dlg.NovoNome);
            if (!valido)
            {
                MessageBox.Show(msg, "Nome inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ← Guarda o nome ANTES de chamar CarregarFazendas (que zera _selecionada)
            string nomeParaLog = dlg.NovoNome;

            try
            {
                _selecionada.Nome = dlg.NovoNome;
                _selecionada.Localizacao = dlg.NovaLocalizacao;
                _selecionada.CaminhoImagem = dlg.NovoCaminhoImagem;

                _repo.Salvar(_selecionada);

                // CarregarFazendas() zera _selecionada — qualquer acesso depois disso quebra
                CarregarFazendas();

                // ← Usa a variável local, não _selecionada (que agora é null)
                AppLogger.Info($"Fazenda editada: {nomeParaLog}");
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FrmMenuFazendas.btnEditar_Click", ex);
                MessageBox.Show($"Erro ao salvar:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}