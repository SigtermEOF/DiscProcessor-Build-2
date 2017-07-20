using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APS_DiscProcessor_Build_2
{
    public partial class About : Form
    {
        // Common class suffix = 14
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns14 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing14 = new DataGatheringAndProcessing();
        string sBreakPoint = string.Empty;

        public About()
        {
            InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            string sVersion = string.Empty;

            DataTable dTblVersion = new DataTable("dTblVersion");
            string sCommText = "SELECT * FROM [ChangeLog] WHERE [App] = 'Processor2' ORDER BY [Version]";

            dbConns14.SQLQuery(sDiscProcessorConnString, sCommText, dTblVersion);

            if (dTblVersion.Rows.Count > 0)
            {
                DataRow dRowVersion = dTblVersion.Rows[dTblVersion.Rows.Count - 1];

                sVersion = Convert.ToString(dRowVersion["Version"]).Trim();

                this.lblVersion.Text = "Version: " + sVersion;

                this.lblCopyright.Text = "© Advanced Photographic Solutions LLC " + DateTime.Now.Year + " TM: Advanced Photographic Solutions LLC";

                this.Refresh();
            }
            else if (dTblVersion.Rows.Count == 0)
            {
                sBreakPoint = string.Empty;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
