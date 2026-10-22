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

            Controls.Add(panelGrid);
            Controls.Add(panelRingkasan);
            Controls.Add(panelFilter);
            Controls.Add(header);

            btnTampilkan.Click +=
                (_, _) => MuatLaporan();

            btnExportCsv.Click +=
                (_, _) => EksporPdf();
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
                    cu.serial_number AS [Serial Number],
                    COALESCE(pt.name, 'Tidak ditentukan')
                        AS [Tipe Harga],
                    s.sold_price AS [Harga Jual],
                    s.notes AS Catatan
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

            int totalUnit = dataLaporan.Rows.Count;

            long totalOmzet = 0;

            foreach (DataRow baris in dataLaporan.Rows)
            {
                if (baris["Harga Jual"] != DBNull.Value)
                {
                    totalOmzet += Convert.ToInt64(
                        baris["Harga Jual"]);
                }
            }

            lblTotalUnit.Text =
                $"Unit Terjual: {totalUnit:N0}";

            lblTotalOmzet.Text =
                $"Omzet: {totalOmzet.ToString(
                    "C0", budayaIndonesia)}";
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
