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
    public partial class ProcessOrder : Form
    {
        // Common class suffix = 13
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        public string sGatheredProdNum02 { get; set; }
        public string sGatheredDiscType { get; set; }
        bool bClicked = false;
        DBConnections dbConns13 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing13 = new DataGatheringAndProcessing();
        TaskMethods taskMethods13 = new TaskMethods();

        public ProcessOrder()
        {
            InitializeComponent();
        }

        private void ProcessOrder_Load(object sender, EventArgs e)
        {
            this.ActiveControl = textBox1;
        }

        private void ProcessOrder_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (bClicked != true)
            {
                sGatheredProdNum02 = string.Empty;
            }

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                bClicked = true;

                if (textBox1.Text.Length > 0)
                {
                    string sInput = textBox1.Text.Trim();
                    bool bDigitOnly = false;

                    taskMethods13.IsDigitsOnly(sInput, ref bDigitOnly);

                    if (bDigitOnly == true)
                    {
                        DataTable dTblItems = new DataTable("dTblItems");
                        string sCommText = "SELECT * FROM Items WHERE Lookupnum = '" + sInput + "'";

                        dbConns13.CDSQuery(sCDSConnString, sCommText, dTblItems);

                        if (dTblItems.Rows.Count > 0)
                        {
                            DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                            sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sInput + "'";

                            dbConns13.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                            if (dTblDiscOrders.Rows.Count > 0)
                            {
                                sGatheredDiscType = Convert.ToString(dTblDiscOrders.Rows[0]["DiscType"]).Trim();

                                sGatheredProdNum02 = sInput;

                                this.Close();
                            }
                            else if (dTblDiscOrders.Rows.Count == 0)
                            {
                                sGatheredDiscType = "";

                                sGatheredProdNum02 = sInput;

                                this.Close();
                            }
                        }
                        else if (dTblItems.Rows.Count == 0)
                        {
                            MessageBox.Show("Not a valid production number.");
                            sGatheredProdNum02 = string.Empty;
                            this.textBox1.Text = string.Empty;
                            this.ActiveControl = textBox1;
                            this.Refresh();
                        }
                    }
                    else if (bDigitOnly != true)
                    {
                        MessageBox.Show("Enter a numeric production number please.");
                        sGatheredProdNum02 = string.Empty;
                        this.textBox1.Text = string.Empty;
                        this.ActiveControl = textBox1;
                        this.Refresh();
                    }
                }
                else if (textBox1.Text.Length == 0)
                {
                    MessageBox.Show("Enter a numeric production number please.");
                    sGatheredProdNum02 = string.Empty;
                    this.textBox1.Text = string.Empty;
                    this.ActiveControl = textBox1;
                    this.Refresh();
                }
            }
            catch (Exception ex)
            {
                dbConns13.SaveExceptionToDB(ex);
            }
        }


    }
}
