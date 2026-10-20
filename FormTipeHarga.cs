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
    public partial class FormTipeHarga : Form
    {
        private readonly DataGridView grid = new();
        private readonly TextBox txtNama = new();
        private readonly Button btnTambah = new();
        private readonly Button btnNonaktifkan = new();

        public FormTipeHarga()
        {
            InitializeComponent();

            Text = "Kelola Tipe Harga";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(650, 450);
            MinimumSize = new Size(550, 350);

            BangunForm();
            MuatData();
        }

        private void BangunForm()
        {
            var panelInput = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(12),
                WrapContents = false
            };

            txtNama.Width = 250;
            txtNama.PlaceholderText = "Contoh: Retail, Reseller, Proyek";

            btnTambah.Text = "+ Tambah Tipe";
            btnTambah.AutoSize = true;
            btnTambah.Click += (_, _) => TambahTipe();

            panelInput.Controls.Add(txtNama);
            panelInput.Controls.Add(btnTambah);

            var panelBawah = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(12),
                FlowDirection = FlowDirection.RightToLeft
            };

            btnNonaktifkan.Text = "Nonaktifkan Tipe";
            btnNonaktifkan.AutoSize = true;
            btnNonaktifkan.Click +=
                (_, _) => NonaktifkanTipe();

            panelBawah.Controls.Add(btnNonaktifkan);

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = true;

            Controls.Add(grid);
            Controls.Add(panelInput);
            Controls.Add(panelBawah);
        }

        private void MuatData()
        {
            using var koneksi = Database.GetConnection();

            using var perintah = koneksi.CreateCommand();
            perintah.CommandText = @"
                SELECT
                    id AS [ID],
                    name AS [Tipe Harga],
                    CASE
                        WHEN is_active = 1 THEN 'Aktif'
                        ELSE 'Nonaktif'
                    END AS [Status]
                FROM price_types
                ORDER BY name;
            ";

            using var reader = perintah.ExecuteReader();

            var tabel = new System.Data.DataTable();
            tabel.Load(reader);

            grid.DataSource = null;
            grid.Columns.Clear();
            grid.DataSource = tabel;

            if (grid.Columns.Contains("ID"))
                grid.Columns["ID"].Visible = false;
        }

        private void TambahTipe()
        {
            string nama = txtNama.Text.Trim();

            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Nama tipe harga wajib diisi.");
                txtNama.Focus();
                return;
            }

            try
            {
                using var koneksi = Database.GetConnection();

                using var perintah = koneksi.CreateCommand();
                perintah.CommandText = @"
                    INSERT INTO price_types (name)
                    VALUES ($nama);
                ";

                perintah.Parameters.AddWithValue("$nama", nama);
                perintah.ExecuteNonQuery();

                txtNama.Clear();
                MuatData();

                MessageBox.Show("Tipe harga berhasil ditambahkan.");
            }
            catch (SqliteException ex)
                when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show(
                    "Nama tipe harga sudah ada atau melanggar aturan database.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menambah tipe harga:\n{ex.Message}");
            }
        }

        private void NonaktifkanTipe()
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Pilih tipe harga terlebih dahulu.");
                return;
            }

            int id = Convert.ToInt32(
                grid.CurrentRow.Cells["ID"].Value);

            string nama = Convert.ToString(
                grid.CurrentRow.Cells["Tipe Harga"].Value) ?? "";

            var konfirmasi = MessageBox.Show(
                $"Nonaktifkan tipe harga '{nama}'?",
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes)
                return;

            try
            {
                using var koneksi = Database.GetConnection();

                using var perintah = koneksi.CreateCommand();
                perintah.CommandText = @"
                    UPDATE price_types
                    SET is_active = 0
                    WHERE id = $id;
                ";

                perintah.Parameters.AddWithValue("$id", id);
                perintah.ExecuteNonQuery();

                MuatData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Gagal menonaktifkan tipe harga:\n{ex.Message}");
            }
        }
    }
}
