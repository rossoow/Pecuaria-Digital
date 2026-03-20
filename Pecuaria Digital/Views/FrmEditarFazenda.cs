using Pecuaria_Digital.Models;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pecuaria_Digital.Views
{
    /// <summary>
    /// Diálogo para editar nome, localização e imagem da fazenda.
    /// </summary>
    public class FrmEditarFazenda : Form
    {
        public string NovoNome { get; private set; }
        public string NovaLocalizacao { get; private set; }
        public string NovoCaminhoImagem { get; private set; }

        private TextBox _txtNome;
        private TextBox _txtLocal;
        private PictureBox _pic;
        private string _caminhoImagemTemp;

        public FrmEditarFazenda(FazendaData fazenda)
        {
            Text = $"Editar — {fazenda.Nome}";
            Width = 440;
            Height = 380;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            // Valores iniciais
            NovoNome = fazenda.Nome;
            NovaLocalizacao = fazenda.Localizacao;
            NovoCaminhoImagem = fazenda.CaminhoImagem;
            _caminhoImagemTemp = fazenda.CaminhoImagem;

            ConstruirControles(fazenda);
        }

        private void ConstruirControles(FazendaData fazenda)
        {
            // ── Nome ──────────────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "Nome da fazenda:",
                Left = 20,
                Top = 20,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            });
            _txtNome = new TextBox
            {
                Left = 20,
                Top = 44,
                Width = 385,
                Text = fazenda.Nome,
                Font = new Font("Segoe UI", 10f)
            };
            Controls.Add(_txtNome);

            // ── Localização ───────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "Localização:",
                Left = 20,
                Top = 80,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            });
            _txtLocal = new TextBox
            {
                Left = 20,
                Top = 104,
                Width = 385,
                Text = fazenda.Localizacao,
                Font = new Font("Segoe UI", 10f)
            };
            Controls.Add(_txtLocal);

            // ── Imagem ────────────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "Imagem do card:",
                Left = 20,
                Top = 145,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            });

            _pic = new PictureBox
            {
                Left = 20,
                Top = 168,
                Width = 100,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            CarregarImagemNoPic(fazenda.CaminhoImagem);
            Controls.Add(_pic);

            var btnEscolherImagem = new Button
            {
                Text = "📂 Escolher imagem...",
                Left = 135,
                Top = 168,
                Width = 150,
                Height = 32,
                FlatStyle = FlatStyle.Flat
            };
            btnEscolherImagem.Click += BtnEscolherImagem_Click;
            Controls.Add(btnEscolherImagem);

            var btnRemoverImagem = new Button
            {
                Text = "✕ Remover imagem",
                Left = 135,
                Top = 210,
                Width = 150,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Red
            };
            btnRemoverImagem.Click += (s, e) =>
            {
                _caminhoImagemTemp = "";
                _pic.Image = null;
                _pic.BackColor = Color.FromArgb(200, 200, 200);
            };
            Controls.Add(btnRemoverImagem);

            // ── Botões OK / Cancelar ──────────────────────────────────────────
            var btnSalvar = new Button
            {
                Text = "Salvar",
                Left = 230,
                Top = 305,
                Width = 80,
                Height = 34,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += (s, e) =>
            {
                NovoNome = _txtNome.Text.Trim();
                NovaLocalizacao = _txtLocal.Text.Trim();
                NovoCaminhoImagem = _caminhoImagemTemp;
                Close();
            };
            Controls.Add(btnSalvar);

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 320,
                Top = 305,
                Width = 80,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.Click += (s, e) => Close();
            Controls.Add(btnCancelar);

            AcceptButton = btnSalvar;
        }

        private void BtnEscolherImagem_Click(object sender, System.EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Escolher imagem para o card",
                Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            _caminhoImagemTemp = ofd.FileName;
            CarregarImagemNoPic(ofd.FileName);
        }

        private void CarregarImagemNoPic(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho))
            {
                _pic.Image = null;
                return;
            }
            try
            {
                // ✅ Bitmap carrega tudo em memória — sem dependência do stream
                _pic.Image = new Bitmap(caminho);
                _pic.BackColor = Color.White;
            }
            catch { _pic.Image = null; }
        }
    }
}