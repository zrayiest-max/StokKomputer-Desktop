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
    public partial class FormUnit : Form
    {
        private ComboBox cmbModel = new();
        private TextBox txtSerialNumbers = new();
        private NumericUpDown numHargaModal = new();
        private DateTimePicker dtpTanggalMasuk = new();
        private Button btnSimpan = new();
        private Button btnBatal = new();

        public FormUnit(int? modelId = null)
        {
            InitializeComponent();
            Text = "Tambah Unit Komputer";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(620, 550);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BangunForm();
            MuatModel();

            if (modelId.HasValue)
            {
                for (int i = 0; i < cmbModel.Items.Count; i++)
                {
                    if (cmbModel.Items[i] is PilihanModel model
                        && model.Id == modelId.Value)
                    {
                        cmbModel.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void BangunForm()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 6
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 145));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Model
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190)); // Serial number
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Harga modal
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Tanggal
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Tombol
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            cmbModel.DropDownStyle =
                ComboBoxStyle.DropDownList;
            cmbModel.Dock = DockStyle.Fill;

            txtSerialNumbers.Multiline = true;
            txtSerialNumbers.ScrollBars = ScrollBars.Vertical;
            txtSerialNumbers.Dock = DockStyle.Fill;
            txtSerialNumbers.Font = new Font("Consolas", 11);

            numHargaModal.Dock = DockStyle.Fill;
            numHargaModal.Minimum = 0;
            numHargaModal.Maximum = 100000000000;
            numHargaModal.ThousandsSeparator = true;
            numHargaModal.Increment = 1000;

            dtpTanggalMasuk.Dock = DockStyle.Fill;
            dtpTanggalMasuk.Format =
                DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "dd MMMM yyyy";

            TambahBaris(layout, 0, "Model komputer", cmbModel);
            TambahBaris(layout, 1, "Serial number", txtSerialNumbers);
            TambahBaris(layout, 2, "Harga modal (Rp)", numHargaModal);
            TambahBaris(layout, 3, "Tanggal masuk", dtpTanggalMasuk);

            var panelTombol = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            btnSimpan.Text = "Simpan Unit";
            btnSimpan.Width = 110;
            btnSimpan.Height = 32;

            btnBatal.Text = "Batal";
            btnBatal.Width = 80;
            btnBatal.Height = 32;

            btnSimpan.Click += (_, _) => SimpanUnit();
            btnBatal.Click += (_, _) => Close();

            panelTombol.Controls.Add(btnSimpan);
            panelTombol.Controls.Add(btnBatal);

            layout.Controls.Add(panelTombol, 0, 4);
            layout.SetColumnSpan(panelTombol, 2);

            Controls.Add(layout);
        }

        private void TambahBaris(
            TableLayoutPanel layout,
            int baris,
            string teks,
            Control input)
        {
            var label = new Label
            {
                Text = teks,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(label, 0, baris);
            layout.Controls.Add(input, 1, baris);
        }

        private void MuatModel()
        {
            cmbModel.Items.Clear();

            using var koneksi = Database.GetConnection();

            using var perintah = koneksi.CreateCommand();
            perintah.CommandText = @"
                SELECT id, brand, model_name
                FROM computer_models
                ORDER BY brand, model_name;
            ";

            using var reader = perintah.ExecuteReader();

            while (reader.Read())
            {
                cmbModel.Items.Add(new PilihanModel
                {
                    Id = reader.GetInt32(0),
                    Nama = $"{reader.GetString(1)} - {reader.GetString(2)}"
                });
            }

            if (cmbModel.Items.Count > 0)
                cmbModel.SelectedIndex = 0;
        }

        private void SimpanUnit()
        {
            if (cmbModel.SelectedItem is not PilihanModel model)
            {
                MessageBox.Show(
                    "Pilih model komputer terlebih dahulu.");
                return;
            }

            // Ambil semua SN, satu SN per baris.
            string[] serialNumbers = txtSerialNumbers.Lines
                .Select(sn => sn.Trim())
                .Where(sn => !string.IsNullOrWhiteSpace(sn))
                .ToArray();

            if (serialNumbers.Length == 0)
            {
                MessageBox.Show("Masukkan minimal satu serial number.");
                txtSerialNumbers.Focus();
                return;
            }

            // Periksa SN duplikat dalam input yang sama.
            var duplikat = serialNumbers
                .GroupBy(sn => sn, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(grup => grup.Count() > 1);

            if (duplikat != null)
            {
                MessageBox.Show(
                    $"Serial number {duplikat.Key} ditulis lebih dari sekali.",
                    "Serial Number Duplikat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var koneksi = Database.GetConnection();

                using var transaksi = koneksi.BeginTransaction();



                foreach (string serial in serialNumbers)
                {
                    using var perintah = koneksi.CreateCommand();
                    perintah.Transaction = transaksi;

                    perintah.CommandText = @"
                        INSERT INTO computer_units
                            (model_id, serial_number,
                             purchase_price, purchase_date)
                        VALUES
                            ($modelId, $serial, $harga, $tanggal);
                    ";

                    perintah.Parameters.AddWithValue(
                        "$modelId", model.Id);

                    perintah.Parameters.AddWithValue(
                        "$serial", serial);

                    perintah.Parameters.AddWithValue(
                        "$harga", (long)numHargaModal.Value);

                    perintah.Parameters.AddWithValue(
                        "$tanggal",
                        dtpTanggalMasuk.Value.ToString("yyyy-MM-dd"));

                    perintah.ExecuteNonQuery();

                    // Ambil ID unit yang baru ditambahkan.
                    long unitId;

                    using (var perintahId = koneksi.CreateCommand())
                    {
                        perintahId.Transaction = transaksi;
                        perintahId.CommandText =
                            "SELECT last_insert_rowid();";

                        unitId = (long)perintahId.ExecuteScalar()!;
                    }

                    // Catat pergerakan stok masuk.
                    using var perintahRiwayat = koneksi.CreateCommand();
                    perintahRiwayat.Transaction = transaksi;

                    perintahRiwayat.CommandText = @"
                        INSERT INTO stock_movements
                            (unit_id, movement_type, movement_date, notes)
                        VALUES
                            ($unitId, 'Masuk', $tanggal, $notes);
                    ";

                    perintahRiwayat.Parameters.AddWithValue(
                        "$unitId", unitId);

                    perintahRiwayat.Parameters.AddWithValue(
                        "$tanggal",
                        dtpTanggalMasuk.Value.ToString("yyyy-MM-dd"));

                    perintahRiwayat.Parameters.AddWithValue(
                        "$notes", "Penambahan unit komputer");

                    perintahRiwayat.ExecuteNonQuery();
                }


                transaksi.Commit();

                MessageBox.Show(
            $"{serialNumbers.Length} unit berhasil disimpan!",
            "Berhasil",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqliteException ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan unit.\n\n" +
                    $"Kode SQLite: {ex.SqliteErrorCode}\n" +
                    $"Pesan: {ex.Message}\n\n" +
                    "Tidak ada unit dari batch ini yang disimpan.",
                    "Error Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Gagal menyimpan unit:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private class PilihanModel
        {
            public int Id { get; set; }
            public string Nama { get; set; } = "";

            public override string ToString() => Nama;
        }
    }
}
