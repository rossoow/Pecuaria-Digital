namespace Pecuaria_Digital.Views
{
    partial class FrmMenuIATFs
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
            gridProtocolos = new DataGridView();
            btnFiltroAtrasado = new Button();
            btnFiltroAvancado = new Button();
            btnFiltroEmDia = new Button();
            btnFiltroDiaIdeal = new Button();
            btnFiltroFinalizado = new Button();
            btnFiltroTodos = new Button();
            panel1 = new Panel();
            flpEstatProtocolo = new FlowLayoutPanel();
            label1 = new Label();
            grpEstatFazenda = new GroupBox();
            flpEstatFazenda = new FlowLayoutPanel();
            btnAbrir = new Button();
            btnExcluir = new Button();
            btnCriar = new Button();
            lblNomeFazenda = new Label();
            btnImprimir = new Button();
            btnExportar = new Button();
            btnImportar = new Button();
            ((System.ComponentModel.ISupportInitialize)gridProtocolos).BeginInit();
            panel1.SuspendLayout();
            grpEstatFazenda.SuspendLayout();
            SuspendLayout();
            // 
            // gridProtocolos
            // 
            gridProtocolos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridProtocolos.Location = new Point(12, 49);
            gridProtocolos.Name = "gridProtocolos";
            gridProtocolos.RowTemplate.Height = 25;
            gridProtocolos.Size = new Size(1781, 429);
            gridProtocolos.TabIndex = 0;
            gridProtocolos.CellDoubleClick += Grid_CellDoubleClick;
            gridProtocolos.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;
            gridProtocolos.SelectionChanged += Grid_SelectionChanged;
            // 
            // btnFiltroAtrasado
            // 
            btnFiltroAtrasado.Location = new Point(1637, 20);
            btnFiltroAtrasado.Name = "btnFiltroAtrasado";
            btnFiltroAtrasado.Size = new Size(75, 23);
            btnFiltroAtrasado.TabIndex = 1;
            btnFiltroAtrasado.Text = "ATRASADO";
            btnFiltroAtrasado.UseVisualStyleBackColor = true;
            btnFiltroAtrasado.Click += BtnFiltro_Click;
            // 
            // btnFiltroAvancado
            // 
            btnFiltroAvancado.Location = new Point(1718, 20);
            btnFiltroAvancado.Name = "btnFiltroAvancado";
            btnFiltroAvancado.Size = new Size(75, 23);
            btnFiltroAvancado.TabIndex = 2;
            btnFiltroAvancado.Text = "Avançado";
            btnFiltroAvancado.UseVisualStyleBackColor = true;
            btnFiltroAvancado.Click += BtnFiltro_Click;
            // 
            // btnFiltroEmDia
            // 
            btnFiltroEmDia.Location = new Point(1556, 20);
            btnFiltroEmDia.Name = "btnFiltroEmDia";
            btnFiltroEmDia.Size = new Size(75, 23);
            btnFiltroEmDia.TabIndex = 3;
            btnFiltroEmDia.Text = "EM DIA";
            btnFiltroEmDia.UseVisualStyleBackColor = true;
            btnFiltroEmDia.Click += BtnFiltro_Click;
            // 
            // btnFiltroDiaIdeal
            // 
            btnFiltroDiaIdeal.Location = new Point(1475, 20);
            btnFiltroDiaIdeal.Name = "btnFiltroDiaIdeal";
            btnFiltroDiaIdeal.Size = new Size(75, 23);
            btnFiltroDiaIdeal.TabIndex = 4;
            btnFiltroDiaIdeal.Text = "DIA IDEAL";
            btnFiltroDiaIdeal.UseVisualStyleBackColor = true;
            btnFiltroDiaIdeal.Click += BtnFiltro_Click;
            // 
            // btnFiltroFinalizado
            // 
            btnFiltroFinalizado.Location = new Point(1385, 20);
            btnFiltroFinalizado.Name = "btnFiltroFinalizado";
            btnFiltroFinalizado.Size = new Size(84, 23);
            btnFiltroFinalizado.TabIndex = 5;
            btnFiltroFinalizado.Text = "FINALIZADO";
            btnFiltroFinalizado.UseVisualStyleBackColor = true;
            btnFiltroFinalizado.Click += BtnFiltro_Click;
            // 
            // btnFiltroTodos
            // 
            btnFiltroTodos.Location = new Point(1295, 20);
            btnFiltroTodos.Name = "btnFiltroTodos";
            btnFiltroTodos.Size = new Size(84, 23);
            btnFiltroTodos.TabIndex = 6;
            btnFiltroTodos.Text = "TODOS";
            btnFiltroTodos.UseVisualStyleBackColor = true;
            btnFiltroTodos.Click += BtnFiltro_Click;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlLightLight;
            panel1.Controls.Add(flpEstatProtocolo);
            panel1.Location = new Point(12, 518);
            panel1.Name = "panel1";
            panel1.Size = new Size(676, 218);
            panel1.TabIndex = 7;
            // 
            // flpEstatProtocolo
            // 
            flpEstatProtocolo.AutoScroll = true;
            flpEstatProtocolo.Dock = DockStyle.Fill;
            flpEstatProtocolo.FlowDirection = FlowDirection.TopDown;
            flpEstatProtocolo.Location = new Point(0, 0);
            flpEstatProtocolo.Name = "flpEstatProtocolo";
            flpEstatProtocolo.Size = new Size(676, 218);
            flpEstatProtocolo.TabIndex = 13;
            flpEstatProtocolo.WrapContents = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(12, 490);
            label1.Name = "label1";
            label1.Size = new Size(106, 25);
            label1.TabIndex = 8;
            label1.Text = "Estatísticas";
            // 
            // grpEstatFazenda
            // 
            grpEstatFazenda.Controls.Add(flpEstatFazenda);
            grpEstatFazenda.Location = new Point(705, 490);
            grpEstatFazenda.Name = "grpEstatFazenda";
            grpEstatFazenda.Size = new Size(498, 246);
            grpEstatFazenda.TabIndex = 9;
            grpEstatFazenda.TabStop = false;
            grpEstatFazenda.Text = "Estatísticas da Fazenda";
            // 
            // flpEstatFazenda
            // 
            flpEstatFazenda.AutoScroll = true;
            flpEstatFazenda.Dock = DockStyle.Fill;
            flpEstatFazenda.FlowDirection = FlowDirection.TopDown;
            flpEstatFazenda.Location = new Point(3, 19);
            flpEstatFazenda.Name = "flpEstatFazenda";
            flpEstatFazenda.Size = new Size(492, 224);
            flpEstatFazenda.TabIndex = 0;
            flpEstatFazenda.WrapContents = false;
            // 
            // btnAbrir
            // 
            btnAbrir.Location = new Point(1389, 609);
            btnAbrir.Name = "btnAbrir";
            btnAbrir.Size = new Size(106, 36);
            btnAbrir.TabIndex = 1;
            btnAbrir.Text = "Abrir";
            btnAbrir.UseVisualStyleBackColor = true;
            btnAbrir.Click += btnAbrir_Click;
            // 
            // btnExcluir
            // 
            btnExcluir.Location = new Point(1504, 609);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Size = new Size(106, 36);
            btnExcluir.TabIndex = 10;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = true;
            btnExcluir.Click += btnExcluir_Click;
            // 
            // btnCriar
            // 
            btnCriar.Location = new Point(1616, 609);
            btnCriar.Name = "btnCriar";
            btnCriar.Size = new Size(106, 36);
            btnCriar.TabIndex = 11;
            btnCriar.Text = "Criar";
            btnCriar.UseVisualStyleBackColor = true;
            btnCriar.Click += btnCriar_Click;
            // 
            // lblNomeFazenda
            // 
            lblNomeFazenda.AutoSize = true;
            lblNomeFazenda.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblNomeFazenda.Location = new Point(12, 12);
            lblNomeFazenda.Name = "lblNomeFazenda";
            lblNomeFazenda.Size = new Size(188, 30);
            lblNomeFazenda.TabIndex = 12;
            lblNomeFazenda.Text = "Nome da Fazenda";
            // 
            // btnImprimir
            // 
            btnImprimir.Location = new Point(1616, 660);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(106, 36);
            btnImprimir.TabIndex = 15;
            btnImprimir.Text = "🖨 Imprimir";
            btnImprimir.UseVisualStyleBackColor = true;
            btnImprimir.Click += btnImprimir_Click;
            // 
            // btnExportar
            // 
            btnExportar.Location = new Point(1504, 660);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(106, 36);
            btnExportar.TabIndex = 14;
            btnExportar.Text = "⬆ Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnImportar
            // 
            btnImportar.Location = new Point(1389, 660);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new Size(106, 36);
            btnImportar.TabIndex = 13;
            btnImportar.Text = "⬇ Importar";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += btnImportar_Click;
            // 
            // FrmMenuIATFs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1805, 748);
            Controls.Add(btnImprimir);
            Controls.Add(btnExportar);
            Controls.Add(btnImportar);
            Controls.Add(lblNomeFazenda);
            Controls.Add(btnCriar);
            Controls.Add(btnExcluir);
            Controls.Add(btnAbrir);
            Controls.Add(grpEstatFazenda);
            Controls.Add(label1);
            Controls.Add(panel1);
            Controls.Add(btnFiltroTodos);
            Controls.Add(btnFiltroFinalizado);
            Controls.Add(btnFiltroDiaIdeal);
            Controls.Add(btnFiltroEmDia);
            Controls.Add(btnFiltroAvancado);
            Controls.Add(btnFiltroAtrasado);
            Controls.Add(gridProtocolos);
            Name = "FrmMenuIATFs";
            Text = "Form1";
            FormClosing += FrmMenuIATFs_FormClosing;
            Load += FrmMenuIATFs_Load;
            ((System.ComponentModel.ISupportInitialize)gridProtocolos).EndInit();
            panel1.ResumeLayout(false);
            grpEstatFazenda.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView gridProtocolos;
        private Button btnFiltroAtrasado;
        private Button btnFiltroAvancado;
        private Button btnFiltroEmDia;
        private Button btnFiltroDiaIdeal;
        private Button btnFiltroFinalizado;
        private Button btnFiltroTodos;
        private Panel panel1;
        private Label label1;
        private FlowLayoutPanel flpEstatProtocolo;
        private GroupBox grpEstatFazenda;
        private FlowLayoutPanel flpEstatFazenda;
        private Button btnAbrir;
        private Button btnExcluir;
        private Button btnCriar;
        private Label lblNomeFazenda;
        private Button btnImprimir;
        private Button btnExportar;
        private Button btnImportar;
    }
}