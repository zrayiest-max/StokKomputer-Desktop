using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace StokKomputer
{
    public partial class FormModel : Form
    {
        private readonly TextBox txtMerek = new()
        {
            CharacterCasing = CharacterCasing.Upper
        };
        private readonly TextBox txtModel = new()
        {
            CharacterCasing = CharacterCasing.Upper
        };
        private readonly ComboBox cmbKategori = new();
        private readonly TextBox txtProsesor = new()
        {
            CharacterCasing = CharacterCasing.Upper
        };
        private readonly NumericUpDown numRam = new();
        private readonly ComboBox cmbJenisStorage = new();
        private readonly NumericUpDown numStorage = new();
        private readonly Button btnSimpan = new();
        private readonly Button btnBatal = new();
        private int? modelId;
        public FormModel()
        {
            InitializeComponent();

            Text = "Tambah Model Komputer";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 560);
            MinimumSize = new Size(520, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            BangunForm();
        }

        public FormModel(int idModel) : this()
        {
            modelId = idModel;

            Text = "Edit Model Komputer";
            btnSimpan.Text = "Simpan Perubahan";

            MuatDataModel();
        }

        private void MuatDataModel()
        {
            using var koneksi = Database.GetConnection();
            using var perintah = koneksi.CreateCommand();

            perintah.CommandText = """
                SELECT brand, model_name, category, processor,
                       ram_gb, storage_type, storage_gb
                FROM computer_models
                WHERE id = $id;
                """;

            perintah.Parameters.AddWithValue("$id", modelId!.Value);

            using var reader = perintah.ExecuteReader();

            if (!reader.Read())
            {
                MessageBox.Show("Model tidak ditemukan.");
                Close();
                return;
            }

            txtMerek.Text = reader["brand"].ToString();
            txtModel.Text = reader["model_name"].ToString();
            cmbKategori.SelectedItem = reader["category"].ToString();
            txtProsesor.Text = reader["processor"].ToString();
            numRam.Value = Convert.ToDecimal(reader["ram_gb"]);
            cmbJenisStorage.SelectedItem = reader["storage_type"].ToString();
            numStorage.Value = Convert.ToDecimal(reader["storage_gb"]);
        }

        private void BangunForm()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 9,
                AutoSize = false
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 8; i++)
                layout.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, 48));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            txtMerek.PlaceholderText = "Contoh: ASUS";
            txtModel.PlaceholderText = "Contoh: A3202WB";
            txtProsesor.PlaceholderText = "Contoh: Intel Core i5";

            cmbKategori.DropDownStyle =
                ComboBoxStyle.DropDownList;
            cmbKategori.Items.AddRange(
                new object[] { "Laptop", "AIO" });
            cmbKategori.SelectedIndex = 0;

            cmbJenisStorage.DropDownStyle =
                ComboBoxStyle.DropDownList;
            cmbJenisStorage.Items.AddRange(
                new object[] { "SSD", "HDD" });
            cmbJenisStorage.SelectedIndex = 0;

            numRam.Minimum = 0;
            numRam.Maximum = 1024;
            numRam.Increment = 8;
            numRam.Value = 8;

            numStorage.Minimum = 0;
            numStorage.Maximum = 100000;
            numStorage.Increment = 128;
            numStorage.Value = 512;

            TambahBaris(layout, 0, "Merek", txtMerek);
            TambahBaris(layout, 1, "Model", txtModel);
            TambahBaris(layout, 2, "Kategori", cmbKategori);
            TambahBaris(layout, 3, "Prosesor", txtProsesor);
            TambahBaris(layout, 4, "RAM (GB)", numRam);
            TambahBaris(layout, 5, "Jenis storage", cmbJenisStorage);
            TambahBaris(layout, 6, "Storage (GB)", numStorage);

            btnBatal.Text = "Batal";
            btnBatal.Height = 36;
            btnBatal.Width = 100;
            btnBatal.Click += (_, _) => Close();

            btnSimpan.Text = "Simpan Model";
            btnSimpan.Height = 36;
            btnSimpan.Width = 140;
            btnSimpan.Click += (_, _) => SimpanModel();

            var tombol = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            tombol.Controls.Add(btnSimpan);
            tombol.Controls.Add(btnBatal);

            layout.Controls.Add(tombol, 0, 8);
            layout.SetColumnSpan(tombol, 2);

            Controls.Add(layout);
        }

        private void TambahBaris(
           TableLayoutPanel layout,
           int baris,
           string label,
           Control input)
        {
            var teks = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10)
            };

            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 7, 3, 7);

            layout.Controls.Add(teks, 0, baris);
            layout.Controls.Add(input, 1, baris);
        }


        private void SimpanModel()
        {
            string merek = txtMerek.Text.Trim();
            string model = txtModel.Text.Trim();
            string prosesor = txtProsesor.Text.Trim();

            if (string.IsNullOrWhiteSpace(merek) ||
                string.IsNullOrWhiteSpace(model))
            {
                MessageBox.Show(
                    "Merek dan model wajib diisi.",
                    "Data belum lengkap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={Database.DatabasePath}");

                connection.Open();

                using var command = connection.CreateCommand();

                if (modelId.HasValue)
                {
                    // Mode edit
                    command.CommandText = """
                UPDATE computer_models
                SET brand = $brand,
                    model_name = $model,
                    category = $category,
                    processor = $processor,
                    ram_gb = $ram,
                    storage_type = $storageType,
                    storage_gb = $storage
                WHERE id = $id;
                """;

                    command.Parameters.AddWithValue("$id", modelId.Value);
                }
                else
                {
                    // Mode tambah
                    command.CommandText = """
                INSERT INTO computer_models
                (
                    brand, model_name, category,
                    processor, ram_gb,
                    storage_type, storage_gb
                )
                VALUES
                (
                    $brand, $model, $category,
                    $processor, $ram,
                    $storageType, $storage
                );
                """;
                }

                command.Parameters.AddWithValue("$brand", merek);
                command.Parameters.AddWithValue("$model", model);
                command.Parameters.AddWithValue(
                    "$category", cmbKategori.Text);
                command.Parameters.AddWithValue(
                    "$processor", prosesor);
                command.Parameters.AddWithValue(
                    "$ram", (int)numRam.Value);
                command.Parameters.AddWithValue(
                    "$storageType", cmbJenisStorage.Text);
                command.Parameters.AddWithValue(
                    "$storage", (int)numStorage.Value);

                int hasil = command.ExecuteNonQuery();

                if (modelId.HasValue && hasil == 0)
                {
                    MessageBox.Show("Model tidak ditemukan.");
                    return;
                }

                MessageBox.Show(
                    modelId.HasValue
                        ? "Model komputer berhasil diperbarui!"
                        : "Model komputer berhasil disimpan!",
                    "Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqliteException ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan model:\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}
