namespace Pecuaria_Digital
{
    partial class FrmGerenciadorArquivos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lstArquivos = new ListBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnAbrir = new Button();
            btnNovo = new Button();
            btnExluirArquivo = new Button();
            btnAbrirPasta = new Button();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lstArquivos
            // 
            lstArquivos.Dock = DockStyle.Fill;
            lstArquivos.Font = new Font("Segoe UI", 10F);
            lstArquivos.FormattingEnabled = true;
            lstArquivos.Location = new Point(0, 0);
            lstArquivos.Name = "lstArquivos";
            lstArquivos.Size = new Size(800, 450);
            lstArquivos.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnAbrir);
            flowLayoutPanel1.Controls.Add(btnNovo);
            flowLayoutPanel1.Controls.Add(btnExluirArquivo);
            flowLayoutPanel1.Controls.Add(btnAbrirPasta);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.Location = new Point(0, 350);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(800, 100);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // btnAbrir
            // 
            btnAbrir.Location = new Point(3, 3);
            btnAbrir.Name = "btnAbrir";
            btnAbrir.Size = new Size(75, 23);
            btnAbrir.TabIndex = 2;
            btnAbrir.Text = "Abrir";
            btnAbrir.UseVisualStyleBackColor = true;
            btnAbrir.Visible = false;
            btnAbrir.Click += btnAbrir_Click_1;
            // 
            // btnNovo
            // 
            btnNovo.Location = new Point(84, 3);
            btnNovo.Name = "btnNovo";
            btnNovo.Size = new Size(75, 23);
            btnNovo.TabIndex = 3;
            btnNovo.Text = "Novo";
            btnNovo.UseVisualStyleBackColor = true;
            btnNovo.Click += btnNovo_Click_1;
            // 
            // btnExluirArquivo
            // 
            btnExluirArquivo.ForeColor = Color.Red;
            btnExluirArquivo.Location = new Point(165, 3);
            btnExluirArquivo.Name = "btnExluirArquivo";
            btnExluirArquivo.Size = new Size(75, 23);
            btnExluirArquivo.TabIndex = 4;
            btnExluirArquivo.Text = "Excluir";
            btnExluirArquivo.UseVisualStyleBackColor = true;
            btnExluirArquivo.Visible = false;
            btnExluirArquivo.Click += btnExluirArquivo_Click;
            // 
            // btnAbrirPasta
            // 
            btnAbrirPasta.Location = new Point(246, 3);
            btnAbrirPasta.Name = "btnAbrirPasta";
            btnAbrirPasta.Size = new Size(137, 23);
            btnAbrirPasta.TabIndex = 5;
            btnAbrirPasta.Text = "📂 Abrir Tabela";
            btnAbrirPasta.UseVisualStyleBackColor = true;
            btnAbrirPasta.Click += btnAbrirPasta_Click;
            // 
            // FrmGerenciadorArquivos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(lstArquivos);
            Name = "FrmGerenciadorArquivos";
            Text = "FrmGerenciadorArquivos";
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion

        private ListBox lstArquivos;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnAbrir;
        private Button btnNovo;
        private Button btnExluirArquivo;
        private Button btnAbrirPasta;
    }
}