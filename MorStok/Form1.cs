using MaterialSkin;
using MaterialSkin.Controls;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace MorStok
{
    public partial class MorStok : MaterialForm
    {
        databaseOperations dbOperations = new databaseOperations();

        public MorStok()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void listLowStockProduct()
        {
            DataTable lowStockProducts = dbOperations.getLowStockProduct();

            materialListView1.Items.Clear();
            ListViewItem item;

            foreach (DataRow row in lowStockProducts.Rows)
            {
                item = new ListViewItem(row["product_sku"].ToString());
                item.SubItems.Add(row["product_name"].ToString());
                item.SubItems.Add(row["product_qty"].ToString());
                item.SubItems.Add(row["product_category"].ToString());
                item.SubItems.Add(row["product_price"]?.ToString());
                materialListView1.Items.Add(item);
            }
        }


        private void loadFunctions()
        {
            ListProducts();
            listLowStockProduct();

            int[] qtyCounts = dbOperations.getProductQtyCounts();
            materialLabel2.Text = dbOperations.getProductCount().ToString();
            materialLabel8.Text = qtyCounts[0].ToString();
            materialLabel10.Text = qtyCounts[1].ToString();
        }

        private void ListProducts()
        {
            string productName = materialTextBox1.Text;
            string productSKU = materialTextBox2.Text;
            string productCategory = materialTextBox4.Text;
            DataTable items = dbOperations.GetProductsByLike(productName, productSKU, productCategory);
            materialListView5.Items.Clear();
            ListViewItem item;

            foreach (DataRow row in items.Rows)
            {
                item = new ListViewItem(row["product_sku"]?.ToString() ?? string.Empty);
                item.SubItems.Add(row["product_name"]?.ToString() ?? string.Empty);
                item.SubItems.Add(row["product_qty"]?.ToString() ?? string.Empty);
                item.SubItems.Add(row["product_category"]?.ToString() ?? string.Empty);
                item.SubItems.Add(row["product_price"]?.ToString() ?? string.Empty);
                materialListView5.Items.Add(item);
            }
        }

        private void materialLabel10_Click(object sender, EventArgs e)
        {

        }

        private void materialListView5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialListView5.SelectedItems.Count > 0)
            {
                var selectedItem = materialListView5.SelectedItems[0];

                materialTextBox1.Text = selectedItem.SubItems[1].Text;
                materialTextBox2.Text = selectedItem.SubItems[0].Text;
                materialTextBox3.Text = selectedItem.SubItems[2].Text;
                materialTextBox4.Text = selectedItem.SubItems[3].Text;
            }
        }

        private void MorStok_Load(object sender, EventArgs e)
        {
            loadFunctions();
            materialTabControl1.KeyDown += MaterialTabControl1_KeyDown;

        }

        private void MaterialTabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            // Eðer þu anda tabPage2 aktifse ve Enter tuþuna basýldýysa
            if (materialTabControl1.SelectedTab == tabPage2 && e.KeyCode == Keys.Enter)
            {
                materialButton5.PerformClick(); // materialButton5'i tetiklemek için
                e.SuppressKeyPress = true; // Enter tuþu baþka bir yere gitmesin
            }
        }
        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(materialTextBox1.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox2.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox3.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox4.Text))
            {
                MessageBox.Show("Lütfen tüm alanlarý doldurunuz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productQty = int.Parse(materialTextBox3.Text);
            if (productQty < 0)
            {
                MessageBox.Show("Ürün adedi 0'dan küçük olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ürün bilgilerini al
            string productName = materialTextBox1.Text;
            string productSku = materialTextBox2.Text;
            string productCategory = materialTextBox4.Text;
            string productPrice = materialTextBox5.Text;

            try
            {
                // Ürünü ekle ve ekleme sonucuna göre iþlem yap
                bool isAdded = dbOperations.AddProduct(productSku, productName, productQty, productCategory, productPrice);

                if (isAdded)
                {
                    MessageBox.Show("Ürün baþarýyla eklendi.");

                    // Textboxlarý temizle
                    materialTextBox1.Text = "";
                    materialTextBox2.Text = "";
                    materialTextBox3.Text = "";
                    materialTextBox4.Text = "";

                    loadFunctions(); // Fonksiyonlarý tekrar yükle
                }

            }
            catch
            {
                MessageBox.Show("Ayný isimde bir ürün zaten var.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            materialTextBox2.Text = dbOperations.generateStockCode();
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(materialTextBox1.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox2.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox3.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox4.Text))
            {
                MessageBox.Show("Lütfen tüm alanlarý doldurunuz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(materialTextBox3.Text, out int productQty))
            {
                MessageBox.Show("Ürün adedi yalnýzca sayý olabilir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (productQty < 0)
            {
                MessageBox.Show("Ürün adedi 0'dan küçük olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string productName = materialTextBox1.Text;
            string productSku = materialTextBox2.Text;
            string productCategory = materialTextBox4.Text;
            string price = materialTextBox5.Text;

            bool isUpdated = dbOperations.UpdateProduct(productSku, productName, productQty, productCategory, price);

            if (isUpdated)
            {

                MessageBox.Show("Ürün baþarýyla güncellendi.");
                loadFunctions();
            }
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            string selectedSku = materialTextBox2.Text;

            if (string.IsNullOrWhiteSpace(selectedSku))
            {
                MessageBox.Show("Lütfen silmek için bir ürün seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isDeleted = dbOperations.DeleteProduct(selectedSku);

            if (isDeleted)
            {
                MessageBox.Show("Ürün baþarýyla silindi.");
                loadFunctions();
            }
        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            ListProducts();
        }

        private void materialTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {

            // Eðer basýlan tuþ rakam deðilse ve 'Backspace' gibi kontrol tuþu deðilse, tuþu iptal et.
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Tuþu iptal eder
            }


        }

        private void materialTextBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Basýlan tuþ rakam deðilse, 'Backspace' deðilse, '.' ve ',' karakterlerinden biri deðilse, tuþu iptal et.
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Tuþu iptal eder
            }
        }

        private void MorStok_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void MorStok_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
