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
    public partial class PushOrder : Form
    {
        // Common class suffix = 12
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        public string sGatheredProdNum { get; set; }
        bool bClicked = false;
        DBConnections dbConns12 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing12 = new DataGatheringAndProcessing();
        TaskMethods taskMethods12 = new TaskMethods();

        public PushOrder()
        {
            InitializeComponent();
        }

        private void PushOrder_Load(object sender, EventArgs e)
        {
            this.ActiveControl = textBox1;
        }

        private void PushOrder_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (bClicked != true)
            {
                sGatheredProdNum = string.Empty;
            }

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bClicked = true;

            try
            {
                if (textBox1.Text.Length > 0)
                {
                    string sInput = textBox1.Text.Trim();
                    bool bDigitOnly = false;

                    taskMethods12.IsDigitsOnly(sInput, ref bDigitOnly);

                    if (bDigitOnly == true)
                    {
                        DataTable dTblItems = new DataTable("dTblItems");
                        string sCommText = "SELECT * FROM Items WHERE Lookupnum = '" + sInput + "'";

                        dbConns12.CDSQuery(sCDSConnString, sCommText, dTblItems);

                        if (dTblItems.Rows.Count > 0)
                        {
                            DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                            sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sInput + "'";

                            dbConns12.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                            if (dTblDiscOrders.Rows.Count > 0)
                            {
                                MessageBox.Show("Order already exists in the DiscOrders table. If no new frames have been added a resubmit would be needed.");

                                sGatheredProdNum = sInput;

                                this.Close();
                            }
                            else if (dTblDiscOrders.Rows.Count == 0)
                            {
                                sGatheredProdNum = sInput;

                                this.Close();
                            }
                        }
                        else if (dTblItems.Rows.Count == 0)
                        {
                            MessageBox.Show("Not a valid production number.");
                            sGatheredProdNum = string.Empty;
                            this.textBox1.Text = string.Empty;
                            this.ActiveControl = textBox1;
                            this.Refresh();
                        }
                    }
                    else if (bDigitOnly != true)
                    {
                        MessageBox.Show("Enter a numeric production number please.");
                        sGatheredProdNum = string.Empty;
                        this.textBox1.Text = string.Empty;
                        this.ActiveControl = textBox1;
                        this.Refresh();
                    }
                }
                else if (textBox1.Text.Length == 0)
                {
                    MessageBox.Show("Enter a numeric production number please.");
                    sGatheredProdNum = string.Empty;
                    this.textBox1.Text = string.Empty;
                    this.ActiveControl = textBox1;
                    this.Refresh();
                }
            }
            catch (Exception ex)
            {
                dbConns12.SaveExceptionToDB(ex);
            }
        }
    }
}
