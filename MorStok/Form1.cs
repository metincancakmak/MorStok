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
            // E�er �u anda tabPage2 aktifse ve Enter tu�una bas�ld�ysa
            if (materialTabControl1.SelectedTab == tabPage2 && e.KeyCode == Keys.Enter)
            {
                materialButton5.PerformClick(); // materialButton5'i tetiklemek i�in
                e.SuppressKeyPress = true; // Enter tu�u ba�ka bir yere gitmesin
            }
        }
        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(materialTextBox1.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox2.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox3.Text) ||
            string.IsNullOrWhiteSpace(materialTextBox4.Text))
            {
                MessageBox.Show("L�tfen t�m alanlar� doldurunuz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productQty = int.Parse(materialTextBox3.Text);
            if (productQty < 0)
            {
                MessageBox.Show("�r�n adedi 0'dan k���k olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // �r�n bilgilerini al
            string productName = materialTextBox1.Text;
            string productSku = materialTextBox2.Text;
            string productCategory = materialTextBox4.Text;
            string productPrice = materialTextBox5.Text;

            try
            {
                // �r�n� ekle ve ekleme sonucuna g�re i�lem yap
                bool isAdded = dbOperations.AddProduct(productSku, productName, productQty, productCategory, productPrice);

                if (isAdded)
                {
                    MessageBox.Show("�r�n ba�ar�yla eklendi.");

                    // Textboxlar� temizle
                    materialTextBox1.Text = "";
                    materialTextBox2.Text = "";
                    materialTextBox3.Text = "";
                    materialTextBox4.Text = "";

                    loadFunctions(); // Fonksiyonlar� tekrar y�kle
                }

            }
            catch
            {
                MessageBox.Show("Ayn� isimde bir �r�n zaten var.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("L�tfen t�m alanlar� doldurunuz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(materialTextBox3.Text, out int productQty))
            {
                MessageBox.Show("�r�n adedi yaln�zca say� olabilir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (productQty < 0)
            {
                MessageBox.Show("�r�n adedi 0'dan k���k olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string productName = materialTextBox1.Text;
            string productSku = materialTextBox2.Text;
            string productCategory = materialTextBox4.Text;
            string price = materialTextBox5.Text;

            bool isUpdated = dbOperations.UpdateProduct(productSku, productName, productQty, productCategory, price);

            if (isUpdated)
            {

                MessageBox.Show("�r�n ba�ar�yla g�ncellendi.");
                loadFunctions();
            }
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            string selectedSku = materialTextBox2.Text;

            if (string.IsNullOrWhiteSpace(selectedSku))
            {
                MessageBox.Show("L�tfen silmek i�in bir �r�n se�in.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isDeleted = dbOperations.DeleteProduct(selectedSku);

            if (isDeleted)
            {
                MessageBox.Show("�r�n ba�ar�yla silindi.");
                loadFunctions();
            }
        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            ListProducts();
        }

        private void materialTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {

            // E�er bas�lan tu� rakam de�ilse ve 'Backspace' gibi kontrol tu�u de�ilse, tu�u iptal et.
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Tu�u iptal eder
            }


        }

        private void materialTextBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Bas�lan tu� rakam de�ilse, 'Backspace' de�ilse, '.' ve ',' karakterlerinden biri de�ilse, tu�u iptal et.
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Tu�u iptal eder
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
