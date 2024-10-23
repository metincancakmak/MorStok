using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using MaterialSkin.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MorStok
{
    public class databaseOperations
    {
        private string connectionString = "Data Source="+Application.StartupPath+"mor_stokDB.db;";

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
        public bool AddProduct(string sku, string name, int qty, string category, string price)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                try
                {
                    // Sorgu düzeltildi
                    string query = "INSERT INTO mor_products (product_sku, product_name, product_qty, product_status, product_category, product_price) " +
                                   "VALUES (@sku, @name, @qty, 1, @category, @price)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sku", sku);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (SQLiteException ex)
                {
                    // UNIQUE constraint hatasını yakalayın ve özel mesaj gösterin
                    if (ex.ResultCode == SQLiteErrorCode.Constraint && ex.Message.Contains("UNIQUE"))
                    {
                        MessageBox.Show($"Bu ürünün adı veya stok kodu mevcut ", "UNIQUE Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        // Diğer hatalar için genel hata mesajı
                        MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }
                finally
                {
                    CloseConnection(conn);
                }
            }
        }


        // Ürün "silme" işlemi, ürünün status değerini 0 yapar
        public bool DeleteProduct(string sku)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                try
                {
                    // SKU'nun veritabanında mevcut olup olmadığını kontrol et
                    string checkQuerySku = "SELECT COUNT(*) FROM mor_products WHERE product_sku = @sku";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuerySku, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@sku", sku);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count == 0)
                        {
                            MessageBox.Show("Silinecek ürün bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Ürünü veritabanından sil
                    string query = "DELETE FROM mor_products WHERE product_sku = @sku";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sku", sku);
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    CloseConnection(conn);
                }
            }
        }

        // Ürün güncelleme işlemi
        public bool UpdateProduct(string sku, string name, int qty, string category, string price)
        {
            using (SQLiteConnection conn = OpenConnection())
            {
                try
                {
                    // Güncellenmek istenen SKU'nun varlığını kontrol et
                    string checkQuerySku = "SELECT COUNT(*) FROM mor_products WHERE product_sku = @sku";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuerySku, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@sku", sku);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count == 0)
                        {
                            MessageBox.Show("Güncellenecek Ürün bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    string checkQueryName = "SELECT COUNT(*) FROM mor_products WHERE product_name = @name AND product_sku != @sku";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQueryName, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@name", name);
                        checkCmd.Parameters.AddWithValue("@sku", sku);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Aynı isimde bir ürün zaten var.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Ürün güncelleme sorgusu
                    string query = "UPDATE mor_products SET product_name = @name, product_qty = @qty, product_category = @category, product_price = @price WHERE product_sku = @sku";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sku", sku);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    CloseConnection(conn);
                }
            }
        }

        // Yalnızca status'ü 1 olan ürünleri listele
        public DataTable GetProductsByStatus(int status)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT * FROM mor_products WHERE product_status = @status";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
                CloseConnection(conn);
            }
            return dt;
        }

        // Ürünleri arama listeleme
        public DataTable GetProductsByLike(string productName, string productSKU, string productCategory)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = OpenConnection())
            {
                // Dinamik bir SQL sorgusu oluşturuyoruz
                List<string> conditions = new List<string>();
                if (!string.IsNullOrEmpty(productName))
                {
                    conditions.Add("product_name LIKE @productName");
                }
                if (!string.IsNullOrEmpty(productSKU))
                {
                    conditions.Add("product_sku LIKE @productSKU");
                }
                if (!string.IsNullOrEmpty(productCategory))
                {
                    conditions.Add("product_category LIKE @productCategory");
                }

                // Eğer hiç bir koşul yoksa tüm ürünleri getirecek
                string query = "SELECT * FROM mor_products";
                if (conditions.Count > 0)
                {
                    query += " WHERE " + string.Join(" AND ", conditions);
                }

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    // Parametreleri sadece boş olmayan değişkenler için ekliyoruz
                    if (!string.IsNullOrEmpty(productName))
                    {
                        cmd.Parameters.AddWithValue("@productName", "%" + productName + "%");
                    }
                    if (!string.IsNullOrEmpty(productSKU))
                    {
                        cmd.Parameters.AddWithValue("@productSKU", "%" + productSKU + "%");
                    }
                    if (!string.IsNullOrEmpty(productCategory))
                    {
                        cmd.Parameters.AddWithValue("@productCategory", "%" + productCategory + "%");
                    }

                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
                CloseConnection(conn);
            }
            return dt;
        }


        // Ürün adedi 3'ten az olan ürünleri listele
        public DataTable getLowStockProduct()
        {
            DataTable dt = new DataTable();

            using(SQLiteConnection conn = OpenConnection())
            {
                string query = "SELECT * FROM mor_products WHERE product_qty < 4";

                using(SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using(SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
                CloseConnection(conn);
            }
            return dt;
        }

        public string generateStockCode()
        {
            Random rnd = new Random();
            string stockCode = "";

            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] lettersArray = new char[7];

            for (int i = 0; i < lettersArray.Length; i++)
            {
                lettersArray[i] = letters[rnd.Next(letters.Length)];
            }

            string digits = "0123456789";
            char[] digitArray = new char[4];

            for (int i = 0; i < digitArray.Length; i++)
            {
                digitArray[i] = digits[rnd.Next(digits.Length)];
            }

            return stockCode = new string(lettersArray) + new string(digitArray);
        }

        public int getProductCount()
        {
            DataTable products = GetProductsByStatus(1);

            int productCount = products.Rows.Count;

            return productCount;
        }

        public int[] getProductQtyCounts()
        {
            DataTable products = GetProductsByStatus(1);

            int qtyGreaterThanZero = products.AsEnumerable()
                                             .Where(row => row.Field<long>("product_qty") > 0)
                                             .Count();

            int qtyLessThanOne = products.AsEnumerable()
                                         .Where(row => row.Field<long>("product_qty") < 1)
                                         .Count();

            return new int[] { qtyGreaterThanZero, qtyLessThanOne };
        }
    }
}
