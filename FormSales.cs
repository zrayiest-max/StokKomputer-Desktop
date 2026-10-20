using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using System.Linq;

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
        public FormSales()
        {
            InitializeComponent();
            Text = "Penjualan Komputer";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 500);
            MinimumSize = new Size(600, 400);

            BangunForm();
            txtHargaJual.TextChanged += TxtHargaJual_TextChanged;
            MuatModel();
            MuatTipeHarga();
            cmbModel.SelectedIndexChanged += (_, _) =>
            {
                MuatSN();
                MuatHargaAcuan();
            };

            cmbSN.SelectedIndexChanged += (_, _) => MuatHargaAcuan();
        }
        private void BangunForm()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 8,
                AutoScroll = true
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 150));

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            // Pilih model
            layout.Controls.Add(new Label
            {
                Text = "Model komputer",
                AutoSize = true
            }, 0, 0);

            cmbModel.Dock = DockStyle.Fill;
            cmbModel.DropDownStyle = ComboBoxStyle.DropDownList;
            layout.Controls.Add(cmbModel, 1, 0);

            // Pilih SN
            layout.Controls.Add(new Label
            {
                Text = "Serial Number (SN)",
                AutoSize = true
            }, 0, 1);

            cmbSN.Dock = DockStyle.Fill;
            cmbSN.DropDownStyle = ComboBoxStyle.DropDownList;
            layout.Controls.Add(cmbSN, 1, 1);

            layout.Controls.Add(new Label
            {
                Text = "Tipe harga",
                AutoSize = true
            }, 0, 1);

            cmbTipeHarga.Dock = DockStyle.Fill;
            cmbTipeHarga.DropDownStyle = ComboBoxStyle.DropDownList;
            layout.Controls.Add(cmbTipeHarga, 1, 1);

            layout.Controls.Add(new Label
            {
                Text = "Harga acuan",
                AutoSize = true
            }, 0, 2);

            lblHargaAcuan.AutoSize = true;
            layout.Controls.Add(lblHargaAcuan, 1, 2);

            layout.Controls.Add(new Label
            {
                Text = "Harga jual",
                AutoSize = true
            }, 0, 3);

            txtHargaJual.Dock = DockStyle.Fill;
            layout.Controls.Add(txtHargaJual, 1, 3);

            layout.Controls.Add(new Label
            {
                Text = "Catatan",
                AutoSize = true
            }, 0, 4);

            txtCatatan.Dock = DockStyle.Fill;
            layout.Controls.Add(txtCatatan, 1, 4);

            chkPengingatScan.Text =
            "Ingat scan di aplikasi sales terkait (eSales / Acer / Lenovo)";

            chkPengingatScan.AutoSize = true;
            chkPengingatScan.Checked = false;

            layout.Controls.Add(chkPengingatScan, 0, 6);
            layout.SetColumnSpan(chkPengingatScan, 2);

            btnSimpan.Text = "Simpan Penjualan";
            btnSimpan.Size = new Size(200, 40);
            btnSimpan.Anchor = AnchorStyles.None;
            btnSimpan.Margin = new Padding(0);
            btnSimpan.Click += (_, _) => SimpanPenjualan();

            var panelTombol = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            panelTombol.Controls.Add(btnSimpan);

            layout.Controls.Add(panelTombol, 0, 7);
            layout.SetColumnSpan(panelTombol, 2);

            Controls.Add(layout);

            cmbSN.SelectedIndexChanged += (_, _) => MuatHargaAcuan();
            cmbTipeHarga.SelectedIndexChanged += (_, _) => MuatHargaAcuan();
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

            perintah.CommandText = """
                SELECT DISTINCT
                    m.id,
                    m.brand || ' ' ||
                    m.model_name || ' | ' ||
                    m.processor || ' | RAM ' ||
                    m.ram_gb || ' GB | ' ||
                    m.storage_type || ' ' ||
                    m.storage_gb || ' GB' AS nama_model
                FROM computer_models m
                JOIN computer_units u ON u.model_id = m.id
                WHERE u.status = 'Tersedia'
                ORDER BY m.brand, m.model_name;
            """;

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
                ORDER BY serial_number;
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
                ORDER BY id;
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
            if (cmbSN.SelectedValue == null ||
                cmbTipeHarga.SelectedValue == null)
            {
                MessageBox.Show(
                    "Pilih unit dan tipe harga terlebih dahulu.");
                return;
            }

            if (!int.TryParse(
                Convert.ToString(cmbSN.SelectedValue),
                out int unitId) ||
                !int.TryParse(
                Convert.ToString(cmbTipeHarga.SelectedValue),
                out int tipeHargaId))
            {
                MessageBox.Show("Pilihan unit atau tipe harga tidak valid.");
                return;
            }

            string angkaHarga = new string(
                txtHargaJual.Text.Where(char.IsDigit).ToArray());

            if (!long.TryParse(angkaHarga, out long hargaJual) ||
                hargaJual <= 0)
            {
                MessageBox.Show(
                    "Harga jual harus lebih besar dari nol.");
                txtHargaJual.Focus();
                return;
            }

            using var koneksi = Database.GetConnection();
            using var transaksi = koneksi.BeginTransaction();

            try
            {
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
                        "Unit sudah tidak tersedia. Muat ulang form Sales.");
                }

                int modelId = Convert.ToInt32(hasilUnit);

                // 2. Pastikan tipe harga aktif dan harga acuan tersedia.
                using var cekHarga = koneksi.CreateCommand();
                cekHarga.Transaction = transaksi;
                cekHarga.CommandText = """
                    SELECT mp.price
                    FROM model_prices mp
                    JOIN price_types pt
                        ON pt.id = mp.price_type_id
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
                        "Harga acuan belum diisi atau tipe harga tidak aktif.");
                }

                // 3. Ubah status unit. Hanya unit tersedia yang bisa dijual.
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
                        "Status unit berubah. Penjualan dibatalkan.");
                }

                // 4. Simpan transaksi penjualan dan harga aktual.
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
                    "$notes",
                    "Penjualan melalui modul Sales");

                simpanPergerakan.ExecuteNonQuery();

                // Semua perubahan berhasil.
                transaksi.Commit();

                MessageBox.Show(
                    "Penjualan berhasil disimpan!",
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
                    // Transaksi mungkin sudah tidak aktif.
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
