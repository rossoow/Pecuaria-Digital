using Pecuaria_Digital.Services;
using System.Windows.Forms;
using System.Drawing;

namespace Pecuaria_Digital.Views
{
    /// <summary>
    /// Diálogo para criar uma nova fazenda.
    /// Captura nome e localização com validação embutida.
    /// Não requer Designer — controles criados em código.
    /// </summary>
    public class FrmNovaFazenda : Form
    {
        // ─── Propriedades de retorno ──────────────────────────────────────────
        public string NomeFazenda { get; private set; } = "";
        public string Localizacao { get; private set; } = "";

        // ─── Controles ────────────────────────────────────────────────────────
        private TextBox _txtNome;
        private TextBox _txtLocal;
        private Label _lblErro;

        public FrmNovaFazenda()
        {
            ConfigurarForm();
            ConstruirControles();
        }

        private void ConfigurarForm()
        {
            this.Text = "Nova Fazenda";
            this.Width = 420;
            this.Height = 250;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void ConstruirControles()
        {
            // ── Nome ──────────────────────────────────────────────────────────
            var lblNome = new Label
            {
                Text = "Nome da fazenda: *",
                Left = 20,
                Top = 20,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            _txtNome = new TextBox
            {
                Left = 20,
                Top = 44,
                Width = 365,
                Font = new Font("Segoe UI", 10f),
                MaxLength = 60,
                PlaceholderText = "Ex: Fazenda Santa Cruz"
            };

            // ── Localização ───────────────────────────────────────────────────
            var lblLocal = new Label
            {
                Text = "Localização (opcional):",
                Left = 20,
                Top = 80,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            _txtLocal = new TextBox
            {
                Left = 20,
                Top = 104,
                Width = 365,
                Font = new Font("Segoe UI", 10f),
                MaxLength = 100,
                PlaceholderText = "Ex: Rua Serafim Vieira, São Carlos SP"
            };

            // ── Label de erro ─────────────────────────────────────────────────
            _lblErro = new Label
            {
                Left = 20,
                Top = 133,
                Width = 365,
                AutoSize = false,
                Height = 18,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 8.5f),
                Text = ""
            };

            // ── Botões ────────────────────────────────────────────────────────
            var btnCriar = new Button
            {
                Text = "Criar Fazenda",
                Left = 200,
                Top = 160,
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f)
            };
            btnCriar.FlatAppearance.BorderSize = 0;
            btnCriar.Click += BtnCriar_Click;

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 310,
                Top = 160,
                Width = 80,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f)
            };
            btnCancelar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // ── Montar ────────────────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblNome, _txtNome,
                lblLocal, _txtLocal,
                _lblErro,
                btnCriar, btnCancelar
            });

            this.AcceptButton = btnCriar;
            this.ActiveControl = _txtNome;
        }

        private void BtnCriar_Click(object sender, System.EventArgs e)
        {
            _lblErro.Text = "";

            // Validação via ValidacaoService (reutiliza regra já existente)
            var (valido, mensagem) = ValidacaoService.ValidarNomeFazenda(_txtNome.Text);
            if (!valido)
            {
                _lblErro.Text = mensagem;
                _txtNome.Focus();
                return;
            }

            NomeFazenda = _txtNome.Text.Trim();
            Localizacao = _txtLocal.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}