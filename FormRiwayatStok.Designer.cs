namespace StokKomputer
{
    partial class FormRiwayatStok
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
            lblJudul = new Label();
            cmbJenis = new ComboBox();
            txtCari = new TextBox();
            label1 = new Label();
            label2 = new Label();
            grid = new DataGridView();
            dtpTanggalAwal = new DateTimePicker();
            dtpTanggalAkhir = new DateTimePicker();
            label3 = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblJudul
            // 
            lblJudul.AutoSize = true;
            lblJudul.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblJudul.Location = new Point(22, 21);
            lblJudul.Name = "lblJudul";
            lblJudul.Size = new Size(188, 38);
            lblJudul.TabIndex = 0;
            lblJudul.Text = "Riwayat Stok";
            lblJudul.Click += label1_Click;
            // 
            // cmbJenis
            // 
            cmbJenis.FormattingEnabled = true;
            cmbJenis.Location = new Point(170, 74);
            cmbJenis.Name = "cmbJenis";
            cmbJenis.Size = new Size(182, 33);
            cmbJenis.TabIndex = 1;
            cmbJenis.SelectedIndexChanged += cmbJenis_SelectedIndexChanged;
            // 
            // txtCari
            // 
            txtCari.Location = new Point(170, 128);
            txtCari.Name = "txtCari";
            txtCari.Size = new Size(300, 31);
            txtCari.TabIndex = 2;
            txtCari.TextChanged += txtCari_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 78);
            label1.Name = "label1";
            label1.Size = new Size(92, 25);
            label1.TabIndex = 4;
            label1.Text = "Filter Jenis";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(22, 131);
            label2.Name = "label2";
            label2.Size = new Size(42, 25);
            label2.TabIndex = 5;
            label2.Text = "Cari";
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Location = new Point(20, 181);
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersWidth = 62;
            grid.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
            grid.Size = new Size(1144, 551);
            grid.TabIndex = 3;
            grid.CellContentClick += grid_CellContentClick;
            // 
            // dtpTanggalAwal
            // 
            dtpTanggalAwal.Format = DateTimePickerFormat.Short;
            dtpTanggalAwal.Location = new Point(734, 73);
            dtpTanggalAwal.Name = "dtpTanggalAwal";
            dtpTanggalAwal.Size = new Size(152, 31);
            dtpTanggalAwal.TabIndex = 6;
            // 
            // dtpTanggalAkhir
            // 
            dtpTanggalAkhir.Format = DateTimePickerFormat.Short;
            dtpTanggalAkhir.Location = new Point(734, 125);
            dtpTanggalAkhir.Name = "dtpTanggalAkhir";
            dtpTanggalAkhir.Size = new Size(152, 31);
            dtpTanggalAkhir.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(603, 76);
            label3.Name = "label3";
            label3.Size = new Size(116, 25);
            label3.TabIndex = 8;
            label3.Text = "Tanggal Awal";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(603, 128);
            label4.Name = "label4";
            label4.Size = new Size(119, 25);
            label4.TabIndex = 9;
            label4.Text = "Tanggal Akhir";
            // 
            // FormRiwayatStok
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1178, 744);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(dtpTanggalAkhir);
            Controls.Add(dtpTanggalAwal);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(grid);
            Controls.Add(txtCari);
            Controls.Add(cmbJenis);
            Controls.Add(lblJudul);
            Name = "FormRiwayatStok";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Riwayat Stok - Citra Persada";
            Load += FormRiwayatStok_Load;
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblJudul;
        private ComboBox cmbJenis;
        private TextBox txtCari;
        private Label label1;
        private Label label2;
        private DataGridView grid;
        private DateTimePicker dtpTanggalAwal;
        private DateTimePicker dtpTanggalAkhir;
        private Label label3;
        private Label label4;
    }
}