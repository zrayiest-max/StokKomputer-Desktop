
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StokKomputer
{
    public partial class FormSmartRestock : Form
    {
        private readonly TextBox txtCari = new();
        private readonly FlowLayoutPanel panelKonten = new();

        private DataTable dataRestock = new();

        public FormSmartRestock()
        {
            InitializeComponent();

            Text = "Smart Restock - Citra Persada";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 600);

            BangunTampilan();
            MuatData();

            Shown += (_, _) => TampilkanData();

            Resize += (_, _) =>
            {
                if (IsHandleCreated && Visible)
                    TampilkanData();
            };
        }

        private void BangunTampilan()
        {
            Controls.Clear();

            // HEADER
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = Color.FromArgb(30, 64, 175),
                Padding = new Padding(20)
            };

            var judul = new Label
            {
                Text = "SMART RESTOCK",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12)
            };

            var subjudul = new Label
            {
                Text = "Rekomendasi pengadaan berdasarkan penjualan 30 hari terakhir",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 58)
            };

            header.Controls.Add(judul);
            header.Controls.Add(subjudul);

            // PANEL PENCARIAN
            var panelCari = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(15)
            };

            var lblCari = new Label
            {
                Text = "Cari merek / prosesor:",
                AutoSize = true,
                Location = new Point(15, 23),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtCari.Location = new Point(220, 16);
            txtCari.Size = new Size(300, 30);
            txtCari.Font = new Font("Segoe UI", 10);

            panelCari.Controls.Add(lblCari);
            panelCari.Controls.Add(txtCari);

            // AREA KONTEN
            panelKonten.Dock = DockStyle.Fill;
            panelKonten.FlowDirection = FlowDirection.TopDown;
            panelKonten.WrapContents = false;
            panelKonten.AutoScroll = true;
            panelKonten.Padding = new Padding(15);
            panelKonten.BackColor = Color.FromArgb(245, 247, 250);

            Controls.Add(panelKonten);
            Controls.Add(panelCari);
            Controls.Add(header);

            txtCari.TextChanged += (_, _) => TampilkanData();
        }

        private void MuatData()
        {
            dataRestock = Database.GetSmartRestockSummary();
            TampilkanData();
        }

        // RINGKAS KELUARGA PROSESOR
        private string RingkasProsesor(string prosesor)
        {
            if (string.IsNullOrWhiteSpace(prosesor))
                return "-";

            prosesor = prosesor.Trim().ToUpperInvariant();

            string namaDasar = prosesor.Split('-')[0].Trim();

            string[] bagian = namaDasar.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (bagian.Length >= 2 &&
                bagian[0] == "INTEL" &&
                int.TryParse(bagian[1], out int generasi) &&
                new[] { 3, 5, 7, 9 }.Contains(generasi))
            {
                return $"INTEL {generasi}";
            }

            if (bagian.Length >= 2 &&
                bagian[0] == "RYZEN" &&
                int.TryParse(bagian[1], out int seri))
            {
                return $"RYZEN {seri}";
            }

            return namaDasar;
        }

        // MEMBUAT SATU BARIS REKOMENDASI
        private Panel BuatBarisRestock(
            int nomor,
            string prosesor,
            string ram,
            string jenisStorage,
            string kapasitasStorage,
            int stok,
            int terjual,
            int rekomendasi,
            int lebar)
        {
            var baris = new Panel
            {
                Width = lebar,
                Height = 85,
                BackColor = Color.White,
                Margin = new Padding(3, 4, 3, 4),
                Padding = new Padding(10)
            };

            // NOMOR
            var lblNomor = new Label
            {
                Text = nomor.ToString(),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 64, 175),
                BackColor = Color.FromArgb(219, 234, 254),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 17),
                Size = new Size(38, 38)
            };

            // NAMA PROSESOR
            var lblProsesor = new Label
            {
                Text = prosesor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(62, 12)
            };

            // SPESIFIKASI
            var lblSpesifikasi = new Label
            {
                Text = $"RAM {ram} GB  |  {jenisStorage} {kapasitasStorage} GB",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(63, 38)
            };

            // INFORMASI STOK DAN PENJUALAN
            var lblStatistik = new Label
            {
                Text = $"Stok: {stok} unit     Terjual 30 hari: {terjual} unit",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true,
                Location = new Point(63, 59)
            };

            // PANEL REKOMENDASI
            var panelRekomendasi = new Panel
            {
                Size = new Size(155, 59),
                BackColor = Color.FromArgb(220, 252, 231),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(lebar - 175, 12)
            };

            var lblRekomendasi = new Label
            {
                Text = "REKOMENDASI BELI",
                Font = new Font("Segoe UI", 7, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 128, 61),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 23
            };

            var lblJumlah = new Label
            {
                Text = $"{rekomendasi} unit",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            panelRekomendasi.Controls.Add(lblJumlah);
            panelRekomendasi.Controls.Add(lblRekomendasi);

            baris.Controls.Add(lblNomor);
            baris.Controls.Add(lblProsesor);
            baris.Controls.Add(lblSpesifikasi);
            baris.Controls.Add(lblStatistik);
            baris.Controls.Add(panelRekomendasi);

            return baris;
        }

        // MENAMPILKAN DATA SMART RESTOCK
        private void TampilkanData()
        {
            panelKonten.SuspendLayout();
            panelKonten.Controls.Clear();

            string kataKunci = txtCari.Text.Trim();

            int lebarKonten = Math.Max(
                750,
                panelKonten.ClientSize.Width - 55);

            var baris = dataRestock.AsEnumerable()
                .Where(r =>
                    string.IsNullOrWhiteSpace(kataKunci) ||
                    Convert.ToString(r["Merek"])
                        .Contains(kataKunci,
                            StringComparison.OrdinalIgnoreCase) ||
                    Convert.ToString(r["Prosesor"])
                        .Contains(kataKunci,
                            StringComparison.OrdinalIgnoreCase));

            var kelompokMerek = baris
                .GroupBy(r => Convert.ToString(r["Merek"]))
                .OrderBy(g => g.Key);


            foreach (var kelompok in kelompokMerek)
            {
                // PANEL MEREK
                var groupMerek = new Panel
                {
                    Width = lebarKonten,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.Transparent,
                    Padding = new Padding(0),
                    Margin = new Padding(5, 5, 5, 20)
                };

                var lblMerek = new Label
                {
                    Text = string.IsNullOrWhiteSpace(kelompok.Key)
                        ? "Merek tidak diketahui"
                        : kelompok.Key,
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 64, 175),
                    AutoSize = true,
                    Location = new Point(5, 0)
                };

                var panelKategori = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Width = lebarKonten - 35,
                    Location = new Point(5, 28),
                    BackColor = Color.Transparent
                };

                string[] kategoriList = { "Laptop", "AIO" };

                foreach (string kategori in kategoriList)
                {
                    // FILTER DAN GABUNGKAN SPESIFIKASI
                    var dataKategori = kelompok
                        .Where(r =>
                            string.Equals(
                                Convert.ToString(r["Kategori"]),
                                kategori,
                                StringComparison.OrdinalIgnoreCase))
                        .GroupBy(r => new
                        {
                            Prosesor = RingkasProsesor(
                                Convert.ToString(r["Prosesor"])),

                            RAM = Convert.ToString(r["RAM"]),
                            JenisStorage = Convert.ToString(r["JenisStorage"]),
                            KapasitasStorage = Convert.ToString(r["KapasitasStorage"])
                        })
                        .Select(g => new
                        {
                            g.Key.Prosesor,
                            g.Key.RAM,
                            g.Key.JenisStorage,
                            g.Key.KapasitasStorage,

                            StokTersedia = g.Sum(r =>
                                Convert.ToInt32(r["StokTersedia"])),

                            Terjual30Hari = g.Sum(r =>
                                Convert.ToInt32(r["Terjual30Hari"]))
                        })
                        .Select(x => new
                        {
                            x.Prosesor,
                            x.RAM,
                            x.JenisStorage,
                            x.KapasitasStorage,
                            x.StokTersedia,
                            x.Terjual30Hari,

                            RekomendasiBeli = Math.Max(
                                0,
                                x.Terjual30Hari - x.StokTersedia)
                        })
                        .Where(x => x.RekomendasiBeli > 0)
                        .OrderByDescending(x => x.RekomendasiBeli)
                        .ThenBy(x => x.Prosesor)
                        .ToList();

                    if (dataKategori.Count == 0)
                        continue;

                    // PANEL KATEGORI TANPA BORDER
                    var groupKategori = new Panel
                    {
                        Width = lebarKonten - 25,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        BackColor = Color.Transparent,
                        Padding = new Padding(0),
                        Margin = new Padding(5, 8, 5, 8)
                    };

                    var lblKategori = new Label
                    {
                        Text = kategori == "AIO"
                            ? "All-in-One (AIO)"
                            : "Laptop",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(51, 65, 85),
                        AutoSize = true,
                        Location = new Point(5, 0)
                    };

                    var panelDaftar = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Width = groupKategori.Width - 10,
                        Location = new Point(5, 25),
                        BackColor = Color.Transparent
                    };

                    // DAFTAR BERNOMOR
                    int nomor = 1;

                    foreach (var item in dataKategori)
                    {
                        var panelBaris = BuatBarisRestock(
                            nomor,
                            item.Prosesor,
                            item.RAM,
                            item.JenisStorage,
                            item.KapasitasStorage,
                            item.StokTersedia,
                            item.Terjual30Hari,
                            item.RekomendasiBeli,
                            panelDaftar.Width - 10
                        );

                        panelDaftar.Controls.Add(panelBaris);
                        nomor++;
                    }

                    groupKategori.Controls.Add(lblKategori);
                    groupKategori.Controls.Add(panelDaftar);

                    groupKategori.Height = panelDaftar.Height + 30;

                    panelKategori.Controls.Add(groupKategori);
                }

                if (panelKategori.Controls.Count == 0)
                    continue;

                groupMerek.Controls.Add(lblMerek);
                groupMerek.Controls.Add(panelKategori);

                groupMerek.Height = panelKategori.Height + 40;

                panelKonten.Controls.Add(groupMerek);
            }


            // JIKA DATA KOSONG
            if (panelKonten.Controls.Count == 0)
            {
                var lblKosong = new Label
                {
                    Text = "Data Smart Restock tidak ditemukan.",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(10)
                };

                panelKonten.Controls.Add(lblKosong);
            }

            panelKonten.ResumeLayout(true);
        }
    }
}
