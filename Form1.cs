using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.IO;

namespace StokKomputer
{
    public partial class Form1 : Form
    {
        private readonly DataGridView grid = new();
        private readonly TextBox txtCari = new();
        private readonly ComboBox cmbKategori = new();
        private readonly ComboBox cmbMerek = new();
        private readonly ComboBox cmbKetersediaan = new();
        private readonly ComboBox cmbTipeHarga = new();
        private bool sedangFormatHarga;
        private readonly TextBox txtHargaMin = new();
        private readonly TextBox txtHargaMax = new();
        private readonly Label lblJumlah = new();
        private DataTable dataStok = new();

        public Form1()
        {
            InitializeComponent();
            Text = "Stok Komputer - Citra Persada";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 600);

            Controls.Clear();
            BangunDashboard();
            MuatData();
        }

        private void BangunDashboard()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(30, 64, 175),
                Padding = new Padding(22, 12, 12, 12)
            };

            var judul = new Label
            {
                Text = "CITRA PERSADA",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 10)
            };

            var subjudul = new Label
            {
                Text = "Sistem Inventaris Laptop & All-in-One",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(22, 56)
            };

            header.Controls.Add(judul);
            header.Controls.Add(subjudul);

            var filter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 125,
                Padding = new Padding(15, 10, 10, 5),
                BackColor = Color.WhiteSmoke,
                WrapContents = true,
                AutoScroll = true
            };

            txtCari.Width = 500;
            SiapkanCombo(cmbKategori, 200);
            SiapkanCombo(cmbMerek, 200);
            SiapkanCombo(cmbKetersediaan, 200);

            cmbKategori.Items.AddRange(
                new object[] { "Semua kategori", "Laptop", "AIO" });
            cmbKategori.SelectedIndex = 0;

            cmbKetersediaan.Items.AddRange(
                new object[] {
                    "Semua stok", "Ada stok", "Stok habis"
                });
            cmbKetersediaan.SelectedIndex = 0;

            SiapkanCombo(cmbTipeHarga, 160);

            cmbTipeHarga.Items.AddRange(
                new object[] { "Umum", "Kantor", "Proyek" });

            cmbTipeHarga.SelectedIndex = 0;

            txtHargaMin.Width = 160;
            txtHargaMax.Width = 160;

            TambahFilter(filter, "Cari model/prosesor", txtCari);
            TambahFilter(filter, "Kategori", cmbKategori);
            TambahFilter(filter, "Merek", cmbMerek);
            TambahFilter(filter, "Ketersediaan", cmbKetersediaan);
            TambahFilter(filter, "Tipe Harga", cmbTipeHarga);
            TambahFilter(filter, "Harga min (Rp)", txtHargaMin);
            TambahFilter(filter, "Harga maks (Rp)", txtHargaMax);

            var bawah = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 8, 15, 15)
            };

            lblJumlah.Dock = DockStyle.Top;
            lblJumlah.Height = 35;
            lblJumlah.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblJumlah.TextAlign = ContentAlignment.MiddleLeft;

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = true;

            bawah.Controls.Add(grid);
            bawah.Controls.Add(lblJumlah);


            var btnTambah = new Button
            {
                Text = "+ Tambah Model",
                Size = new Size(150, 38),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 64, 175),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };

            btnTambah.FlatAppearance.BorderSize = 0;

            var btnPenjualan = new Button
            {
                Text = "+ Penjualan",
                Size = new Size(150, 38),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(22, 163, 74),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnPenjualan.FlatAppearance.BorderSize = 0;


            var btnRiwayat = new Button
            {
                Text = "+ Riwayat Stok",
                Size = new Size(150, 38),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnRiwayat.FlatAppearance.BorderSize = 0;

            var btnLaporan = new Button
            {
                Text = "+ Laporan Stok",
                Size = new Size(150, 38),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 95, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnLaporan.FlatAppearance.BorderSize = 0;


            var btnBackup = new Button
            {
                Text = "Backup Database",
                Size = new Size(155, 40),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btnBackup.FlatAppearance.BorderSize = 0;
            btnBackup.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(29, 78, 216);

            var btnRestore = new Button
            {
                Text = "Restore",
                Size = new Size(150, 38),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 95, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnLaporan.FlatAppearance.BorderSize = 0;

            var btnReset = new Button
            {
                Text = "Reset",
                Size = new Size(150, 38),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 95, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnReset.FlatAppearance.BorderSize = 0;

            header.Controls.Add(btnTambah);
            header.Controls.Add(btnPenjualan);
            header.Controls.Add(btnRiwayat);
            header.Controls.Add(btnLaporan);
            header.Controls.Add(btnBackup);
            header.Controls.Add(btnRestore);
            header.Controls.Add(btnReset);


            void AturPosisiTombol()
            {
                btnTambah.Location = new Point(
                    header.ClientSize.Width - btnTambah.Width - 20,
                    (header.ClientSize.Height - btnTambah.Height) / 2
                );

                btnPenjualan.Location = new Point(
                    btnTambah.Left - btnPenjualan.Width - 10,
                    (header.ClientSize.Height - btnPenjualan.Height) / 2
                );

                btnRiwayat.Location = new Point(
                    btnPenjualan.Left - btnRiwayat.Width - 10,
                    (header.ClientSize.Height - btnRiwayat.Height) / 2
                );

                btnLaporan.Location = new Point(
                    btnRiwayat.Left - btnLaporan.Width - 10,
                    (header.ClientSize.Height - btnRiwayat.Height) / 2
                );

                btnBackup.Location = new Point(
                    btnLaporan.Left - btnBackup.Width - 10,
                    (header.ClientSize.Height - btnBackup.Height) / 2
                );

                btnRestore.Location = new Point(
                    btnBackup.Left - btnRestore.Width - 10,
                    (header.ClientSize.Height - btnRestore.Height) / 2
                );

                btnReset.Location = new Point(
                    btnRestore.Left - btnReset.Width - 10,
                    (header.ClientSize.Height - btnReset.Height) / 2
                );
            }


            AturPosisiTombol();

            header.Resize += (_, _) => AturPosisiTombol();

            btnTambah.Click += (_, _) => BukaFormModel();
            btnBackup.Click += (_, _) => BackupDatabase();
            btnRestore.Click += (_, _) => RestoreDatabase();
            btnReset.Click += (_, _) => ResetSeluruhToko();
            btnRiwayat.Click += (_, _) =>
            {
                using var form = new FormRiwayatStok();
                form.ShowDialog(this);
            };

            btnLaporan.Click += (_, _) =>
            {
                using var form = new FormLaporanPenjualan();
                form.ShowDialog(this);
            };

            btnPenjualan.Click += (_, _) =>
            {
                using var form = new FormSales();

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    MuatData();
                }
            };

            btnTambah.BringToFront();
            btnRiwayat.BringToFront();
            btnPenjualan.BringToFront();
            btnLaporan.BringToFront();
            btnBackup.BringToFront();
            btnRestore.BringToFront();

            Controls.Add(bawah);
            Controls.Add(filter);
            Controls.Add(header);

            txtCari.TextChanged += (_, _) => TerapkanFilter();
            cmbKategori.SelectedIndexChanged +=
                (_, _) => TerapkanFilter();
            cmbMerek.SelectedIndexChanged +=
                (_, _) => TerapkanFilter();
            cmbKetersediaan.SelectedIndexChanged +=
                (_, _) => TerapkanFilter();
            cmbTipeHarga.SelectedIndexChanged +=
                (_, _) => TerapkanFilter();
            txtHargaMin.TextChanged += (_, _) =>
            {
                FormatInputHarga(txtHargaMin);
                TerapkanFilter();
            };
            txtHargaMax.TextChanged += (_, _) =>
            {
                FormatInputHarga(txtHargaMax);
                TerapkanFilter();
            };
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0)
                    BukaDetailModel();
            };
        }

        private void SiapkanCombo(ComboBox combo, int lebar)
        {
            combo.Width = lebar;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void TambahFilter(
            FlowLayoutPanel panel,
            string label,
            Control input)
        {
            var wadah = new Panel
            {
                Width = input.Width + 10,
                Height = 82,
                Margin = new Padding(4)
            };

            var teks = new Label
            {
                Text = label,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 9)
            };

            input.Location = new Point(0, 32);
            input.Height = 28;

            wadah.Controls.Add(input);
            wadah.Controls.Add(teks);
            panel.Controls.Add(wadah);
        }

        private void MuatData()
        {
            dataStok = Database.GetStockSummary();

            var merekTerpilih = cmbMerek.Text;

            cmbMerek.Items.Clear();
            cmbMerek.Items.Add("Semua merek");

            foreach (string merek in dataStok.AsEnumerable()
                .Select(r => r.Field<string>("Merek"))
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .OrderBy(m => m))
            {
                cmbMerek.Items.Add(merek);
            }

            cmbMerek.SelectedItem =
                cmbMerek.Items.Contains(merekTerpilih)
                    ? merekTerpilih
                    : "Semua merek";

            TerapkanFilter();
        }

        private void BukaDetailModel()
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Pilih model komputer terlebih dahulu.");
                return;
            }

            if (!grid.Columns.Contains("id"))
            {
                MessageBox.Show("Kolom ID model tidak ditemukan.");
                return;
            }

            int modelId = Convert.ToInt32(
                grid.CurrentRow.Cells["id"].Value
            );

            using var form = new FormDetailModel(modelId);
            form.ShowDialog(this);

            MuatData();
        }


        private void TerapkanFilter()
        {
            if (dataStok == null ||
                !dataStok.Columns.Contains("Merek"))
                return;

            var tampilan = dataStok.Clone();

            string cari = txtCari.Text.Trim();
            string kategori = cmbKategori.Text;
            string merek = cmbMerek.Text;
            string ketersediaan = cmbKetersediaan.Text;

            string tipeHarga = cmbTipeHarga.SelectedItem?.ToString()
                ?? "Umum";

            string kolomHargaAktif = tipeHarga switch
            {
                "Kantor" => "HargaKantor",
                "Proyek" => "HargaProyek",
                _ => "HargaUmum"
            };

            string angkaMin = new string(
                txtHargaMin.Text.Where(char.IsDigit).ToArray());

            string angkaMax = new string(
                txtHargaMax.Text.Where(char.IsDigit).ToArray());

            bool minValid = long.TryParse(
                angkaMin, out long hargaMin);

            bool maxValid = long.TryParse(
                angkaMax, out long hargaMax);

            foreach (DataRow baris in dataStok.Rows)
            {
                string namaMerek = baris["Merek"]?.ToString() ?? "";
                string model = baris["Model"]?.ToString() ?? "";
                string prosesor = baris["Prosesor"]?.ToString() ?? "";
                string jenis = baris["Kategori"]?.ToString() ?? "";

                int stok = Convert.ToInt32(baris["StokTersedia"]);

                long hargaAcuan = Convert.ToInt64(
                    baris[kolomHargaAktif]);

                bool cocokCari =
                    string.IsNullOrWhiteSpace(cari) ||
                    namaMerek.Contains(cari,
                        StringComparison.OrdinalIgnoreCase) ||
                    model.Contains(cari,
                        StringComparison.OrdinalIgnoreCase) ||
                    prosesor.Contains(cari,
                        StringComparison.OrdinalIgnoreCase);

                bool cocokKategori =
                    kategori == "Semua kategori" || jenis == kategori;

                bool cocokMerek =
                    merek == "Semua merek" || namaMerek == merek;

                bool cocokStok =
                    ketersediaan == "Semua stok" ||
                    (ketersediaan == "Ada stok" && stok > 0) ||
                    (ketersediaan == "Stok habis" && stok == 0);

                bool adaFilterHarga = minValid || maxValid;

                bool cocokHarga =
                    !adaFilterHarga ||
                    (hargaAcuan > 0 &&
                     (!minValid || hargaAcuan >= hargaMin) &&
                     (!maxValid || hargaAcuan <= hargaMax));

                if (cocokCari &&
                    cocokKategori &&
                    cocokMerek &&
                    cocokStok &&
                    cocokHarga)
                {
                    tampilan.ImportRow(baris);
                }
            }

            grid.DataSource = null;
            grid.Columns.Clear();
            grid.AutoGenerateColumns = true;
            grid.DataSource = tampilan;

            if (grid.Columns.Contains("id"))
                grid.Columns["id"].Visible = false;

            if (grid.Columns.Contains("JenisStorage"))
                grid.Columns["JenisStorage"].HeaderText = "Jenis Storage";

            if (grid.Columns.Contains("KapasitasStorage"))
                grid.Columns["KapasitasStorage"].HeaderText = "Storage (GB)";

            if (grid.Columns.Contains("TotalUnit"))
                grid.Columns["TotalUnit"].Visible = false;

            if (grid.Columns.Contains("StokTersedia"))
                grid.Columns["StokTersedia"].HeaderText = "Stok Tersedia";

            if (grid.Columns.Contains("ModalTerendah"))
                grid.Columns["ModalTerendah"].Visible = false;

            if (grid.Columns.Contains("ModalTertinggi"))
                grid.Columns["ModalTertinggi"].Visible = false;

            string[] semuaKolomHarga =
            {
        "HargaUmum",
        "HargaKantor",
        "HargaProyek"
    };

            foreach (string kolom in semuaKolomHarga)
            {
                if (!grid.Columns.Contains(kolom))
                    continue;

                grid.Columns[kolom].Visible =
                    kolom == kolomHargaAktif;

                grid.Columns[kolom].DefaultCellStyle.Format = "N0";
            }

            if (grid.Columns.Contains(kolomHargaAktif))
            {
                grid.Columns[kolomHargaAktif].HeaderText =
                    $"Harga {tipeHarga}";
            }

            lblJumlah.Text =
                $"Ditemukan {tampilan.Rows.Count} model komputer";
        }


        private void BukaFormModel()
        {
            using var form = new FormModel();
            if(form.ShowDialog(this) ==  DialogResult.OK)
            {
                MuatData();
            }
        }


        private void FormatInputHarga(TextBox input)
        {
            if (sedangFormatHarga)
                return;

            sedangFormatHarga = true;

            try
            {
                int posisi = input.SelectionStart;

                int jumlahDigitSebelumKursor = input.Text
                    .Take(posisi)
                    .Count(char.IsDigit);

                string angka = new string(
                    input.Text.Where(char.IsDigit).ToArray());

                if (long.TryParse(angka, out long nilai))
                {
                    input.Text = nilai.ToString(
                        "N0",
                        CultureInfo.GetCultureInfo("id-ID"));
                }
                else
                {
                    input.Text = "";
                }

                int posisiBaru = 0;
                int digitDitemukan = 0;

                while (posisiBaru < input.Text.Length &&
                       digitDitemukan < jumlahDigitSebelumKursor)
                {
                    if (char.IsDigit(input.Text[posisiBaru]))
                        digitDitemukan++;

                    posisiBaru++;
                }

                input.SelectionStart = posisiBaru;
            }
            finally
            {
                sedangFormatHarga = false;
            }
        }


        private void BackupDatabase()
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Simpan Backup Database",
                Filter = "SQLite Database (*.db)|*.db",
                FileName = $"BackupStokKomputer_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                DefaultExt = "db",
                AddExtension = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                Database.Backup(dialog.FileName);

                MessageBox.Show(
                    "Backup database berhasil disimpan!",
                    "Backup Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Backup gagal:\n{ex.Message}",
                    "Backup Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RestoreDatabase()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Pilih Backup Database",
                Filter = "SQLite Database (*.db)|*.db",
                CheckFileExists = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var konfirmasi = MessageBox.Show(
                "Data toko akan diganti dengan data dari backup.\n\n" +
                "Pastikan file yang dipilih benar.\n" +
                "Lanjutkan restore?",
                "Konfirmasi Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (konfirmasi != DialogResult.Yes)
                return;

            try
            {
                Database.Restore(dialog.FileName);

                MessageBox.Show(
                    "Restore berhasil! Dashboard akan dimuat ulang.",
                    "Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Muat ulang dashboard
                // Ganti dengan method refresh dashboard milikmu.
                MuatData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Restore gagal:\n{ex.Message}",
                    "Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ResetSeluruhToko()
        {
            var konfirmasi1 = MessageBox.Show(
                "PERINGATAN!\n\n" +
                "Semua data model, unit, harga, penjualan, " +
                "dan riwayat stok akan dihapus.\n\n" +
                "Backup otomatis akan dibuat sebelum reset.\n\n" +
                "Apakah kamu ingin melanjutkan?",
                "Reset Seluruh Toko",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (konfirmasi1 != DialogResult.Yes)
                return;

            var konfirmasi2 = MessageBox.Show(
                "Konfirmasi terakhir!\n\n" +
                "Apakah kamu benar-benar yakin ingin " +
                "mengosongkan seluruh data toko?",
                "Konfirmasi Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop);

            if (konfirmasi2 != DialogResult.Yes)
                return;

            try
            {
                string fileBackup = Database.ResetStore();

                MessageBox.Show(
                    "Reset toko berhasil!\n\n" +
                    "Backup tersimpan di:\n" + fileBackup,
                    "Reset Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Ganti dengan method refresh dashboard milikmu.
                MuatData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Reset gagal:\n" + ex.Message,
                    "Reset Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


    }
}
