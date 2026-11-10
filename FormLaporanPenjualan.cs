using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace StokKomputer
{
    public partial class FormLaporanPenjualan : Form
    {
        private readonly DataGridView grid = new();
        private readonly DateTimePicker dtpAwal = new();
        private readonly DateTimePicker dtpAkhir = new();
        private readonly Label lblTotalUnit = new();
        private readonly Label lblTotalOmzet = new();
        private readonly Button btnTampilkan = new();
        private readonly Button btnExportCsv = new();
        private readonly ContextMenuStrip menuPenjualan = new();
        private readonly ToolStripMenuItem menuBatalkan = new("Batalkan Penjualan");
        private readonly ToolStripMenuItem menuDetail = new("Detail Penjualan");
        private DataTable dataLaporan = new();

        private readonly CultureInfo budayaIndonesia =
            CultureInfo.GetCultureInfo("id-ID");

        public FormLaporanPenjualan()
        {
            InitializeComponent();

            QuestPDF.Settings.License =
                QuestPDF.Infrastructure.LicenseType.Community;

            Text = "Laporan Penjualan - Citra Persada";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 600);

            Controls.Clear();

            BangunForm();
            MuatLaporan();
        }

        private void BangunForm()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.FromArgb(30, 64, 175),
                Padding = new Padding(20, 10, 10, 10)
            };

            var judul = new Label
            {
                Text = "LAPORAN PENJUALAN",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            header.Controls.Add(judul);

            var panelFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(15, 10, 10, 5),
                BackColor = Color.WhiteSmoke,
                WrapContents = true
            };

            dtpAwal.Format = DateTimePickerFormat.Short;
            dtpAwal.Width = 130;
            dtpAwal.Value = DateTime.Today.AddDays(-30);

            dtpAkhir.Format = DateTimePickerFormat.Short;
            dtpAkhir.Width = 130;
            dtpAkhir.Value = DateTime.Today;

            SiapkanTombol(
                btnTampilkan,
                "Tampilkan Laporan",
                Color.FromArgb(30, 64, 175));

            SiapkanTombol(
                btnExportCsv,
                "Ekspor PDF",
                Color.FromArgb(22, 163, 74));

            TambahFilter(
                panelFilter, "Tanggal Awal", dtpAwal);

            TambahFilter(
                panelFilter, "Tanggal Akhir", dtpAkhir);

            TambahFilter(
                panelFilter, " ", btnTampilkan);

            TambahFilter(
                panelFilter, " ", btnExportCsv);

            var panelRingkasan = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(15, 10, 10, 5),
                BackColor = Color.White,
                WrapContents = false
            };

            AturLabelRingkasan(lblTotalUnit);
            AturLabelRingkasan(lblTotalOmzet);

            lblTotalUnit.Text = "Unit Terjual: 0";
            lblTotalOmzet.Text = "Omzet: Rp0";

            panelRingkasan.Controls.Add(lblTotalUnit);
            panelRingkasan.Controls.Add(lblTotalOmzet);

            var panelGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 5, 15, 15),
                BackColor = Color.White
            };

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.AllCells;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = true;
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
            grid.ColumnHeadersHeight = 40;

            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(30, 64, 175);
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);
            grid.RowTemplate.Height = 36;

            panelGrid.Controls.Add(grid);

            menuPenjualan.Items.Add(menuDetail);
            menuPenjualan.Items.Add(new ToolStripSeparator());
            menuPenjualan.Items.Add(menuBatalkan);

            grid.ContextMenuStrip = menuPenjualan;

            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right ||
                    e.RowIndex < 0 ||
                    e.ColumnIndex < 0)
                    return;

                if (!grid.Rows[e.RowIndex].Visible ||
                    !grid.Columns[e.ColumnIndex].Visible)
                    return;

                grid.ClearSelection();
                grid.Rows[e.RowIndex].Selected = true;
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            };

            menuDetail.Click += (_, _) => TampilkanDetailPenjualan();
            menuBatalkan.Click += (_, _) => BatalkanPenjualan();

            Controls.Add(panelGrid);
            Controls.Add(panelRingkasan);
            Controls.Add(panelFilter);
            Controls.Add(header);

            btnTampilkan.Click +=
                (_, _) => MuatLaporan();

            btnExportCsv.Click +=
                (_, _) => EksporPdf();
        }
        private void TampilkanDetailPenjualan()
        {
            if (grid.CurrentRow == null)
                return;

            var baris = grid.CurrentRow;

            string Ambil(string kolom)
            {
                object? nilai = baris.Cells[kolom].Value;

                return nilai == null || nilai == DBNull.Value
                    ? "-"
                    : nilai.ToString() ?? "-";
            }

            string harga = baris.Cells["Harga Jual"].Value == DBNull.Value
                ? "Rp0"
                : Convert.ToInt64(baris.Cells["Harga Jual"].Value)
                    .ToString("C0", budayaIndonesia);

            using var dialog = new Form
            {
                Text = "Detail Penjualan",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(700, 650),
                MinimumSize = new Size(600, 500),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25),
                AutoScroll = true
            };

            var judul = new Label
            {
                Text = "DETAIL PENJUALAN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 64, 175),
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var tabel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 20, 0, 10)
            };

            tabel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 170));

            tabel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            void TambahBaris(string nama, string nilai)
            {
                int barisBaru = tabel.RowCount++;

                tabel.RowStyles.Add(
                    new RowStyle(SizeType.AutoSize));

                var lblNama = new Label
                {
                    Text = nama,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Padding = new Padding(5, 8, 5, 8)
                };

                var lblNilai = new Label
                {
                    Text = nilai,
                    Font = new Font("Segoe UI", 10),
                    AutoSize = true,
                    MaximumSize = new Size(430, 0),
                    Padding = new Padding(5, 8, 5, 8)
                };

                tabel.Controls.Add(lblNama, 0, barisBaru);
                tabel.Controls.Add(lblNilai, 1, barisBaru);
            }

            TambahBaris("ID Transaksi", Ambil("ID"));
            TambahBaris("Tanggal", Ambil("Tanggal"));

            TambahBaris("Merek", Ambil("Merek"));
            TambahBaris("Model", Ambil("Model"));
            TambahBaris("Processor", Ambil("CPU"));
            TambahBaris("RAM", Ambil("RAM") + " GB");
            TambahBaris("Storage", Ambil("Storage") + " GB");
            TambahBaris("Jenis Storage", Ambil("Type"));
            TambahBaris("Serial Number", Ambil("SN"));

            TambahBaris("Tipe Harga", Ambil("Tipe Harga"));
            TambahBaris("Harga Jual", harga);
            TambahBaris("Catatan", Ambil("Catatan"));

            TambahBaris("Status", Ambil("Status"));
            TambahBaris("Waktu Pembatalan", Ambil("Waktu Pembatalan"));
            TambahBaris("Alasan Pembatalan", Ambil("Alasan Pembatalan"));

            var tombolTutup = new Button
            {
                Text = "Tutup",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(30, 64, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            tombolTutup.Click += (_, _) => dialog.Close();

            panel.Controls.Add(tabel);
            panel.Controls.Add(judul);

            dialog.Controls.Add(panel);
            dialog.Controls.Add(tombolTutup);

            dialog.ShowDialog(this);
        }

        private void BatalkanPenjualan()
        {
            if (grid.CurrentRow == null)
                return;

            int salesId = Convert.ToInt32(
                grid.CurrentRow.Cells["ID"].Value);

            string status = Convert.ToString(
                grid.CurrentRow.Cells["Status"].Value) ?? "";

            if (status != "Selesai")
            {
                MessageBox.Show("Transaksi ini sudah dibatalkan.");
                return;
            }

            var konfirmasi = MessageBox.Show(
                "Pastikan barang BELUM diserahkan kepada pembeli.\n\n" +
                "Apakah kamu yakin ingin membatalkan penjualan ini?",
                "Konfirmasi Pembatalan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (konfirmasi != DialogResult.Yes)
                return;

            string? alasan = MintaAlasanPembatalan();

            if (string.IsNullOrWhiteSpace(alasan))
                return;

            using var koneksi = Database.GetConnection();
            using var transaksi = koneksi.BeginTransaction();

            try
            {
                int unitId;

                using (var cek = koneksi.CreateCommand())
                {
                    cek.Transaction = transaksi;
                    cek.CommandText = """
                SELECT unit_id
                FROM sales
                WHERE id = $id
                  AND status = 'Selesai';
                """;

                    cek.Parameters.AddWithValue("$id", salesId);

                    object? hasil = cek.ExecuteScalar();

                    if (hasil == null)
                        throw new InvalidOperationException(
                            "Transaksi tidak ditemukan atau sudah dibatalkan.");

                    unitId = Convert.ToInt32(hasil);
                }

                using (var updateSales = koneksi.CreateCommand())
                {
                    updateSales.Transaction = transaksi;
                    updateSales.CommandText = """
                UPDATE sales
                SET status = 'Dibatalkan',
                    cancelled_at = CURRENT_TIMESTAMP,
                    cancel_reason = $alasan
                WHERE id = $id
                  AND status = 'Selesai';
                """;

                    updateSales.Parameters.AddWithValue("$id", salesId);
                    updateSales.Parameters.AddWithValue("$alasan", alasan);

                    if (updateSales.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException(
                            "Gagal membatalkan transaksi.");
                }

                using (var updateUnit = koneksi.CreateCommand())
                {
                    updateUnit.Transaction = transaksi;
                    updateUnit.CommandText = """
                UPDATE computer_units
                SET status = 'Tersedia'
                WHERE id = $unitId
                  AND status = 'Terjual';
                """;

                    updateUnit.Parameters.AddWithValue("$unitId", unitId);

                    if (updateUnit.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException(
                            "Status unit gagal dikembalikan.");
                }

                using (var movement = koneksi.CreateCommand())
                {
                    movement.Transaction = transaksi;
                    movement.CommandText = """
                INSERT INTO stock_movements
                    (unit_id, movement_type, notes)
                VALUES
                    ($unitId, 'Koreksi', $notes);
                """;

                    movement.Parameters.AddWithValue("$unitId", unitId);
                    movement.Parameters.AddWithValue(
                        "$notes",
                        $"Pembatalan penjualan #{salesId}: {alasan}");

                    movement.ExecuteNonQuery();
                }

                transaksi.Commit();

                MessageBox.Show(
                    "Penjualan berhasil dibatalkan.\nStok unit sudah dikembalikan.",
                    "Pembatalan Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                MuatLaporan();
            }
            catch (Exception ex)
            {
                try { transaksi.Rollback(); }
                catch { }

                MessageBox.Show(
                    "Pembatalan gagal:\n" + ex.Message,
                    "Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string? MintaAlasanPembatalan()
        {
            using var dialog = new Form
            {
                Text = "Alasan Pembatalan",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(400, 250),
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = "Masukkan alasan pembatalan:",
                Location = new Point(15, 15),
                AutoSize = true
            };

            var input = new TextBox
            {
                Location = new Point(15, 45),
                Width = 365,
                Height = 55,
                Multiline = true,
                MaxLength = 300
            };

            var btnOK = new Button
            {
                Text = "Batalkan Penjualan",
                DialogResult = DialogResult.OK,
                Location = new Point(210, 120),
                Width = 170,
                Height = 80
            };

            var btnBatal = new Button
            {
                Text = "Kembali",
                DialogResult = DialogResult.Cancel,
                Location = new Point(115, 120),
                Width = 85,
                Height = 80
            };

            dialog.Controls.AddRange(
                new Control[] { label, input, btnOK, btnBatal });

            dialog.AcceptButton = btnOK;
            dialog.CancelButton = btnBatal;

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return null;

            return input.Text.Trim();
        }


        private void TambahFilter(
            FlowLayoutPanel panel,
            string teks,
            Control input)
        {
            var wadah = new Panel
            {
                Width = input.Width + 10,
                Height = 65,
                Margin = new Padding(4)
            };

            var label = new Label
            {
                Text = teks,
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9)
            };

            input.Location = new Point(0, 27);
            input.Height = 28;

            wadah.Controls.Add(input);
            wadah.Controls.Add(label);
            panel.Controls.Add(wadah);
        }

        private void SiapkanTombol(
            Button tombol,
            string teks,
            Color warna)
        {
            tombol.Text = teks;
            tombol.Size = new Size(155, 32);
            tombol.BackColor = warna;
            tombol.ForeColor = Color.White;
            tombol.FlatStyle = FlatStyle.Flat;
            tombol.FlatAppearance.BorderSize = 0;
            tombol.Font = new Font(
                "Segoe UI", 9, FontStyle.Bold);
        }

        private void AturLabelRingkasan(Label label)
        {
            label.AutoSize = false;
            label.Width = 300;
            label.Height = 45;
            label.Margin = new Padding(5, 5, 20, 5);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Font = new Font(
                "Segoe UI", 12, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(31, 41, 55);
        }

        private void MuatLaporan()
        {
            if (dtpAwal.Value.Date > dtpAkhir.Value.Date)
            {
                MessageBox.Show(
                    "Tanggal awal tidak boleh melewati tanggal akhir.",
                    "Periksa Tanggal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText = @"
                SELECT
                    s.id AS ID,
                    s.sold_at AS Tanggal,
                    cm.brand AS Merek,
                    cm.model_name AS Model,
                    cm.processor AS CPU,
                    cm.ram_gb AS RAM,
                    cm.storage_gb AS Storage,
                    cm.storage_type AS Type,
                    cu.serial_number AS [SN],
                    COALESCE(pt.name, 'Tidak ditentukan') AS [Tipe Harga],
                    s.sold_price AS [Harga Jual],
                    s.notes AS Catatan,
                    s.status AS Status,
                    s.cancelled_at AS [Waktu Pembatalan],
                    s.cancel_reason AS [Alasan Pembatalan]
                FROM sales s
                INNER JOIN computer_units cu
                    ON cu.id = s.unit_id
                INNER JOIN computer_models cm
                    ON cm.id = cu.model_id
                LEFT JOIN price_types pt
                    ON pt.id = s.price_type_id
                WHERE date(s.sold_at) >= date($awal)
                  AND date(s.sold_at) < date($akhir, '+1 day')
                ORDER BY s.sold_at DESC, s.id DESC;
            ";

            perintah.Parameters.AddWithValue(
                "$awal",
                dtpAwal.Value.ToString("yyyy-MM-dd"));

            perintah.Parameters.AddWithValue(
                "$akhir",
                dtpAkhir.Value.ToString("yyyy-MM-dd"));

            using var reader = perintah.ExecuteReader();

            dataLaporan = new DataTable();
            dataLaporan.Load(reader);

            grid.DataSource = dataLaporan;

            if (grid.Columns.Contains("ID"))
                grid.Columns["ID"].Visible = false;

            if (grid.Columns.Contains("Harga Jual"))
            {
                grid.Columns["Harga Jual"]
                    .DefaultCellStyle.Format = "N0";

                grid.Columns["Harga Jual"]
                    .DefaultCellStyle.FormatProvider =
                    budayaIndonesia;
            }

            var penjualanSelesai = dataLaporan.AsEnumerable()
            .Where(r => r["Status"].ToString() == "Selesai")
            .ToList();

            int totalUnit = penjualanSelesai.Count;

            long totalOmzet = penjualanSelesai.Sum(r =>
                r["Harga Jual"] == DBNull.Value
                    ? 0L
                    : Convert.ToInt64(r["Harga Jual"]));

            // Update ringkasan
            lblTotalUnit.Text = $"Unit Terjual: {totalUnit:N0}";
            lblTotalOmzet.Text = $"Omzet: Rp{totalOmzet:N0}";
        }


        private void EksporPdf()
        {
            if (dataLaporan.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Tidak ada transaksi untuk diekspor.",
                    "Laporan Penjualan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Simpan Laporan Penjualan PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName =
                    $"Laporan_Penjualan_{dtpAwal.Value:yyyyMMdd}" +
                    $"_{dtpAkhir.Value:yyyyMMdd}.pdf"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            long totalOmzet = 0;

            foreach (DataRow baris in dataLaporan.Rows)
            {
                if (baris["Harga Jual"] != DBNull.Value)
                {
                    totalOmzet += Convert.ToInt64(
                        baris["Harga Jual"]);
                }
            }

            int totalUnit = dataLaporan.Rows.Count;

            string periode =
                $"{dtpAwal.Value:dd MMMM yyyy} - " +
                $"{dtpAkhir.Value:dd MMMM yyyy}";

            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(
                        x => x.FontSize(9));

                    page.Header().Column(header =>
                    {
                        header.Item()
                            .Text("CITRA PERSADA")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        header.Item()
                            .Text("LAPORAN PENJUALAN")
                            .FontSize(14)
                            .Bold();

                        header.Item()
                            .Text($"Periode: {periode}")
                            .FontSize(9);

                        header.Item()
                            .PaddingTop(8)
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingVertical(12).Column(content =>
                    {
                        content.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Column(col =>
                                {
                                    col.Item()
                                        .Text("TOTAL UNIT TERJUAL")
                                        .FontSize(9);

                                    col.Item()
                                        .Text(totalUnit.ToString("N0", budayaIndonesia))
                                        .FontSize(16)
                                        .Bold();
                                });

                            row.ConstantItem(12);

                            row.RelativeItem().Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Column(col =>
                                {
                                    col.Item()
                                        .Text("TOTAL OMZET")
                                        .FontSize(9);

                                    col.Item()
                                        .Text(totalOmzet.ToString("C0", budayaIndonesia))
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(Colors.Green.Darken2);
                                });
                        });

                        content.Item().PaddingTop(18)
                            .Text("Detail Transaksi")
                            .FontSize(12)
                            .Bold();

                        content.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(65);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.3f);
                                columns.RelativeColumn(1.0f);
                                columns.ConstantColumn(100);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Tanggal");
                                header.Cell().Element(HeaderCell).Text("Merek");
                                header.Cell().Element(HeaderCell).Text("Model");
                                header.Cell().Element(HeaderCell).Text("Serial Number");
                                header.Cell().Element(HeaderCell).Text("Tipe Harga");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Harga Jual");
                                header.Cell().Element(HeaderCell).Text("Catatan");
                            });

                            foreach (DataRow baris in dataLaporan.Rows)
                            {
                                string tanggal = "";

                                if (DateTime.TryParse(
                                    baris["Tanggal"]?.ToString(),
                                    out DateTime tanggalJual))
                                {
                                    tanggal = tanggalJual.ToString("dd/MM/yyyy");
                                }

                                string merek =
                                    baris["Merek"]?.ToString() ?? "";

                                string model =
                                    baris["Model"]?.ToString() ?? "";

                                string serial =
                                    baris["Serial Number"]?.ToString() ?? "";

                                string tipeHarga =
                                    baris["Tipe Harga"]?.ToString() ?? "";

                                string harga = baris["Harga Jual"] == DBNull.Value
                                    ? "Rp0"
                                    : Convert.ToInt64(baris["Harga Jual"])
                                        .ToString("C0", budayaIndonesia);

                                string catatan =
                                    baris["Catatan"] == DBNull.Value
                                        ? ""
                                        : baris["Catatan"].ToString() ?? "";

                                table.Cell().Element(BodyCell).Text(tanggal);
                                table.Cell().Element(BodyCell).Text(merek);
                                table.Cell().Element(BodyCell).Text(model);
                                table.Cell().Element(BodyCell).Text(serial);
                                table.Cell().Element(BodyCell).Text(tipeHarga);
                                table.Cell().Element(BodyCell).AlignRight().Text(harga);
                                table.Cell().Element(BodyCell).Text(catatan);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Halaman ");
                        text.CurrentPageNumber();
                        text.Span(" dari ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(dialog.FileName);

            MessageBox.Show(
                "Laporan PDF berhasil disimpan.",
                "Ekspor Berhasil",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static QuestPDF.Infrastructure.IContainer HeaderCell(
            QuestPDF.Infrastructure.IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken2)
                .DefaultTextStyle(x => x
                    .FontColor(Colors.White)
                    .Bold())
                .PaddingVertical(7)
                .PaddingHorizontal(5);
        }

        private static QuestPDF.Infrastructure.IContainer BodyCell(
            QuestPDF.Infrastructure.IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(6)
                .PaddingHorizontal(5);
        }

    }
}
