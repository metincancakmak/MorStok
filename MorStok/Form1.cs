using MaterialSkin;
using MaterialSkin.Controls;
using System.Data.SQLite;

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

        private void materialLabel10_Click(object sender, EventArgs e)
        {

        }

        private void materialListView5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MorStok_Load(object sender, EventArgs e)
        {

        }
    }
}
