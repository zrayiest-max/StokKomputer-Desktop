using System.Data;

namespace StokKomputer
{
    public partial class FormSales : Form
    {
        private readonly ComboBox cmbModel = new();
        private readonly ComboBox cmbSN = new();
        private readonly ComboBox cmbTipeHarga = new();
        private readonly TextBox txtHargaJual = new();
        private readonly TextBox txtCatatan = new()
        {
            CharacterCasing = CharacterCasing.Upper
        };
        private readonly Button btnSimpan = new();
        private readonly Label lblHargaAcuan = new();
        private readonly CheckBox chkPengingatScan = new();
        private readonly ComboBox cmbMerek = new();
        private readonly ComboBox cmbKategori = new();
        private readonly TextBox txtCariModel = new()
        {
            CharacterCasing = CharacterCasing.Upper
        };

        private readonly DataGridView dgvKeranjang = new();
        private readonly Button btnTambahKeranjang = new();
        private readonly Button btnHapusKeranjang = new();

        private readonly FlowLayoutPanel panelDaftarCard = new();

        private readonly int? modelIdAwal;
        //public FormSales() : this(null)
        //{
        //    InitializeComponent();
        //    dgvKeranjang.AllowUserToAddRows = false;
        //    Text = "Penjualan Komputer";
        //    StartPosition = FormStartPosition.CenterScreen;
        //    WindowState = FormWindowState.Maximized;
        //    Size = new Size(1000, 500);
        //    MinimumSize = new Size(600, 400);

        //    BangunForm();
        //    MuatFilter();

        //    txtHargaJual.TextChanged += TxtHargaJual_TextChanged;

        //    cmbMerek.SelectedIndexChanged += (_, _) => MuatModel();
        //    cmbKategori.SelectedIndexChanged += (_, _) => MuatModel();
        //    txtCariModel.TextChanged += (_, _) => MuatModel();

        //    MuatModel();
        //    MuatTipeHarga();

        //    cmbModel.SelectedIndexChanged += (_, _) =>
        //    {
        //        MuatSN();
        //        MuatHargaAcuan();
        //    };

        //    cmbSN.SelectedIndexChanged += (_, _) => MuatHargaAcuan();

        //    btnTambahKeranjang.Click += (_, _) => TambahKeKeranjang();
        //    btnHapusKeranjang.Click += (_, _) =>
        //    {
        //        if (dgvKeranjang.SelectedRows.Count > 0)
        //            dgvKeranjang.Rows.Remove(
        //                dgvKeranjang.SelectedRows[0]);
        //    };
        //    MuatCardKeranjang();
        //}

        public FormSales() : this(null)
        {

        }

        public FormSales(int? modelId)
        {
            modelIdAwal = modelId;

            InitializeComponent();
            dgvKeranjang.AllowUserToAddRows = false;
            Text = "Penjualan Komputer";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Size = new Size(1000, 500);
            MinimumSize = new Size(600, 400);

            BangunForm();
            MuatFilter();

            txtHargaJual.TextChanged += TxtHargaJual_TextChanged;

            cmbMerek.SelectedIndexChanged += (_, _) => MuatModel();
            cmbKategori.SelectedIndexChanged += (_, _) => MuatModel();
            txtCariModel.TextChanged += (_, _) => MuatModel();

            MuatTipeHarga();

            cmbModel.SelectedIndexChanged += (_, _) =>
            {
                if (cmbModel.SelectedValue is int ||
                    int.TryParse(
                        Convert.ToString(cmbModel.SelectedValue),
                        out _))
                {
                    MuatSN();
                    MuatHargaAcuan();
                }
            };

            MuatModel();

            cmbSN.SelectedIndexChanged += (_, _) => MuatHargaAcuan();

            btnTambahKeranjang.Click += (_, _) => TambahKeKeranjang();
            btnHapusKeranjang.Click += (_, _) =>
            {
                if (dgvKeranjang.SelectedRows.Count > 0)
                    dgvKeranjang.Rows.Remove(
                        dgvKeranjang.SelectedRows[0]);

                MuatCardKeranjang();
            };

            MuatCardKeranjang();

            // Pilih model otomatis jika dibuka dari menu "Jual Model".
            if (modelIdAwal.HasValue)
            {
                cmbModel.SelectedValue = modelIdAwal.Value;
            }

            Shown += (_, _) =>
            {
                MuatSN();
                MuatHargaAcuan();
            };
        }
        private void MuatFilter()
        {
            using var koneksi = Database.GetConnection();

            // Filter merek
            using (var perintah = koneksi.CreateCommand())
            {
                perintah.CommandText = """
                SELECT DISTINCT m.brand
                FROM computer_models m
                JOIN computer_units u ON u.model_id = m.id
                WHERE u.status = 'Tersedia'
                ORDER BY m.brand;
                """;

                var tabel = new DataTable();
                tabel.Columns.Add("brand", typeof(string));
                tabel.Rows.Add("");

                using var reader = perintah.ExecuteReader();
                while (reader.Read())
                    tabel.Rows.Add(reader["brand"]);

                cmbMerek.DataSource = tabel;
                cmbMerek.DisplayMember = "brand";
                cmbMerek.ValueMember = "brand";
            }

            // Filter kategori
            using (var perintah = koneksi.CreateCommand())
            {
                perintah.CommandText = """
            SELECT DISTINCT m.category
            FROM computer_models m
            JOIN computer_units u ON u.model_id = m.id
            WHERE u.status = 'Tersedia'
            ORDER BY m.category;
            """;

                var tabel = new DataTable();
                tabel.Columns.Add("category", typeof(string));
                tabel.Rows.Add("");

                using var reader = perintah.ExecuteReader();
                while (reader.Read())
                    tabel.Rows.Add(reader["category"]);

                cmbKategori.DataSource = tabel;
                cmbKategori.DisplayMember = "category";
                cmbKategori.ValueMember = "category";
            }
        }



