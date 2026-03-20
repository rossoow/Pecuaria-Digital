namespace Pecuaria_Digital
{
    partial class FrmMenuTabelas
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabela = new DataGridView();
            numero = new DataGridViewTextBoxColumn();
            PV = new DataGridViewTextBoxColumn();
            Escore = new DataGridViewTextBoxColumn();
            Touro = new DataGridViewTextBoxColumn();
            Categoria = new DataGridViewTextBoxColumn();
            Raça = new DataGridViewTextBoxColumn();
            observacao = new DataGridViewTextBoxColumn();
            _numero = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            _ecc = new TextBox();
            inserir = new Button();
            _observacoes = new TextBox();
            _cbResultadoDG = new ComboBox();
            label6 = new Label();
            _categoria = new ComboBox();
            _touro = new ComboBox();
            _raca = new ComboBox();
            btnExcluir = new Button();
            btnTesteD0 = new Button();
            btnTesteD8 = new Button();
            btnTesteIATF = new Button();
            btnTesteDG = new Button();
            pnlD0 = new GroupBox();
            label26 = new Label();
            _lote = new ComboBox();
            _dtpD0 = new DateTimePicker();
            label13 = new Label();
            label12 = new Label();
            label11 = new Label();
            label9 = new Label();
            pnlDG = new GroupBox();
            _txtDestino = new ComboBox();
            _dtpDG = new DateTimePicker();
            _txtOvario = new ComboBox();
            label15 = new Label();
            label28 = new Label();
            label14 = new Label();
            pnlD8 = new GroupBox();
            label27 = new Label();
            _chkEcg = new CheckBox();
            _dtpD8 = new DateTimePicker();
            _chkPerdeuImplante = new CheckBox();
            pnlIATF = new GroupBox();
            _txtEscoreCio = new TextBox();
            _dtpIATF = new DateTimePicker();
            _inseminador = new ComboBox();
            I = new Label();
            label25 = new Label();
            label10 = new Label();
            _chkGnrh = new CheckBox();
            btnAvancar = new Button();
            btnVoltar = new Button();
            pnlContainerInsercao = new Panel();
            pnlBotoes = new Panel();
            pnlObs = new Panel();
            contextMenuStrip1 = new ContextMenuStrip(components);
            grpAjuda = new GroupBox();
            lblAjudaTitulo = new Label();
            lblAjudaDescricao = new Label();
            grpFiltros = new GroupBox();
            flowPainelMestre = new FlowLayoutPanel();
            flowAreaDeFiltros = new FlowLayoutPanel();
            pnlFiltroD0 = new Panel();
            label7 = new Label();
            _cmbFiltroRaca = new ComboBox();
            label5 = new Label();
            _cmbFiltroCategoria = new ComboBox();
            label4 = new Label();
            _cmbOperadorECC = new ComboBox();
            _txtFiltroValorECC = new TextBox();
            pnlFiltroD8 = new Panel();
            _chkFiltroPerdeuImplante = new CheckBox();
            _chkFiltroEcg = new CheckBox();
            pnlFiltroIATF = new Panel();
            label17 = new Label();
            label16 = new Label();
            label8 = new Label();
            _cmbOperadorCio = new ComboBox();
            _txtFiltroValorCio = new TextBox();
            _cmbFiltroTouro = new ComboBox();
            _cmbFiltroInseminador = new ComboBox();
            pnlFiltroDG = new Panel();
            panel1 = new Panel();
            label19 = new Label();
            _rbFiltroVazia = new RadioButton();
            _rbFiltroPrenha = new RadioButton();
            _rbFiltroTodos = new RadioButton();
            label18 = new Label();
            _cmbFiltroDestino = new ComboBox();
            pnlBotoesContainer = new Panel();
            btnLimparFiltros = new Button();
            btnFiltrar = new Button();
            grpInserir = new GroupBox();
            pnlNumero = new Panel();
            grpEtapas = new GroupBox();
            groupBox1 = new GroupBox();
            label24 = new Label();
            label23 = new Label();
            label22 = new Label();
            label21 = new Label();
            label20 = new Label();
            lblStatTotal = new Label();
            lblStatDoses = new Label();
            lblStatServico = new Label();
            lblStatPrenhez = new Label();
            lblStatConcepcao = new Label();
            lblMediaD0 = new Label();
            lblMediaD8 = new Label();
            lblMediaIATF = new Label();
            lblMediaDG = new Label();
            grpDatas = new GroupBox();
            btnExportar = new Button();
            btnSalvar = new Button();
            btnConcluir = new Button();
            btnImprimir = new Button();
            ((System.ComponentModel.ISupportInitialize)tabela).BeginInit();
            pnlD0.SuspendLayout();
            pnlDG.SuspendLayout();
            pnlD8.SuspendLayout();
            pnlIATF.SuspendLayout();
            pnlContainerInsercao.SuspendLayout();
            pnlBotoes.SuspendLayout();
            pnlObs.SuspendLayout();
            grpAjuda.SuspendLayout();
            grpFiltros.SuspendLayout();
            flowPainelMestre.SuspendLayout();
            flowAreaDeFiltros.SuspendLayout();
            pnlFiltroD0.SuspendLayout();
            pnlFiltroD8.SuspendLayout();
            pnlFiltroIATF.SuspendLayout();
            pnlFiltroDG.SuspendLayout();
            panel1.SuspendLayout();
            pnlBotoesContainer.SuspendLayout();
            grpInserir.SuspendLayout();
            pnlNumero.SuspendLayout();
            grpEtapas.SuspendLayout();
            groupBox1.SuspendLayout();
            grpDatas.SuspendLayout();
            SuspendLayout();
            // 
            // tabela
            // 
            tabela.ColumnHeadersHeight = 34;
            tabela.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            tabela.Location = new Point(12, 12);
            tabela.MultiSelect = false;
            tabela.Name = "tabela";
            tabela.RowHeadersWidth = 62;
            tabela.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tabela.Size = new Size(1785, 367);
            tabela.TabIndex = 0;
            tabela.CellEndEdit += tabela_CellEndEdit;
            tabela.CellFormatting += tabela_CellFormatting;
            tabela.CellValueChanged += tabela_CellValueChanged;
            tabela.ColumnHeaderMouseClick += tabela_ColumnHeaderMouseClick;
            tabela.DataError += tabela_DataError;
            // 
            // numero
            // 
            numero.HeaderText = "Número";
            numero.MinimumWidth = 8;
            numero.Name = "numero";
            numero.Width = 150;
            // 
            // PV
            // 
            PV.HeaderText = "P/V";
            PV.MinimumWidth = 8;
            PV.Name = "PV";
            PV.Width = 150;
            // 
            // Escore
            // 
            Escore.HeaderText = "Escore";
            Escore.MinimumWidth = 8;
            Escore.Name = "Escore";
            Escore.Width = 150;
            // 
            // Touro
            // 
            Touro.HeaderText = "Touro";
            Touro.MinimumWidth = 8;
            Touro.Name = "Touro";
            Touro.Width = 150;
            // 
            // Categoria
            // 
            Categoria.HeaderText = "Categoria";
            Categoria.MinimumWidth = 8;
            Categoria.Name = "Categoria";
            Categoria.Width = 150;
            // 
            // Raça
            // 
            Raça.HeaderText = "Raça";
            Raça.MinimumWidth = 8;
            Raça.Name = "Raça";
            Raça.Width = 150;
            // 
            // observacao
            // 
            observacao.HeaderText = "Oberservações";
            observacao.MinimumWidth = 8;
            observacao.Name = "observacao";
            observacao.Width = 150;
            // 
            // _numero
            // 
            _numero.AcceptsTab = true;
            _numero.CausesValidation = false;
            _numero.Cursor = Cursors.IBeam;
            _numero.Location = new Point(66, 6);
            _numero.Name = "_numero";
            _numero.PlaceholderText = "(Obrigatório)";
            _numero.Size = new Size(85, 23);
            _numero.TabIndex = 1;
            _numero.KeyDown += _numero_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 9);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 3;
            label1.Text = "Número";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(28, 29);
            label2.Name = "label2";
            label2.Size = new Size(26, 15);
            label2.TabIndex = 4;
            label2.Text = "P/V";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 19);
            label3.Name = "label3";
            label3.Size = new Size(74, 15);
            label3.TabIndex = 8;
            label3.Text = "Observações";
            // 
            // _ecc
            // 
            _ecc.AcceptsReturn = true;
            _ecc.AcceptsTab = true;
            _ecc.Cursor = Cursors.IBeam;
            _ecc.Location = new Point(97, 86);
            _ecc.Name = "_ecc";
            _ecc.PlaceholderText = "(Obrigatório)";
            _ecc.Size = new Size(211, 23);
            _ecc.TabIndex = 4;
            _ecc.KeyDown += _textBox_KeyDown;
            // 
            // inserir
            // 
            inserir.Location = new Point(222, 3);
            inserir.Name = "inserir";
            inserir.Size = new Size(86, 23);
            inserir.TabIndex = 101;
            inserir.Text = "Inserir";
            inserir.UseVisualStyleBackColor = true;
            inserir.Click += inserir_Click;
            // 
            // _observacoes
            // 
            _observacoes.AcceptsReturn = true;
            _observacoes.AcceptsTab = true;
            _observacoes.Cursor = Cursors.IBeam;
            _observacoes.Location = new Point(100, 16);
            _observacoes.Name = "_observacoes";
            _observacoes.Size = new Size(337, 23);
            _observacoes.TabIndex = 100;
            _observacoes.KeyDown += _observacoes_KeyDown;
            // 
            // _cbResultadoDG
            // 
            _cbResultadoDG.AccessibleName = "";
            _cbResultadoDG.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbResultadoDG.FormattingEnabled = true;
            _cbResultadoDG.Items.AddRange(new object[] { "PRENHA", "VAZIO" });
            _cbResultadoDG.Location = new Point(60, 27);
            _cbResultadoDG.Name = "_cbResultadoDG";
            _cbResultadoDG.Size = new Size(211, 23);
            _cbResultadoDG.TabIndex = 2;
            _cbResultadoDG.KeyDown += _comboBox_KeyDown;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(56, 81);
            label6.Name = "label6";
            label6.Size = new Size(38, 15);
            label6.TabIndex = 12;
            label6.Text = "Touro";
            // 
            // _categoria
            // 
            _categoria.AutoCompleteMode = AutoCompleteMode.Append;
            _categoria.AutoCompleteSource = AutoCompleteSource.ListItems;
            _categoria.FormattingEnabled = true;
            _categoria.Location = new Point(97, 55);
            _categoria.Name = "_categoria";
            _categoria.Size = new Size(211, 23);
            _categoria.TabIndex = 3;
            _categoria.KeyDown += _comboBox_KeyDown;
            // 
            // _touro
            // 
            _touro.AutoCompleteMode = AutoCompleteMode.Append;
            _touro.AutoCompleteSource = AutoCompleteSource.ListItems;
            _touro.FormattingEnabled = true;
            _touro.Location = new Point(100, 75);
            _touro.Name = "_touro";
            _touro.Size = new Size(211, 23);
            _touro.TabIndex = 4;
            _touro.KeyDown += _comboBox_KeyDown;
            // 
            // _raca
            // 
            _raca.AutoCompleteMode = AutoCompleteMode.Append;
            _raca.AutoCompleteSource = AutoCompleteSource.ListItems;
            _raca.FormattingEnabled = true;
            _raca.Location = new Point(97, 26);
            _raca.Name = "_raca";
            _raca.Size = new Size(211, 23);
            _raca.TabIndex = 2;
            _raca.KeyDown += _comboBox_KeyDown;
            // 
            // btnExcluir
            // 
            btnExcluir.ForeColor = Color.Crimson;
            btnExcluir.Location = new Point(131, 3);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Size = new Size(86, 23);
            btnExcluir.TabIndex = 102;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = true;
            btnExcluir.Click += btnExcluir_Click;
            // 
            // btnTesteD0
            // 
            btnTesteD0.Location = new Point(64, 74);
            btnTesteD0.Name = "btnTesteD0";
            btnTesteD0.Size = new Size(75, 23);
            btnTesteD0.TabIndex = 105;
            btnTesteD0.Text = "D0";
            btnTesteD0.UseVisualStyleBackColor = true;
            btnTesteD0.Click += btnTesteD0_Click;
            // 
            // btnTesteD8
            // 
            btnTesteD8.Location = new Point(145, 74);
            btnTesteD8.Name = "btnTesteD8";
            btnTesteD8.Size = new Size(75, 23);
            btnTesteD8.TabIndex = 106;
            btnTesteD8.Text = "D8";
            btnTesteD8.UseVisualStyleBackColor = true;
            btnTesteD8.Click += btnTesteD8_Click;
            // 
            // btnTesteIATF
            // 
            btnTesteIATF.Location = new Point(226, 74);
            btnTesteIATF.Name = "btnTesteIATF";
            btnTesteIATF.Size = new Size(75, 23);
            btnTesteIATF.TabIndex = 107;
            btnTesteIATF.Text = "IATF";
            btnTesteIATF.UseVisualStyleBackColor = true;
            btnTesteIATF.Click += btnTesteIATF_Click;
            // 
            // btnTesteDG
            // 
            btnTesteDG.Location = new Point(307, 74);
            btnTesteDG.Name = "btnTesteDG";
            btnTesteDG.Size = new Size(75, 23);
            btnTesteDG.TabIndex = 108;
            btnTesteDG.Text = "DG";
            btnTesteDG.UseVisualStyleBackColor = true;
            btnTesteDG.Click += btnTesteDG_Click;
            // 
            // pnlD0
            // 
            pnlD0.AutoSize = true;
            pnlD0.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlD0.Controls.Add(label26);
            pnlD0.Controls.Add(_lote);
            pnlD0.Controls.Add(_dtpD0);
            pnlD0.Controls.Add(label13);
            pnlD0.Controls.Add(label12);
            pnlD0.Controls.Add(label11);
            pnlD0.Controls.Add(label9);
            pnlD0.Controls.Add(_raca);
            pnlD0.Controls.Add(_categoria);
            pnlD0.Controls.Add(_ecc);
            pnlD0.Location = new Point(0, 476);
            pnlD0.Name = "pnlD0";
            pnlD0.Size = new Size(314, 192);
            pnlD0.TabIndex = 26;
            pnlD0.TabStop = false;
            pnlD0.Text = "D0";
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(8, 150);
            label26.Name = "label26";
            label26.Size = new Size(88, 15);
            label26.TabIndex = 117;
            label26.Text = "Data Início (D0)";
            label26.Click += label26_Click;
            // 
            // _lote
            // 
            _lote.AutoCompleteMode = AutoCompleteMode.Append;
            _lote.AutoCompleteSource = AutoCompleteSource.ListItems;
            _lote.FormattingEnabled = true;
            _lote.Location = new Point(97, 118);
            _lote.Name = "_lote";
            _lote.Size = new Size(211, 23);
            _lote.TabIndex = 5;
            _lote.KeyDown += _comboBox_KeyDown;
            // 
            // _dtpD0
            // 
            _dtpD0.CustomFormat = "dd/MM/yyyy HH:mm";
            _dtpD0.Format = DateTimePickerFormat.Custom;
            _dtpD0.Location = new Point(98, 147);
            _dtpD0.Name = "_dtpD0";
            _dtpD0.Size = new Size(210, 23);
            _dtpD0.TabIndex = 6;
            _dtpD0.ValueChanged += _dtpD0_ValueChanged;
            _dtpD0.KeyDown += _textBox_KeyDown;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(59, 118);
            label13.Name = "label13";
            label13.Size = new Size(30, 15);
            label13.TabIndex = 4;
            label13.Text = "Lote";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(30, 86);
            label12.Name = "label12";
            label12.Size = new Size(63, 15);
            label12.TabIndex = 3;
            label12.Text = "ECC Inicial";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(35, 57);
            label11.Name = "label11";
            label11.Size = new Size(58, 15);
            label11.TabIndex = 2;
            label11.Text = "Categoria";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(62, 26);
            label9.Name = "label9";
            label9.Size = new Size(32, 15);
            label9.TabIndex = 1;
            label9.Text = "Raça";
            // 
            // pnlDG
            // 
            pnlDG.AutoSize = true;
            pnlDG.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlDG.Controls.Add(_txtDestino);
            pnlDG.Controls.Add(_dtpDG);
            pnlDG.Controls.Add(_txtOvario);
            pnlDG.Controls.Add(label15);
            pnlDG.Controls.Add(label28);
            pnlDG.Controls.Add(label14);
            pnlDG.Controls.Add(_cbResultadoDG);
            pnlDG.Controls.Add(label2);
            pnlDG.Location = new Point(0, 0);
            pnlDG.Name = "pnlDG";
            pnlDG.Size = new Size(333, 178);
            pnlDG.TabIndex = 29;
            pnlDG.TabStop = false;
            pnlDG.Text = "DG";
            // 
            // _txtDestino
            // 
            _txtDestino.AccessibleName = "";
            _txtDestino.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _txtDestino.AutoCompleteSource = AutoCompleteSource.ListItems;
            _txtDestino.FormattingEnabled = true;
            _txtDestino.Location = new Point(60, 91);
            _txtDestino.Name = "_txtDestino";
            _txtDestino.Size = new Size(211, 23);
            _txtDestino.TabIndex = 4;
            _txtDestino.KeyDown += _comboBox_KeyDown;
            // 
            // _dtpDG
            // 
            _dtpDG.CustomFormat = "dd/MM/yyyy HH:mm";
            _dtpDG.Format = DateTimePickerFormat.Custom;
            _dtpDG.Location = new Point(117, 133);
            _dtpDG.Name = "_dtpDG";
            _dtpDG.Size = new Size(210, 23);
            _dtpDG.TabIndex = 5;
            _dtpDG.KeyDown += _textBox_KeyDown;
            // 
            // _txtOvario
            // 
            _txtOvario.AccessibleName = "";
            _txtOvario.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _txtOvario.AutoCompleteSource = AutoCompleteSource.ListItems;
            _txtOvario.FormattingEnabled = true;
            _txtOvario.Location = new Point(60, 59);
            _txtOvario.Name = "_txtOvario";
            _txtOvario.Size = new Size(211, 23);
            _txtOvario.TabIndex = 3;
            _txtOvario.KeyDown += _comboBox_KeyDown;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(9, 96);
            label15.Name = "label15";
            label15.Size = new Size(47, 15);
            label15.TabIndex = 32;
            label15.Text = "Destino";
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(14, 138);
            label28.Name = "label28";
            label28.Size = new Size(97, 15);
            label28.TabIndex = 119;
            label28.Text = "Data Diagnóstico";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(14, 62);
            label14.Name = "label14";
            label14.Size = new Size(42, 15);
            label14.TabIndex = 30;
            label14.Text = "Ovario";
            // 
            // pnlD8
            // 
            pnlD8.AutoSize = true;
            pnlD8.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlD8.Controls.Add(label27);
            pnlD8.Controls.Add(_chkEcg);
            pnlD8.Controls.Add(_dtpD8);
            pnlD8.Controls.Add(_chkPerdeuImplante);
            pnlD8.Location = new Point(0, 356);
            pnlD8.Name = "pnlD8";
            pnlD8.Size = new Size(351, 120);
            pnlD8.TabIndex = 27;
            pnlD8.TabStop = false;
            pnlD8.Text = "D8";
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(27, 81);
            label27.Name = "label27";
            label27.Size = new Size(102, 15);
            label27.TabIndex = 118;
            label27.Text = "Data Retirada (D8)";
            // 
            // _chkEcg
            // 
            _chkEcg.AutoSize = true;
            _chkEcg.Location = new Point(169, 38);
            _chkEcg.Name = "_chkEcg";
            _chkEcg.Size = new Size(45, 19);
            _chkEcg.TabIndex = 3;
            _chkEcg.Text = "Ecg";
            _chkEcg.UseVisualStyleBackColor = true;
            _chkEcg.KeyDown += _checkBox_KeyDown;
            // 
            // _dtpD8
            // 
            _dtpD8.CustomFormat = "dd/MM/yyyy HH:mm";
            _dtpD8.Format = DateTimePickerFormat.Custom;
            _dtpD8.Location = new Point(135, 75);
            _dtpD8.Name = "_dtpD8";
            _dtpD8.Size = new Size(210, 23);
            _dtpD8.TabIndex = 3;
            _dtpD8.ValueChanged += _dtpD8_ValueChanged;
            _dtpD8.KeyDown += _textBox_KeyDown;
            // 
            // _chkPerdeuImplante
            // 
            _chkPerdeuImplante.AutoSize = true;
            _chkPerdeuImplante.Location = new Point(27, 41);
            _chkPerdeuImplante.Name = "_chkPerdeuImplante";
            _chkPerdeuImplante.Size = new Size(113, 19);
            _chkPerdeuImplante.TabIndex = 2;
            _chkPerdeuImplante.Text = "Perdeu Implante";
            _chkPerdeuImplante.UseVisualStyleBackColor = true;
            _chkPerdeuImplante.KeyDown += _checkBox_KeyDown;
            // 
            // pnlIATF
            // 
            pnlIATF.AutoSize = true;
            pnlIATF.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlIATF.Controls.Add(_txtEscoreCio);
            pnlIATF.Controls.Add(_dtpIATF);
            pnlIATF.Controls.Add(_inseminador);
            pnlIATF.Controls.Add(I);
            pnlIATF.Controls.Add(label25);
            pnlIATF.Controls.Add(label10);
            pnlIATF.Controls.Add(_chkGnrh);
            pnlIATF.Controls.Add(_touro);
            pnlIATF.Controls.Add(label6);
            pnlIATF.Location = new Point(0, 178);
            pnlIATF.Name = "pnlIATF";
            pnlIATF.Size = new Size(333, 178);
            pnlIATF.TabIndex = 28;
            pnlIATF.TabStop = false;
            pnlIATF.Text = "IATF";
            // 
            // _txtEscoreCio
            // 
            _txtEscoreCio.Location = new Point(101, 18);
            _txtEscoreCio.Name = "_txtEscoreCio";
            _txtEscoreCio.Size = new Size(210, 23);
            _txtEscoreCio.TabIndex = 2;
            _txtEscoreCio.KeyDown += _textBox_KeyDown;
            // 
            // _dtpIATF
            // 
            _dtpIATF.CustomFormat = "dd/MM/yyyy HH:mm";
            _dtpIATF.Format = DateTimePickerFormat.Custom;
            _dtpIATF.Location = new Point(117, 133);
            _dtpIATF.Name = "_dtpIATF";
            _dtpIATF.Size = new Size(210, 23);
            _dtpIATF.TabIndex = 6;
            _dtpIATF.ValueChanged += _dtpIATF_ValueChanged;
            _dtpIATF.KeyDown += _textBox_KeyDown;
            // 
            // _inseminador
            // 
            _inseminador.AutoCompleteMode = AutoCompleteMode.Append;
            _inseminador.AutoCompleteSource = AutoCompleteSource.ListItems;
            _inseminador.FormattingEnabled = true;
            _inseminador.Location = new Point(100, 104);
            _inseminador.Name = "_inseminador";
            _inseminador.Size = new Size(211, 23);
            _inseminador.TabIndex = 5;
            _inseminador.KeyDown += _comboBox_KeyDown;
            // 
            // I
            // 
            I.AutoSize = true;
            I.Location = new Point(21, 107);
            I.Name = "I";
            I.Size = new Size(73, 15);
            I.TabIndex = 14;
            I.Text = "Inseminador";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(3, 139);
            label25.Name = "label25";
            label25.Size = new Size(101, 15);
            label25.TabIndex = 116;
            label25.Text = "Data Inseminação";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(33, 23);
            label10.Name = "label10";
            label10.Size = new Size(62, 15);
            label10.TabIndex = 10;
            label10.Text = "Escore Cio";
            // 
            // _chkGnrh
            // 
            _chkGnrh.AutoSize = true;
            _chkGnrh.Location = new Point(101, 48);
            _chkGnrh.Name = "_chkGnrh";
            _chkGnrh.Size = new Size(59, 19);
            _chkGnrh.TabIndex = 3;
            _chkGnrh.Text = "GNRH";
            _chkGnrh.UseVisualStyleBackColor = true;
            _chkGnrh.KeyDown += _checkBox_KeyDown;
            // 
            // btnAvancar
            // 
            btnAvancar.Location = new Point(226, 20);
            btnAvancar.Name = "btnAvancar";
            btnAvancar.Size = new Size(91, 48);
            btnAvancar.TabIndex = 104;
            btnAvancar.Text = "Avançar ▶️";
            btnAvancar.UseVisualStyleBackColor = true;
            btnAvancar.Click += btnAvancar_Click;
            // 
            // btnVoltar
            // 
            btnVoltar.Location = new Point(135, 20);
            btnVoltar.Name = "btnVoltar";
            btnVoltar.Size = new Size(85, 48);
            btnVoltar.TabIndex = 103;
            btnVoltar.Text = "◀️ Voltar";
            btnVoltar.UseVisualStyleBackColor = true;
            btnVoltar.Click += btnVoltar_Click;
            // 
            // pnlContainerInsercao
            // 
            pnlContainerInsercao.AutoScroll = true;
            pnlContainerInsercao.AutoSize = true;
            pnlContainerInsercao.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlContainerInsercao.Controls.Add(pnlBotoes);
            pnlContainerInsercao.Controls.Add(pnlObs);
            pnlContainerInsercao.Controls.Add(pnlD0);
            pnlContainerInsercao.Controls.Add(pnlD8);
            pnlContainerInsercao.Controls.Add(pnlIATF);
            pnlContainerInsercao.Controls.Add(pnlDG);
            pnlContainerInsercao.Dock = DockStyle.Top;
            pnlContainerInsercao.Location = new Point(3, 51);
            pnlContainerInsercao.MaximumSize = new Size(460, 590);
            pnlContainerInsercao.Name = "pnlContainerInsercao";
            pnlContainerInsercao.Size = new Size(460, 590);
            pnlContainerInsercao.TabIndex = 2;
            // 
            // pnlBotoes
            // 
            pnlBotoes.Controls.Add(btnExcluir);
            pnlBotoes.Controls.Add(inserir);
            pnlBotoes.Dock = DockStyle.Bottom;
            pnlBotoes.Location = new Point(0, 668);
            pnlBotoes.Name = "pnlBotoes";
            pnlBotoes.Size = new Size(443, 33);
            pnlBotoes.TabIndex = 123;
            // 
            // pnlObs
            // 
            pnlObs.Controls.Add(_observacoes);
            pnlObs.Controls.Add(label3);
            pnlObs.Dock = DockStyle.Bottom;
            pnlObs.Location = new Point(0, 701);
            pnlObs.Name = "pnlObs";
            pnlObs.Size = new Size(443, 54);
            pnlObs.TabIndex = 122;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // grpAjuda
            // 
            grpAjuda.BackColor = Color.WhiteSmoke;
            grpAjuda.Controls.Add(lblAjudaTitulo);
            grpAjuda.Controls.Add(lblAjudaDescricao);
            grpAjuda.Location = new Point(487, 504);
            grpAjuda.Name = "grpAjuda";
            grpAjuda.Size = new Size(471, 164);
            grpAjuda.TabIndex = 109;
            grpAjuda.TabStop = false;
            grpAjuda.Text = "Ajuda Contextual";
            // 
            // lblAjudaTitulo
            // 
            lblAjudaTitulo.AutoSize = true;
            lblAjudaTitulo.Dock = DockStyle.Left;
            lblAjudaTitulo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblAjudaTitulo.Location = new Point(3, 19);
            lblAjudaTitulo.Name = "lblAjudaTitulo";
            lblAjudaTitulo.Size = new Size(131, 15);
            lblAjudaTitulo.TabIndex = 0;
            lblAjudaTitulo.Text = "Selecione um campo...";
            // 
            // lblAjudaDescricao
            // 
            lblAjudaDescricao.Location = new Point(3, 34);
            lblAjudaDescricao.Name = "lblAjudaDescricao";
            lblAjudaDescricao.Size = new Size(461, 117);
            lblAjudaDescricao.TabIndex = 1;
            lblAjudaDescricao.Text = "...";
            // 
            // grpFiltros
            // 
            grpFiltros.AutoSize = true;
            grpFiltros.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpFiltros.BackColor = Color.WhiteSmoke;
            grpFiltros.Controls.Add(flowPainelMestre);
            grpFiltros.Location = new Point(973, 395);
            grpFiltros.MaximumSize = new Size(468, 0);
            grpFiltros.MinimumSize = new Size(468, 0);
            grpFiltros.Name = "grpFiltros";
            grpFiltros.Size = new Size(468, 434);
            grpFiltros.TabIndex = 110;
            grpFiltros.TabStop = false;
            grpFiltros.Text = "Filtros";
            // 
            // flowPainelMestre
            // 
            flowPainelMestre.AutoSize = true;
            flowPainelMestre.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowPainelMestre.Controls.Add(flowAreaDeFiltros);
            flowPainelMestre.Controls.Add(pnlBotoesContainer);
            flowPainelMestre.Dock = DockStyle.Fill;
            flowPainelMestre.FlowDirection = FlowDirection.TopDown;
            flowPainelMestre.Location = new Point(3, 19);
            flowPainelMestre.MaximumSize = new Size(458, 0);
            flowPainelMestre.MinimumSize = new Size(458, 0);
            flowPainelMestre.Name = "flowPainelMestre";
            flowPainelMestre.Size = new Size(458, 412);
            flowPainelMestre.TabIndex = 0;
            flowPainelMestre.WrapContents = false;
            // 
            // flowAreaDeFiltros
            // 
            flowAreaDeFiltros.AutoSize = true;
            flowAreaDeFiltros.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowAreaDeFiltros.Controls.Add(pnlFiltroD0);
            flowAreaDeFiltros.Controls.Add(pnlFiltroD8);
            flowAreaDeFiltros.Controls.Add(pnlFiltroIATF);
            flowAreaDeFiltros.Controls.Add(pnlFiltroDG);
            flowAreaDeFiltros.Location = new Point(3, 3);
            flowAreaDeFiltros.MinimumSize = new Size(468, 0);
            flowAreaDeFiltros.Name = "flowAreaDeFiltros";
            flowAreaDeFiltros.Size = new Size(468, 363);
            flowAreaDeFiltros.TabIndex = 0;
            // 
            // pnlFiltroD0
            // 
            pnlFiltroD0.BackColor = Color.Transparent;
            pnlFiltroD0.Controls.Add(label7);
            pnlFiltroD0.Controls.Add(_cmbFiltroRaca);
            pnlFiltroD0.Controls.Add(label5);
            pnlFiltroD0.Controls.Add(_cmbFiltroCategoria);
            pnlFiltroD0.Controls.Add(label4);
            pnlFiltroD0.Controls.Add(_cmbOperadorECC);
            pnlFiltroD0.Controls.Add(_txtFiltroValorECC);
            pnlFiltroD0.Location = new Point(3, 3);
            pnlFiltroD0.Name = "pnlFiltroD0";
            pnlFiltroD0.Size = new Size(310, 98);
            pnlFiltroD0.TabIndex = 0;
            pnlFiltroD0.Visible = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(38, 70);
            label7.Name = "label7";
            label7.Size = new Size(32, 15);
            label7.TabIndex = 113;
            label7.Text = "ECC:";
            // 
            // _cmbFiltroRaca
            // 
            _cmbFiltroRaca.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFiltroRaca.FormattingEnabled = true;
            _cmbFiltroRaca.Location = new Point(76, 8);
            _cmbFiltroRaca.Name = "_cmbFiltroRaca";
            _cmbFiltroRaca.Size = new Size(121, 23);
            _cmbFiltroRaca.TabIndex = 0;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(9, 40);
            label5.Name = "label5";
            label5.Size = new Size(61, 15);
            label5.TabIndex = 112;
            label5.Text = "Categoria:";
            // 
            // _cmbFiltroCategoria
            // 
            _cmbFiltroCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFiltroCategoria.FormattingEnabled = true;
            _cmbFiltroCategoria.Location = new Point(76, 37);
            _cmbFiltroCategoria.Name = "_cmbFiltroCategoria";
            _cmbFiltroCategoria.Size = new Size(121, 23);
            _cmbFiltroCategoria.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(35, 11);
            label4.Name = "label4";
            label4.Size = new Size(35, 15);
            label4.TabIndex = 111;
            label4.Text = "Raça:";
            // 
            // _cmbOperadorECC
            // 
            _cmbOperadorECC.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbOperadorECC.FormattingEnabled = true;
            _cmbOperadorECC.Location = new Point(76, 67);
            _cmbOperadorECC.Name = "_cmbOperadorECC";
            _cmbOperadorECC.Size = new Size(91, 23);
            _cmbOperadorECC.TabIndex = 2;
            // 
            // _txtFiltroValorECC
            // 
            _txtFiltroValorECC.Location = new Point(173, 67);
            _txtFiltroValorECC.Name = "_txtFiltroValorECC";
            _txtFiltroValorECC.Size = new Size(100, 23);
            _txtFiltroValorECC.TabIndex = 3;
            // 
            // pnlFiltroD8
            // 
            pnlFiltroD8.Controls.Add(_chkFiltroPerdeuImplante);
            pnlFiltroD8.Controls.Add(_chkFiltroEcg);
            pnlFiltroD8.Location = new Point(3, 107);
            pnlFiltroD8.Name = "pnlFiltroD8";
            pnlFiltroD8.Size = new Size(310, 62);
            pnlFiltroD8.TabIndex = 111;
            pnlFiltroD8.Visible = false;
            // 
            // _chkFiltroPerdeuImplante
            // 
            _chkFiltroPerdeuImplante.AutoSize = true;
            _chkFiltroPerdeuImplante.Location = new Point(9, 8);
            _chkFiltroPerdeuImplante.Name = "_chkFiltroPerdeuImplante";
            _chkFiltroPerdeuImplante.Size = new Size(164, 19);
            _chkFiltroPerdeuImplante.TabIndex = 113;
            _chkFiltroPerdeuImplante.Text = "Apenas perda de implante";
            _chkFiltroPerdeuImplante.UseVisualStyleBackColor = true;
            // 
            // _chkFiltroEcg
            // 
            _chkFiltroEcg.AutoSize = true;
            _chkFiltroEcg.Location = new Point(9, 33);
            _chkFiltroEcg.Name = "_chkFiltroEcg";
            _chkFiltroEcg.Size = new Size(117, 19);
            _chkFiltroEcg.TabIndex = 112;
            _chkFiltroEcg.Text = "Apenas com eCG";
            _chkFiltroEcg.UseVisualStyleBackColor = true;
            // 
            // pnlFiltroIATF
            // 
            pnlFiltroIATF.Controls.Add(label17);
            pnlFiltroIATF.Controls.Add(label16);
            pnlFiltroIATF.Controls.Add(label8);
            pnlFiltroIATF.Controls.Add(_cmbOperadorCio);
            pnlFiltroIATF.Controls.Add(_txtFiltroValorCio);
            pnlFiltroIATF.Controls.Add(_cmbFiltroTouro);
            pnlFiltroIATF.Controls.Add(_cmbFiltroInseminador);
            pnlFiltroIATF.Location = new Point(3, 175);
            pnlFiltroIATF.Name = "pnlFiltroIATF";
            pnlFiltroIATF.Size = new Size(310, 104);
            pnlFiltroIATF.TabIndex = 112;
            pnlFiltroIATF.Visible = false;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(20, 68);
            label17.Name = "label17";
            label17.Size = new Size(65, 15);
            label17.TabIndex = 6;
            label17.Text = "Escore Cio:";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(44, 39);
            label16.Name = "label16";
            label16.Size = new Size(41, 15);
            label16.TabIndex = 5;
            label16.Text = "Touro:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(9, 11);
            label8.Name = "label8";
            label8.Size = new Size(76, 15);
            label8.TabIndex = 4;
            label8.Text = "Inseminador:";
            // 
            // _cmbOperadorCio
            // 
            _cmbOperadorCio.FormattingEnabled = true;
            _cmbOperadorCio.Location = new Point(91, 65);
            _cmbOperadorCio.Name = "_cmbOperadorCio";
            _cmbOperadorCio.Size = new Size(92, 23);
            _cmbOperadorCio.TabIndex = 3;
            // 
            // _txtFiltroValorCio
            // 
            _txtFiltroValorCio.Location = new Point(189, 65);
            _txtFiltroValorCio.Name = "_txtFiltroValorCio";
            _txtFiltroValorCio.Size = new Size(100, 23);
            _txtFiltroValorCio.TabIndex = 2;
            // 
            // _cmbFiltroTouro
            // 
            _cmbFiltroTouro.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFiltroTouro.FormattingEnabled = true;
            _cmbFiltroTouro.Location = new Point(91, 36);
            _cmbFiltroTouro.Name = "_cmbFiltroTouro";
            _cmbFiltroTouro.Size = new Size(121, 23);
            _cmbFiltroTouro.TabIndex = 1;
            // 
            // _cmbFiltroInseminador
            // 
            _cmbFiltroInseminador.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFiltroInseminador.FormattingEnabled = true;
            _cmbFiltroInseminador.Location = new Point(91, 7);
            _cmbFiltroInseminador.Name = "_cmbFiltroInseminador";
            _cmbFiltroInseminador.Size = new Size(121, 23);
            _cmbFiltroInseminador.TabIndex = 0;
            // 
            // pnlFiltroDG
            // 
            pnlFiltroDG.Controls.Add(panel1);
            pnlFiltroDG.Controls.Add(label18);
            pnlFiltroDG.Controls.Add(_cmbFiltroDestino);
            pnlFiltroDG.Location = new Point(3, 285);
            pnlFiltroDG.Name = "pnlFiltroDG";
            pnlFiltroDG.Size = new Size(462, 75);
            pnlFiltroDG.TabIndex = 113;
            pnlFiltroDG.Visible = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(label19);
            panel1.Controls.Add(_rbFiltroVazia);
            panel1.Controls.Add(_rbFiltroPrenha);
            panel1.Controls.Add(_rbFiltroTodos);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(462, 36);
            panel1.TabIndex = 0;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(33, 11);
            label19.Name = "label19";
            label19.Size = new Size(52, 15);
            label19.TabIndex = 3;
            label19.Text = "Prenhez:";
            // 
            // _rbFiltroVazia
            // 
            _rbFiltroVazia.AutoSize = true;
            _rbFiltroVazia.Location = new Point(222, 9);
            _rbFiltroVazia.Name = "_rbFiltroVazia";
            _rbFiltroVazia.Size = new Size(51, 19);
            _rbFiltroVazia.TabIndex = 2;
            _rbFiltroVazia.TabStop = true;
            _rbFiltroVazia.Text = "Vazia";
            _rbFiltroVazia.UseVisualStyleBackColor = true;
            // 
            // _rbFiltroPrenha
            // 
            _rbFiltroPrenha.AutoSize = true;
            _rbFiltroPrenha.Location = new Point(154, 9);
            _rbFiltroPrenha.Name = "_rbFiltroPrenha";
            _rbFiltroPrenha.Size = new Size(62, 19);
            _rbFiltroPrenha.TabIndex = 1;
            _rbFiltroPrenha.TabStop = true;
            _rbFiltroPrenha.Text = "Prenha";
            _rbFiltroPrenha.UseVisualStyleBackColor = true;
            // 
            // _rbFiltroTodos
            // 
            _rbFiltroTodos.AutoSize = true;
            _rbFiltroTodos.Location = new Point(91, 9);
            _rbFiltroTodos.Name = "_rbFiltroTodos";
            _rbFiltroTodos.Size = new Size(57, 19);
            _rbFiltroTodos.TabIndex = 0;
            _rbFiltroTodos.TabStop = true;
            _rbFiltroTodos.Text = "Todos";
            _rbFiltroTodos.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(35, 48);
            label18.Name = "label18";
            label18.Size = new Size(50, 15);
            label18.TabIndex = 2;
            label18.Text = "Destino:";
            // 
            // _cmbFiltroDestino
            // 
            _cmbFiltroDestino.FormattingEnabled = true;
            _cmbFiltroDestino.Location = new Point(91, 45);
            _cmbFiltroDestino.Name = "_cmbFiltroDestino";
            _cmbFiltroDestino.Size = new Size(121, 23);
            _cmbFiltroDestino.TabIndex = 1;
            // 
            // pnlBotoesContainer
            // 
            pnlBotoesContainer.Controls.Add(btnLimparFiltros);
            pnlBotoesContainer.Controls.Add(btnFiltrar);
            pnlBotoesContainer.Dock = DockStyle.Top;
            pnlBotoesContainer.Location = new Point(3, 379);
            pnlBotoesContainer.Margin = new Padding(3, 10, 3, 3);
            pnlBotoesContainer.MaximumSize = new Size(458, 30);
            pnlBotoesContainer.MinimumSize = new Size(458, 30);
            pnlBotoesContainer.Name = "pnlBotoesContainer";
            pnlBotoesContainer.Size = new Size(458, 30);
            pnlBotoesContainer.TabIndex = 1;
            // 
            // btnLimparFiltros
            // 
            btnLimparFiltros.ForeColor = Color.Crimson;
            btnLimparFiltros.Location = new Point(275, 4);
            btnLimparFiltros.Name = "btnLimparFiltros";
            btnLimparFiltros.Size = new Size(99, 23);
            btnLimparFiltros.TabIndex = 2;
            btnLimparFiltros.Text = "Limpar Filtros";
            btnLimparFiltros.UseVisualStyleBackColor = true;
            btnLimparFiltros.Click += btnLimparFiltros_Click;
            // 
            // btnFiltrar
            // 
            btnFiltrar.Location = new Point(380, 4);
            btnFiltrar.Name = "btnFiltrar";
            btnFiltrar.Size = new Size(75, 23);
            btnFiltrar.TabIndex = 1;
            btnFiltrar.Text = "Filtrar";
            btnFiltrar.UseVisualStyleBackColor = true;
            btnFiltrar.Click += btnFiltrar_Click;
            // 
            // grpInserir
            // 
            grpInserir.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpInserir.AutoSize = true;
            grpInserir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpInserir.BackColor = Color.WhiteSmoke;
            grpInserir.Controls.Add(pnlContainerInsercao);
            grpInserir.Controls.Add(pnlNumero);
            grpInserir.Location = new Point(12, 395);
            grpInserir.MaximumSize = new Size(460, 670);
            grpInserir.MinimumSize = new Size(466, 0);
            grpInserir.Name = "grpInserir";
            grpInserir.Size = new Size(466, 644);
            grpInserir.TabIndex = 111;
            grpInserir.TabStop = false;
            grpInserir.Text = "Inserir";
            // 
            // pnlNumero
            // 
            pnlNumero.AutoScroll = true;
            pnlNumero.AutoSize = true;
            pnlNumero.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlNumero.Controls.Add(label1);
            pnlNumero.Controls.Add(_numero);
            pnlNumero.Dock = DockStyle.Top;
            pnlNumero.Location = new Point(3, 19);
            pnlNumero.Name = "pnlNumero";
            pnlNumero.Size = new Size(460, 32);
            pnlNumero.TabIndex = 0;
            // 
            // grpEtapas
            // 
            grpEtapas.BackColor = Color.WhiteSmoke;
            grpEtapas.Controls.Add(btnTesteD0);
            grpEtapas.Controls.Add(btnTesteD8);
            grpEtapas.Controls.Add(btnTesteIATF);
            grpEtapas.Controls.Add(btnTesteDG);
            grpEtapas.Controls.Add(btnAvancar);
            grpEtapas.Controls.Add(btnVoltar);
            grpEtapas.Location = new Point(973, 835);
            grpEtapas.Name = "grpEtapas";
            grpEtapas.Size = new Size(468, 109);
            grpEtapas.TabIndex = 112;
            grpEtapas.TabStop = false;
            grpEtapas.Text = "Etapas";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.WhiteSmoke;
            groupBox1.Controls.Add(label24);
            groupBox1.Controls.Add(label23);
            groupBox1.Controls.Add(label22);
            groupBox1.Controls.Add(label21);
            groupBox1.Controls.Add(label20);
            groupBox1.Controls.Add(lblStatTotal);
            groupBox1.Controls.Add(lblStatDoses);
            groupBox1.Controls.Add(lblStatServico);
            groupBox1.Controls.Add(lblStatPrenhez);
            groupBox1.Controls.Add(lblStatConcepcao);
            groupBox1.Location = new Point(487, 680);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(471, 212);
            groupBox1.TabIndex = 113;
            groupBox1.TabStop = false;
            groupBox1.Text = "Resumo de Lote (Estatísticas)";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(25, 131);
            label24.Name = "label24";
            label24.Size = new Size(97, 15);
            label24.TabIndex = 9;
            label24.Text = "Total de prenhas:";
            label24.MouseEnter += MostrarAjudaEstatistica;
            label24.MouseLeave += LimparAjudaEstatistica;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(15, 106);
            label23.Name = "label23";
            label23.Size = new Size(107, 15);
            label23.TabIndex = 8;
            label23.Text = "Doses por prenhez:";
            label23.MouseEnter += MostrarAjudaEstatistica;
            label23.MouseLeave += LimparAjudaEstatistica;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(28, 79);
            label22.Name = "label22";
            label22.Size = new Size(94, 15);
            label22.TabIndex = 7;
            label22.Text = "Taxa de prenhez:";
            label22.MouseEnter += MostrarAjudaEstatistica;
            label22.MouseLeave += LimparAjudaEstatistica;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(33, 53);
            label21.Name = "label21";
            label21.Size = new Size(89, 15);
            label21.TabIndex = 6;
            label21.Text = "Taxa de serviço:";
            label21.MouseEnter += MostrarAjudaEstatistica;
            label21.MouseLeave += LimparAjudaEstatistica;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(12, 28);
            label20.Name = "label20";
            label20.Size = new Size(110, 15);
            label20.TabIndex = 5;
            label20.Text = "Taxa de concepção:";
            label20.MouseEnter += MostrarAjudaEstatistica;
            label20.MouseLeave += LimparAjudaEstatistica;
            // 
            // lblStatTotal
            // 
            lblStatTotal.AutoSize = true;
            lblStatTotal.Location = new Point(128, 131);
            lblStatTotal.Name = "lblStatTotal";
            lblStatTotal.Size = new Size(44, 15);
            lblStatTotal.TabIndex = 4;
            lblStatTotal.Text = "label20";
            lblStatTotal.MouseEnter += MostrarAjudaEstatistica;
            lblStatTotal.MouseLeave += LimparAjudaEstatistica;
            // 
            // lblStatDoses
            // 
            lblStatDoses.AutoSize = true;
            lblStatDoses.Location = new Point(128, 106);
            lblStatDoses.Name = "lblStatDoses";
            lblStatDoses.Size = new Size(44, 15);
            lblStatDoses.TabIndex = 3;
            lblStatDoses.Text = "label20";
            lblStatDoses.MouseEnter += MostrarAjudaEstatistica;
            lblStatDoses.MouseLeave += LimparAjudaEstatistica;
            // 
            // lblStatServico
            // 
            lblStatServico.AutoSize = true;
            lblStatServico.Location = new Point(128, 53);
            lblStatServico.Name = "lblStatServico";
            lblStatServico.Size = new Size(44, 15);
            lblStatServico.TabIndex = 2;
            lblStatServico.Text = "label22";
            lblStatServico.MouseEnter += MostrarAjudaEstatistica;
            lblStatServico.MouseLeave += LimparAjudaEstatistica;
            // 
            // lblStatPrenhez
            // 
            lblStatPrenhez.AutoSize = true;
            lblStatPrenhez.Location = new Point(128, 79);
            lblStatPrenhez.Name = "lblStatPrenhez";
            lblStatPrenhez.Size = new Size(44, 15);
            lblStatPrenhez.TabIndex = 1;
            lblStatPrenhez.Text = "label21";
            lblStatPrenhez.MouseEnter += MostrarAjudaEstatistica;
            lblStatPrenhez.MouseLeave += LimparAjudaEstatistica;
            // 
            // lblStatConcepcao
            // 
            lblStatConcepcao.AutoSize = true;
            lblStatConcepcao.Location = new Point(128, 28);
            lblStatConcepcao.Name = "lblStatConcepcao";
            lblStatConcepcao.Size = new Size(44, 15);
            lblStatConcepcao.TabIndex = 0;
            lblStatConcepcao.Text = "label20";
            lblStatConcepcao.MouseEnter += MostrarAjudaEstatistica;
            lblStatConcepcao.MouseLeave += LimparAjudaEstatistica;
            // 
            // lblMediaD0
            // 
            lblMediaD0.AutoSize = true;
            lblMediaD0.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblMediaD0.Location = new Point(20, 23);
            lblMediaD0.Name = "lblMediaD0";
            lblMediaD0.Size = new Size(52, 17);
            lblMediaD0.TabIndex = 114;
            lblMediaD0.Text = "label29";
            // 
            // lblMediaD8
            // 
            lblMediaD8.AutoSize = true;
            lblMediaD8.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblMediaD8.Location = new Point(20, 57);
            lblMediaD8.Name = "lblMediaD8";
            lblMediaD8.Size = new Size(52, 17);
            lblMediaD8.TabIndex = 115;
            lblMediaD8.Text = "label30";
            // 
            // lblMediaIATF
            // 
            lblMediaIATF.AutoSize = true;
            lblMediaIATF.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblMediaIATF.Location = new Point(241, 23);
            lblMediaIATF.Name = "lblMediaIATF";
            lblMediaIATF.Size = new Size(52, 17);
            lblMediaIATF.TabIndex = 116;
            lblMediaIATF.Text = "label31";
            // 
            // lblMediaDG
            // 
            lblMediaDG.AutoSize = true;
            lblMediaDG.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblMediaDG.Location = new Point(241, 57);
            lblMediaDG.Name = "lblMediaDG";
            lblMediaDG.Size = new Size(52, 17);
            lblMediaDG.TabIndex = 117;
            lblMediaDG.Text = "label32";
            // 
            // grpDatas
            // 
            grpDatas.BackColor = Color.WhiteSmoke;
            grpDatas.Controls.Add(lblMediaD0);
            grpDatas.Controls.Add(lblMediaD8);
            grpDatas.Controls.Add(lblMediaIATF);
            grpDatas.Controls.Add(lblMediaDG);
            grpDatas.Location = new Point(494, 395);
            grpDatas.Name = "grpDatas";
            grpDatas.Size = new Size(456, 94);
            grpDatas.TabIndex = 118;
            grpDatas.TabStop = false;
            grpDatas.Text = "Datas ";
            // 
            // btnExportar
            // 
            btnExportar.Location = new Point(1267, 958);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(75, 48);
            btnExportar.TabIndex = 119;
            btnExportar.Text = "Exportar Tabela";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(1186, 958);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(75, 48);
            btnSalvar.TabIndex = 120;
            btnSalvar.Text = "💾 Salvar e Sair";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += btnSalvar_Click;
            // 
            // btnConcluir
            // 
            btnConcluir.ForeColor = Color.Green;
            btnConcluir.Location = new Point(1095, 958);
            btnConcluir.Name = "btnConcluir";
            btnConcluir.Size = new Size(84, 48);
            btnConcluir.TabIndex = 121;
            btnConcluir.Text = "Concluir ✓";
            btnConcluir.UseVisualStyleBackColor = true;
            btnConcluir.Click += btnConcluir_Click;
            // 
            // btnImprimir
            // 
            btnImprimir.Location = new Point(1348, 958);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(93, 48);
            btnImprimir.TabIndex = 122;
            btnImprimir.Text = "🖨 Imprimir";
            btnImprimir.UseVisualStyleBackColor = true;
            btnImprimir.Click += btnImprimir_Click;
            // 
            // FrmMenuTabelas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1809, 1064);
            Controls.Add(btnImprimir);
            Controls.Add(btnConcluir);
            Controls.Add(btnSalvar);
            Controls.Add(btnExportar);
            Controls.Add(grpDatas);
            Controls.Add(groupBox1);
            Controls.Add(grpEtapas);
            Controls.Add(grpInserir);
            Controls.Add(grpFiltros);
            Controls.Add(grpAjuda);
            Controls.Add(tabela);
            Name = "FrmMenuTabelas";
            Text = "Form1";
            Load += FrmMenuTabela_Load;
            ((System.ComponentModel.ISupportInitialize)tabela).EndInit();
            pnlD0.ResumeLayout(false);
            pnlD0.PerformLayout();
            pnlDG.ResumeLayout(false);
            pnlDG.PerformLayout();
            pnlD8.ResumeLayout(false);
            pnlD8.PerformLayout();
            pnlIATF.ResumeLayout(false);
            pnlIATF.PerformLayout();
            pnlContainerInsercao.ResumeLayout(false);
            pnlContainerInsercao.PerformLayout();
            pnlBotoes.ResumeLayout(false);
            pnlObs.ResumeLayout(false);
            pnlObs.PerformLayout();
            grpAjuda.ResumeLayout(false);
            grpAjuda.PerformLayout();
            grpFiltros.ResumeLayout(false);
            grpFiltros.PerformLayout();
            flowPainelMestre.ResumeLayout(false);
            flowPainelMestre.PerformLayout();
            flowAreaDeFiltros.ResumeLayout(false);
            pnlFiltroD0.ResumeLayout(false);
            pnlFiltroD0.PerformLayout();
            pnlFiltroD8.ResumeLayout(false);
            pnlFiltroD8.PerformLayout();
            pnlFiltroIATF.ResumeLayout(false);
            pnlFiltroIATF.PerformLayout();
            pnlFiltroDG.ResumeLayout(false);
            pnlFiltroDG.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            pnlBotoesContainer.ResumeLayout(false);
            grpInserir.ResumeLayout(false);
            grpInserir.PerformLayout();
            pnlNumero.ResumeLayout(false);
            pnlNumero.PerformLayout();
            grpEtapas.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpDatas.ResumeLayout(false);
            grpDatas.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView tabela;
        private DataGridViewTextBoxColumn Número;
        private TextBox _numero;
        private TextBox _observacoes;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox textBox3;
        private TextBox _ecc;
        private DataGridViewTextBoxColumn numero;
        private DataGridViewTextBoxColumn PV;
        private DataGridViewTextBoxColumn Escore;
        private DataGridViewTextBoxColumn Touro;
        private DataGridViewTextBoxColumn Categoria;
        private DataGridViewTextBoxColumn Raça;
        private DataGridViewTextBoxColumn observacao;
        private Button inserir;
        private ComboBox _cbResultadoDG;
        private Label label6;
        private ComboBox _categoria;
        private ComboBox _touro;
        private ComboBox _raca;
        private Button btnExcluir;
        private Button btnTesteD0;
        private Button btnTesteD8;
        private Button btnTesteIATF;
        private Button btnTesteDG;
        private GroupBox pnlD0;
        private Label label9;
        private TextBox textBox4;
        private TextBox textBox2;
        private Label label13;
        private Label label12;
        private Label label11;
        private ComboBox _lote;
        private GroupBox pnlD8;
        private CheckBox _chkEcg;
        private CheckBox _chkPerdeuImplante;
        private GroupBox pnlIATF;
        private Label I;
        private Label label10;
        private CheckBox _chkGnrh;
        private GroupBox pnlDG;
        private ComboBox _txtDestino;
        private ComboBox _txtOvario;
        private Label label15;
        private Label label14;
        private ComboBox _inseminador;
        private Button btnAvancar;
        private Button btnVoltar;
        private Panel pnlContainerInsercao;
        private TextBox _txtEscoreCio;
        private ContextMenuStrip contextMenuStrip1;
        private GroupBox grpAjuda;
        private Label lblAjudaTitulo;
        private Label lblAjudaDescricao;
        private GroupBox grpFiltros;
        private FlowLayoutPanel flowPainelMestre;
        private ComboBox _cmbFiltroRaca;
        private ComboBox _cmbFiltroCategoria;
        private ComboBox _cmbOperadorECC;
        private TextBox _txtFiltroValorECC;
        private Label label4;
        private Label label5;
        private Label label7;
        private Panel pnlFiltroD0;
        private Panel pnlFiltroD8;
        private Panel pnlFiltroIATF;
        private Panel pnlFiltroDG;
        private CheckBox _chkFiltroPerdeuImplante;
        private CheckBox _chkFiltroEcg;
        private Label label17;
        private Label label16;
        private Label label8;
        private ComboBox _cmbOperadorCio;
        private TextBox _txtFiltroValorCio;
        private ComboBox _cmbFiltroTouro;
        private ComboBox _cmbFiltroInseminador;
        private Label label18;
        private ComboBox _cmbFiltroDestino;
        private Panel panel1;
        private RadioButton _rbFiltroVazia;
        private RadioButton _rbFiltroPrenha;
        private RadioButton _rbFiltroTodos;
        private Label label19;
        private Button btnLimparFiltros;
        private Button btnFiltrar;
        private FlowLayoutPanel flowAreaDeFiltros;
        private Panel pnlBotoesContainer;
        private GroupBox grpInserir;
        private GroupBox grpEtapas;
        private GroupBox groupBox1;
        private Label lblStatPrenhez;
        private Label lblStatConcepcao;
        private Label lblStatServico;
        private Label lblStatDoses;
        private Label lblStatTotal;
        private Label label20;
        private Label label23;
        private Label label22;
        private Label label21;
        private Label label24;
        private DateTimePicker _dtpIATF;
        private Label label25;
        private Label label26;
        private DateTimePicker _dtpD0;
        private DateTimePicker _dtpDG;
        private Label label28;
        private Label label27;
        private DateTimePicker _dtpD8;
        private Label lblMediaD0;
        private Label lblMediaD8;
        private Label lblMediaIATF;
        private Label lblMediaDG;
        private GroupBox grpDatas;
        private Button btnExportar;
        private Panel pnlNumero;
        private Panel pnlObs;
        private Panel pnlBotoes;
        private Button btnSalvar;
        private Button btnConcluir;
        private Button btnImprimir;
    }
}
