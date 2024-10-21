using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace MorStok
{
    public class databaseOperations
    {
        private string connectionString = "Data Source=mor_stokDB.db;Version=3;";

        // Veritabanına bağlantı açma
        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bağlantı açma hatası: " + ex.Message);
            }
            return conn;
        }

        // Veritabanı bağlantısını kapatma
        private void CloseConnection(SQLiteConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        // Ürün ekleme işlemi, status otomatik 1 olarak ayarlanacak
        public void AddProduct(string sku, string name, int qty, string category)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "INSERT INTO mor_products (product_sku, product_name, product_qty, product_status, product_category) VALUES (@sku, @name, @qty, 1, @category)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@sku", sku);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@category", category);
                    cmd.ExecuteNonQuery();
                }
                CloseConnection(conn);
            }
        }

        // Ürün "silme" işlemi, ürünün status değerini 0 yapar
        public void DeleteProduct(int id)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "UPDATE mor_products SET product_status = 0 WHERE ID = @id";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                CloseConnection(conn);
            }
        }

        // Ürün güncelleme işlemi
        public void UpdateProduct(int id, string name, int qty, string category)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "UPDATE mor_products SET product_name = @name, product_qty = @qty, product_category = @category WHERE ID = @id";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@category", category);
                    cmd.ExecuteNonQuery();
                }
                CloseConnection(conn);
            }
        }

        // Yalnızca status'ü 1 olan ürünleri listele
        public DataTable GetActiveProducts()
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT * FROM mor_products WHERE product_status = 1";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
                CloseConnection(conn);
            }
            return dt;
        }
    }
}
