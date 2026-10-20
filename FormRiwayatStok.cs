using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StokKomputer
{
    public partial class FormRiwayatStok : Form
    {
        private DataTable dataRiwayat = new();
        public FormRiwayatStok()
        {
            InitializeComponent();

            dtpTanggalAwal.Value = DateTime.Today.AddDays(-30);
            dtpTanggalAkhir.Value = DateTime.Today;

            dtpTanggalAwal.ValueChanged +=
                (_, _) => TerapkanFilter();

            dtpTanggalAkhir.ValueChanged +=
                (_, _) => TerapkanFilter();

            AturLayoutGrid();

            Load += FormRiwayatStok_Load;
            cmbJenis.SelectedIndexChanged +=
                cmbJenis_SelectedIndexChanged;
            txtCari.TextChanged += txtCari_TextChanged;


        }

        private void FormRiwayatStok_Load(object sender, EventArgs e)
        {
            cmbJenis.Items.Clear();
            cmbJenis.Items.AddRange(new object[]
            {
                "Semua",
                "Masuk",
                "Penjualan",
                "Koreksi"
            });

            cmbJenis.SelectedIndex = 0;

            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoGenerateColumns = true;
            grid.RowHeadersVisible = false;

            MuatRiwayat();
            AturLayoutGrid();
        }

        private void MuatRiwayat()
        {
            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText = @"
                SELECT
                    sm.id AS ID,
                    sm.movement_date AS Tanggal,
                    sm.movement_type AS Pergerakan,
                    cm.brand AS Merek,
                    cm.model_name AS Model,
                    cu.serial_number AS [Serial Number],
                    sm.notes AS Catatan
                FROM stock_movements sm
                INNER JOIN computer_units cu
                    ON cu.id = sm.unit_id
                INNER JOIN computer_models cm
                    ON cm.id = cu.model_id
                ORDER BY sm.movement_date DESC, sm.id DESC;
            ";

            using var reader = perintah.ExecuteReader();

            dataRiwayat = new DataTable();
            dataRiwayat.Load(reader);

            grid.DataSource = dataRiwayat;
            AturGrid();

            if (grid.Columns.Contains("ID"))
                grid.Columns["ID"].Visible = false;

            TerapkanFilter();
        }


        private void AturGrid()
        {
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoGenerateColumns = true;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(229, 231, 235);

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersHeight = 42;

            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);
            grid.DefaultCellStyle.ForeColor =
                Color.FromArgb(31, 41, 55);
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(30, 64, 175);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);

            grid.RowTemplate.Height = 36;

            if (grid.Columns.Contains("ID"))
                grid.Columns["ID"].Visible = false;

            AturKolom("Tanggal", 25);
            AturKolom("Pergerakan", 16);
            AturKolom("Merek", 14);
            AturKolom("Model", 20);
            AturKolom("Serial Number", 20);
            AturKolom("Catatan", 30);
        }

        private void AturKolom(string nama, float bobot)
        {
            if (!grid.Columns.Contains(nama))
                return;

            grid.Columns[nama].FillWeight = bobot;
            grid.Columns[nama].MinimumWidth = 80;
            grid.Columns[nama].SortMode =
                DataGridViewColumnSortMode.Automatic;
        }


        private void TerapkanFilter()
        {
            if (dataRiwayat.Columns.Count == 0)
                return;

            var tampilan = dataRiwayat.Clone();

            string jenis = cmbJenis.Text;
            string cari = txtCari.Text.Trim();
            DateTime tanggalAwal = dtpTanggalAwal.Value.Date;
            DateTime tanggalAkhir = dtpTanggalAkhir.Value.Date;

            foreach (DataRow baris in dataRiwayat.Rows)
            {
                string pergerakan =
                    baris["Pergerakan"]?.ToString() ?? "";

                string merek =
                    baris["Merek"]?.ToString() ?? "";

                string model =
                    baris["Model"]?.ToString() ?? "";

                string serial =
                    baris["Serial Number"]?.ToString() ?? "";

                DateTime tanggalRiwayat;

                bool tanggalValid = DateTime.TryParse(
                    baris["Tanggal"]?.ToString(),
                    out tanggalRiwayat);

                bool cocokTanggal =
                    tanggalValid &&
                    tanggalRiwayat.Date >= tanggalAwal &&
                    tanggalRiwayat.Date <= tanggalAkhir;

                bool cocokJenis =
                    jenis == "Semua" || pergerakan == jenis;

                bool cocokCari =
                    string.IsNullOrWhiteSpace(cari) ||
                    merek.Contains(
                        cari, StringComparison.OrdinalIgnoreCase) ||
                    model.Contains(
                        cari, StringComparison.OrdinalIgnoreCase) ||
                    serial.Contains(
                        cari, StringComparison.OrdinalIgnoreCase);

                if (cocokJenis && cocokCari && cocokTanggal)
                    tampilan.ImportRow(baris);
            }

            grid.DataSource = tampilan;

            if (grid.Columns.Contains("ID"))
                grid.Columns["ID"].Visible = false;
        }


        private void AturLayoutGrid()
        {
            // Grid memenuhi area form yang tersisa.
            grid.Dock = DockStyle.Fill;
            grid.Anchor = AnchorStyles.Top
                        | AnchorStyles.Bottom
                        | AnchorStyles.Left
                        | AnchorStyles.Right;

            // Tampilan tabel.
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoGenerateColumns = true;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(229, 231, 235);

            // Header biru.
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersHeight = 42;

            // Isi tabel.
            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);
            grid.DefaultCellStyle.ForeColor =
                Color.FromArgb(31, 41, 55);
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(30, 64, 175);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);

            grid.RowTemplate.Height = 36;
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void cmbJenis_SelectedIndexChanged(
           object sender, EventArgs e)
        {
            TerapkanFilter();
        }

        private void txtCari_TextChanged(
            object sender, EventArgs e)
        {
            TerapkanFilter();
        }

        private void grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
