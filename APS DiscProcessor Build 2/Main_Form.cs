//*****************************
//#define dev
//*****************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    public partial class Main_Form : Form
    {

#if(dev)
        bool bDebug = true;
#endif
#if(!dev)
        bool bDebug = false;
#endif
        #region Global form variables/objects.

        // Common class suffix = 01
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns01 = null; // suffix = 02
        DataGatheringAndProcessing dataGatheringAndProcessing01 = null; // suffix = 03
        DataSet dataSetMain = new DataSet("dataSetMain");
        string sBreakpoint = string.Empty;        
        private int iLoopInterval = 0;
        private bool bIdle = false; // LooperMode0
        private bool bLooperMode1 = false;
        private bool bLooperMode2 = false;
        private bool bLooperMode3 = false;
        private bool bLooperMode4 = false;
        private bool bLooperMode5 = false;
        private bool bLooperMode6 = false;
        private bool bLooperMode7 = false;
        private bool bLooperMode8 = false;
        private bool bLooperMode9 = false;
        private bool bLooperMode10 = false;
        private bool bLooperMode11 = false;
        private bool bLooperMode12 = false;
        PECProcessing PECProc = null; // suffix = 04
        ICDProcessing ICDProc = null; // suffix = 05
        MEGProcessing MEGProc = null; // suffix = 06
        CCDProcessing CCDProc = null; // suffix = 07
        SCDProcessing SCDProc = null; // suffix = 08
        ICSProcessing ICSProc = null; // suffix = 09
        RCDProcessing RCDProc = null; // suffix = 10
        PCDProcessing PCDProc = null; // suffix = 11
        PushOrder pushOrd = null; // suffix = 12
        ProcessOrder processOrd = null; // suffix = 13
        // About.cs = suffix 14
        RIDProcessing RIDProc = null; // suffix = 15
        TaskMethods taskMethods01 = null; // suffix = 16
        // LockFile.cs = suffix 17
        InsertsOrUpdates insertsOrUpdates01 = null; // suffix = 18
        ErrorHandling errorHandling01 = null; // suffixc = 19
        ResubmitHandling resubmitHandling01 = null; // suffix = 20
        GatherWorld gatherWorld01 = null; // suffix = 21

        #endregion

        public Main_Form()
        {
            InitializeComponent();

            dbConns01 = new DBConnections();
            dataGatheringAndProcessing01 = new DataGatheringAndProcessing();
            PECProc = new PECProcessing();
            ICDProc = new ICDProcessing();
            MEGProc = new MEGProcessing();
            CCDProc = new CCDProcessing();
            SCDProc = new SCDProcessing();
            ICSProc = new ICSProcessing();
            RCDProc = new RCDProcessing();
            PCDProc = new PCDProcessing();
            pushOrd = new PushOrder();
            processOrd = new ProcessOrder();
            RIDProc = new RIDProcessing();
            taskMethods01 = new TaskMethods();
            insertsOrUpdates01 = new InsertsOrUpdates();
            errorHandling01 = new ErrorHandling();
            resubmitHandling01 = new ResubmitHandling();
            gatherWorld01 = new GatherWorld();
        }

        #region Form events.
        
        private void Main_Form_Load(object sender, EventArgs e)
        {
            string sDPLooperMachine = string.Empty;
            string sMachineName = Convert.ToString(Environment.MachineName).Trim();
            string sVersion = string.Empty;

            if (bDebug == true)
            {
                MessageBox.Show("Debug mode is enabled.");
            }

            dataGatheringAndProcessing01.GatherDiscProcessorVariables(dataSetMain);

            this.comboBox1.DataSource = dataSetMain.Tables["dTblLooperModes"];
            this.comboBox1.DisplayMember = "Mode";
            this.comboBox1.ValueMember = "Mode";


            dataGatheringAndProcessing01.GatherStartUpVariables(dataSetMain, ref iLoopInterval, ref sDPLooperMachine, ref sVersion);

            string sSearchPattern01 = "Label = 'DP_LooperMachine'";
            DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

            if (dRowGatheredPattern01.Length > 0)
            {
                sDPLooperMachine = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                if (sDPLooperMachine == sMachineName)
                {
                    this.comboBox1.SelectedIndex = 1;
                }
                else if (sDPLooperMachine != sMachineName)
                {
                    this.comboBox1.SelectedIndex = 0;
                }
            }
            else if (dRowGatheredPattern01.Length == 0)
            {
                sBreakpoint = string.Empty;
            }

            this.Text = "A.P.S. DiscProcessor v" + sVersion;
            timerDoWork01.Interval = iLoopInterval;
            this.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iCurrentSelectedIndex = Convert.ToInt32(this.comboBox1.SelectedIndex);
            DateTime dTimeMidnight = DateTime.Now.Date;
            DateTime dTimeMidnightPlus1Hour = DateTime.Now.Date.AddHours(1);

            if ((DateTime.Now > dTimeMidnight && DateTime.Now < dTimeMidnightPlus1Hour))
            {
                //this.comboBox1.SelectedIndex = 0;
                //this.comboBox1.SelectedIndex = 12;
                //this.Refresh();

                //this.DoWork02();

                //this.comboBox1.SelectedIndex = iCurrentSelectedIndex;
                //this.Refresh();
            }
            else
            {
                this.DoWork01();
            }
        }

        private void timerDoWork01_Tick(object sender, EventArgs e)
        {
            int iCurrentSelectedIndex = Convert.ToInt32(this.comboBox1.SelectedIndex);
            DateTime dTimeMidnight = DateTime.Now.Date;
            DateTime dTimeMidnightPlus1Hour = DateTime.Now.Date.AddHours(1);

            if ((DateTime.Now > dTimeMidnight && DateTime.Now < dTimeMidnightPlus1Hour))
            {
                //this.comboBox1.SelectedIndex = 0;
                //this.comboBox1.SelectedIndex = 12;
                //this.Refresh();

                //this.DoWork02(); // taking far too long to run. disabled - jl

                //this.comboBox1.SelectedIndex = iCurrentSelectedIndex;
                //this.Refresh();
            }
            else
            {
                this.DoWork01();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // On click event verify the user wishes to exit the program.
            DialogResult verifyExit;
            verifyExit = MessageBox.Show("Exit the program?", "Exit?", MessageBoxButtons.YesNo);
            // Exit the application if yes is chosen.
            if (verifyExit == DialogResult.Yes)
            {
                Application.Exit();
            }
            else if (verifyExit == DialogResult.No)
            {
                // Do nothing if the user answers no.
                return;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            About about = new About();
            about.ShowDialog();
            this.Enabled = true;
        }

        private void cleanDPTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            taskMethods01.CleanDPTables(dataSetMain, this);
        }

        private void pushOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                comboBox1.SelectedIndex = 0;
                this.Refresh();

                this.Enabled = false;
                pushOrd.ShowDialog();
                string sPassedProdNum = pushOrd.sGatheredProdNum.Trim();
                this.Enabled = true;

                if (sPassedProdNum.Length == 0)
                {
                    MessageBox.Show("No production numbered entered.");
                }
                else if (sPassedProdNum.Length > 0 && sPassedProdNum != "")
                {
                    this.SearchCDSForSingleOrder(sPassedProdNum);

                    string sText = "[Pushing of order complete.]";
                    taskMethods01.LogText(sText, this);
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void processSingleOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                comboBox1.SelectedIndex = 0;
                this.Refresh();

                this.Enabled = false;
                processOrd.ShowDialog();
                string sPassedProdNum02 = processOrd.sGatheredProdNum02.Trim();
                string sPassedDiscType = processOrd.sGatheredDiscType.Trim();
                this.Enabled = true;

                if (sPassedProdNum02.Length == 0)
                {
                    MessageBox.Show("No production numbered entered.");
                }
                else if (sPassedProdNum02.Length > 0 && sPassedProdNum02 != "")
                {
                    this.QueryDiscOrdersForSingleProdNum(sPassedProdNum02);

                    if (sPassedDiscType.Length != 0 && sPassedDiscType != "")
                    {
                        string sSearchPattern01 = "GatherDiscType = '" + sPassedDiscType + "'";
                        DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                        if (dRowGatheredPattern01.Length > 0)
                        {
                            bool bSittingBased = Convert.ToBoolean(dRowGatheredPattern01[0]["SittingBased"]);

                            if (bSittingBased != true)
                            {
                                this.ProcessSingleOrdersFrameDataRecords(sPassedProdNum02);

                                string sText = "[Processing of order complete.]";
                                taskMethods01.LogText(sText, this);
                            }
                            else if (bSittingBased == true)
                            {
                                string sText = "[Processing of order complete.]";
                                taskMethods01.LogText(sText, this);
                            }
                        }
                        else if (dRowGatheredPattern01.Length == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void rtxtboxLog_TextChanged(object sender, EventArgs e)
        {
            try
            {
                rtxtboxLog.SelectionStart = rtxtboxLog.Text.Length;
                rtxtboxLog.ScrollToCaret();
                rtxtboxLog.Refresh();
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedIndex == 0) // Idle.
            {
                this.label1.ForeColor = Color.Red;

                bIdle = true;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                taskMethods01.Clear(this);

                this.toolStripStatusLabel1.Text = "[ Status: Idle ]";
                this.Refresh();

                this.button1.Enabled = false;
                this.timerDoWork01.Stop();
                this.timerDoWork01.Enabled = false;
            }
            else if (this.comboBox1.SelectedIndex == 1)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = true;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 2)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = true;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 3)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = true;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 4)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = true;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 5)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = true;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 6)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = true;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 7)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = true;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 8)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = true;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 9)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = true;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 10)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = true;
                bLooperMode11 = false;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 11)
            {
                this.label1.ForeColor = Color.Green;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = true;
                bLooperMode12 = false;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            else if (this.comboBox1.SelectedIndex == 12)
            {
                this.label1.ForeColor = Color.Red;

                bIdle = false;
                bLooperMode1 = false;
                bLooperMode2 = false;
                bLooperMode3 = false;
                bLooperMode4 = false;
                bLooperMode5 = false;
                bLooperMode6 = false;
                bLooperMode7 = false;
                bLooperMode8 = false;
                bLooperMode9 = false;
                bLooperMode10 = false;
                bLooperMode11 = false;
                bLooperMode12 = true;

                this.button1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
        }

        #endregion

        #region Begin doing work methods.

        private void DoWork01()
        {
            try
            {
                string sText = string.Empty;

                if (bIdle != true)
                {
                    this.button1.Enabled = false;
                    this.comboBox1.Enabled = false;
                    this.timerDoWork01.Stop();
                    this.timerDoWork01.Enabled = false;
                    this.menuStrip1.Enabled = false;

                    string sCurrentFormattedDateTime = DateTime.Now.ToString("HH:mm:ss").Trim();

                    this.toolStripStatusLabel1.Text = "Status: [Current cycle started: " + DateTime.Now.ToString().Trim() + "]";
                    this.Refresh();

                    taskMethods01.Clear(this);

                    if (bLooperMode7 != true && bLooperMode8 != true && bLooperMode9 != true && bLooperMode10 != true && bLooperMode12 != true)
                    {
                        sText = "[Checking for errors.]";
                        taskMethods01.LogText(sText, this);

                        errorHandling01.GatherErrors(dataSetMain, this);

                        sText = "[Checking for resubmits.]";
                        taskMethods01.LogText(sText, this);

                        resubmitHandling01.GatherResubmits(dataSetMain, this);
                    }
                    if (bLooperMode1 == true)
                    {
                        // 1: Gather and Process frame and sitting based work + task methods

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 0 AND [InDevelopment] = 0";
                        this.SearchCDSForReadyWorkInPackages(sGatherDiscTypesString);
                        sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 1 AND [InDevelopment] = 0 AND [OrderItemBased] = 0";
                        this.SearchCDSForReadyWorkInPackages(sGatherDiscTypesString);
                        sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 0 AND [InDevelopment] = 0";
                        this.ProcessAllDiscOrdersRecords(sGatherDiscTypesString);
                        this.ProcessAllFrameDataRecords();
                        sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 1 AND [InDevelopment] = 0 AND [OrderItemBased] = 0";
                        this.ProcessAllDiscOrdersRecords(sGatherDiscTypesString);
                        dataGatheringAndProcessing01.CheckForFrameBasedRenderedImages(dataSetMain, this);
                        dataGatheringAndProcessing01.CheckForSittingBasedRenderedImages(dataSetMain, this);
                        RCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                        PCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                    }
                    else if (bLooperMode2 == true)
                    {
                        // 2: Gather and Process frame based work only + task methods

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 0 AND [InDevelopment] = 0";
                        this.SearchCDSForReadyWorkInPackages(sGatherDiscTypesString);
                        sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 0 AND [InDevelopment] = 0";
                        this.ProcessAllDiscOrdersRecords(sGatherDiscTypesString);
                        this.ProcessAllFrameDataRecords();
                        dataGatheringAndProcessing01.CheckForFrameBasedRenderedImages(dataSetMain, this);
                        RCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                        PCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                    }
                    else if (bLooperMode3 == true)
                    {
                        // 3: Gather and Process sitting based work only + task methods

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 1 AND [InDevelopment] = 0 AND [OrderItemBased] = 0";
                        this.SearchCDSForReadyWorkInPackages(sGatherDiscTypesString);
                        sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 1 AND [InDevelopment] = 0 AND [OrderItemBased] = 0";
                        this.ProcessAllDiscOrdersRecords(sGatherDiscTypesString);
                        dataGatheringAndProcessing01.CheckForSittingBasedRenderedImages(dataSetMain, this);
                    }
                    else if (bLooperMode4 == true)
                    {
                        // 4: Check for frame and sitting based rendered images only + task methods

                        dataGatheringAndProcessing01.CheckForFrameBasedRenderedImages(dataSetMain, this);
                        dataGatheringAndProcessing01.CheckForSittingBasedRenderedImages(dataSetMain, this);
                        RCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                        PCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                    }
                    else if (bLooperMode5 == true)
                    {
                        // 5: Check for frame based rendered images only + task methods

                        dataGatheringAndProcessing01.CheckForFrameBasedRenderedImages(dataSetMain, this);
                        RCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                        PCDProc.CheckForRenderedOrderItems(dataSetMain, this);
                    }
                    else if (bLooperMode6 == true)
                    {
                        // 6: Check for sitting based rendered images only + task methods

                        dataGatheringAndProcessing01.CheckForSittingBasedRenderedImages(dataSetMain, this);
                    }
                    else if (bLooperMode7 == true)
                    {
                        // 7: Gather frame based work only

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 0 AND [InDevelopment] = 0";
                        this.SearchCDSForReadyWorkInPackages(sGatherDiscTypesString);
                    }
                    else if (bLooperMode8 == true)
                    {
                        // 8: Process DiscOrders and FrameData frame based records only

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 0 AND [InDevelopment] = 0";
                        this.ProcessAllDiscOrdersRecords(sGatherDiscTypesString);
                        this.ProcessAllFrameDataRecords();
                    }
                    else if (bLooperMode9 == true)
                    {
                        // 9: Gather Sitting based work only

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 1 AND [InDevelopment] = 0 AND [OrderItemBased] = 0";
                        this.SearchCDSForReadyWorkInPackages(sGatherDiscTypesString);
                    }
                    else if (bLooperMode10 == true)
                    {
                        // 10: Process DiscOrders sitting based records only

                        string sGatherDiscTypesString = "[Gather] = 1 AND [SittingBased] = 1 AND [InDevelopment] = 0 AND [OrderItemBased] = 0";
                        this.ProcessAllDiscOrdersRecords(sGatherDiscTypesString);
                    }
                    else if (bLooperMode11 == true)
                    {
                        // Handled above.
                    }
                    else if (bLooperMode12 == true)
                    {
                        // This mode is driven by a timer. Do nothing if user selects it.
                    }

                    string sDTime1 = DateTime.Now.ToString("MM-dd-yy").Trim();
                    string sDTime2 = DateTime.Now.ToString("HH:mm:ss").Trim();
                    string sDTime3 = "[" + sDTime1 + "][" + sDTime2 + "]";
                    int iLoopIntervalInMins = iLoopInterval / 60000;

                    sText = "[This cycle has completed.]" + Environment.NewLine + sDTime3 + "[Idle for " + iLoopIntervalInMins + " minutes.]" + Environment.NewLine + Environment.NewLine;
                    taskMethods01.LogText(sText, this);

                    string sNextCycle = DateTime.Now.AddMinutes(iLoopIntervalInMins).ToString("HH:mm:ss").Trim();
                    string sDTEnd = DateTime.Now.ToString("HH:mm:ss").Trim();
                    TimeSpan tSpanDuration = DateTime.Parse(sDTEnd).Subtract(DateTime.Parse(sCurrentFormattedDateTime));
                    this.toolStripStatusLabel1.Text = "[ Status: Idle ][ Duration of last cycle: " + tSpanDuration + " ][ Next cycle: " + sNextCycle + " ]";
                    this.Refresh();

                    this.menuStrip1.Enabled = true;
                    this.button1.Enabled = true;
                    this.comboBox1.Enabled = true;
                    this.timerDoWork01.Enabled = true;
                    this.timerDoWork01.Start();
                }
                else if (bIdle == true)
                {
                    // Idle...
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void DoWork02()
        {
            try
            {
                this.button1.Enabled = false;
                this.comboBox1.Enabled = false;
                this.timerDoWork01.Stop();
                this.timerDoWork01.Enabled = false;
                this.menuStrip1.Enabled = false;

                string sCurrentFormattedDateTime = DateTime.Now.ToString("HH:mm:ss").Trim();

                this.toolStripStatusLabel1.Text = "Status: [Current cycle started: " + DateTime.Now.ToString().Trim() + "]";
                this.Refresh();

                string sText = "[Gathering the world.]";

                taskMethods01.LogText(sText, this);

                gatherWorld01.GatherAllReadyWorkInPackagesNoSearchDays(dataSetMain, this);

                string sDTime1 = DateTime.Now.ToString("MM-dd-yy").Trim();
                string sDTime2 = DateTime.Now.ToString("HH:mm:ss").Trim();
                string sDTime3 = "[" + sDTime1 + "][" + sDTime2 + "]";
                int iLoopIntervalInMins = iLoopInterval / 60000;

                sText = "[This cycle has completed.]" + Environment.NewLine + sDTime3 + "[Idle for " + iLoopIntervalInMins + " minutes.]" + Environment.NewLine + Environment.NewLine;

                taskMethods01.LogText(sText, this);

                string sNextCycle = DateTime.Now.AddMinutes(iLoopIntervalInMins).ToString("HH:mm:ss").Trim();
                string sDTEnd = DateTime.Now.ToString("HH:mm:ss").Trim();
                TimeSpan tSpanDuration = DateTime.Parse(sDTEnd).Subtract(DateTime.Parse(sCurrentFormattedDateTime));
                this.toolStripStatusLabel1.Text = "[ Status: Idle ][ Duration of last cycle: " + tSpanDuration + " ][ Next cycle: " + sNextCycle + " ]";

                taskMethods01.Clear(this);

                this.menuStrip1.Enabled = true;
                this.button1.Enabled = true;
                this.comboBox1.Enabled = true;
                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        #endregion

        #region Common gathering methods.

        private void SearchCDSForReadyWorkInPackages(string sGatherDiscTypesString)
        {
            try
            {
                DataTable dTblCollectedPackageRecords = new DataTable("dTblCollectedPackageRecords");

                string sGetSearchDays = "[Label] = 'GatherDays'";
                DataRow[] dRowGetSearchDays = dataSetMain.Tables["dTblVariables"].Select(sGetSearchDays);

                if (dRowGetSearchDays.Length > 0)
                {
                    double dSearchDays = Convert.ToDouble(dRowGetSearchDays[0]["Value"]);

                    string sGetDiscTypesToGather = sGatherDiscTypesString;
                    DataRow[] dRowGetDisctypesToGather = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sGetDiscTypesToGather);

                    if (dRowGetDisctypesToGather.Length > 0)
                    {
                        foreach(DataRow dRow in dRowGetDisctypesToGather)
                        {
                            this.Refresh();

                            dTblCollectedPackageRecords.Clear();
                            dTblCollectedPackageRecords.Dispose();

                            string sDiscType = Convert.ToString(dRow["GatherDiscType"]).Trim();

                            string sText = "[Gathering orders containing " + sDiscType + " discs due out within the last " + dSearchDays.ToString().TrimStart(new Char[] { '-' }).Trim() + " days.]";
                            taskMethods01.LogText(sText, this);

                            DateTime dateTimeMinusSearchDays = DateTime.Now.AddDays(dSearchDays);
                            DateTime dateTimeMinusSearchDaysDateOnly = dateTimeMinusSearchDays.Date;
                            string sDateTimeMinusSearchDaysDateOnly = dateTimeMinusSearchDaysDateOnly.ToString("MM/dd/yy");
                            
                            string sCommText = "SELECT Lookupnum, Packagetag, Order FROM ITEMS WHERE items.d_dueout > CTOD('" + sDateTimeMinusSearchDaysDateOnly + "') AND ITEMS.PACKAGETAG IN" +
                                        " (SELECT PACKAGETAG FROM LABELS WHERE LABELS.CODE = '" + sDiscType + "' AND LABELS.PACKAGETAG <> '    ') ORDER BY items.d_dueout";

                            dbConns01.CDSQuery(sCDSConnString, sCommText, dTblCollectedPackageRecords);

                            if (dTblCollectedPackageRecords.Rows.Count > 0)
                            {
                                int iCollectedPackageRecordsRowCount = dTblCollectedPackageRecords.Rows.Count;

                                if (iCollectedPackageRecordsRowCount == 1)
                                {
                                    sText = "[Gathered " + iCollectedPackageRecordsRowCount + " package record containing " + sDiscType + " discs for processing.]";
                                }
                                else if (iCollectedPackageRecordsRowCount > 1)
                                {
                                    sText = "[Gathered " + iCollectedPackageRecordsRowCount + " package records containing " + sDiscType + " discs for processing.]";
                                }

                                taskMethods01.LogText(sText, this);

                                this.SearchCDSForALaCarteBasedReadyWork(sDateTimeMinusSearchDaysDateOnly, sDiscType, dTblCollectedPackageRecords);
                            }
                            else if (dTblCollectedPackageRecords.Rows.Count == 0)
                            {
                                sText = "[Gathered 0 package records containing " + sDiscType + " discs for processing.]";

                                taskMethods01.LogText(sText, this);

                                this.SearchCDSForALaCarteBasedReadyWork(sDateTimeMinusSearchDaysDateOnly, sDiscType, dTblCollectedPackageRecords);
                            }
                        }
                    }
                    else if (dRowGetDisctypesToGather.Length == 0)
                    {
                        sBreakpoint = string.Empty;
                    }
                }
                else if (dRowGetSearchDays.Length == 0)
                {
                    sBreakpoint = string.Empty;
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void SearchCDSForALaCarteBasedReadyWork(string sDateTimeMinusSearchDaysDateOnly, string sDiscType, DataTable dTblCollectedPackageRecords)
        {
            try
            {
                this.Refresh();

                DataTable dTblCollectedALaCarteRecords = new DataTable("dTblCollectedALaCarteRecords");
                string sCommText = "SELECT Lookupnum, Packagetag, Order FROM ITEMS WHERE items.d_dueout > CTOD('" + sDateTimeMinusSearchDaysDateOnly + "') AND ITEMS.Lookupnum IN" +
                    " (SELECT Lookupnum FROM CODES WHERE Codes.CODE = '" + sDiscType + "') ORDER BY items.d_dueout";

                dbConns01.CDSQuery(sCDSConnString, sCommText, dTblCollectedALaCarteRecords);

                if (dTblCollectedALaCarteRecords.Rows.Count > 0)
                {
                    int iCollectedALaCarteRecordsRowCount = dTblCollectedALaCarteRecords.Rows.Count;
                    string sText = string.Empty;

                    if (iCollectedALaCarteRecordsRowCount == 1)
                    {
                        sText = "[Gathered " + iCollectedALaCarteRecordsRowCount + " a la carte record containing " + sDiscType + " discs for processing.]";
                    }
                    else if (iCollectedALaCarteRecordsRowCount > 1)
                    {
                        sText = "[Gathered " + iCollectedALaCarteRecordsRowCount + " a la carte records containing " + sDiscType + " discs for processing.]";
                    }

                    taskMethods01.LogText(sText, this);                   

                    DataTable dTblTotalCollectedRecords = new DataTable("dTblTotalCollectedRecords");

                    if (dTblCollectedALaCarteRecords.Rows.Count > 0)
                    {
                        dTblTotalCollectedRecords = dTblCollectedALaCarteRecords.Copy();
                    }

                    if (dTblCollectedPackageRecords.Rows.Count > 0)
                    {
                        dTblTotalCollectedRecords.Merge(dTblCollectedPackageRecords);
                    }

                    if (dTblTotalCollectedRecords.Rows.Count > 0)
                    {
                        int iTotalCollectedRecordsRowCount = dTblTotalCollectedRecords.Rows.Count;

                        this.ScanForTriggerPoints(sDiscType, dTblTotalCollectedRecords);
                    }
                    else if (dTblTotalCollectedRecords.Rows.Count == 0)
                    {
                        sText = "[No " + sDiscType + " disc records gathered this cycle.]";

                        taskMethods01.LogText(sText, this);
                        return;
                    }
                }
                else if (dTblCollectedALaCarteRecords.Rows.Count == 0)
                {
                    string sText = "[Gathered 0 a la carte records containing " + sDiscType + " discs for processing.]";

                    taskMethods01.LogText(sText, this);

                    DataTable dTblTotalCollectedRecords = new DataTable("dTblTotalCollectedRecords");

                    if (dTblCollectedPackageRecords.Rows.Count > 0)
                    {
                        dTblTotalCollectedRecords = dTblCollectedPackageRecords.Copy();

                        int iTotalCollectedRecordsRowCount = dTblCollectedPackageRecords.Rows.Count;

                        this.ScanForTriggerPoints(sDiscType, dTblTotalCollectedRecords);
                    }
                    else if (dTblCollectedPackageRecords.Rows.Count == 0)
                    {
                        sText = "[No " + sDiscType + " disc records gathered this cycle.]";

                        taskMethods01.LogText(sText, this);
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        public void ScanForTriggerPoints(string sDiscType, DataTable dTblTotalCollectedRecords)
        {
            try
            {
                string sSavedStamp = sDiscType + @" Saved";

                int iRowCountTotalCollectedRecords = dTblTotalCollectedRecords.Rows.Count;
                string sText = string.Empty;

                if (iRowCountTotalCollectedRecords == 1)
                {
                    sText = "[Checking " + iRowCountTotalCollectedRecords + " " + sDiscType + " disc record for needed trigger points.]";
                }
                else if (iRowCountTotalCollectedRecords > 1)
                {
                    sText = "[Checking " + iRowCountTotalCollectedRecords + " " + sDiscType + " disc records for needed trigger points.]";
                }
                
                taskMethods01.LogText(sText, this);

                bool bPreInsertionDataPrepSetUp = false;
                DataTable dTblPreInsertionDataPrep = new DataTable("dTblPreInsertionDataPrep");

                taskMethods01.SetUpPreInsertionDataPrepTable(dTblPreInsertionDataPrep, ref bPreInsertionDataPrepSetUp);

                if (bPreInsertionDataPrepSetUp == true)
                {
                    if (!dataSetMain.Tables.Contains("dTblPreInsertionDataPrep"))
                    {
                        dataSetMain.Tables.Add(dTblPreInsertionDataPrep);
                    }
                    else if (dataSetMain.Tables.Contains("dTblPreInsertionDataPrep"))
                    {
                        dataSetMain.Tables["dTblPreInsertionDataPrep"].Clear();
                        dataSetMain.Tables["dTblPreInsertionDataPrep"].Dispose();
                    }                    

                    DataTable dTblPassedScanPointsRecords = new DataTable("dTblPassedScanPointsRecords");
                    DataTable dTblExistsInDP2 = new DataTable("dTblExistsInDP2");

                    foreach (DataRow dRow in dTblTotalCollectedRecords.Rows)
                    {
                        bool bPreviouslyProcessed = false;

                        string sProdNum = Convert.ToString(dRow["Lookupnum"]).Trim();
                        string sRefNum = Convert.ToString(dRow["Order"]).Trim();

                        if (sDiscType == "RCD")
                        {
                            sRefNum = "ROES" + sRefNum;
                        }

                        string sPackageTag = Convert.ToString(dRow["Packagetag"]).Trim();

                        DataTable dTblStamps = new DataTable("dTblStamps");
                        string sCommText = "SELECT * FROM STAMPS WHERE Lookupnum = '" + sProdNum + "'";

                        dbConns01.CDSQuery(sCDSConnString, sCommText, dTblStamps);

                        if (dTblStamps.Rows.Count > 0)
                        {
                            string sSearchPattern01 = "((Action = 'DIGI PRINT' OR (Action = 'PRNT_TRAV' AND Stationid <> 'RECEIVING')))";
                            DataRow[] dRowGatheredPattern01 = dTblStamps.Select(sSearchPattern01);

                            if (dRowGatheredPattern01.Length > 0)
                            {
                                foreach(DataRow dRowStamps in dRowGatheredPattern01)
                                {
                                    string sAction = Convert.ToString(dRowStamps["Action"]).Trim();

                                    if (sAction == sSavedStamp)
                                    {
                                        bPreviouslyProcessed = true;
                                        break;
                                    }
                                }

                                if (bPreviouslyProcessed != true)
                                {
                                    this.ExistsInDP2(sRefNum, dTblExistsInDP2, sProdNum, sPackageTag, sDiscType, dTblPreInsertionDataPrep);
                                }
                                else if (bPreviouslyProcessed == true)
                                {
                                    continue;
                                }
                            }
                            else if (dRowGatheredPattern01.Length == 0)
                            {
                                continue;
                            }
                        }
                        else if (dTblStamps.Rows.Count == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }

                    // ************end of foreach

                    int iVerifiedToExistInDP2RowCount = dataSetMain.Tables["dTblPreInsertionDataPrep"].Rows.Count;

                    if (iVerifiedToExistInDP2RowCount == 1)
                    {
                        sText = "[Verified " + iVerifiedToExistInDP2RowCount + " " + sDiscType + " record exists in DP2 and have passed the needed trigger points.]";

                        taskMethods01.LogText(sText, this);
                    }
                    else if (iVerifiedToExistInDP2RowCount > 1)
                    {
                        sText = "[Verified " + iVerifiedToExistInDP2RowCount + " " + sDiscType + " records exist in DP2 and have passed the needed trigger points.]";

                        taskMethods01.LogText(sText, this);
                    }
                    else if (iVerifiedToExistInDP2RowCount == 0)
                    {
                        sBreakpoint = string.Empty;
                    }

                    this.GatherPreDiscOrdersInsertionOrderData();

                }
                else if (bPreInsertionDataPrepSetUp != true)
                {
                    sBreakpoint = string.Empty;
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        public void ExistsInDP2(string sRefNum, DataTable dTblExistsInDP2, string sProdNum, string sPackageTag, string sDiscType, DataTable dTblPreInsertionDataPrep)
        {
            try
            {
                this.Refresh();

                DataRow dRowPreInsertionDataPrep = dataSetMain.Tables["dTblPreInsertionDataPrep"].NewRow();

                DataTable dTblDP2Orders = new DataTable("dTblDP2Orders");
                string sCommText = "SELECT [ID] FROM [Orders] WHERE [ID] = '" + sRefNum + "'";

                dbConns01.SQLQuery(sDP2ConnString, sCommText, dTblDP2Orders);

                if (dTblDP2Orders.Rows.Count > 0)
                {
                    DataTable dTblImages = new DataTable("dTblImages");
                    sCommText = "SELECT * FROM [Images] WHERE [OrderID] = '" + sRefNum + "'";

                    dbConns01.SQLQuery(sDP2ConnString, sCommText, dTblImages);

                    if (dTblImages.Rows.Count > 0)
                    {
                        dRowPreInsertionDataPrep["ProdNum"] = sProdNum;
                        dRowPreInsertionDataPrep["RefNum"] = sRefNum;
                        dRowPreInsertionDataPrep["DiscType"] = sDiscType;
                        dRowPreInsertionDataPrep["PackageTag"] = sPackageTag;

                        dataSetMain.Tables["dTblPreInsertionDataPrep"].Rows.Add(dRowPreInsertionDataPrep.ItemArray);
                    }
                    else if (dTblImages.Rows.Count == 0)
                    {

                    }
                }
                else if (dTblDP2Orders.Rows.Count == 0)
                {

                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        public void GatherPreDiscOrdersInsertionOrderData()
        {
            try
            {
                string sDiscType = string.Empty;
                int iSavedRecordsCount = 0;

                foreach(DataRow dRow in dataSetMain.Tables["dTblPreInsertionDataPrep"].Rows)
                {
                    this.Refresh();

                    string sProdNum = Convert.ToString(dRow["ProdNum"]).Trim();
                    string sRefNum = Convert.ToString(dRow["RefNum"]).Trim();                    
                    string sPackageTag = Convert.ToString(dRow["PackageTag"]).Trim();
                    sDiscType = Convert.ToString(dRow["DiscType"]).Trim();
                    bool bSittingBased = false;

                    string sSearchPattern01 = "GatherDiscType = '" + sDiscType + "'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        bSittingBased = Convert.ToBoolean(dRowGatheredPattern01[0]["SittingBased"]);

                        dataGatheringAndProcessing01.GatherPreDiscOrdersInsertionrOrderData(sProdNum, sRefNum, sDiscType, sPackageTag, dataSetMain, bSittingBased);

                        if (dataSetMain.Tables["dTblPDOIODCodes"].Rows.Count > 0)
                        {
                            if (bSittingBased != true)
                            {
                                this.SaveFrameBasedReadyWorkToDiscOrdersTable(sProdNum, sRefNum, sDiscType, sPackageTag, bSittingBased, ref iSavedRecordsCount);
                            }
                            else if (bSittingBased == true)
                            {
                                this.SaveSittingBasedReadyWorkToDiscOrdersTable(sProdNum, sRefNum, sDiscType, sPackageTag, bSittingBased, ref iSavedRecordsCount);
                            }
                        }
                        else if (dataSetMain.Tables["dTblPDOIODCodes"].Rows.Count == 0)
                        {
                            continue;
                        }                        
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        sBreakpoint = string.Empty;
                    }
                }

                if (dataSetMain.Tables["dTblPreInsertionDataPrep"].Rows.Count > 0)
                {
                    string sText = "[" + sDiscType + " records saved to database this cycle: " + iSavedRecordsCount + "]";

                    taskMethods01.LogText(sText, this);
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        public void SaveFrameBasedReadyWorkToDiscOrdersTable(string sProdNum, string sRefNum, string sDiscType, string sPackageTag, bool bSittingBased, ref int iSavedRecordsCount)
        {
            try
            {
                if (sDiscType == "RCD" && !sRefNum.Contains("ROES"))
                {
                    sRefNum = "ROES" + sRefNum;
                }

                string sText = string.Empty;
                string sImagePath = string.Empty;

                foreach(DataRow dRow in dataSetMain.Tables["dTblPDOIODCodes"].Rows)
                {
                    this.Refresh();

                    iSavedRecordsCount += 1;
                    sImagePath = string.Empty;

                    string sCDSSequence = Convert.ToString(dRow["Sequence"]);
                    string sDP2FrameNum = sCDSSequence.PadLeft(4, '0');
                    int iQuantity = Convert.ToInt32(dRow["Quantity"]);
                    string sOrderType = Convert.ToString(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["Batch"]).Trim();
                    string sServiceType = Convert.ToString(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["Sertype"]).Trim();
                    DateTime dateTimeEntered = Convert.ToDateTime(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["D_dateent"]);
                    string sDateEntered = dateTimeEntered.ToString("M/dd/yy").Trim();
                    string sCustNum = Convert.ToString(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["Customer"]).Trim();

                    if (sCustNum == "58241") // Get original customer number from an IMQ order.
                    {
                        dataGatheringAndProcessing01.GatherOriginalCustNumFromIMQOrder(sProdNum, ref sCustNum);
                    }

                    string sSearchPattern01 = "Sequence = '" + sCDSSequence + "'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblPDOIODFrames"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        string sSitting = Convert.ToString(dRowGatheredPattern01[0]["Sitting"]);

                        string sSearchPattern02 = "Frame = '" + sCDSSequence + "'";
                        DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblPDOIODDp2image"].Select(sSearchPattern02);

                        if (dRowGatheredPattern02.Length > 0 || sDiscType == "RCD") // Have to add RCD to condition here due to ROES orders not storing image data in cds.dp2image
                        {
                            if (sDiscType != "RCD" && sDiscType != "PCD") // Do not need an image path for these disc types as they are order item based.
                            {
                                sImagePath = Convert.ToString(dRowGatheredPattern02[0]["Path"]).Trim();
                            }                            

                            using (SqlConnection sqlConn = new SqlConnection(sDiscProcessorConnString))
                            {
                                sqlConn.Open();

                                try
                                {
                                    string sCommText = "INSERT INTO [DiscOrders] ([ProdNum], [RefNum], [FrameNum], [Status], [LastCheck], [CustNum], [Packagetag]," +
                                    " [Quantity], [DiscType], [OrderType], [ServiceType], [Received], [ImageLocation], [RecordCollectedDate], [UniqueID], [Sitting], [ResubmitCount], [Error])" +
                                    " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sDP2FrameNum + "', '10', '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() + "', '" + sCustNum +
                                    "', '" + sPackageTag + "', '" + iQuantity + "', '" + sDiscType + "', '" + sOrderType + "', '" + sServiceType + "', '" + sDateEntered +
                                    "', '" + sImagePath + "', '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() + "', '" + sProdNum + sDP2FrameNum + sDiscType + "', '" + sSitting +
                                    "', '0', '0' )";

                                    SqlCommand sqlCommand = sqlConn.CreateCommand();
                                    sqlCommand.CommandText = sCommText;
                                    sqlCommand.ExecuteNonQuery();

                                    sText = "[Saved " + sDiscType + " record #: " + iSavedRecordsCount + "][Added reference #: " + sRefNum + " and frame #: " + sDP2FrameNum + " to the database.]";

                                    taskMethods01.LogText(sText, this);

                                    bool bInsertSuccess = false;

                                    insertsOrUpdates01.InsertIntoCDSDiscOrders(dataSetMain, sRefNum, sProdNum, sCDSSequence, sSitting, sDiscType, ref bInsertSuccess, sDP2FrameNum); // Insert records into CDS.DiscOrders for resubmitting through APS LAB

                                    if (bInsertSuccess == true)
                                    {
                                        bool bStamped = false;
                                        string sAction = sDiscType + @" Saved";

                                        insertsOrUpdates01.UpdateStamps(sProdNum, sAction, ref bStamped);

                                        if (bStamped == true)
                                        {
                                            
                                        }
                                        else if (bStamped != true)
                                        {
                                            sBreakpoint = string.Empty;
                                        }

                                        if (iQuantity == 0)
                                        {
                                            string sErrorDescription = "No quantity for disc.";

                                            insertsOrUpdates01.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sDP2FrameNum, sDiscType, sSitting, bSittingBased);
                                        }
                                        if (sDiscType != "RCD" && (sPackageTag == "" || sPackageTag.Length == 0))
                                        {
                                            string sErrorDescription = "No packagetag gathered.";

                                            insertsOrUpdates01.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sDP2FrameNum, sDiscType, sSitting, bSittingBased);
                                        }
                                    }
                                    else if (bInsertSuccess != true)
                                    {
                                        sBreakpoint = string.Empty;
                                    }
                                }
                                catch (System.Data.SqlClient.SqlException)
                                {
                                    iSavedRecordsCount -= 1;
                                }
                            }
                        }
                        else if (dRowGatheredPattern02.Length == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        sBreakpoint = string.Empty;
                    }
                }
                
                //end of foreach
                sBreakpoint = string.Empty;
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        public void SaveSittingBasedReadyWorkToDiscOrdersTable(string sProdNum, string sRefNum, string sDiscType, string sPackageTag, bool bSittingBased, ref int iSavedRecordsCount)
        {
            try
            {
                this.Refresh();

                string sText = string.Empty;
                string sImagePath = string.Empty;
                string sSitting = string.Empty;

                foreach (DataRow dRowPDOIODFrames in dataSetMain.Tables["dTblPDOIODFrames"].Rows)
                {
                    iSavedRecordsCount += 1;

                    string sCDSSequence = Convert.ToString(dRowPDOIODFrames["Sequence"]);
                    string sDP2FrameNum = sCDSSequence.PadLeft(4, '0');
                    int iQuantity = Convert.ToInt32(dRowPDOIODFrames["Quantity"]);
                    sSitting = Convert.ToString(dRowPDOIODFrames["Sitting"]);
                    string sOrderType = Convert.ToString(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["Batch"]).Trim();
                    string sServiceType = Convert.ToString(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["Sertype"]).Trim();
                    DateTime dateTimeEntered = Convert.ToDateTime(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["D_dateent"]);
                    string sDateEntered = dateTimeEntered.ToString("M/dd/yy").Trim();
                    string sCustNum = Convert.ToString(dataSetMain.Tables["dTblPDOIODItems"].Rows[0]["Customer"]).Trim();

                    if (sCustNum == "58241") // Get original customer number from an IMQ order.
                    {
                        dataGatheringAndProcessing01.GatherOriginalCustNumFromIMQOrder(sProdNum, ref sCustNum);
                    }

                    string sSearchPattern01 = "Frame = '" + sCDSSequence + "'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblPDOIODDp2image"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        sImagePath = Convert.ToString(dRowGatheredPattern01[0]["Path"]).Trim();

                        using (SqlConnection sqlConn = new SqlConnection(sDiscProcessorConnString))
                        {
                            sqlConn.Open();

                            try
                            {
                                string sCommText = "INSERT INTO [DiscOrders] ([ProdNum], [RefNum], [FrameNum], [Status], [LastCheck], [CustNum], [Packagetag]," +
                                " [Quantity], [DiscType], [OrderType], [ServiceType], [Received], [ImageLocation], [RecordCollectedDate], [UniqueID], [Sitting], [ResubmitCount], [Error])" +
                                " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sDP2FrameNum + "', '10', '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() + "', '" + sCustNum +
                                "', '" + sPackageTag + "', '" + iQuantity + "', '" + sDiscType + "', '" + sOrderType + "', '" + sServiceType + "', '" + sDateEntered +
                                "', '" + sImagePath + "', '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() + "', '" + sProdNum + sSitting.Trim() + sDiscType + "', '" + sSitting +
                                "', '0', '0' )";

                                SqlCommand sqlCommand = sqlConn.CreateCommand();
                                sqlCommand.CommandText = sCommText;
                                sqlCommand.ExecuteNonQuery();

                                sText = "[Saved " + sDiscType + " record #: " + iSavedRecordsCount + "][Added reference #: " + sRefNum + " and sitting #: " + sSitting.Trim() + " to the database.]";

                                taskMethods01.LogText(sText, this);

                                bool bInsertSuccess = false;

                                insertsOrUpdates01.InsertIntoCDSDiscOrders(dataSetMain, sRefNum, sProdNum, sCDSSequence, sSitting, sDiscType, ref bInsertSuccess, sDP2FrameNum); // Insert records into CDS.DiscOrders for resubmitting through APS LAB

                                if (bInsertSuccess == true)
                                {
                                    bool bStamped = false;
                                    string sAction = sDiscType + @" Saved";

                                    insertsOrUpdates01.UpdateStamps(sProdNum, sAction, ref bStamped);

                                    if (bStamped == true)
                                    {
                                        continue;
                                    }
                                    else if (bStamped != true)
                                    {
                                        sBreakpoint = string.Empty;
                                    }

                                    if (iQuantity == 0)
                                    {
                                        string sErrorDescription = "No quantity for disc.";

                                        insertsOrUpdates01.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sDP2FrameNum, sDiscType, sSitting, bSittingBased);
                                    }
                                }
                                else if (bInsertSuccess != true)
                                {
                                    sBreakpoint = string.Empty;
                                }
                            }
                            catch (System.Data.SqlClient.SqlException)
                            {
                                iSavedRecordsCount -= 1;
                            }
                        }
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        sBreakpoint = string.Empty;
                    }
                }

                //end of foreach
                sBreakpoint = string.Empty;
            }
            catch (Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        #endregion

        #region Common processing methods.

        private void ProcessAllDiscOrdersRecords(string sGatherDiscTypesString)
        {
            try
            {
                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");

                DataTable dTblDiscOrdersDistinctProdNums = new DataTable("dTblDiscOrdersDistinctProdNums");
                string sCommText = "SELECT DISTINCT [ProdNum] FROM [DiscOrders] WHERE [Status] = '10'";

                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrdersDistinctProdNums);

                if (dTblDiscOrdersDistinctProdNums.Rows.Count > 0)
                {
                    foreach (DataRow dRowDiscOrdersDistinctProdNums in dTblDiscOrdersDistinctProdNums.Rows)
                    {
                        string sProdNum = Convert.ToString(dRowDiscOrdersDistinctProdNums["ProdNum"]).Trim();

                        string sGetDiscTypesToGather = sGatherDiscTypesString;
                        DataRow[] dRowGetDisctypesToGather = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sGetDiscTypesToGather);

                        if (dRowGetDisctypesToGather.Length > 0)
                        {
                            foreach (DataRow dRow in dRowGetDisctypesToGather)
                            {
                                dTblDiscOrders.Clear();

                                string sDiscType = Convert.ToString(dRow["GatherDiscType"]);
                                bool bSittingBased = Convert.ToBoolean(dRow["SittingBased"]);
                                bool bGetsCopyrightReleaseImage = Convert.ToBoolean(dRow["GetsCopyrightReleaseImage"]);
                                bool bSittingInPackagesBased = Convert.ToBoolean(dRow["SittingInPackagesBased"]);
                                bool bOrderItemBased = Convert.ToBoolean(dRow["OrderItemBased"]);
                                bool bGenerateNWPFiles = Convert.ToBoolean(dRow["GenerateNWPFiles"]);

                                sCommText = "SELECT * FROM [DiscOrders] WHERE [Status] = '10' AND [DiscType] = '" + sDiscType + "' AND [ProdNum] = '" + sProdNum + "'";

                                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                                if (dTblDiscOrders.Rows.Count > 0)
                                {
                                    foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                                    {
                                        this.Refresh();

                                        string sPackageTag = Convert.ToString(dRowDiscOrders["PackageTag"]).Trim();
                                        string sCustNum = Convert.ToString(dRowDiscOrders["CustNum"]).Trim();
                                        string sFrameNum = Convert.ToString(dRowDiscOrders["FrameNum"]).Trim();
                                        string sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                                        string sImageLocation = Convert.ToString(dRowDiscOrders["ImageLocation"]).Trim();
                                        int iQuantity = Convert.ToInt32(dRowDiscOrders["Quantity"]);
                                        string sSitting = Convert.ToString(dRowDiscOrders["Sitting"]);
                                        bool bInitialPass01 = false;

                                        string sText = "[Currently processing DiscOrder record]";
                                        taskMethods01.LogText(sText, this);
                                        sText = "[Disc type]: " + sDiscType;
                                        taskMethods01.LogText(sText, this);
                                        sText = "[ProdNum]: " + sProdNum;
                                        taskMethods01.LogText(sText, this);
                                        sText = "[RefNum]: " + sRefNum;
                                        taskMethods01.LogText(sText, this);
                                        sText = "[FrameNum]: " + sFrameNum;
                                        taskMethods01.LogText(sText, this);
                                        sText = "[Sitting]: " + sSitting.Trim();
                                        taskMethods01.LogText(sText + Environment.NewLine, this);

                                        dataGatheringAndProcessing01.GatherPreFrameDataInsertionData(sProdNum, sRefNum, sDiscType, sPackageTag, dataSetMain, bSittingBased);

                                        if (bSittingBased != true)
                                        {
                                            if (bLooperMode1 == true || bLooperMode2 == true || bLooperMode8 == true)
                                            {
                                                if (sDiscType != "RCD" && sDiscType != "PCD")
                                                {
                                                    dataGatheringAndProcessing01.QueryDiscOrdersForAllFrames(dTblDiscOrders, sDiscType, bSittingBased, sCustNum, sPackageTag, sProdNum, sFrameNum, sRefNum, sImageLocation, iQuantity, sSitting, dataSetMain, this);
                                                }
                                                else if (sDiscType == "RCD")
                                                {
                                                    RCDProc.PushRCDToRender(sProdNum, dataSetMain, this);
                                                }
                                                else if (sDiscType == "PCD")
                                                {
                                                    PCDProc.PushPCDToRender(sProdNum, dataSetMain, this);
                                                }
                                            }
                                        }
                                        else if (bSittingBased == true)
                                        {
                                            if (bLooperMode1 == true || bLooperMode3 == true || bLooperMode10 == true)
                                            {
                                                if (sDiscType == "PEC")
                                                {
                                                    bool bCreated = false;
                                                    DataTable dTblOrder = new DataTable("dTblOrder");
                                                    PECProc.PECGatherRenderInfo(sProdNum, ref bInitialPass01, ref bCreated, dTblOrder, sSitting, dataSetMain, this);
                                                }
                                                else if (sDiscType == "SCD")
                                                {
                                                    bool bCreated = false;
                                                    DataTable dTblOrder = new DataTable("dTblOrder");
                                                    SCDProc.SCDGatherRenderInfo(sProdNum, ref bInitialPass01, ref bCreated, dTblOrder, sSitting, dataSetMain, this);
                                                }
                                                else if (sDiscType == "ICS")
                                                {
                                                    bool bCreated = false;
                                                    DataTable dTblOrder = new DataTable("dTblOrder");
                                                    ICSProc.ICSGatherRenderInfo(sProdNum, ref bInitialPass01, ref bCreated, dTblOrder, sSitting, dataSetMain, this);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (dTblDiscOrders.Rows.Count == 0)
                                {
                                    continue;
                                }
                            }
                        }
                        else if (dRowGetDisctypesToGather.Length == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }
                }
                else if (dTblDiscOrdersDistinctProdNums.Rows.Count == 0)
                {
                    // Continue.
                }
            }
            catch(Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void ProcessAllFrameDataRecords()
        {
            try
            {
                int iRowCount = 0;

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                DataTable dTblFrameData = new DataTable("dTblFrameData");
                DataTable dTblFrames = new DataTable("dTblFrames");
                bool bSittingBased = false;
                string sImageLocation = string.Empty;
                int iJobIDsNeeded = 0;
                string sGSBkGrnd = string.Empty;
                string sExportDefFile = string.Empty;
                string sUniqueID = string.Empty;
                string sNameOn = string.Empty;
                string sYearOn = string.Empty;
                string sCustNum = string.Empty;
                string sDP2Mask = string.Empty;
                string sPackageTag = string.Empty;
                bool bMultiRenderGS = false;
                DataTable dTblJob = new DataTable("dTblJob");
                bool bCreated = false;
                int iRenderedCount = 0;
                int iJobID = 0;
                int iBatchID = 0;
                bool bIDsGathered = false;
                int iLoops = 0;
                int iCount = 0;
                bool bInitialJobIDAssigned = false;
                bool bGoodResults = true;

                string sCommText = "SELECT DISTINCT [FrameData].[ProdNum], [FrameData].[RefNum], [FrameData].[FrameNum], [FrameData].[Sitting], [FrameData].[DiscType] FROM [FrameData], [DiscOrders] WHERE" +
                    " [FrameData].[Processed] = '1' AND ([FrameData].[ExportDefGenerated] IS NULL OR [FrameData].[ExportDefGenerated] = '0') AND ([DiscOrders].[Error] != '1' OR [DiscOrders].[Error] IS NULL) AND [FrameData].[ProdNum] = [DiscOrders].[ProdNum]";

                DataTable dTblDistinctFrameDataRecords = new DataTable("dTblDistinctFrameDataRecords");

                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDistinctFrameDataRecords);

                if (dTblDistinctFrameDataRecords.Rows.Count > 0)
                {
                    foreach (DataRow dRowDistinctFrameDataRecords in dTblDistinctFrameDataRecords.Rows)
                    {
                        this.Refresh();

                        dTblJob.Clear();
                        dTblDiscOrders.Clear();
                        dTblFrameData.Clear();
                        dTblFrames.Clear();
                        bIDsGathered = false;
                        iRenderedCount = 0;
                        iLoops = 0;

                        string sProdNum = Convert.ToString(dRowDistinctFrameDataRecords["ProdNum"]).Trim();
                        string sFrameNum = Convert.ToString(dRowDistinctFrameDataRecords["FrameNum"]).Trim();
                        string sSitting = Convert.ToString(dRowDistinctFrameDataRecords["Sitting"]);
                        string sDiscType = Convert.ToString(dRowDistinctFrameDataRecords["DiscType"]).Trim();
                        string sRefNum = Convert.ToString(dRowDistinctFrameDataRecords["RefNum"]).Trim();

                        string sText = "[Currently processing FrameData record]";
                        taskMethods01.LogText(sText, this);
                        sText = "[Disc type]: " + sDiscType;
                        taskMethods01.LogText(sText, this);
                        sText = "[ProdNum]: " + sProdNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[RefNum]: " + sRefNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[FrameNum]: " + sFrameNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[Sitting]: " + sSitting.Trim();
                        taskMethods01.LogText(sText + Environment.NewLine, this);

                        string sSearchPattern01 = "GatherDiscType = '" + sDiscType + "'";
                        DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                        if (dRowGatheredPattern01.Length > 0)
                        {
                            bSittingBased = Convert.ToBoolean(dRowGatheredPattern01[0]["SittingBased"]);

                            sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "'";

                            dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                            if (dTblDiscOrders.Rows.Count > 0)
                            {
                                sCommText = "SELECT * FROM [FrameData] WHERE [Prodnum] = '" + sProdNum + "'";

                                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblFrameData);

                                if (dTblFrameData.Rows.Count > 0)
                                {
                                    sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "'";

                                    dbConns01.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                                    if (dTblFrames.Rows.Count > 0)
                                    {
                                        bool bFoundImageSuccess = false;

                                        taskMethods01.CheckForImages(sRefNum, sFrameNum, sDiscType, ref bFoundImageSuccess, bSittingBased, dTblDiscOrders, dTblFrames, sSitting, sProdNum);

                                        if (bFoundImageSuccess == true)
                                        {
                                            string sSearchPattern02 = "FrameNum = '" + sFrameNum + "'";
                                            DataRow[] dRowGatheredPattern02 = dTblFrameData.Select(sSearchPattern02);

                                            if (dRowGatheredPattern02.Length > 0)
                                            {
                                                iCount = dRowGatheredPattern02.Length * 3;

                                                foreach (DataRow dRowSearchPattern02 in dRowGatheredPattern02)
                                                {                                                    
                                                    iRowCount += 1;

                                                    sPackageTag = Convert.ToString(dRowSearchPattern02["PkgTag"]).Trim();
                                                    bMultiRenderGS = Convert.ToBoolean(dRowSearchPattern02["MultiRenderGS"]);
                                                    sGSBkGrnd = Convert.ToString(dRowSearchPattern02["GSBackground"]).Trim();
                                                    sExportDefFile = Convert.ToString(dRowSearchPattern02["ExportDefFile"]).Trim();
                                                    sUniqueID = Convert.ToString(dRowSearchPattern02["UniqueID"]).Trim();
                                                    sNameOn = Convert.ToString(dRowSearchPattern02["NameOn"]).Trim();
                                                    sYearOn = Convert.ToString(dRowSearchPattern02["YearOn"]).Trim();
                                                    sDP2Mask = Convert.ToString(dRowSearchPattern02["DP2Mask"]).Trim();

                                                    string sSearchPattern03 = "FrameNum = '" + sFrameNum + "'";
                                                    DataRow[] dRowGatheredPattern03 = dTblDiscOrders.Select(sSearchPattern03);

                                                    if (dRowGatheredPattern03.Length > 0)
                                                    {
                                                        sImageLocation = Convert.ToString(dRowGatheredPattern03[0]["ImageLocation"]).Trim();
                                                        iJobIDsNeeded = Convert.ToInt32(dRowGatheredPattern03[0]["JobIDsNeeded"]);
                                                        sCustNum = Convert.ToString(dRowGatheredPattern03[0]["CustNum"]).Trim();

                                                        if (sCustNum == "58241") // Get original customer number from an IMQ order.
                                                        {
                                                            dataGatheringAndProcessing01.GatherOriginalCustNumFromIMQOrder(sProdNum, ref sCustNum);
                                                        }

                                                        if (sDiscType == "ICD")
                                                        {
                                                            if (bMultiRenderGS == true)
                                                            {
                                                                bool bStylesAndBGs = true;
                                                                ICDProc.ICDExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, sGSBkGrnd, bStylesAndBGs, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                            }
                                                            else if (bMultiRenderGS != true)
                                                            {
                                                                bool bStylesAndBGs = false;
                                                                ICDProc.ICDExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, sGSBkGrnd, bStylesAndBGs, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                            }
                                                        }
                                                        else if (sDiscType == "MEG")
                                                        {
                                                            bool bInitialPass01 = true;
                                                            MEGProc.MEGExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, ref bInitialPass01, sGSBkGrnd, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                        }
                                                        else if (sDiscType == "CCD")
                                                        {
                                                            bool bInitialPass01 = true;
                                                            CCDProc.CCDExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, ref bInitialPass01, sGSBkGrnd, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                        }
                                                    }
                                                    else if (dRowGatheredPattern03.Length == 0)
                                                    {
                                                        sBreakpoint = string.Empty;
                                                    }
                                                }

                                                // End of foreach.

                                                if (dTblJob.Rows.Count > 0)
                                                {
                                                    dataGatheringAndProcessing01.ExportDefProcessing(sProdNum, bGoodResults, dTblJob, sDiscType, bSittingBased, dataSetMain, this);
                                                }
                                                else if (dTblJob.Rows.Count == 0)
                                                {
                                                    sBreakpoint = string.Empty;
                                                }
                                            }
                                            else if (dRowGatheredPattern02.Length == 0)
                                            {
                                                sBreakpoint = string.Empty;
                                            }
                                        }
                                        else if (bFoundImageSuccess != true)
                                        {
                                            DataTable dTblDiscOrdersErrors = new DataTable("dTblDiscOrdersErrors");

                                            sCommText = "SELECT [Error], [ErrorChecked], [ErrorDescription] FROM [DiscOrders] WHERE [UniqueID] = '" + sProdNum + sFrameNum + sDiscType + "'";

                                            dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrdersErrors);

                                            if (dTblDiscOrdersErrors.Rows.Count > 0)
                                            {
                                                string sError = Convert.ToString(dTblDiscOrders.Rows[0]["Error"]).Trim();
                                                string sErrorChecked = Convert.ToString(dTblDiscOrders.Rows[0]["ErrorChecked"]).Trim();
                                                string sErrorDescription = Convert.ToString(dTblDiscOrders.Rows[0]["ErrorDescription"]).Trim();

                                                if (sError == "1" || sErrorChecked == "1" || sErrorDescription.Contains("Images not located on server."))
                                                {
                                                    // Do nothing.
                                                }
                                                else if (sError != "1" || sErrorChecked != "1" || (!sErrorDescription.Contains("Images not located on server.")))
                                                {
                                                    string sErrorDescript = "[ " + DateTime.Now.ToString() + " ][ Images not located on server. ]";

                                                    insertsOrUpdates01.UpdateDiscOrdersForErrors(sErrorDescript, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    string sStatus = "90";

                                                    insertsOrUpdates01.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (dTblDiscOrders.Rows.Count == 0)
                                            {
                                                sBreakpoint = string.Empty;
                                            }
                                        }
                                    }
                                    else if (dTblFrames.Rows.Count == 0)
                                    {
                                        sBreakpoint = string.Empty;
                                    }
                                }
                                else if (dTblFrameData.Rows.Count == 0)
                                {
                                    sBreakpoint = string.Empty;
                                }
                            }
                            else if (dTblDiscOrders.Rows.Count == 0)
                            {
                                sBreakpoint = string.Empty;
                            }
                        }
                        else if (dRowGatheredPattern01.Length == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }
                }
                else if (dTblDistinctFrameDataRecords.Rows.Count == 0)
                {
                    // Continue, no ready work.
                }
            }
            catch (Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        #endregion

        #region Single order handling methods.

        private void SearchCDSForSingleOrder(string sProdNum)
        {
            try
            {
                string sText = string.Empty;

                this.button1.Enabled = false;
                this.comboBox1.Enabled = false;

                this.timerDoWork01.Stop();
                this.timerDoWork01.Enabled = false;

                string sDTStart = DateTime.Now.ToString("HH:mm:ss").Trim();

                this.toolStripStatusLabel1.Text = "Status: [Current cycle started: " + DateTime.Now.ToString().Trim() + "]";
                this.Refresh();

                DataTable dTblItems = new DataTable("dTblItems");
                string sCommText = "SELECT * FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                dbConns01.CDSQuery(sCDSConnString, sCommText, dTblItems);

                if (dTblItems.Rows.Count > 0)
                {
                    string sRefNum = Convert.ToString(dTblItems.Rows[0]["Order"]).Trim();
                    string sPackageTag = Convert.ToString(dTblItems.Rows[0]["Packagetag"]).Trim();

                    DataTable dTblLabels = new DataTable("dTblLabels");
                    DataRow dRowLables = dTblLabels.NewRow();
                    dTblLabels.Columns.Add("Lookupnum", typeof(String));
                    dTblLabels.Columns.Add("Order", typeof(String));
                    dTblLabels.Columns.Add("Packagetag", typeof(string));

                    dRowLables["Lookupnum"] = sProdNum;
                    dRowLables["Order"] = sRefNum;
                    dRowLables["Packagetag"] = sPackageTag;
                    dTblLabels.Rows.Add(dRowLables.ItemArray);

                    string sSearchPattern01 = "Gather = 1 AND InDevelopment = 0";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        foreach (DataRow dRowGatherDiscTypes in dRowGatheredPattern01)
                        {
                            string sDiscType = Convert.ToString(dRowGatherDiscTypes["GatherDiscType"]).Trim();
                            bool bGather = Convert.ToBoolean(dRowGatherDiscTypes["Gather"]);

                            sText = "[Pushing production number: " + sProdNum + ". Checking for " + sDiscType + " in order.]";
                            taskMethods01.LogText(sText, this);

                            this.ScanForTriggerPoints(sDiscType, dTblLabels);
                        }
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        sBreakpoint = string.Empty;
                    }
                }
                else if (dTblItems.Rows.Count == 0)
                {
                    sBreakpoint = string.Empty;
                }

                this.button1.Enabled = true;
                this.comboBox1.Enabled = true;

                this.timerDoWork01.Enabled = true;
                this.timerDoWork01.Start();
            }
            catch (Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void QueryDiscOrdersForSingleProdNum(string sProdNum)
        {
            try
            {
                bool bInitialPass01 = true;

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                string sCommText = "SELECT * FROM [DiscOrders] WHERE [Status] = '10' AND [ProdNum] = '" + sProdNum + "'";

                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                if (dTblDiscOrders.Rows.Count > 0)
                {
                    foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                    {
                        string sDiscType = Convert.ToString(dRowDiscOrders["DiscType"]).Trim();
                        string sSitting = Convert.ToString(dRowDiscOrders["Sitting"]);
                        string sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                        string sFrameNum = Convert.ToString(dRowDiscOrders["FrameNum"]).Trim();
                        string sCustNum = Convert.ToString(dRowDiscOrders["CustNum"]).Trim();
                        string sPackageTag = Convert.ToString(dRowDiscOrders["PackageTag"]).Trim();
                        string sImageLocation = Convert.ToString(dRowDiscOrders["ImageLocation"]).Trim();
                        int iQuantity = Convert.ToInt32(dRowDiscOrders["Quantity"]);

                        string sText = "[Currently processing DiscOrder record]";
                        taskMethods01.LogText(sText, this);
                        sText = "[Disc type]: " + sDiscType;
                        taskMethods01.LogText(sText, this);
                        sText = "[ProdNum]: " + sProdNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[RefNum]: " + sRefNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[FrameNum]: " + sFrameNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[Sitting]: " + sSitting.Trim();
                        taskMethods01.LogText(sText + Environment.NewLine, this);

                        string sSearchPattern01 = "GatherDiscType = '" + sDiscType + "'";
                        DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                        if (dRowGatheredPattern01.Length > 0)
                        {
                            bool bSittingBased = Convert.ToBoolean(dRowGatheredPattern01[0]["SittingBased"]);

                            if (bSittingBased != true)
                            {
                                if (sDiscType != "RCD" && sDiscType != "PCD")
                                {
                                    dataGatheringAndProcessing01.QueryDiscOrdersForAllFrames(dTblDiscOrders, sDiscType, bSittingBased, sCustNum, sPackageTag, sProdNum, sFrameNum, sRefNum, sImageLocation, iQuantity, sSitting, dataSetMain, this);
                                }
                                else if (sDiscType == "RCD")
                                {
                                    RCDProc.PushRCDToRender(sProdNum, dataSetMain, this);
                                }
                                else if (sDiscType == "PCD")
                                {
                                    PCDProc.PushPCDToRender(sProdNum, dataSetMain, this);
                                }
                            }
                            else if (bSittingBased == true)
                            {
                                if (sDiscType == "PEC")
                                {
                                    bool bCreated = false;
                                    DataTable dTblOrder = new DataTable("dTblOrder");
                                    PECProc.PECGatherRenderInfo(sProdNum, ref bInitialPass01, ref bCreated, dTblOrder, sSitting, dataSetMain, this);
                                }
                                else if (sDiscType == "SCD")
                                {
                                    bool bCreated = false;
                                    DataTable dTblOrder = new DataTable("dTblOrder");
                                    SCDProc.SCDGatherRenderInfo(sProdNum, ref bInitialPass01, ref bCreated, dTblOrder, sSitting, dataSetMain, this);
                                }
                                else if (sDiscType == "ICS")
                                {
                                    bool bCreated = false;
                                    DataTable dTblOrder = new DataTable("dTblOrder");
                                    ICSProc.ICSGatherRenderInfo(sProdNum, ref bInitialPass01, ref bCreated, dTblOrder, sSitting, dataSetMain, this);
                                }
                            }
                        }
                        else if (dRowGatheredPattern01.Length == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }
                }
                else if (dTblDiscOrders.Rows.Count == 0)
                {
                    sBreakpoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        private void ProcessSingleOrdersFrameDataRecords(string sProdNum)
        {
            try
            {
                int iRowCount = 0;

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                DataTable dTblFrameData = new DataTable("dTblFrameData");
                DataTable dTblFrames = new DataTable("dTblFrames");
                bool bSittingBased = false;
                string sImageLocation = string.Empty;
                int iJobIDsNeeded = 0;
                string sGSBkGrnd = string.Empty;
                string sExportDefFile = string.Empty;
                string sUniqueID = string.Empty;
                string sNameOn = string.Empty;
                string sYearOn = string.Empty;
                string sCustNum = string.Empty;
                string sDP2Mask = string.Empty;
                string sPackageTag = string.Empty;
                bool bMultiRenderGS = false;
                DataTable dTblJob = new DataTable("dTblJob");
                bool bCreated = false;
                int iRenderedCount = 0;
                int iJobID = 0;
                int iBatchID = 0;
                bool bIDsGathered = false;
                int iLoops = 0;
                int iCount = 0;
                bool bInitialJobIDAssigned = false;
                bool bGoodResults = true;

                string sCommText = "SELECT DISTINCT [FrameData].[RefNum], [FrameData].[FrameNum], [FrameData].[Sitting], [FrameData].[DiscType] FROM [FrameData], [DiscOrders] WHERE" +
                    " [FrameData].[ProdNum] = '" + sProdNum + "' AND" +
                    " [FrameData].[Processed] = '1' AND ([FrameData].[ExportDefGenerated] IS NULL OR [FrameData].[ExportDefGenerated] = '0') AND ([DiscOrders].[Error] != '1' OR" +
                    " [DiscOrders].[Error] IS NULL) AND [FrameData].[ProdNum] = [DiscOrders].[ProdNum]";

                DataTable dTblDistinctFrameDataRecords = new DataTable("dTblDistinctFrameDataRecords");

                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDistinctFrameDataRecords);

                if (dTblDistinctFrameDataRecords.Rows.Count > 0)
                {
                    string sText = "[Beginning ExportDef file generation for gathered records.]";
                    taskMethods01.LogText(sText, this);

                    foreach (DataRow dRowDistinctFrameDataRecords in dTblDistinctFrameDataRecords.Rows)
                    {
                        dTblDiscOrders.Clear();
                        dTblFrameData.Clear();
                        dTblFrames.Clear();
                        bIDsGathered = false;
                        iRenderedCount = 0;
                        iLoops = 0;

                        string sFrameNum = Convert.ToString(dRowDistinctFrameDataRecords["FrameNum"]).Trim();
                        string sSitting = Convert.ToString(dRowDistinctFrameDataRecords["Sitting"]);
                        string sDiscType = Convert.ToString(dRowDistinctFrameDataRecords["DiscType"]).Trim();
                        string sRefNum = Convert.ToString(dRowDistinctFrameDataRecords["RefNum"]).Trim();

                        sText = "[Currently processing FrameData record]";
                        taskMethods01.LogText(sText, this);
                        sText = "[Disc type]: " + sDiscType;
                        taskMethods01.LogText(sText, this);
                        sText = "[ProdNum]: " + sProdNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[RefNum]: " + sRefNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[FrameNum]: " + sFrameNum;
                        taskMethods01.LogText(sText, this);
                        sText = "[Sitting]: " + sSitting.Trim();
                        taskMethods01.LogText(sText + Environment.NewLine, this);

                        string sSearchPattern01 = "GatherDiscType = '" + sDiscType + "'";
                        DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                        if (dRowGatheredPattern01.Length > 0)
                        {
                            bSittingBased = Convert.ToBoolean(dRowGatheredPattern01[0]["SittingBased"]);

                            sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "'";

                            dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                            if (dTblDiscOrders.Rows.Count > 0)
                            {
                                sCommText = "SELECT * FROM [FrameData] WHERE [Prodnum] = '" + sProdNum + "'";

                                dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblFrameData);

                                if (dTblFrameData.Rows.Count > 0)
                                {
                                    sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "'";

                                    dbConns01.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                                    if (dTblFrames.Rows.Count > 0)
                                    {
                                        bool bFoundImageSuccess = false;

                                        taskMethods01.CheckForImages(sRefNum, sFrameNum, sDiscType, ref bFoundImageSuccess, bSittingBased, dTblDiscOrders, dTblFrames, sSitting, sProdNum);

                                        if (bFoundImageSuccess == true)
                                        {
                                            string sSearchPattern02 = "FrameNum = '" + sFrameNum + "'";
                                            DataRow[] dRowGatheredPattern02 = dTblFrameData.Select(sSearchPattern02);

                                            if (dRowGatheredPattern02.Length > 0)
                                            {
                                                iCount = dRowGatheredPattern02.Length * 3;

                                                foreach (DataRow dRowSearchPattern02 in dRowGatheredPattern02)
                                                {
                                                    iRowCount += 1;

                                                    sPackageTag = Convert.ToString(dRowSearchPattern02["PkgTag"]).Trim();
                                                    bMultiRenderGS = Convert.ToBoolean(dRowSearchPattern02["MultiRenderGS"]);
                                                    sGSBkGrnd = Convert.ToString(dRowSearchPattern02["GSBackground"]).Trim();
                                                    sExportDefFile = Convert.ToString(dRowSearchPattern02["ExportDefFile"]).Trim();
                                                    sUniqueID = Convert.ToString(dRowSearchPattern02["UniqueID"]).Trim();
                                                    sNameOn = Convert.ToString(dRowSearchPattern02["NameOn"]).Trim();
                                                    sYearOn = Convert.ToString(dRowSearchPattern02["YearOn"]).Trim();
                                                    sDP2Mask = Convert.ToString(dRowSearchPattern02["DP2Mask"]).Trim();

                                                    string sSearchPattern03 = "FrameNum = '" + sFrameNum + "'";
                                                    DataRow[] dRowGatheredPattern03 = dTblDiscOrders.Select(sSearchPattern03);

                                                    if (dRowGatheredPattern03.Length > 0)
                                                    {
                                                        sImageLocation = Convert.ToString(dRowGatheredPattern03[0]["ImageLocation"]).Trim();
                                                        iJobIDsNeeded = Convert.ToInt32(dRowGatheredPattern03[0]["JobIDsNeeded"]);
                                                        sCustNum = Convert.ToString(dRowGatheredPattern03[0]["CustNum"]).Trim();

                                                        if (sCustNum == "58241") // Get original customer number from an IMQ order.
                                                        {
                                                            dataGatheringAndProcessing01.GatherOriginalCustNumFromIMQOrder(sProdNum, ref sCustNum);
                                                        }

                                                        if (sDiscType == "ICD")
                                                        {
                                                            if (bMultiRenderGS == true)
                                                            {
                                                                bool bStylesAndBGs = true;
                                                                ICDProc.ICDExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, sGSBkGrnd, bStylesAndBGs, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                            }
                                                            else if (bMultiRenderGS != true)
                                                            {
                                                                bool bStylesAndBGs = false;
                                                                ICDProc.ICDExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, sGSBkGrnd, bStylesAndBGs, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                            }
                                                        }
                                                        else if (sDiscType == "MEG")
                                                        {
                                                            bool bInitialPass01 = true;
                                                            MEGProc.MEGExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, ref bInitialPass01, sGSBkGrnd, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                        }
                                                        else if (sDiscType == "CCD")
                                                        {
                                                            bool bInitialPass01 = true;
                                                            CCDProc.CCDExportDefModifying(sImageLocation, sRefNum, sExportDefFile, sProdNum, sFrameNum, iJobIDsNeeded, sYearOn, sNameOn, sUniqueID, ref dTblJob, ref bCreated, ref iRenderedCount, ref bIDsGathered, ref iJobID, ref iBatchID, ref iLoops, iCount, ref bInitialJobIDAssigned, sCustNum, ref bGoodResults, ref bInitialPass01, sGSBkGrnd, dRowGatheredPattern02, sDP2Mask, sSitting, dataSetMain, this);
                                                        }
                                                    }
                                                    else if (dRowGatheredPattern03.Length == 0)
                                                    {
                                                        sBreakpoint = string.Empty;
                                                    }
                                                }

                                                // End of foreach.

                                                if (dTblJob.Rows.Count > 0)
                                                {
                                                    dataGatheringAndProcessing01.ExportDefProcessing(sProdNum, bGoodResults, dTblJob, sDiscType, bSittingBased, dataSetMain, this);
                                                }
                                                else if (dTblJob.Rows.Count == 0)
                                                {
                                                    sBreakpoint = string.Empty;
                                                }
                                            }
                                            else if (dRowGatheredPattern02.Length == 0)
                                            {
                                                sBreakpoint = string.Empty;
                                            }
                                        }
                                        else if (bFoundImageSuccess != true)
                                        {
                                            DataTable dTblDiscOrdersErrors = new DataTable("dTblDiscOrdersErrors");

                                            sCommText = "SELECT [Error], [ErrorChecked], [ErrorDescription] FROM [DiscOrders] WHERE [UniqueID] = '" + sProdNum + sFrameNum + sDiscType + "'";

                                            dbConns01.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrdersErrors);

                                            if (dTblDiscOrdersErrors.Rows.Count > 0)
                                            {
                                                string sError = Convert.ToString(dTblDiscOrders.Rows[0]["Error"]).Trim();
                                                string sErrorChecked = Convert.ToString(dTblDiscOrders.Rows[0]["ErrorChecked"]).Trim();
                                                string sErrorDescription = Convert.ToString(dTblDiscOrders.Rows[0]["ErrorDescription"]).Trim();

                                                if (sError == "1" || sErrorChecked == "1" || sErrorDescription.Contains("Images not located on server."))
                                                {
                                                    // Do nothing.
                                                }
                                                else if (sError != "1" || sErrorChecked != "1" || (!sErrorDescription.Contains("Images not located on server.")))
                                                {
                                                    string sErrorDescript = "[ " + DateTime.Now.ToString() + " ][ Images not located on server. ]";

                                                    insertsOrUpdates01.UpdateDiscOrdersForErrors(sErrorDescript, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    string sStatus = "90";

                                                    insertsOrUpdates01.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (dTblDiscOrders.Rows.Count == 0)
                                            {
                                                sBreakpoint = string.Empty;
                                            }
                                        }
                                    }
                                    else if (dTblFrames.Rows.Count == 0)
                                    {
                                        sBreakpoint = string.Empty;
                                    }
                                }
                                else if (dTblFrameData.Rows.Count == 0)
                                {
                                    sBreakpoint = string.Empty;
                                }
                            }
                            else if (dTblDiscOrders.Rows.Count == 0)
                            {
                                sBreakpoint = string.Empty;
                            }
                        }
                        else if (dRowGatheredPattern01.Length == 0)
                        {
                            sBreakpoint = string.Empty;
                        }
                    }
                }
                else if (dTblDistinctFrameDataRecords.Rows.Count == 0)
                {
                    // Continue, no ready work.
                }
            }
            catch (Exception ex)
            {
                dbConns01.SaveExceptionToDB(ex);
            }
        }

        #endregion
    }
}
