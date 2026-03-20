namespace Pecuaria_Digital.Views
{
    partial class FrmMenuFazendas
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
            pnlTopo = new Panel();
            lblTitulo = new Label();
            pnlCentro = new Panel();
            flpFazendas = new FlowLayoutPanel();
            pnlRodape = new Panel();
            pnlEstatisticas = new Panel();
            flpEstatisticas = new FlowLayoutPanel();
            pnlBotoes = new Panel();
            btnEditar = new Button();
            btnCriar = new Button();
            btnExcluir = new Button();
            btnAbrir = new Button();
            pnlTopo.SuspendLayout();
            pnlCentro.SuspendLayout();
            pnlRodape.SuspendLayout();
            pnlEstatisticas.SuspendLayout();
            pnlBotoes.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTopo
            // 
            pnlTopo.BackColor = Color.FromArgb(28, 87, 54);
            pnlTopo.Controls.Add(lblTitulo);
            pnlTopo.Dock = DockStyle.Top;
            pnlTopo.Location = new Point(0, 0);
            pnlTopo.Name = "pnlTopo";
            pnlTopo.Padding = new Padding(10, 0, 0, 0);
            pnlTopo.Size = new Size(684, 50);
            pnlTopo.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 12.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(10, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(352, 23);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = " Pecuária Digital — Selecione uma Fazenda";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlCentro
            // 
            pnlCentro.BackColor = Color.FromArgb(227, 227, 227);
            pnlCentro.Controls.Add(flpFazendas);
            pnlCentro.Dock = DockStyle.Fill;
            pnlCentro.Location = new Point(0, 50);
            pnlCentro.Name = "pnlCentro";
            pnlCentro.Padding = new Padding(10);
            pnlCentro.Size = new Size(684, 411);
            pnlCentro.TabIndex = 1;
            // 
            // flpFazendas
            // 
            flpFazendas.AutoScroll = true;
            flpFazendas.Dock = DockStyle.Fill;
            flpFazendas.Location = new Point(10, 10);
            flpFazendas.Name = "flpFazendas";
            flpFazendas.Padding = new Padding(5);
            flpFazendas.Size = new Size(664, 391);
            flpFazendas.TabIndex = 0;
            // 
            // pnlRodape
            // 
            pnlRodape.BackColor = Color.FromArgb(235, 235, 235);
            pnlRodape.Controls.Add(pnlEstatisticas);
            pnlRodape.Controls.Add(pnlBotoes);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Location = new Point(0, 331);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.Padding = new Padding(10, 8, 10, 8);
            pnlRodape.Size = new Size(684, 130);
            pnlRodape.TabIndex = 2;
            // 
            // pnlEstatisticas
            // 
            pnlEstatisticas.BackColor = Color.White;
            pnlEstatisticas.BorderStyle = BorderStyle.FixedSingle;
            pnlEstatisticas.Controls.Add(flpEstatisticas);
            pnlEstatisticas.Dock = DockStyle.Fill;
            pnlEstatisticas.Location = new Point(10, 8);
            pnlEstatisticas.Name = "pnlEstatisticas";
            pnlEstatisticas.Padding = new Padding(8, 4, 8, 4);
            pnlEstatisticas.Size = new Size(382, 114);
            pnlEstatisticas.TabIndex = 0;
            // 
            // flpEstatisticas
            // 
            flpEstatisticas.Dock = DockStyle.Fill;
            flpEstatisticas.Location = new Point(8, 4);
            flpEstatisticas.Name = "flpEstatisticas";
            flpEstatisticas.Size = new Size(364, 104);
            flpEstatisticas.TabIndex = 0;
            // 
            // pnlBotoes
            // 
            pnlBotoes.BackColor = Color.Transparent;
            pnlBotoes.Controls.Add(btnEditar);
            pnlBotoes.Controls.Add(btnCriar);
            pnlBotoes.Controls.Add(btnExcluir);
            pnlBotoes.Controls.Add(btnAbrir);
            pnlBotoes.Dock = DockStyle.Right;
            pnlBotoes.Location = new Point(392, 8);
            pnlBotoes.Name = "pnlBotoes";
            pnlBotoes.Size = new Size(282, 114);
            pnlBotoes.TabIndex = 1;
            // 
            // btnEditar
            // 
            btnEditar.AutoSize = true;
            btnEditar.BackColor = Color.Indigo;
            btnEditar.FlatStyle = FlatStyle.Flat;
            btnEditar.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnEditar.ForeColor = Color.White;
            btnEditar.Location = new Point(99, 65);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(80, 34);
            btnEditar.TabIndex = 3;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = false;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnCriar
            // 
            btnCriar.AutoSize = true;
            btnCriar.BackColor = Color.ForestGreen;
            btnCriar.FlatStyle = FlatStyle.Flat;
            btnCriar.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnCriar.ForeColor = Color.White;
            btnCriar.Location = new Point(190, 23);
            btnCriar.Name = "btnCriar";
            btnCriar.Size = new Size(80, 34);
            btnCriar.TabIndex = 2;
            btnCriar.Text = "+ Criar";
            btnCriar.UseVisualStyleBackColor = false;
            btnCriar.Click += btnCriar_Click;
            // 
            // btnExcluir
            // 
            btnExcluir.BackColor = Color.White;
            btnExcluir.FlatStyle = FlatStyle.Flat;
            btnExcluir.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnExcluir.ForeColor = Color.Red;
            btnExcluir.Location = new Point(100, 23);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Size = new Size(80, 34);
            btnExcluir.TabIndex = 1;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = false;
            btnExcluir.Click += btnExcluir_Click;
            // 
            // btnAbrir
            // 
            btnAbrir.BackColor = Color.FromArgb(0, 122, 204);
            btnAbrir.FlatStyle = FlatStyle.Flat;
            btnAbrir.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnAbrir.ForeColor = Color.White;
            btnAbrir.Location = new Point(10, 23);
            btnAbrir.Name = "btnAbrir";
            btnAbrir.Size = new Size(80, 30);
            btnAbrir.TabIndex = 0;
            btnAbrir.Text = "Abrir";
            btnAbrir.UseVisualStyleBackColor = false;
            btnAbrir.Click += btnAbrir_Click;
            // 
            // FrmMenuFazendas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(684, 461);
            Controls.Add(pnlRodape);
            Controls.Add(pnlCentro);
            Controls.Add(pnlTopo);
            MaximumSize = new Size(700, 500);
            Name = "FrmMenuFazendas";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pecuária Digital - Fazendas";
            Load += FrmMenuFazendas_Load;
            pnlTopo.ResumeLayout(false);
            pnlTopo.PerformLayout();
            pnlCentro.ResumeLayout(false);
            pnlRodape.ResumeLayout(false);
            pnlEstatisticas.ResumeLayout(false);
            pnlBotoes.ResumeLayout(false);
            pnlBotoes.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel pnlTopo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlCentro;
        private System.Windows.Forms.FlowLayoutPanel flpFazendas;
        private System.Windows.Forms.Panel pnlRodape;
        private System.Windows.Forms.Panel pnlEstatisticas;
        private System.Windows.Forms.FlowLayoutPanel flpEstatisticas;
        private System.Windows.Forms.Panel pnlBotoes;
        private System.Windows.Forms.Button btnAbrir;
        private System.Windows.Forms.Button btnExcluir;
        private System.Windows.Forms.Button btnCriar;
        private Button btnEditar;
    }
}