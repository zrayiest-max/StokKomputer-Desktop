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
    public partial class FormDetailModel : Form
    {
        private readonly int modelId;

        private Label lblNama = new();
        private Label lblSpesifikasi = new();
        private Label lblTotalUnit = new();
        private Label lblStokTersedia = new();
        private DataGridView grid = new();
        private Button btnTambahUnit = new();
        private readonly DataGridView gridHarga = new();
        private readonly Button btnSimpanHarga = new();

        public FormDetailModel(int id)
        {
            InitializeComponent();
            modelId = id;

            Text = "Detail Model Komputer";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 650);
            MinimumSize = new Size(800, 500);

            BangunForm();
            MuatDetail();
            MuatUnit();
            MuatHarga();
        }

        private void BangunForm()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 125,
                BackColor = Color.FromArgb(30, 64, 175),
                Padding = new Padding(20)
            };

            lblNama.Text = "Detail Model";
            lblNama.ForeColor = Color.White;
            lblNama.Font = new Font(
                "Segoe UI", 18, FontStyle.Bold);
            lblNama.AutoSize = true;
            lblNama.Location = new Point(20, 12);

            lblSpesifikasi.ForeColor = Color.White;
            lblSpesifikasi.Font = new Font("Segoe UI", 10);
            lblSpesifikasi.AutoSize = true;
            lblSpesifikasi.Location = new Point(22, 55);

            header.Controls.Add(lblNama);
            header.Controls.Add(lblSpesifikasi);

            var ringkasan = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(15),
                BackColor = Color.WhiteSmoke
            };

            lblTotalUnit.AutoSize = true;
            lblTotalUnit.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);
            lblTotalUnit.Margin = new Padding(5, 8, 25, 5);

            lblStokTersedia.AutoSize = true;
            lblStokTersedia.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);
            lblStokTersedia.Margin = new Padding(5, 8, 25, 5);

            btnTambahUnit.Text = "+ Tambah Unit";
            btnTambahUnit.Size = new Size(140, 35);
            btnTambahUnit.BackColor = Color.FromArgb(30, 64, 175);
            btnTambahUnit.ForeColor = Color.White;
            btnTambahUnit.FlatStyle = FlatStyle.Flat;
            btnTambahUnit.Margin = new Padding(10, 0, 0, 0);
            btnTambahUnit.Click += (_, _) => BukaFormUnit();

            ringkasan.Controls.Add(lblTotalUnit);
            ringkasan.Controls.Add(lblStokTersedia);
            ringkasan.Controls.Add(btnTambahUnit);

            var panelGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.White
            };

            grid.Dock = DockStyle.Fill;
            panelGrid.Controls.Add(grid);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
            grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = true;


            var panelHarga = new Panel
            {
                Dock = DockStyle.Top,
                Height = 240,
                Padding = new Padding(15),
                BackColor = Color.White
            };

            var lblHarga = new Label
            {
                Text = "Harga Acuan Model",
                Dock = DockStyle.Top,
                Height = 33,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            gridHarga.Dock = DockStyle.Fill;
            gridHarga.ReadOnly = false;
            gridHarga.AllowUserToAddRows = false;
            gridHarga.AllowUserToDeleteRows = false;
            gridHarga.MultiSelect = false;
            gridHarga.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            gridHarga.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            gridHarga.RowHeadersVisible = false;
            gridHarga.AutoGenerateColumns = true;

            btnSimpanHarga.Text = "Simpan Harga";
            btnSimpanHarga.Dock = DockStyle.Bottom;
            btnSimpanHarga.Height = 42;

            btnSimpanHarga.BackColor = Color.FromArgb(37, 99, 235);
            btnSimpanHarga.ForeColor = Color.White;
            btnSimpanHarga.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            btnSimpanHarga.FlatStyle = FlatStyle.Flat;
            btnSimpanHarga.FlatAppearance.BorderSize = 0;
            btnSimpanHarga.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(29, 78, 216);
            btnSimpanHarga.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(30, 64, 175);

            btnSimpanHarga.Cursor = Cursors.Hand;
            btnSimpanHarga.Click += (_, _) => SimpanHarga();

            panelHarga.Controls.Add(gridHarga);
            panelHarga.Controls.Add(btnSimpanHarga);
            panelHarga.Controls.Add(lblHarga);

            Controls.Add(panelGrid);
            Controls.Add(panelHarga);
            Controls.Add(ringkasan);
            Controls.Add(header);
        }

        private void MuatDetail()
        {
            using var koneksi = Database.GetConnection();

            using var perintah = koneksi.CreateCommand();
            perintah.CommandText = @"
                SELECT brand, model_name, category,
                       processor, ram_gb,
                       storage_type, storage_gb
                FROM computer_models
                WHERE id = $id;
            ";
            perintah.Parameters.AddWithValue("$id", modelId);

            using var reader = perintah.ExecuteReader();

            if (!reader.Read())
            {
                MessageBox.Show("Model tidak ditemukan.");
                Close();
                return;
            }

            string merek = reader["brand"]?.ToString() ?? "";
            string namaModel =
                reader["model_name"]?.ToString() ?? "";
            string kategori =
                reader["category"]?.ToString() ?? "";
            string prosesor =
                reader["processor"]?.ToString() ?? "";
            string ram = reader["ram_gb"]?.ToString() ?? "0";
            string jenisStorage =
                reader["storage_type"]?.ToString() ?? "";
            string kapasitas =
                reader["storage_gb"]?.ToString() ?? "0";

            lblNama.Text = $"{merek} {namaModel}";
            lblSpesifikasi.Text =
                $"{kategori} | {prosesor} | RAM {ram} GB | " +
                $"{jenisStorage} {kapasitas} GB";
        }

        private void MuatUnit()
        {
            using var koneksi = Database.GetConnection();

            using var perintah = koneksi.CreateCommand();
            perintah.CommandText = @"
                SELECT serial_number AS [Serial Number],
                       purchase_date AS [Tanggal Masuk],
                       purchase_price AS [Harga Modal],
                       status AS [Status]
                FROM computer_units
                WHERE model_id = $id
                ORDER BY id DESC;
            ";
            perintah.Parameters.AddWithValue("$id", modelId);

            using var reader = perintah.ExecuteReader();

            var tabel = new System.Data.DataTable();
            tabel.Load(reader);

            grid.DataSource = null;
            grid.Columns.Clear();
            grid.DataSource = tabel;

            if (grid.Columns.Contains("Harga Modal"))
            {
                grid.Columns["Harga Modal"].DefaultCellStyle
                    .Format = "N0";
            }

            lblTotalUnit.Text = $"Total Unit: {tabel.Rows.Count}";

            int tersedia = 0;

            foreach (System.Data.DataRow baris in tabel.Rows)
            {
                if (baris["Status"]?.ToString() == "Tersedia")
                    tersedia++;
            }

            lblStokTersedia.Text = $"Stok Tersedia: {tersedia}";
        }


        private void MuatHarga()
        {
            using var koneksi = Database.GetConnection();

            using var perintah = koneksi.CreateCommand();
            perintah.CommandText = @"
                SELECT
                    pt.id AS price_type_id,
                    pt.name AS [Tipe Harga],
                    COALESCE(mp.price, 0) AS [Harga],
                    COALESCE(mp.price, 0) AS harga_asli
                FROM price_types pt
                LEFT JOIN model_prices mp
                    ON mp.price_type_id = pt.id
                    AND mp.model_id = $modelId
                WHERE pt.is_active = 1
                ORDER BY pt.name;
            ";

            perintah.Parameters.AddWithValue("$modelId", modelId);

            using var reader = perintah.ExecuteReader();

            var tabel = new System.Data.DataTable();
            tabel.Load(reader);

            gridHarga.DataSource = null;
            gridHarga.Columns.Clear();
            gridHarga.DataSource = tabel;

            if (gridHarga.Columns.Contains("price_type_id"))
                gridHarga.Columns["price_type_id"].Visible = false;

            if (gridHarga.Columns.Contains("harga_asli"))
                gridHarga.Columns["harga_asli"].Visible = false;

            if (gridHarga.Columns.Contains("Harga"))
            {
                gridHarga.Columns["Harga"].ValueType = typeof(long);
                gridHarga.Columns["Harga"].DefaultCellStyle.Format = "N0";
            }

            if (gridHarga.Columns.Contains("Tipe Harga"))
                gridHarga.Columns["Tipe Harga"].ReadOnly = true;
        }


        private void SimpanHarga()
        {
            gridHarga.EndEdit();

            using var koneksi = Database.GetConnection();

            using var transaksi = koneksi.BeginTransaction();

            try
            {
                foreach (System.Windows.Forms.DataGridViewRow baris
                    in gridHarga.Rows)
                {
                    int tipeHargaId = Convert.ToInt32(
                        baris.Cells["price_type_id"].Value);

                    if (!long.TryParse(
                        Convert.ToString(baris.Cells["Harga"].Value),
                        out long harga) || harga < 0)
                    {
                        throw new InvalidOperationException(
                            "Harga harus berupa angka nol atau lebih.");
                    }

                    using var perintah = koneksi.CreateCommand();
                    perintah.Transaction = transaksi;

                    perintah.CommandText = @"
                INSERT INTO model_prices
                    (model_id, price_type_id, price)
                VALUES
                    ($modelId, $typeId, $price)
                ON CONFLICT(model_id, price_type_id)
                DO UPDATE SET price = excluded.price;
            ";

                    perintah.Parameters.AddWithValue(
                        "$modelId", modelId);
                    perintah.Parameters.AddWithValue(
                        "$typeId", tipeHargaId);
                    perintah.Parameters.AddWithValue(
                        "$price", harga);

                    perintah.ExecuteNonQuery();
                }

                transaksi.Commit();

                MessageBox.Show(
                    "Harga acuan model berhasil disimpan.",
                    "Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                transaksi.Rollback();

                MessageBox.Show(
                    $"Gagal menyimpan harga:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void BukaFormUnit()
        {
            using var form = new FormUnit(modelId);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                MuatUnit();
            }
        }
    }
}
