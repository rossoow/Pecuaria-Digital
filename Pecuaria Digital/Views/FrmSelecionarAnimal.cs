using Pecuaria_Digital.Constants;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Pecuaria_Digital.Views
{
    /// <summary>
    /// Exibido quando há IDs repetidos na tabela.
    /// O usuário escolhe qual linha recebe os dados da etapa atual.
    /// </summary>
    public class FrmSelecionarAnimal : Form
    {
        public DataGridViewRow LinhaSelecionada { get; private set; }

        public FrmSelecionarAnimal(string id, List<DataGridViewRow> linhas)
        {
            Text = $"Selecionar animal — Brinco {id}";
            Width = 620;
            Height = 300;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;

            ConstruirControles(id, linhas);
        }

        private void ConstruirControles(string id, List<DataGridViewRow> linhas)
        {
            var lbl = new Label
            {
                Text = $"Há {linhas.Count} animais com o brinco '{id}'.\n" +
                           "Selecione em qual deles os dados serão adicionados.\n" +
                           "(Use a coluna 'Observações' para identificá-los.)",
                Left = 12,
                Top = 12,
                Width = 580,
                Height = 48,
                Font = new Font("Segoe UI", 9.5f)
            };

            var grid = new DataGridView
            {
                Left = 12,
                Top = 68,
                Width = 580,
                Height = 150,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && grid.CurrentRow != null)
                {
                    LinhaSelecionada = grid.CurrentRow.Tag as DataGridViewRow;
                    this.DialogResult = DialogResult.OK;
                    Close();
                    e.Handled = true;
                }
            };

            // Colunas que ajudam a distinguir os animais
            foreach (var (nome, titulo) in new[]
            {
                (NomesColunasGrid.Id,         "Brinco"),
                (NomesColunasGrid.Categoria,  "Categoria"),
                (NomesColunasGrid.Raca,       "Raça"),
                (NomesColunasGrid.Ecc,        "ECC"),
                (NomesColunasGrid.Lote,       "Lote"),
                (NomesColunasGrid.DataD0,     "Data D0"),
                (NomesColunasGrid.Obs,        "Observações"),  // ← campo-chave
            })
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = nome, HeaderText = titulo });
            }

            foreach (var row in linhas)
            {
                int idx = grid.Rows.Add(
                    row.Cells[NomesColunasGrid.Id].Value,
                    row.Cells[NomesColunasGrid.Categoria].Value,
                    row.Cells[NomesColunasGrid.Raca].Value,
                    row.Cells[NomesColunasGrid.Ecc].Value,
                    row.Cells[NomesColunasGrid.Lote].Value,
                    row.Cells[NomesColunasGrid.DataD0].Value,
                    row.Cells[NomesColunasGrid.Obs].Value
                );
                grid.Rows[idx].Tag = row; // guarda a linha original
            }

            if (grid.Rows.Count > 0)
            {
                grid.Rows[0].Selected = true;
                grid.CurrentCell = grid.Rows[0].Cells[0];
            }

            var btnOk = new Button
            {
                Text = "Selecionar",
                Left = 400,
                Top = 228,
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (grid.CurrentRow == null) return;
                LinhaSelecionada = grid.CurrentRow.Tag as DataGridViewRow;
                Close();
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 500,
                Top = 228,
                Width = 80,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lbl, grid, btnOk, btnCancelar });
            AcceptButton = btnOk;
        }
    }
}