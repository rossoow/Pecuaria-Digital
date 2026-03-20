using System.Drawing;
using System.Windows.Forms;

namespace Pecuaria_Digital.Views
{
    /// <summary>
    /// Diálogo simples que pergunta o modo da nova tabela antes de criá-la.
    /// </summary>
    public class FrmEscolhaModo : Form
    {
        public bool ModoCompleto { get; private set; } = false;

        public FrmEscolhaModo()
        {
            Text = "Novo Protocolo — Escolha o Modo";
            Width = 420;
            Height = 220;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            ConstruirControles();
        }

        private void ConstruirControles()
        {
            var lblTitulo = new Label
            {
                Text = "Como deseja preencher esta tabela?",
                Left = 20,
                Top = 18,
                Width = 370,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                AutoSize = false
            };

            // ── Opção: Por Etapas ─────────────────────────────────────────────
            var rbEtapas = new RadioButton
            {
                Text = "Por Etapas  (recomendado)",
                Left = 20,
                Top = 50,
                Width = 360,
                Checked = true,
                Font = new Font("Segoe UI", 9.5f)
            };
            var lblEtapas = new Label
            {
                Text = "Libera D0 → D8 → IATF → DG conforme você avança.",
                Left = 40,
                Top = 72,
                Width = 340,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false
            };

            // ── Opção: Completa ───────────────────────────────────────────────
            var rbCompleta = new RadioButton
            {
                Text = "Completa",
                Left = 20,
                Top = 98,
                Width = 360,
                Font = new Font("Segoe UI", 9.5f)
            };
            var lblCompleta = new Label
            {
                Text = "Todos os campos visíveis desde o início.",
                Left = 40,
                Top = 120,
                Width = 340,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false
            };

            // ── Botões ────────────────────────────────────────────────────────
            var btnOk = new Button
            {
                Text = "Criar",
                Left = 220,
                Top = 150,
                Width = 80,
                Height = 32,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                ModoCompleto = rbCompleta.Checked;
                Close();
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 310,
                Top = 150,
                Width = 80,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                lblTitulo,
                rbEtapas,   lblEtapas,
                rbCompleta, lblCompleta,
                btnOk, btnCancelar
            });

            AcceptButton = btnOk;
        }
    }
}