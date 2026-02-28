using Pecuaria_Digital.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pecuaria_Digital
{
    public partial class FrmGerenciadorArquivos : Form
    {
        // Variáveis Públicas
        public string CaminhoArquivoRetorno { get; private set; } = "";
        public string CaminhoPastaSelecionada { get; private set; } = "";
        public bool CriarNovo { get; private set; } = false;
        public string NomeFazendaDigitado { get; private set; } = "";

        // NOVA PROPRIEDADE: Define se a tabela será completa ou por etapas
        public bool ModoTabelaCompleta { get; private set; } = false;

        // Variáveis Privadas
        private string _pastaDados;
        private string _arquivoConfig;

        public FrmGerenciadorArquivos()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Gerenciador de Tabelas";

            _arquivoConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_pasta.txt");
            string pastaSalva = LerConfiguracaoDoDisco();

            if (!string.IsNullOrWhiteSpace(pastaSalva) && Directory.Exists(pastaSalva))
                _pastaDados = pastaSalva;
            else
                _pastaDados = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dados");

            if (!Directory.Exists(_pastaDados)) { try { Directory.CreateDirectory(_pastaDados); }
                catch (Exception ex)
                {
                    AppLogger.Erro(
                        "Erro ao criar diretório de dados", ex);
                }
            }
        }

        private void FrmGerenciadorArquivos_Load(object sender, EventArgs e) { CarregarListaVisual(); }

        private string LerConfiguracaoDoDisco()
        {
            try { if (File.Exists(_arquivoConfig)) return File.ReadAllText(_arquivoConfig).Trim(); }
            catch (Exception ex)
            {
                AppLogger.Erro(
                    "Erro ao ler diretório de dados", ex);
            }

            return null;
        }

        private void SalvarConfiguracaoNoDisco()
        {
            try { File.WriteAllText(_arquivoConfig, _pastaDados); }
            catch (Exception ex)
            {
                AppLogger.Erro(
                    "Erro ao criar diretório de dados", ex);
            }

        }

        private void CarregarListaVisual()
        {
            lstArquivos.Items.Clear();
            try
            {
                if (_pastaDados.Length < 60) this.Text = "Lendo de: " + _pastaDados;
                else this.Text = "Lendo de: ...\\" + new DirectoryInfo(_pastaDados).Name;
            }
            catch (Exception ex)
            {
                AppLogger.Erro(
                    "Erro ao carregar diretório de dados", ex);
            }


            if (!Directory.Exists(_pastaDados)) return;

            try
            {
                var arquivos = Directory.GetFiles(_pastaDados, "*.*");
                foreach (var arquivo in arquivos)
                {
                    string ext = Path.GetExtension(arquivo).ToLower();
                    if (ext == ".csv" || ext == ".xlsx") lstArquivos.Items.Add(Path.GetFileName(arquivo));
                }
            }
            catch (IOException ex)
            {
                AppLogger.Erro("Falha ao carregar lista de arquivos", ex);
                MessageBox.Show(
                    "Não foi possível abrir a lista.\n\n" +
                    "Detalhes registrados no log.",
                    "Erro ao Carregar Lista",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }

        // --- AÇÕES DOS BOTÕES ---

        private void btnAbrir_Click_1(object sender, EventArgs e)
        {
            if (lstArquivos.SelectedItem == null)
            {
                MessageBox.Show("Selecione um arquivo na lista.");
                return;
            }
            string nomeArquivo = lstArquivos.SelectedItem.ToString();
            CaminhoArquivoRetorno = Path.Combine(_pastaDados, nomeArquivo);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnNovo_Click_1(object sender, EventArgs e)
        {
            string nomeSugestao = "Fazenda_" + new Random().Next(100, 999);

            using (Form prompt = new Form())
            {
                // Aumentei a altura para caber os RadioButtons
                prompt.Width = 450; prompt.Height = 250;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Criar Nova Tabela";
                prompt.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label() { Left = 20, Top = 20, Text = "1. Digite o nome:", AutoSize = true };
                TextBox txt = new TextBox() { Left = 20, Top = 45, Width = 390, Text = nomeSugestao };

                // --- NOVOS CONTROLES DE ESCOLHA ---
                GroupBox grpModo = new GroupBox() { Left = 20, Top = 80, Width = 390, Height = 80, Text = "2. Escolha o Modo de Visualização" };

                RadioButton rbEtapas = new RadioButton() { Left = 20, Top = 25, Width = 300, Text = "Por Etapas (Libera conforme avança)", Checked = true, AutoSize = true };
                RadioButton rbCompleta = new RadioButton() { Left = 20, Top = 50, Width = 300, Text = "Completa (Tudo liberado desde o início)", AutoSize = true };

                grpModo.Controls.Add(rbEtapas);
                grpModo.Controls.Add(rbCompleta);

                Button btn = new Button() { Text = "Escolher Pasta e Criar...", Left = 210, Top = 170, Width = 200, DialogResult = DialogResult.OK };

                btn.Click += (s, args) => { prompt.Close(); };

                prompt.Controls.Add(lbl);
                prompt.Controls.Add(txt);
                prompt.Controls.Add(grpModo); // Adiciona o grupo
                prompt.Controls.Add(btn);

                prompt.AcceptButton = btn;
                prompt.Shown += (s, args) => { txt.Focus(); txt.SelectAll(); };

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string nome = txt.Text.Trim();
                    if (string.IsNullOrWhiteSpace(nome)) return;

                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "Onde salvar?";
                        if (Directory.Exists(_pastaDados)) folderDialog.SelectedPath = _pastaDados;

                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            _pastaDados = folderDialog.SelectedPath;
                            SalvarConfiguracaoNoDisco();

                            this.NomeFazendaDigitado = nome;
                            this.CaminhoPastaSelecionada = _pastaDados;

                            // SALVA A ESCOLHA DO USUÁRIO
                            this.ModoTabelaCompleta = rbCompleta.Checked;

                            this.CriarNovo = true;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                }
            }
        }

        private void btnExluirArquivo_Click(object sender, EventArgs e)
        {
            if (lstArquivos.SelectedItem == null) return;
            string nome = lstArquivos.SelectedItem.ToString();

            if (MessageBox.Show($"Apagar '{nome}'?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try { File.Delete(Path.Combine(_pastaDados, nome)); CarregarListaVisual(); }
                catch (Exception ex)
                {
                    AppLogger.Erro(
                        "Erro ao excluir dados", ex);
                }

            }
        }

        private void btnAbrirPasta_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Selecione a Tabela para Abrir";
                dialog.Filter = "Tabelas (*.csv; *.xlsx)|*.csv;*.xlsx";
                if (Directory.Exists(_pastaDados)) dialog.InitialDirectory = _pastaDados;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string arquivoSelecionado = dialog.FileName;
                    _pastaDados = Path.GetDirectoryName(arquivoSelecionado);
                    SalvarConfiguracaoNoDisco();
                    CaminhoArquivoRetorno = arquivoSelecionado;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void lstArquivos_DoubleClick(object sender, EventArgs e)
        {
            btnAbrir_Click_1(sender, e);
        }
    }
}