        private void BangunForm()
        {
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            Text = "Penjualan Komputer";
            MinimumSize = new Size(1000, 650);

            // ===== HEADER =====
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Color.FromArgb(23, 105, 170),
                Padding = new Padding(22, 10, 22, 8)
            };

            var lblJudul = new Label
            {
                Text = "Penjualan Komputer",
                Dock = DockStyle.Top,
                Height = 50,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };
            header.Controls.Add(lblJudul);

            // ===== FOOTER =====
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            chkPengingatScan.Text =
                "Ingat scan di aplikasi sales terkait (eSales / Acer / Lenovo)";
            chkPengingatScan.AutoSize = true;
            chkPengingatScan.Dock = DockStyle.Left;

            btnSimpan.Text = "Simpan Penjualan";
            btnSimpan.Size = new Size(190, 42);
            btnSimpan.Dock = DockStyle.Right;
            btnSimpan.BackColor = Color.FromArgb(23, 105, 170);
            btnSimpan.ForeColor = Color.White;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSimpan.Margin = new Padding(0);

            btnSimpan.Click += (_, _) => SimpanPenjualan();

            footer.Controls.Add(btnSimpan);
            footer.Controls.Add(chkPengingatScan);

            // ===== KONTEN UTAMA: DUA KOLOM =====
            var konten = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 12, 18, 12),
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            konten.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 48));
            konten.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 52));

            var panelProduk = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 10, 0)
            };

            var panelKeranjangUtama = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                Margin = new Padding(10, 0, 0, 0)
            };


            // ===== KOLOM KIRI: PRODUK DAN HARGA =====
            var kiri = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(2, 0, 8, 10)
            };

            Label BuatJudul(string teks)
            {
                return new Label
                {
                    Text = teks,
                    Width = 700,
                    Height = 38,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 0, 0, 8)
                };
            }

            Panel BuatField(string teks, Control input, int tinggi = 62)
            {
                var field = new Panel
                {
                    Width = 500,
                    Height = tinggi,
                    Margin = new Padding(0, 0, 0, 12),
                    BackColor = Color.White
                };

                var label = new Label
                {
                    Text = teks,
                    Dock = DockStyle.Top,
                    Height = 23,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                input.Dock = DockStyle.Bottom;
                input.Height = 32;
                input.Margin = new Padding(0);

                if (input is ComboBox combo)
                {
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.Font = new Font("Segoe UI", 9.5F);
                }
                else if (input is TextBox textBox)
                {
                    textBox.Font = new Font("Segoe UI", 9.5F);
                }

                field.Controls.Add(input);
                field.Controls.Add(label);

                return field;
            }

            //kiri.Controls.Add(BuatJudul("Pilih Produk"));

            txtCariModel.PlaceholderText =
                "Ketik merek, model, atau spesifikasi...";

            kiri.Controls.Add(BuatField("Merek", cmbMerek));
            kiri.Controls.Add(BuatField("Kategori", cmbKategori));
            kiri.Controls.Add(BuatField("Cari model", txtCariModel, 66));
            kiri.Controls.Add(BuatField("Model komputer", cmbModel));
            kiri.Controls.Add(BuatField("Serial Number (SN)", cmbSN));
            kiri.Controls.Add(BuatField("Tipe harga", cmbTipeHarga));

            lblHargaAcuan.Dock = DockStyle.Bottom;
            lblHargaAcuan.Height = 32;
            lblHargaAcuan.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            lblHargaAcuan.ForeColor =
                Color.FromArgb(23, 105, 170);
            lblHargaAcuan.TextAlign = ContentAlignment.MiddleLeft;

            kiri.Controls.Add(
                BuatField("Harga acuan", lblHargaAcuan, 55));

            kiri.Controls.Add(
                BuatField("Harga jual (Rp)", txtHargaJual));

            kiri.Controls.Add(
                BuatField("Catatan", txtCatatan));

            // Tombol aksi dalam satu baris
            var panelAksi = new TableLayoutPanel
            {
                Width = 400,
                Height = 44,
                //ColumnCount = 2,
                //RowCount = 1,
                Margin = new Padding(0, 5, 0, 0),
                BackColor = Color.White
            };

            //panelAksi.ColumnStyles.Add(
            //    new ColumnStyle(SizeType.Percent, 100));
            //panelAksi.ColumnStyles.Add(
            //    new ColumnStyle(SizeType.Percent, 35));

            btnTambahKeranjang.Text = "+ Tambah ke Keranjang";
            btnTambahKeranjang.Dock = DockStyle.Fill;
            btnTambahKeranjang.Margin = new Padding(0, 0, 6, 0);
            btnTambahKeranjang.BackColor =
                Color.FromArgb(23, 105, 170);
            btnTambahKeranjang.ForeColor = Color.White;
            btnTambahKeranjang.FlatStyle = FlatStyle.Flat;
            btnTambahKeranjang.FlatAppearance.BorderSize = 0;
            btnTambahKeranjang.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            //btnHapusKeranjang.Text = "Hapus Pilihan";
            //btnHapusKeranjang.Dock = DockStyle.Fill;
            //btnHapusKeranjang.Margin = new Padding(6, 0, 0, 0);
            //btnHapusKeranjang.BackColor =
            //    Color.FromArgb(220, 75, 75);
            //btnHapusKeranjang.ForeColor = Color.White;
            //btnHapusKeranjang.FlatStyle = FlatStyle.Flat;
            //btnHapusKeranjang.FlatAppearance.BorderSize = 0;
            //btnHapusKeranjang.Font =
            //    new Font("Segoe UI", 9F, FontStyle.Bold);

            panelAksi.Controls.Add(btnTambahKeranjang);
            //panelAksi.Controls.Add(btnHapusKeranjang, 1, 0);

            kiri.Controls.Add(panelAksi);



            // ===== KOLOM KANAN: KERANJANG =====

            var kanan = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            kanan.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            kanan.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            kanan.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));

            var lblKeranjang = new Label
            {
                Text = "Keranjang Penjualan",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Panel tempat card ditampilkan
            panelDaftarCard.Dock = DockStyle.Fill;
            panelDaftarCard.FlowDirection = FlowDirection.TopDown;
            panelDaftarCard.WrapContents = false;
            panelDaftarCard.AutoScroll = true;
            panelDaftarCard.Padding = new Padding(3, 5, 8, 8);
            panelDaftarCard.BackColor = Color.FromArgb(245, 247, 250);

            // Total dan informasi keranjang
            var panelRingkasan = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 0)
            };

            var lblInfoKeranjang = new Label
            {
                Name = "lblInfoKeranjang",
                Text = "Belum ada unit di keranjang",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 9F)
            };

            var lblTotalKeranjang = new Label
            {
                Name = "lblTotalKeranjang",
                Text = "Total: Rp 0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(23, 105, 170),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            };

            panelRingkasan.Controls.Add(lblTotalKeranjang);
            panelRingkasan.Controls.Add(lblInfoKeranjang);

            kanan.Controls.Add(lblKeranjang, 0, 0);
            kanan.Controls.Add(panelDaftarCard, 0, 1);
            kanan.Controls.Add(panelRingkasan, 0, 2);

            panelProduk.Controls.Add(kiri);
            panelKeranjangUtama.Controls.Add(kanan);

            konten.Controls.Add(panelProduk, 0, 0);
            konten.Controls.Add(panelKeranjangUtama, 1, 0);


            // ===== SUSUN FORM =====
            Controls.Clear();
            Controls.Add(konten);
            Controls.Add(footer);
            Controls.Add(header);

            cmbTipeHarga.SelectedIndexChanged +=
                (_, _) => MuatHargaAcuan();
        }


        private void MuatCardKeranjang()
        {
            panelDaftarCard.SuspendLayout();
            panelDaftarCard.Controls.Clear();

            long total = 0;

            foreach (DataGridViewRow baris in dgvKeranjang.Rows)
            {
                if (baris.IsNewRow) continue;

                int unitId = Convert.ToInt32(
                    baris.Cells["unit_id"].Value);

                string model = Convert.ToString(
                    baris.Cells["model"].Value) ?? "-";

                string sn = Convert.ToString(
                    baris.Cells["sn"].Value) ?? "-";

                int tipeHargaId = Convert.ToInt32(
                    baris.Cells["tipe_harga_id"].Value);

                string teksHarga = Convert.ToString(
                    baris.Cells["harga"].Value) ?? "0";

                string angkaHarga = new string(
                    teksHarga.Where(char.IsDigit).ToArray());

                long harga = long.TryParse(angkaHarga, out long nilai)
                    ? nilai : 0;

                total += harga;

                string namaTipe = cmbTipeHarga.Items.Count > 0
                    ? CariNamaTipeHarga(tipeHargaId)
                    : "-";

                string catatan = Convert.ToString(
                    baris.Cells["catatan"].Value) ?? "-";

                var card = new Panel
                {
                    Width = Math.Max(250, panelDaftarCard.ClientSize.Width - 20),
                    Height = 132,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 10),
                    Padding = new Padding(12),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = unitId
                };

                var btnHapus = new Button
                {
                    Text = "✕",
                    Width = 30,
                    Height = 28,
                    Dock = DockStyle.Right,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.Firebrick,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };

                btnHapus.FlatAppearance.BorderSize = 0;
                btnHapus.Click += (_, _) =>
                {
                    foreach (DataGridViewRow baris in dgvKeranjang.Rows)
                    {
                        if (baris.IsNewRow) continue;

                        if (Convert.ToInt32(
                                baris.Cells["unit_id"].Value) == unitId)
                        {
                            dgvKeranjang.Rows.Remove(baris);
                            break;
                        }
                    }

                    MuatCardKeranjang();
                };

                var lblModel = new Label
                {
                    Text = model,
                    Dock = DockStyle.Top,
                    Height = 28,
                    AutoEllipsis = true,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59)
                };

                var lblSN = new Label
                {
                    Text = "SN: " + sn,
                    Dock = DockStyle.Top,
                    Height = 25,
                    AutoEllipsis = true,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(71, 85, 105)
                };

                var lblTipe = new Label
                {
                    Text = "Tipe harga: " + namaTipe,
                    Dock = DockStyle.Top,
                    Height = 22,
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                var lblCatatan = new Label
                {
                    Text = "Catatan: " + catatan,
                    Dock = DockStyle.Top,
                    Height = 22,
                    Font = new Font("Segoe UI", 6.5F),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };

                var lblHarga = new Label
                {
                    Text = "Rp " + harga.ToString("N0"),
                    Dock = DockStyle.Bottom,
                    Height = 27,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(23, 105, 170)
                };

                card.Controls.Add(lblHarga);
                card.Controls.Add(lblTipe);
                card.Controls.Add(lblSN);
                card.Controls.Add(lblModel);
                card.Controls.Add(lblCatatan);
                card.Controls.Add(btnHapus);
                panelDaftarCard.Controls.Add(card);
            }

            var lblTotal = Controls
                .Find("lblTotalKeranjang", true)
                .FirstOrDefault() as Label;

            if (lblTotal != null)
                lblTotal.Text = $"Total: Rp {total:N0}";

            var lblInfo = Controls
            .Find("lblInfoKeranjang", true)
            .FirstOrDefault() as Label;

            if (lblInfo != null)
            {
                int jumlahUnit = dgvKeranjang.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => !r.IsNewRow);

                lblInfo.Text = jumlahUnit == 0
                    ? "Belum ada unit di keranjang"
                    : $"{jumlahUnit} unit di keranjang";

                lblInfo.Visible = true;
            }

            panelDaftarCard.ResumeLayout();
        }

        private string CariNamaTipeHarga(int tipeHargaId)
        {
            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText =
                "SELECT name FROM price_types WHERE id = $id";

            perintah.Parameters.AddWithValue("$id", tipeHargaId);

            return Convert.ToString(perintah.ExecuteScalar()) ?? "-";
        }


        private void TambahKeKeranjang()
        {
            if (cmbModel.SelectedValue == null ||
                cmbSN.SelectedValue == null ||
                cmbTipeHarga.SelectedValue == null)
            {
                MessageBox.Show("Pilih model, serial number, dan tipe harga dulu.");
                return;
            }

            if (!int.TryParse(Convert.ToString(cmbSN.SelectedValue), out int unitId))
                return;

            if (!decimal.TryParse(
                txtHargaJual.Text.Replace(".", "").Replace(",", ""),
                out decimal harga) || harga <= 0)
            {
                MessageBox.Show("Masukkan harga jual yang valid.");
                return;
            }

            foreach (DataGridViewRow baris in dgvKeranjang.Rows)
            {
                if (Convert.ToInt32(baris.Cells["unit_id"].Value) == unitId)
                {
                    MessageBox.Show("Serial number ini sudah ada di keranjang.");
                    return;
                }
            }

            if (dgvKeranjang.Columns.Count == 0)
            {
                dgvKeranjang.Columns.Add("unit_id", "ID Unit");
                dgvKeranjang.Columns.Add("model", "Model");
                dgvKeranjang.Columns.Add("sn", "Serial Number");
                dgvKeranjang.Columns.Add("tipe_harga_id", "ID Tipe Harga");
                dgvKeranjang.Columns.Add("harga", "Harga Jual");
                dgvKeranjang.Columns.Add("catatan", "Catatan");
                dgvKeranjang.Columns["catatan"].Visible = false;

                dgvKeranjang.Columns["unit_id"].Visible = false;
                dgvKeranjang.Columns["tipe_harga_id"].Visible = false;
            }

            dgvKeranjang.Rows.Add(
                unitId,
                cmbModel.Text,
                cmbSN.Text,
                Convert.ToInt32(cmbTipeHarga.SelectedValue),
                harga,
                txtCatatan.Text.Trim()
            );
            MuatCardKeranjang();

            MessageBox.Show("Berhasil ditambahkan ke keranjang.");
        }


        private bool sedangMemformatHarga = false;

        private void TxtHargaJual_TextChanged(object? sender, EventArgs e)
        {
            if (sedangMemformatHarga)
                return;

            sedangMemformatHarga = true;

            try
            {
                int posisiKursor = txtHargaJual.SelectionStart;

                string angka = new string(
                    txtHargaJual.Text
                        .Where(char.IsDigit)
                        .ToArray());

                if (string.IsNullOrEmpty(angka))
                {
                    txtHargaJual.Text = "";
                    return;
                }

                if (!long.TryParse(angka, out long harga))
                {
                    txtHargaJual.Text = "";
                    return;
                }

                string hasil = harga.ToString("N0");

                txtHargaJual.Text = hasil;
                txtHargaJual.SelectionStart = Math.Min(
                    posisiKursor,
                    txtHargaJual.Text.Length);
            }
            finally
            {
                sedangMemformatHarga = false;
            }
        }

        private void MuatModel()
        {
            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            string merek = cmbMerek.SelectedValue?.ToString() ?? "";
            string kategori = cmbKategori.SelectedValue?.ToString() ?? "";
            string cari = txtCariModel.Text.Trim();

            perintah.CommandText = """
        SELECT
            m.id,
            m.category || ' | ' ||
            m.brand || ' ' ||
            m.model_name || ' | ' ||
            COALESCE(m.processor, '') || ' | ' ||
            COALESCE(m.ram_gb, 0) || ' GB | ' ||
            COALESCE(m.storage_type, '') || ' ' ||
            COALESCE(m.storage_gb, 0) || ' GB'
            AS nama_model
        FROM computer_models m
        JOIN computer_units u ON u.model_id = m.id
        WHERE u.status = 'Tersedia'
          AND ($merek = '' OR m.brand = $merek)
          AND ($kategori = '' OR m.category = $kategori)
          AND (
              $cari = ''
              OR m.brand LIKE $pola
              OR m.model_name LIKE $pola
              OR COALESCE(m.processor, '') LIKE $pola
          )
        GROUP BY m.id
        ORDER BY m.brand, m.model_name;
        """;

            perintah.Parameters.AddWithValue("$merek", merek);
            perintah.Parameters.AddWithValue("$kategori", kategori);
            perintah.Parameters.AddWithValue("$cari", cari);
            perintah.Parameters.AddWithValue("$pola", "%" + cari + "%");

            using var reader = perintah.ExecuteReader();

            var tabel = new DataTable();
            tabel.Load(reader);

            cmbModel.DataSource = tabel;
            cmbModel.DisplayMember = "nama_model";
            cmbModel.ValueMember = "id";
        }

        private void MuatSN()
        {
            cmbSN.DataSource = null;

            if (cmbModel.SelectedValue == null ||
                !int.TryParse(
                    Convert.ToString(cmbModel.SelectedValue),
                    out int modelId))
            {
                return;
            }

            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText = """
                SELECT id, serial_number
                FROM computer_units
                WHERE model_id = $modelId
                  AND status = 'Tersedia'
                ORDER BY created_at ASC;
            """;

            perintah.Parameters.AddWithValue("$modelId", modelId);

            using var reader = perintah.ExecuteReader();
            var tabel = new DataTable();
            tabel.Load(reader);

            cmbSN.DataSource = tabel;
            cmbSN.DisplayMember = "serial_number";
            cmbSN.ValueMember = "id";
        }
        private void MuatTipeHarga()
        {
            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText = """
    SELECT id, name
    FROM price_types
    WHERE is_active = 1
    ORDER BY CASE name
        WHEN 'Kantor' THEN 1
        WHEN 'Umum' THEN 2
        WHEN 'Proyek' THEN 3
        ELSE 4
    END, name;
""";

            using var reader = perintah.ExecuteReader();
            var tabel = new DataTable();
            tabel.Load(reader);

            cmbTipeHarga.DataSource = tabel;
            cmbTipeHarga.DisplayMember = "name";
            cmbTipeHarga.ValueMember = "id";
        }
        private void MuatHargaAcuan()
        {
            if (cmbSN.SelectedValue == null ||
            cmbTipeHarga.SelectedValue == null)
            {
                lblHargaAcuan.Text = "-";
                txtHargaJual.Clear();
                return;
            }

            if (!int.TryParse(
                Convert.ToString(cmbSN.SelectedValue),
                out int unitId))
                return;

            if (!int.TryParse(
                Convert.ToString(cmbTipeHarga.SelectedValue),
                out int tipeHargaId))
                return;

            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText = """
                SELECT COALESCE(mp.price, 0)
                FROM computer_units u
                JOIN model_prices mp ON mp.model_id = u.model_id
                WHERE u.id = $unitId
                  AND mp.price_type_id = $typeId;
            """;

            perintah.Parameters.AddWithValue("$unitId", unitId);
            perintah.Parameters.AddWithValue("$typeId", tipeHargaId);

            object? hasil = perintah.ExecuteScalar();

            long harga = hasil == null || hasil == DBNull.Value
                ? 0
                : Convert.ToInt64(hasil);

            lblHargaAcuan.Text = "Rp " + harga.ToString("N0");
            txtHargaJual.Text = harga.ToString("N0");
        }

        private void SimpanPenjualan()
        {
            int jumlahUnit = dgvKeranjang.Rows
    .Cast<DataGridViewRow>()
    .Count(r => !r.IsNewRow);

            if (jumlahUnit == 0)
            {
                MessageBox.Show("Keranjang masih kosong.");
                return;
            }

            using var koneksi = Database.GetConnection();
            using var transaksi = koneksi.BeginTransaction();

            try
            {
                foreach (DataGridViewRow baris in dgvKeranjang.Rows)
                {
                    if (baris.IsNewRow)
                        continue;
                    int unitId = Convert.ToInt32(baris.Cells["unit_id"].Value);
                    int tipeHargaId = Convert.ToInt32(
                    baris.Cells["tipe_harga_id"].Value);

                    object? nilaiHarga = baris.Cells["harga"].Value;
                    string sn = Convert.ToString(
                        baris.Cells["sn"].Value) ?? "-";

                    if (!decimal.TryParse(
                            Convert.ToString(nilaiHarga),
                            System.Globalization.NumberStyles.Number,
                            System.Globalization.CultureInfo.CurrentCulture,
                            out decimal hargaDecimal) ||
                        hargaDecimal <= 0 ||
                        hargaDecimal != decimal.Truncate(hargaDecimal) ||
                        hargaDecimal > long.MaxValue)
                    {
                        throw new InvalidOperationException(
                            "Harga tidak valid untuk SN " + sn);
                    }

                    long hargaJual = (long)hargaDecimal;

                    // 1. Pastikan unit masih tersedia.
                    using var cekUnit = koneksi.CreateCommand();
                    cekUnit.Transaction = transaksi;
                    cekUnit.CommandText = """
                SELECT model_id
                FROM computer_units
                WHERE id = $unitId
                  AND status = 'Tersedia';
            """;
                    cekUnit.Parameters.AddWithValue("$unitId", unitId);

                    object? hasilUnit = cekUnit.ExecuteScalar();

                    if (hasilUnit == null || hasilUnit == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            $"Unit ID {unitId} sudah tidak tersedia.");
                    }

                    int modelId = Convert.ToInt32(hasilUnit);

                    // 2. Validasi tipe harga dan harga acuan.
                    using var cekHarga = koneksi.CreateCommand();
                    cekHarga.Transaction = transaksi;
                    cekHarga.CommandText = """
                SELECT mp.price
                FROM model_prices mp
                JOIN price_types pt ON pt.id = mp.price_type_id
                WHERE mp.model_id = $modelId
                  AND mp.price_type_id = $typeId
                  AND pt.is_active = 1;
            """;
                    cekHarga.Parameters.AddWithValue("$modelId", modelId);
                    cekHarga.Parameters.AddWithValue("$typeId", tipeHargaId);

                    object? hasilHarga = cekHarga.ExecuteScalar();

                    if (hasilHarga == null || hasilHarga == DBNull.Value ||
                        Convert.ToInt64(hasilHarga) <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Harga acuan unit ID {unitId} belum tersedia.");
                    }

                    // 3. Ubah status unit menjadi Terjual.
                    using var ubahStatus = koneksi.CreateCommand();
                    ubahStatus.Transaction = transaksi;
                    ubahStatus.CommandText = """
                UPDATE computer_units
                SET status = 'Terjual'
                WHERE id = $unitId
                  AND status = 'Tersedia';
            """;
                    ubahStatus.Parameters.AddWithValue("$unitId", unitId);

                    if (ubahStatus.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException(
                            $"Gagal memperbarui status unit ID {unitId}.");
                    }

                    // 4. Simpan penjualan.
                    using var simpanSales = koneksi.CreateCommand();
                    simpanSales.Transaction = transaksi;
                    simpanSales.CommandText = """
                INSERT INTO sales
                    (unit_id, price_type_id, sold_price, notes)
                VALUES
                    ($unitId, $typeId, $harga, $notes);
            """;
                    simpanSales.Parameters.AddWithValue("$unitId", unitId);
                    simpanSales.Parameters.AddWithValue("$typeId", tipeHargaId);
                    simpanSales.Parameters.AddWithValue("$harga", hargaJual);
                    simpanSales.Parameters.AddWithValue(
                        "$notes",
                        string.IsNullOrWhiteSpace(txtCatatan.Text)
                            ? DBNull.Value
                            : txtCatatan.Text.Trim());

                    simpanSales.ExecuteNonQuery();

                    // 5. Catat pergerakan stok.
                    using var simpanPergerakan = koneksi.CreateCommand();
                    simpanPergerakan.Transaction = transaksi;
                    simpanPergerakan.CommandText = """
                INSERT INTO stock_movements
                    (unit_id, movement_type, notes)
                VALUES
                    ($unitId, 'Penjualan', $notes);
            """;
                    simpanPergerakan.Parameters.AddWithValue("$unitId", unitId);
                    simpanPergerakan.Parameters.AddWithValue(
                        "$notes", "Penjualan melalui modul Sales");

                    simpanPergerakan.ExecuteNonQuery();
                }

                transaksi.Commit();

                MessageBox.Show(
                    $"Berhasil menyimpan penjualan {dgvKeranjang.Rows.Count} unit!",
                    "Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                try
                {
                    transaksi.Rollback();
                }
                catch
                {
                    // Transaksi mungkin sudah dibatalkan.
                }

                MessageBox.Show(
                    $"Penjualan gagal disimpan.\n\n{ex.Message}",
                    "Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
