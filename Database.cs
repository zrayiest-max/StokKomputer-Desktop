using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace StokKomputer
{
    public static class Database
    {
        private static readonly string Folder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "CitraPersada",
                "StokKomputer");

        public static readonly string DatabasePath =
            Path.Combine(Folder, "stokKomputer.db");

        public static void Initialize()
        {
            Directory.CreateDirectory(Folder);
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS computer_models (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    brand TEXT NOT NULL,
                    model_name TEXT NOT NULL,
                    category TEXT NOT NULL
                        CHECK(category IN ('Laptop', 'AIO')),
                    processor TEXT,
                    ram_gb INTEGER,
                    storage_gb INTEGER,
                    storage_type TEXT,
                    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS computer_units (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    model_id INTEGER NOT NULL,
                    serial_number TEXT NOT NULL UNIQUE,
                    purchase_date TEXT,
                    purchase_price INTEGER NOT NULL DEFAULT 0
                        CHECK(purchase_price >= 0),
                    status TEXT NOT NULL DEFAULT 'Tersedia'
                        CHECK(status IN (
                            'Tersedia', 'Dipesan', 'Terjual'
                        )),
                    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(model_id)
                        REFERENCES computer_models(id)
                );

                CREATE TABLE IF NOT EXISTS price_types (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    is_active INTEGER NOT NULL DEFAULT 1
                        CHECK(is_active IN (0, 1))
                );

                                
                CREATE TABLE IF NOT EXISTS model_prices (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    model_id INTEGER NOT NULL,
                    price_type_id INTEGER NOT NULL,
                    price INTEGER NOT NULL DEFAULT 0
                        CHECK(price >= 0),

                    UNIQUE(model_id, price_type_id),

                    FOREIGN KEY(model_id)
                        REFERENCES computer_models(id),

                    FOREIGN KEY(price_type_id)
                        REFERENCES price_types(id)
                );
                

                CREATE TABLE IF NOT EXISTS unit_prices (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    unit_id INTEGER NOT NULL,
                    price_type_id INTEGER NOT NULL,
                    price INTEGER NOT NULL CHECK(price >= 0),
                    UNIQUE(unit_id, price_type_id),
                    FOREIGN KEY(unit_id)
                        REFERENCES computer_units(id),
                    FOREIGN KEY(price_type_id)
                        REFERENCES price_types(id)
                );

                CREATE TABLE IF NOT EXISTS sales (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    unit_id INTEGER NOT NULL UNIQUE,
                    price_type_id INTEGER,
                    sold_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    sold_price INTEGER NOT NULL CHECK(sold_price >= 0),
                    notes TEXT,
                    FOREIGN KEY(unit_id)
                        REFERENCES computer_units(id),
                    FOREIGN KEY(price_type_id)
                        REFERENCES price_types(id)
                );

                CREATE TABLE IF NOT EXISTS stock_movements (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    unit_id INTEGER NOT NULL,
                    movement_type TEXT NOT NULL
                        CHECK(movement_type IN (
                            'Masuk', 'Penjualan', 'Koreksi'
                        )),
                    movement_date TEXT NOT NULL
                        DEFAULT CURRENT_TIMESTAMP,
                    notes TEXT,
                    FOREIGN KEY(unit_id)
                        REFERENCES computer_units(id)
                );

                INSERT OR IGNORE INTO price_types (name)
                VALUES ('Umum'), ('Kantor'), ('Proyek');

                """;
            command.ExecuteNonQuery();

            EnsureUnitSchema(connection);
            EnsureSalesSchema(connection);
        }


        private static void EnsureUnitSchema(SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(computer_units);";

            bool sudahAdaKolom = false;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "is_deleted")
                    {
                        sudahAdaKolom = true;
                        break;
                    }
                }
            }

            if (!sudahAdaKolom)
            {
                using var tambahKolom = connection.CreateCommand();
                tambahKolom.CommandText = """
                    ALTER TABLE computer_units
                    ADD COLUMN is_deleted INTEGER NOT NULL DEFAULT 0
                    CHECK(is_deleted IN (0, 1));
                    """;
                tambahKolom.ExecuteNonQuery();
            }
        }

        private static void EnsureSalesSchema(SqliteConnection connection)
        {
            using var transaksi = connection.BeginTransaction();

            try
            {
                bool sudahAdaKolomStatus = false;

                using (var cek = connection.CreateCommand())
                {
                    cek.Transaction = transaksi;
                    cek.CommandText = "PRAGMA table_info(sales);";

                    using var reader = cek.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader["name"].ToString() == "status")
                        {
                            sudahAdaKolomStatus = true;
                            break;
                        }
                    }
                }

                if (!sudahAdaKolomStatus)
                {
                    using var perintah = connection.CreateCommand();
                    perintah.Transaction = transaksi;

                    perintah.CommandText = """
                CREATE TABLE sales_new (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    unit_id INTEGER NOT NULL,
                    price_type_id INTEGER,
                    sold_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    sold_price INTEGER NOT NULL CHECK(sold_price >= 0),
                    notes TEXT,
                    status TEXT NOT NULL DEFAULT 'Selesai'
                        CHECK(status IN ('Selesai', 'Dibatalkan')),
                    cancelled_at TEXT,
                    cancel_reason TEXT,

                    FOREIGN KEY(unit_id)
                        REFERENCES computer_units(id),

                    FOREIGN KEY(price_type_id)
                        REFERENCES price_types(id)
                );

                INSERT INTO sales_new (
                    id,
                    unit_id,
                    price_type_id,
                    sold_at,
                    sold_price,
                    notes,
                    status
                )
                SELECT
                    id,
                    unit_id,
                    price_type_id,
                    sold_at,
                    sold_price,
                    notes,
                    'Selesai'
                FROM sales;

                DROP TABLE sales;

                ALTER TABLE sales_new RENAME TO sales;
                """;

                    perintah.ExecuteNonQuery();
                }

                using (var index = connection.CreateCommand())
                {
                    index.Transaction = transaksi;

                    index.CommandText = """
                CREATE UNIQUE INDEX IF NOT EXISTS idx_sales_unit_aktif
                ON sales(unit_id)
                WHERE status = 'Selesai';
                """;

                    index.ExecuteNonQuery();
                }

                transaksi.Commit();
            }
            catch
            {
                transaksi.Rollback();
                throw;
            }
        }


        public static System.Data.DataTable GetStockSummary()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();


            command.CommandText = """
            SELECT
                m.id,
                m.brand AS Merek,
                m.model_name AS Model,
                m.category AS Kategori,
                m.processor AS Prosesor,
                m.ram_gb AS RAM,
                m.storage_type AS JenisStorage,
                m.storage_gb AS KapasitasStorage,

                COUNT(DISTINCT u.id) AS TotalUnit,

                COUNT(DISTINCT CASE
                    WHEN u.status = 'Tersedia' THEN u.id
                END) AS StokTersedia,

                COALESCE(
                    MIN(CASE WHEN u.status = 'Tersedia'
                        THEN u.purchase_price END), 0
                ) AS ModalTerendah,

                COALESCE(
                    MAX(CASE WHEN u.status = 'Tersedia'
                        THEN u.purchase_price END), 0
                ) AS ModalTertinggi,

                COALESCE(MAX(CASE WHEN pt.name = 'Umum'
                    THEN mp.price END), 0) AS HargaUmum,

                COALESCE(MAX(CASE WHEN pt.name = 'Kantor'
                    THEN mp.price END), 0) AS HargaKantor,

                COALESCE(MAX(CASE WHEN pt.name = 'Proyek'
                    THEN mp.price END), 0) AS HargaProyek

            FROM computer_models m

            LEFT JOIN computer_units u
                ON u.model_id = m.id

            LEFT JOIN model_prices mp
                ON mp.model_id = m.id

            LEFT JOIN price_types pt
                ON pt.id = mp.price_type_id
                AND pt.is_active = 1

            GROUP BY m.id
            ORDER BY
                CASE
                    WHEN COUNT(DISTINCT CASE
                        WHEN u.status = 'Tersedia' THEN u.id
                    END) > 0 THEN 0
                    ELSE 1
                END ASC,
                m.created_at DESC;
            """;


            using var reader = command.ExecuteReader();
            var table = new System.Data.DataTable();
            table.Load(reader);
            return table;
        }

        public static DataTable GetSmartRestockSummary()
        {
            using var connection = GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
        WITH stok AS (
            SELECT
                m.category,
                m.brand,
                m.processor,
                m.ram_gb,
                m.storage_type,
                m.storage_gb,
                COUNT(u.id) AS StokTersedia,
                COUNT(DISTINCT m.id) AS JumlahModel
            FROM computer_models m
            LEFT JOIN computer_units u
                ON u.model_id = m.id
                AND u.status = 'Tersedia'
            GROUP BY
                m.category,
                m.brand,
                m.processor,
                m.ram_gb,
                m.storage_type,
                m.storage_gb
        ),
        penjualan AS (
            SELECT
                m.category,
                m.brand,
                m.processor,
                m.ram_gb,
                m.storage_type,
                m.storage_gb,
                COUNT(s.id) AS Terjual30Hari
            FROM sales s
            INNER JOIN computer_units u
                ON u.id = s.unit_id
            INNER JOIN computer_models m
                ON m.id = u.model_id
            WHERE s.status = 'Selesai'
                AND s.sold_at >= datetime('now', '-30 days')
                AND s.sold_at <= datetime('now')
            GROUP BY
                m.category,
                m.brand,
                m.processor,
                m.ram_gb,
                m.storage_type,
                m.storage_gb
        )
        SELECT
            stok.category AS Kategori,
            stok.brand AS Merek,
            stok.processor AS Prosesor,
            stok.ram_gb AS RAM,
            stok.storage_type AS JenisStorage,
            stok.storage_gb AS KapasitasStorage,
            stok.StokTersedia,
            COALESCE(penjualan.Terjual30Hari, 0) AS Terjual30Hari,

            CASE
                WHEN COALESCE(penjualan.Terjual30Hari, 0) > stok.StokTersedia
                THEN COALESCE(penjualan.Terjual30Hari, 0) - stok.StokTersedia
                ELSE 0
            END AS RekomendasiBeli,

            stok.JumlahModel,

            CASE
                WHEN COALESCE(penjualan.Terjual30Hari, 0) > 0
                    AND stok.StokTersedia = 0
                    THEN 'Kritis'

                WHEN COALESCE(penjualan.Terjual30Hari, 0) > stok.StokTersedia
                    THEN 'Perlu Restok'

                WHEN COALESCE(penjualan.Terjual30Hari, 0) = 0
                    THEN 'Evaluasi'

                ELSE 'Aman'
            END AS StatusRestok

        FROM stok
        LEFT JOIN penjualan ON
            IFNULL(stok.category, '') = IFNULL(penjualan.category, '')
            AND IFNULL(stok.brand, '') = IFNULL(penjualan.brand, '')
            AND IFNULL(stok.processor, '') = IFNULL(penjualan.processor, '')
            AND IFNULL(stok.ram_gb, -1) = IFNULL(penjualan.ram_gb, -1)
            AND IFNULL(stok.storage_type, '') = IFNULL(penjualan.storage_type, '')
            AND IFNULL(stok.storage_gb, -1) = IFNULL(penjualan.storage_gb, -1)

        ORDER BY
            CASE
                WHEN COALESCE(penjualan.Terjual30Hari, 0) > 0
                    AND stok.StokTersedia = 0 THEN 0
                WHEN COALESCE(penjualan.Terjual30Hari, 0) > stok.StokTersedia THEN 1
                WHEN COALESCE(penjualan.Terjual30Hari, 0) = 0 THEN 2
                ELSE 3
            END,
            COALESCE(penjualan.Terjual30Hari, 0) DESC,
            stok.brand,
            stok.processor;
        """;

            using var reader = command.ExecuteReader();

            var table = new DataTable();
            table.Load(reader);

            return table;
        }


        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(
                $"Data Source={DatabasePath}");

            try
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static void Backup(string tujuan)
        {
            if (string.IsNullOrWhiteSpace(tujuan))
                throw new ArgumentException(
                    "Lokasi backup belum ditentukan.");

            string? folder = Path.GetDirectoryName(tujuan);

            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            using var sumber = GetConnection();

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = tujuan
            };

            using var tujuanDb =
                new SqliteConnection(builder.ToString());

            tujuanDb.Open();

            sumber.BackupDatabase(tujuanDb);
        }


        public static void Restore(string fileBackup)
        {
            if (!File.Exists(fileBackup))
                throw new FileNotFoundException(
                    "File backup tidak ditemukan.");

            string backupPath = Path.GetFullPath(fileBackup);
            string activePath = Path.GetFullPath(DatabasePath);

            if (string.Equals(
                backupPath,
                activePath,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "File backup tidak boleh sama dengan database aktif.");
            }

            // 1. Validasi integritas database backup
            using (var sumber = new SqliteConnection(
                new SqliteConnectionStringBuilder
                {
                    DataSource = backupPath,
                    Mode = SqliteOpenMode.ReadOnly
                }.ToString()))
            {
                sumber.Open();

                using var cek = sumber.CreateCommand();
                cek.CommandText = "PRAGMA quick_check;";

                string hasil = Convert.ToString(
                    cek.ExecuteScalar()) ?? "";

                if (hasil != "ok")
                    throw new InvalidOperationException(
                        "File backup rusak atau tidak valid.");

                // Pastikan tabel utama tersedia
                cek.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN (
                  'computer_models',
                  'computer_units',
                  'price_types',
                  'model_prices',
                  'sales',
                  'stock_movements'
              );
            """;

                if (Convert.ToInt32(cek.ExecuteScalar()) != 6)
                    throw new InvalidOperationException(
                        "Struktur database backup tidak lengkap.");
            }

            // 2. Buat cadangan database aktif terlebih dahulu
            string folder = Path.GetDirectoryName(activePath)!;

            string cadangan = Path.Combine(
                folder,
                $"SebelumRestore_{DateTime.Now:yyyyMMdd_HHmmss_fff}.db");

            Backup(cadangan);

            try
            {
                // 3. Pulihkan isi backup ke database aktif
                using var sumber = new SqliteConnection(
                    new SqliteConnectionStringBuilder
                    {
                        DataSource = backupPath,
                        Mode = SqliteOpenMode.ReadOnly
                    }.ToString());

                sumber.Open();

                using var tujuan = GetConnection();

                sumber.BackupDatabase(tujuan);

                // 4. Periksa hasil restore
                using var cekHasil = tujuan.CreateCommand();
                cekHasil.CommandText = "PRAGMA quick_check;";

                string hasilRestore = Convert.ToString(
                    cekHasil.ExecuteScalar()) ?? "";

                if (hasilRestore != "ok")
                    throw new InvalidOperationException(
                        "Pemeriksaan database setelah restore gagal.");
            }
            catch (Exception ex)
            {
                // 5. Jika restore gagal, coba pulihkan cadangan sebelumnya
                try
                {
                    using var sumberCadangan =
                        new SqliteConnection(
                            new SqliteConnectionStringBuilder
                            {
                                DataSource = cadangan,
                                Mode = SqliteOpenMode.ReadOnly
                            }.ToString());

                    sumberCadangan.Open();

                    using var tujuan = GetConnection();

                    sumberCadangan.BackupDatabase(tujuan);
                }
                catch (Exception exRollback)
                {
                    throw new InvalidOperationException(
                        $"Restore gagal: {ex.Message}\n" +
                        $"Pemulihan cadangan otomatis juga gagal: " +
                        $"{exRollback.Message}\n" +
                        $"Cadangan sebelum restore tersimpan di: {cadangan}",
                        ex);
                }

                throw new InvalidOperationException(
                    "Restore gagal. Cadangan database sebelumnya " +
                    "telah dipulihkan.\n" + ex.Message, ex);
            }
        }

        public static string ResetStore()
        {
            // Backup otomatis sebelum reset
            string folder = Path.GetDirectoryName(DatabasePath)!;

            string fileBackup = Path.Combine(
                folder,
                $"SebelumReset_{DateTime.Now:yyyyMMdd_HHmmss}.db");

            Backup(fileBackup);

            using var koneksi = GetConnection();
            using var transaksi = koneksi.BeginTransaction();

            try
            {
                using var perintah = koneksi.CreateCommand();
                perintah.Transaction = transaksi;

                perintah.CommandText = """
            DELETE FROM sales;
            DELETE FROM stock_movements;
            DELETE FROM unit_prices;
            DELETE FROM model_prices;
            DELETE FROM computer_units;
            DELETE FROM computer_models;
            DELETE FROM price_types;

            INSERT INTO price_types (name, is_active)
            VALUES
                ('Umum', 1),
                ('Kantor', 1),
                ('Proyek', 1);

            DELETE FROM sqlite_sequence
            WHERE name IN (
                'sales',
                'stock_movements',
                'unit_prices',
                'model_prices',
                'computer_units',
                'computer_models',
                'price_types'
            );
            """;

                perintah.ExecuteNonQuery();
                transaksi.Commit();

                return fileBackup;
            }
            catch
            {
                transaksi.Rollback();
                throw;
            }
        }
        public static void HapusModel(int idModel)
        {
            using var koneksi = GetConnection();
            using var transaksi = koneksi.BeginTransaction();

            try
            {
                using var perintah = koneksi.CreateCommand();
                perintah.Transaction = transaksi;

                perintah.CommandText = """
            DELETE FROM sales
            WHERE unit_id IN (
                SELECT id FROM computer_units
                WHERE model_id = $id
            );

            DELETE FROM stock_movements
            WHERE unit_id IN (
                SELECT id FROM computer_units
                WHERE model_id = $id
            );

            DELETE FROM unit_prices
            WHERE unit_id IN (
                SELECT id FROM computer_units
                WHERE model_id = $id
            );

            DELETE FROM model_prices
            WHERE model_id = $id;

            DELETE FROM computer_units
            WHERE model_id = $id;

            DELETE FROM computer_models
            WHERE id = $id;
            """;

                perintah.Parameters.AddWithValue("$id", idModel);
                perintah.ExecuteNonQuery();

                transaksi.Commit();
            }
            catch
            {
                transaksi.Rollback();
                throw;
            }
        }

    }
}
