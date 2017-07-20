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
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;

namespace APS_DiscProcessor_Build_2
{
    class DataGatheringAndProcessing
    {
        #region Global form variables.

        // Common class suffix = 03
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns03 = new DBConnections();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods03 = new TaskMethods();
        LockFile lockFile03 = new LockFile();
        InsertsOrUpdates insertsOrUpdates03 = new InsertsOrUpdates();

        #endregion

        #region Gathering methods.

        public void GatherDiscProcessorVariables(DataSet dataSetMain)
        {
            try
            {
                DataTable dTblVariables = new DataTable("dTblVariables");
                string sCommText = "SELECT * FROM [Variables]";

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblVariables);

                if (dTblVariables.Rows.Count > 0)
                {
                    dataSetMain.Tables.Add(dTblVariables);
                }
                else if (dTblVariables.Rows.Count == 0)
                {

                }

                DataTable dTblDiscTypes = new DataTable("dTblDiscTypes");
                sCommText = "SELECT * FROM DiscTypes";

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscTypes);

                if (dTblDiscTypes.Rows.Count > 0)
                {
                    dataSetMain.Tables.Add(dTblDiscTypes);
                }
                else if (dTblDiscTypes.Rows.Count == 0)
                {

                }

                DataTable dTblGatherDiscTypes = new DataTable("dTblGatherDiscTypes");
                sCommText = "SELECT * FROM [GatherDiscTypes]";

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblGatherDiscTypes);

                if (dTblGatherDiscTypes.Rows.Count > 0)
                {
                    dataSetMain.Tables.Add(dTblGatherDiscTypes);
                }
                else if (dTblGatherDiscTypes.Rows.Count == 0)
                {

                }

                DataTable dTblGatherChangeLog = new DataTable("dTblGatherChangeLog");
                sCommText = "SELECT * FROM [ChangeLog]";

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblGatherChangeLog);

                if (dTblGatherChangeLog.Rows.Count > 0)
                {
                    dataSetMain.Tables.Add(dTblGatherChangeLog);
                }
                else if (dTblGatherChangeLog.Rows.Count == 0)
                {

                }

                DataTable dTblLooperModes = new DataTable("dTblLooperModes");
                sCommText = "SELECT * FROM [LooperModes]";

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblLooperModes);

                if (dTblLooperModes.Rows.Count > 0)
                {
                    dataSetMain.Tables.Add(dTblLooperModes);
                }
                else if (dTblLooperModes.Rows.Count == 0)
                {

                }
            }
            catch(Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void GatherPreDiscOrdersInsertionrOrderData(string sProdNum, string sRefNum, string sDiscType, string sPackageTag, DataSet dataSetMain, bool bSittingBased)
        {
            try
            {
                string sCommText = string.Empty;
                DataTable dTblPDOIODCodes = new DataTable("dTblPDOIODCodes");

                if (dataSetMain.Tables.Contains("dTblPDOIODCodes"))
                {
                    dataSetMain.Tables["dTblPDOIODCodes"].Clear();
                }
                else if (!dataSetMain.Tables.Contains("dTblPDOIODCodes"))
                {
                    dataSetMain.Tables.Add(dTblPDOIODCodes);
                }

                sCommText = "SELECT * FROM Codes WHERE Lookupnum = '" + sProdNum + "' AND ((Codes.Code = '" + sDiscType + "' AND Package = .F.) OR (Package = .T. AND"
                + " Code IN (SELECT DISTINCT Packagecod FROM Labels WHERE Labels.packagetag = '" + sPackageTag + "' AND Labels.Code = '" + sDiscType + "'))) ORDER BY Sequence";

                dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPDOIODCodes"]);

                if (dataSetMain.Tables["dTblPDOIODCodes"].Rows.Count > 0)
                {

                }
                else if (dataSetMain.Tables["dTblPDOIODCodes"].Rows.Count == 0)
                {

                }

                DataTable dTblPDOIODFrames = new DataTable("dTblPDOIODFrames");

                if (dataSetMain.Tables.Contains("dTblPDOIODFrames"))
                {
                    dataSetMain.Tables["dTblPDOIODFrames"].Clear();
                }
                else if (!dataSetMain.Tables.Contains("dTblPDOIODFrames"))
                {
                    dataSetMain.Tables.Add(dTblPDOIODFrames);
                }

                if (bSittingBased != true)
                {
                    sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "' ORDER BY Sequence";

                    dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPDOIODFrames"]);

                    if (dataSetMain.Tables["dTblPDOIODFrames"].Rows.Count > 0)
                    {

                    }
                    else if (dataSetMain.Tables["dTblPDOIODFrames"].Rows.Count == 0)
                    {
                        
                    }
                }
                else if (bSittingBased == true)
                {
                    string sCodesSequence = string.Empty;
                    string sFramesSitting = string.Empty;
                    int iCodesQuantity = 0;

                    DataTable dTblFrames = new DataTable("dTblFrames");
                    sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "' ORDER BY Sequence";

                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                    if (dTblFrames.Rows.Count > 0)
                    {
                        if (!dataSetMain.Tables["dTblPDOIODFrames"].Columns.Contains("Sitting"))
                        {
                            dataSetMain.Tables["dTblPDOIODFrames"].Columns.Add("Sitting");
                        }
                        if (!dataSetMain.Tables["dTblPDOIODFrames"].Columns.Contains("Sequence"))
                        {
                            dataSetMain.Tables["dTblPDOIODFrames"].Columns.Add("Sequence");
                        }
                        if (!dataSetMain.Tables["dTblPDOIODFrames"].Columns.Contains("Quantity"))
                        {
                            dataSetMain.Tables["dTblPDOIODFrames"].Columns.Add("Quantity");
                        }

                        DataRow dRowPDOIODFrames = dataSetMain.Tables["dTblPDOIODFrames"].NewRow();

                        if (dataSetMain.Tables["dTblPDOIODCodes"].Rows.Count > 0)
                        {
                            foreach (DataRow dRowCodes in dataSetMain.Tables["dTblPDOIODCodes"].Rows)
                            {
                                sCodesSequence = Convert.ToString(dRowCodes["Sequence"]).Trim();
                                iCodesQuantity = Convert.ToInt32(dRowCodes["Quantity"]);

                                string sSearchPattern01 = "Sequence = '" + sCodesSequence + "'";
                                DataRow[] dRowGatheredPattern01 = dTblFrames.Select(sSearchPattern01);

                                if (dRowGatheredPattern01.Length > 0)
                                {
                                    sFramesSitting = Convert.ToString(dRowGatheredPattern01[0]["Sitting"]).Trim();

                                    dRowPDOIODFrames["Sitting"] = sFramesSitting;
                                    dRowPDOIODFrames["Sequence"] = sCodesSequence;
                                    dRowPDOIODFrames["Quantity"] = iCodesQuantity;

                                    dataSetMain.Tables["dTblPDOIODFrames"].Rows.Add(dRowPDOIODFrames.ItemArray);
                                }
                                else if (dRowGatheredPattern01.Length == 0)
                                {
                                    
                                }
                            }
                        }
                    }
                    else if (dTblFrames.Rows.Count == 0)
                    {
                        sBreakPoint = string.Empty;
                    }
                }

                DataTable dTblPDOIODItems = new DataTable("dTblPDOIODItems");

                if (dataSetMain.Tables.Contains("dTblPDOIODItems"))
                {
                    dataSetMain.Tables["dTblPDOIODItems"].Clear();
                }
                else if (!dataSetMain.Tables.Contains("dTblPDOIODItems"))
                {
                    dataSetMain.Tables.Add(dTblPDOIODItems);
                }

                sCommText = "SELECT * FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPDOIODItems"]);

                if (dataSetMain.Tables["dTblPDOIODItems"].Rows.Count > 0)
                {

                }
                else if (dataSetMain.Tables["dTblPDOIODItems"].Rows.Count == 0)
                {

                }

                DataTable dTblPDOIODDp2image = new DataTable("dTblPDOIODDp2image");

                if (dataSetMain.Tables.Contains("dTblPDOIODDp2image"))
                {
                    dataSetMain.Tables["dTblPDOIODDp2image"].Clear();
                }
                else if (!dataSetMain.Tables.Contains("dTblPDOIODDp2image"))
                {
                    dataSetMain.Tables.Add(dTblPDOIODDp2image);
                }

                sCommText = "SELECT * FROM Dp2image WHERE Lookupnum = '" + sProdNum + "' ORDER BY Frame";

                dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPDOIODDp2image"]);

                if (dataSetMain.Tables["dTblPDOIODDp2image"].Rows.Count > 0)
                {

                }
                else if (dataSetMain.Tables["dTblPDOIODDp2image"].Rows.Count == 0)
                {

                }
            }
            catch(Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void GatherPreFrameDataInsertionData(string sProdNum, string sRefNum, string sDiscType, string sPackageTag, DataSet dataSetMain, bool bSittingBased)
        {
            string sCommText = string.Empty;
            DataTable dTblPFDIDCodes = new DataTable("dTblPFDIDCodes");

            if (dataSetMain.Tables.Contains("dTblPFDIDCodes"))
            {
                dataSetMain.Tables["dTblPFDIDCodes"].Clear();
            }
            else if (!dataSetMain.Tables.Contains("dTblPFDIDCodes"))
            {
                dataSetMain.Tables.Add(dTblPFDIDCodes);
            }

            sCommText = "SELECT * FROM Codes WHERE Lookupnum = '" + sProdNum + "'";

            dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPFDIDCodes"]);

            if (dataSetMain.Tables["dTblPFDIDCodes"].Rows.Count > 0)
            {

            }
            else if (dataSetMain.Tables["dTblPFDIDCodes"].Rows.Count == 0)
            {

            }

            DataTable dTblPFDIDPkgdetails = new DataTable("dTblPFDIDPkgdetails");

            if (dataSetMain.Tables.Contains("dTblPFDIDPkgdetails"))
            {
                dataSetMain.Tables["dTblPFDIDPkgdetails"].Clear();
            }
            else if (!dataSetMain.Tables.Contains("dTblPFDIDPkgdetails"))
            {
                dataSetMain.Tables.Add(dTblPFDIDPkgdetails);
            }

            sCommText = "SELECT * FROM Pkgdetails WHERE Packagetag = '" + sPackageTag + "'";

            dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPFDIDPkgdetails"]);

            if (dataSetMain.Tables["dTblPFDIDPkgdetails"].Rows.Count > 0)
            {

            }
            else if (dataSetMain.Tables["dTblPFDIDPkgdetails"].Rows.Count == 0)
            {

            }

            DataTable dTblPFDIDEndcust = new DataTable("dTblPFDIDEndcust");

            if (dataSetMain.Tables.Contains("dTblPFDIDEndcust"))
            {
                dataSetMain.Tables["dTblPFDIDEndcust"].Clear();
            }
            else if (!dataSetMain.Tables.Contains("dTblPFDIDEndcust"))
            {
                dataSetMain.Tables.Add(dTblPFDIDEndcust);
            }

            sCommText = "SELECT * FROM Endcust WHERE Lookupnum = '" + sProdNum + "'";

            dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPFDIDEndcust"]);

            if (dataSetMain.Tables["dTblPFDIDEndcust"].Rows.Count > 0)
            {

            }
            else if (dataSetMain.Tables["dTblPFDIDEndcust"].Rows.Count == 0)
            {

            }

            DataTable dTblPFDIDSport = new DataTable("dTblPFDIDSport");

            if (dataSetMain.Tables.Contains("dTblPFDIDSport"))
            {
                dataSetMain.Tables["dTblPFDIDSport"].Clear();
            }
            else if (!dataSetMain.Tables.Contains("dTblPFDIDSport"))
            {
                dataSetMain.Tables.Add(dTblPFDIDSport);
            }

            sCommText = "SELECT First_name FROM Sport WHERE Lookupnum = '" + sProdNum + "'";

            dbConns03.CDSQuery(sCDSConnString, sCommText, dataSetMain.Tables["dTblPFDIDSport"]);

            if (dataSetMain.Tables["dTblPFDIDSport"].Rows.Count > 0)
            {

            }
            else if (dataSetMain.Tables["dTblPFDIDSport"].Rows.Count == 0)
            {

            }
        }

        public void GatherOriginalCustNumFromIMQOrder(string sProdNum, ref string sCustNum)
        {
            try
            {
                DataTable dTblItems = new DataTable("dTblItems");
                string sCommText = "SELECT Custid FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                dbConns03.CDSQuery(sCDSConnString, sCommText, dTblItems);

                if (dTblItems.Rows.Count > 0)
                {
                    string sCustID = Convert.ToString(dTblItems.Rows[0]["Custid"]).Trim();

                    DataTable dTblIMQOrders = new DataTable("dTblIMQOrders");
                    sCommText = "SELECT Apscust FROM Imq_orders WHERE Racnum = '" + sCustID + "'";

                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblIMQOrders);

                    if (dTblIMQOrders.Rows.Count > 0)
                    {
                        sCustNum = Convert.ToString(dTblIMQOrders.Rows[0]["Apscust"]).Trim();
                    }
                    else if (dTblIMQOrders.Rows.Count == 0)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (dTblItems.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch(Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void GatherStartUpVariables(DataSet dataSetMain, ref int iLoopInterval, ref string sDPLooperMachine, ref string sVersion)
        {
            try
            {
                string sSearchPattern01 = "Label = 'LoopInterval'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    iLoopInterval = Convert.ToInt32(dRowGatheredPattern01[0]["Value"]);
                }
                else if (dRowGatheredPattern01.Length == 0)
                {

                }

                string sSearchPattern02 = "Label = 'DP_LooperMachine'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                if (dRowGatheredPattern02.Length > 0)
                {
                    sDPLooperMachine = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                }
                else if (dRowGatheredPattern02.Length == 0)
                {

                }

                DataTable dTblVersion = new DataTable("dTblVersion");
                string sCommText = "SELECT * FROM [ChangeLog] WHERE [App] = 'Processor2' ORDER BY [Version]";

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblVersion);

                if (dTblVersion.Rows.Count > 0)
                {
                    DataRow dRowVersion = dTblVersion.Rows[dTblVersion.Rows.Count - 1];
                    sVersion = Convert.ToString(dRowVersion["Version"]).Trim();
                }
                else if (dTblVersion.Rows.Count == 0)
                {

                }
            }
            catch(Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }                                        

        private void GatherNameOnData(string sProdNum, string sFrameNum, ref string sNameOn, DataSet dataSetMain)
        {
            try
            {
                sFrameNum = sFrameNum.TrimStart('0');

                if (dataSetMain.Tables.Contains("dTblPFDIDEndcust") && dataSetMain.Tables["dTblPFDIDEndcust"].Rows.Count > 0)
                {
                    string sSearchPattern01 = "Sequence = '" + sFrameNum + "'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblPFDIDEndcust"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        string sFName = Convert.ToString(dRowGatheredPattern01[0]["First_name"]).Trim();
                        sNameOn = Convert.ToString(dRowGatheredPattern01[0]["Wname"]).Trim();

                        if (sNameOn.Length == 0) // If WName = empty then use First_name value. 
                        {
                            sNameOn = sFName;
                        }

                        sNameOn = sNameOn.Replace("'", "''");
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        string sSearchPattern02 = "Sequence = '" + sFrameNum + "'";
                        DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblPFDIDSport"].Select(sSearchPattern02);

                        if (dRowGatheredPattern02.Length > 0)
                        {
                            sNameOn = Convert.ToString(dRowGatheredPattern02[0]["First_name"]).Trim();

                            sNameOn = sNameOn.Replace("'", "''");
                        }
                        else if (dRowGatheredPattern02.Length == 0)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                }
                else if (!dataSetMain.Tables.Contains("dTblPFDIDEndcust") || dataSetMain.Tables["dTblPFDIDEndcust"].Rows.Count == 0)
                {
                    // No name on.
                }

                if (sNameOn.Length > 0)
                {
                    sNameOn = Regex.Replace(sNameOn, @"[\d]", string.Empty); //Remove any numeric characters from NameOn (prevent the doubling up of year on) The \d identifier simply matches any digit character.
                }

            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }        
        
        private void GatherMergeFileData(string sProdNum, string sCustNum, string sFrameNum, string sImageName, string sRefNum, ref string sMrgFilePath, ref bool bGotMergeFileData, string sDiscType, string sSitting, bool bSittingBased, DataSet dataSetMain)
        {
            try
            {
                string sRenderedPath = string.Empty;
                string sMergeText = string.Empty;

                string sSearchPattern01 = "Label = '" + sDiscType + "RenderedPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    sRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                    string sCustName = string.Empty;
                    string sCommText = "SELECT Name FROM Customer WHERE Customer = " + "'" + sCustNum + "'";
                    DataTable dTblCustomer = new DataTable("dTblCustomer");

                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblCustomer);

                    if (dTblCustomer.Rows.Count > 0)
                    {
                        sCustName = Convert.ToString(dTblCustomer.Rows[0]["Name"]).Trim();

                        string sSequence = sFrameNum.TrimStart('0');

                        sCommText = "SELECT Teacher, First_name, Last_name, Schoolname FROM Endcust WHERE Lookupnum = " + "'" + sProdNum + "'" +
                            " AND Sequence = " + sSequence;
                        DataTable dTblEndCust = new DataTable("dTblEndCust");

                        dbConns03.CDSQuery(sCDSConnString, sCommText, dTblEndCust);

                        string sMrgFullName = string.Empty;
                        string sMrgSchoolName = string.Empty;
                        string sMrgTeacher = string.Empty;

                        if (dTblEndCust.Rows.Count > 0)
                        {
                            string sMrgFName = Convert.ToString(dTblEndCust.Rows[0]["First_name"]).Trim();
                            string sMrgLName = Convert.ToString(dTblEndCust.Rows[0]["Last_name"]).Trim();
                            sMrgFullName = sMrgFName + " " + sMrgLName;
                            sMrgSchoolName = Convert.ToString(dTblEndCust.Rows[0]["Schoolname"]).Trim();
                            sMrgTeacher = Convert.ToString(dTblEndCust.Rows[0]["Teacher"]).Trim();

                            sMrgFullName = sMrgFullName.Replace("'", "''");

                            if (bSittingBased != true)
                            {
                                sMrgFilePath = sRenderedPath + sProdNum + sFrameNum + @"\Merge.txt";
                            }
                            else if (bSittingBased == true)
                            {
                                sMrgFilePath = sRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sProdNum + "_" + sSitting.Trim() + "_" + sDiscType + "Merge.txt";
                            }

                            string sPath = Path.GetDirectoryName(sMrgFilePath);

                            if (!Directory.Exists(sPath))
                            {
                                Directory.CreateDirectory(sPath);
                            }

                            if (bSittingBased != true)
                            {
                                // Example merge file data.
                                // "<Student_Name>","<Prod+Frame>","<JPG>","<SchoolName>","<CustomerName>","<Order>","<Teacher>"

                                sMergeText = "\"" + sMrgFullName + "\",\"" + sProdNum + sFrameNum + "\",\"" + sImageName + "\",\"" + sMrgSchoolName + "\",\"" + sCustName + "\",\"" + sRefNum + "\",\"" + sMrgTeacher + "\"";
                            }
                            else if (bSittingBased == true)
                            {
                                // Example merge file data.
                                // "<Customer>","<PEC_+Prod_+Ref_+Sitting.Trim()>","<?>","<Sitting.Trim()>","<RefNum>","<CustID>"

                                DataTable dTblItems = new DataTable("dTblItems");
                                sCommText = "SELECT Custid FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                                dbConns03.CDSQuery(sCDSConnString, sCommText, dTblItems);

                                if (dTblItems.Rows.Count > 0)
                                {
                                    string sCustID = Convert.ToString(dTblItems.Rows[0]["Custid"]).Trim();

                                    sMergeText = "\"" + sCustName + "\",\"" + sDiscType + "_" + sProdNum + "_" + sRefNum + "_" + sSitting.Trim() + "\",\"" + "\",\"" + sSitting.Trim() + "\",\"" + sRefNum + "\",\"" + sCustID + "\"";
                                }
                                else if (dTblItems.Rows.Count == 0)
                                {
                                    sBreakPoint = string.Empty;
                                }
                            }

                            File.WriteAllText(sMrgFilePath, sMergeText);

                            sMrgFName = sMrgFName.Replace("'", "''");
                            sMrgSchoolName = sMrgSchoolName.Replace("'", "''");
                            sMrgTeacher = sMrgTeacher.Replace("'", "''");
                            sCustName = sCustName.Replace("'", "''");
                            sMergeText = sMergeText.Replace("'", "''");

                            if (bSittingBased != true)
                            {
                                sCommText = "UPDATE [DiscOrders] SET [MergeFileData] = '" + sMergeText + "' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                            }
                            else if (bSittingBased == true)
                            {
                                sCommText = "UPDATE [DiscOrders] SET [MergeFileData] = '" + sMergeText + "' WHERE [ProdNum] = '" + sProdNum + "' AND [Sitting] = '" + sSitting + "' AND [DiscType] = '" + sDiscType + "'";
                            }

                            bool bSuccess = true;

                            dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                            if (bSuccess == true)
                            {
                                bGotMergeFileData = true;
                            }
                            else if (bSuccess != true)
                            {
                                bGotMergeFileData = false;
                            }
                        }
                        else if (dTblEndCust.Rows.Count == 0)
                        {
                            sSequence = sFrameNum.TrimStart('0');

                            DataTable dTblSport = new DataTable("dTblSport");
                            sCommText = "SELECT First_name, Last_name FROM Sport WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sSequence;

                            dbConns03.CDSQuery(sCDSConnString, sCommText, dTblSport);

                            if (dTblSport.Rows.Count > 0)
                            {
                                string sMrgFName = Convert.ToString(dTblSport.Rows[0]["First_name"]).Trim();
                                string sMrgLName = Convert.ToString(dTblSport.Rows[0]["Last_name"]).Trim();
                                sMrgFullName = sMrgFName + " " + sMrgLName;
                                sMrgSchoolName = " ";
                                sMrgTeacher = " ";

                                sMrgFullName = sMrgFullName.Replace("'", "''");

                                if (bSittingBased != true)
                                {
                                    sMrgFilePath = sRenderedPath + sProdNum + sFrameNum + @"\Merge.txt";
                                }
                                else if (bSittingBased == true)
                                {
                                    sMrgFilePath = sRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sProdNum + "_" + sSitting.Trim() + "_" + sDiscType + "Merge.txt";
                                }

                                string sPath = Path.GetDirectoryName(sMrgFilePath);

                                if (!Directory.Exists(sPath))
                                {
                                    Directory.CreateDirectory(sPath);
                                }

                                if (bSittingBased != true)
                                {
                                    // Example merge file data.
                                    // "<Student_Name>","<Prod+Frame>","<JPG>","<SchoolName>","<CustomerName>","<Order>","<Teacher>"

                                    sMergeText = "\"" + sMrgFullName + "\",\"" + sProdNum + sFrameNum + "\",\"" + sImageName + "\",\"" + sMrgSchoolName + "\",\"" + sCustName + "\",\"" + sRefNum + "\",\"" + sMrgTeacher + "\"";
                                }
                                else if (bSittingBased == true)
                                {
                                    // Example merge file data.
                                    // "<Customer>","<PEC_+Prod_+Ref_+Sitting.Trim()>","<?>","<Sitting.Trim()>","<RefNum>","<CustID>"

                                    DataTable dTblItems = new DataTable("dTblItems");
                                    sCommText = "SELECT Custid FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblItems);

                                    if (dTblItems.Rows.Count > 0)
                                    {
                                        string sCustID = Convert.ToString(dTblItems.Rows[0]["Custid"]).Trim();

                                        sMergeText = "\"" + sCustName + "\",\"" + sDiscType + "_" + sProdNum + "_" + sRefNum + "_" + sSitting.Trim() + "\",\"" + "\",\"" + sSitting.Trim() + "\",\"" + sRefNum + "\",\"" + sCustID + "\"";
                                    }
                                    else if (dTblItems.Rows.Count == 0)
                                    {
                                        sBreakPoint = string.Empty;
                                    }
                                }

                                File.WriteAllText(sMrgFilePath, sMergeText);

                                sMrgFName = sMrgFName.Replace("'", "''");
                                sMrgSchoolName = sMrgSchoolName.Replace("'", "''");
                                sMrgTeacher = sMrgTeacher.Replace("'", "''");
                                sCustName = sCustName.Replace("'", "''");
                                sMergeText = sMergeText.Replace("'", "''");

                                if (bSittingBased != true)
                                {
                                    sCommText = "UPDATE [DiscOrders] SET [MergeFileData] = '" + sMergeText + "' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                                }
                                else if (bSittingBased == true)
                                {
                                    sCommText = "UPDATE [DiscOrders] SET [MergeFileData] = '" + sMergeText + "' WHERE [ProdNum] = '" + sProdNum + "' AND [Sitting] = '" + sSitting + "' AND [DiscType] = '" + sDiscType + "'";
                                }

                                bool bSuccess = true;

                                dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                                if (bSuccess == true)
                                {
                                    bGotMergeFileData = true;
                                }
                                else if (bSuccess != true)
                                {
                                    bGotMergeFileData = false;
                                }
                            }
                            else if (dTblSport.Rows.Count == 0)
                            {
                                sMrgSchoolName = " ";
                                sMrgTeacher = " ";
                                sMrgFullName = " ";
                                sCustName = sCustName.Replace("'", "''");

                                if (bSittingBased != true)
                                {
                                    sMrgFilePath = sRenderedPath + sProdNum + sFrameNum + @"\Merge.txt";
                                }
                                else if (bSittingBased == true)
                                {
                                    sMrgFilePath = sRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sProdNum + "_" + sSitting.Trim() + "_" + sDiscType + "Merge.txt";
                                }

                                string sPath = Path.GetDirectoryName(sMrgFilePath);

                                if (!Directory.Exists(sPath))
                                {
                                    Directory.CreateDirectory(sPath);
                                }

                                if (bSittingBased != true)
                                {
                                    // Example merge file data.
                                    // "<Student_Name>","<Prod+Frame>","<JPG>","<SchoolName>","<CustomerName>","<Order>","<Teacher>"

                                    sMergeText = "\"" + sMrgFullName + "\",\"" + sProdNum + sFrameNum + "\",\"" + sImageName + "\",\"" + sMrgSchoolName + "\",\"" + sCustName + "\",\"" + sRefNum + "\",\"" + sMrgTeacher + "\"";
                                }
                                else if (bSittingBased == true)
                                {
                                    // Example merge file data.
                                    // "<Customer>","<PEC_+Prod_+Ref_+Sitting.Trim()>","<?>","<Sitting.Trim()>","<RefNum>","<CustID>"

                                    DataTable dTblItems = new DataTable("dTblItems");
                                    sCommText = "SELECT Custid FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblItems);

                                    if (dTblItems.Rows.Count > 0)
                                    {
                                        string sCustID = Convert.ToString(dTblItems.Rows[0]["Custid"]).Trim();

                                        sMergeText = "\"" + sCustName + "\",\"" + sDiscType + "_" + sProdNum + "_" + sRefNum + "_" + sSitting.Trim() + "\",\"" + "\",\"" + sSitting.Trim() + "\",\"" + sRefNum + "\",\"" + sCustID + "\"";
                                    }
                                    else if (dTblItems.Rows.Count == 0)
                                    {
                                        sBreakPoint = string.Empty;
                                    }
                                }

                                File.WriteAllText(sMrgFilePath, sMergeText);

                                sCustName = sCustName.Replace("'", "''");
                                sMergeText = sMergeText.Replace("'", "''");

                                if (bSittingBased != true)
                                {
                                    sCommText = "UPDATE [DiscOrders] SET [MergeFileData] = '" + sMergeText + "' WHERE [UniqueID] = '" + sProdNum + sFrameNum + sDiscType + "'";
                                }
                                else if (bSittingBased == true)
                                {
                                    sCommText = "UPDATE [DiscOrders] SET [MergeFileData] = '" + sMergeText + "' WHERE [UniqueID] = '" + sProdNum + sSitting.Trim() + sDiscType + "'";
                                }

                                bool bSuccess = true;

                                dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                                if (bSuccess == true)
                                {
                                    bGotMergeFileData = true;
                                }
                                else if (bSuccess != true)
                                {
                                    bGotMergeFileData = false;
                                }
                            }
                        }
                    }
                    else if (dTblCustomer.Rows.Count == 0)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (dRowGatheredPattern01.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
                bGotMergeFileData = false;
            }
        }

        private void GatherNWPFileData(string sProdNum, string sMrgFilePath, string sFrameNum, string sRefNum, ref bool bGotNWPFileData, string sDiscType, string sCustNum, string sExportDefSitting, bool bSittingBased, string sSitting, DataSet dataSetMain)
        {
            //file = f:\OHS-SRS\IBC
            //FileType = Parent
            //copies = 2
            //Label = \\nb\users\bross\Burner-Temps\APS-cdw-4.btw
            //Priority = 1
            //media = CDR
            //Merge = \\nb\jobs\cdsburn\mergefile\_22222_333333_.txt
            //OrderID = Test_22222_333333
            //volume = just-a-test

            try
            {
                string sDiscQuantity = string.Empty;
                string sLogoFile = string.Empty;
                string sCommText = string.Empty;
                string sRenderedPath = string.Empty;
                string sLabelFile = string.Empty;
                string sRenderedPath2 = string.Empty;
                bool bGotLabelFile = false;

                if (bSittingBased != true)
                {
                    sCommText = "SELECT [Quantity] FROM [DiscOrders] WHERE [UniqueID] = '" + sProdNum + sFrameNum + sDiscType + "'";
                }
                else if (bSittingBased == true)
                {
                    sCommText = "SELECT [Quantity] FROM [DiscOrders] WHERE [UniqueID] = '" + sProdNum + sSitting.Trim() + sDiscType + "'";
                }

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");

                dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                if (dTblDiscOrders.Rows.Count > 0)
                {
                    sDiscQuantity = Convert.ToString(dTblDiscOrders.Rows[0]["Quantity"]).Trim();

                    string sSearchPattern01 = "Label = '" + sDiscType + "RenderedPath'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                    string sSearchPattern02 = "Label = '" + sDiscType + "LabelFile'";
                    DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                    if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0)
                    {
                        bGotLabelFile = true;

                        sRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                        sLabelFile = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();

                        DataTable dTblCustomerLabels = new DataTable("dTblCustomerLabels");
                        sCommText = "SELECT * FROM [CustomerLabels]";

                        dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblCustomerLabels);

                        if (dTblCustomerLabels.Rows.Count > 0)
                        {
                            foreach (DataRow dRowCustomerLabels in dTblCustomerLabels.Rows)
                            {
                                string sCustomerLabelsCustNum = Convert.ToString(dRowCustomerLabels["CustNum"]).Trim();
                                string sCustomerLabelsDiscType = Convert.ToString(dRowCustomerLabels["DiscType"]).Trim();

                                if (sCustNum == sCustomerLabelsCustNum && sDiscType == sCustomerLabelsDiscType)
                                {
                                    string sCustomerLabelsLabel = Convert.ToString(dRowCustomerLabels["LabelLocation"]).Trim();
                                    string sCustomerLabelsLogo = Convert.ToString(dRowCustomerLabels["LogoLocation"]).Trim();

                                    sLabelFile = sCustomerLabelsLabel;

                                    bGotLabelFile = true;

                                    if (sCustomerLabelsLogo.Length > 0)
                                    {
                                        sLogoFile = sCustomerLabelsLogo;
                                        break;
                                    }
                                }
                                else if (sCustNum != sCustomerLabelsCustNum)
                                {
                                    continue;
                                }
                            }
                        }
                        else if (dTblCustomerLabels.Rows.Count == 0)
                        {
                            // Continue.
                        }

                        if (bGotLabelFile == true)
                        {
                            string sUniqueID = string.Empty;

                            if (bSittingBased != true)
                            {
                                sUniqueID = sRefNum + sFrameNum;
                            }
                            else if (bSittingBased == true)
                            {
                                sUniqueID = sRefNum + sSitting.Trim() + sFrameNum;
                            }

                            if (bSittingBased != true)
                            {
                                sRenderedPath += sProdNum + sFrameNum;
                            }
                            else if (bSittingBased == true)
                            {
                                sRenderedPath2 = sRenderedPath + sRefNum + @"\" + sProdNum + @"\";
                                sRenderedPath += sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\";
                            }

                            string sFile = Path.GetDirectoryName(sMrgFilePath);
                            StringBuilder sb = new StringBuilder();

                            if (bSittingBased != true)
                            {
                                sb.AppendFormat("file = {0}", sFile);
                            }
                            else if (bSittingBased == true)
                            {
                                sb.AppendFormat("file = {0}", sRenderedPath + Environment.NewLine);
                            }

                            sb.Append(Environment.NewLine);
                            sb.AppendFormat("FileType = Parent");
                            sb.Append(Environment.NewLine);
                            sb.AppendFormat("copies = {0}", sDiscQuantity);
                            sb.Append(Environment.NewLine);

                            if (sLabelFile.Length > 0)
                            {
                                sb.AppendFormat("Label = {0}", sLabelFile);
                                sb.Append(Environment.NewLine);
                            }
                            else if (sLabelFile.Length == 0)
                            {
                                sBreakPoint = string.Empty;
                            }

                            if (sLogoFile.Length > 0)
                            {
                                //Note: this did not work when implemented.

                                //sb.AppendFormat("Logo = {0}", sLogoFile);
                                //sb.Append(Environment.NewLine);
                            }

                            sb.AppendFormat("Priority = 1");
                            sb.Append(Environment.NewLine);
                            sb.AppendFormat("media = CDR");
                            sb.Append(Environment.NewLine);

                            if (bSittingBased != true)
                            {
                                sb.AppendFormat("Merge = {0}", sMrgFilePath);
                            }
                            else if (bSittingBased == true)
                            {
                                sMrgFilePath = sRenderedPath2 + sProdNum + "_" + sSitting.Trim() + "_" + sDiscType + "Merge.txt";
                                sb.AppendFormat("Merge = {0}", sMrgFilePath);
                            }

                            sb.Append(Environment.NewLine);
                            sb.AppendFormat("OrderID = {0}", sUniqueID);
                            sb.Append(Environment.NewLine);
                            sb.AppendFormat("volume = {0}", sUniqueID);
                            sb.Append(Environment.NewLine);

                            if (bSittingBased != true)
                            {
                                sCommText = "UPDATE [DiscOrders] SET [NWPFileData] = '" + sb.ToString() + "' WHERE [UniqueID] = '" + sProdNum + sFrameNum + sDiscType + "'";
                            }
                            else if (bSittingBased == true)
                            {
                                sCommText = "UPDATE [DiscOrders] SET [NWPFileData] = '" + sb.ToString() + "' WHERE [UniqueID] = '" + sProdNum + sSitting.Trim() + sDiscType + "'";
                            }

                            bool bUpdateSuccess = true;

                            dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bUpdateSuccess);

                            if (bUpdateSuccess == true)
                            {
                                bGotNWPFileData = true;
                            }
                            else if (bUpdateSuccess != true)
                            {
                                bGotNWPFileData = false;
                            }
                        }
                        else if (bGotLabelFile != true)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                    else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (dTblDiscOrders.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                    bGotNWPFileData = false;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
                bGotNWPFileData = false;
            }
        }

        public void GatherNWPFileVariables(ref string sReadyDir1, ref string sReadyDir2, ref string sReadyDir3, ref string sReadyDir4, ref string sSendHereFile, ref bool bSuccess, DataSet dataSetMain)
        {
            try
            {
                string sSearchPattern01 = "Label = 'ReadyDir1'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                string sSearchPattern02 = "Label = 'ReadyDir2'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                string sSearchPattern03 = "Label = 'ReadyDir3'";
                DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                string sSearchPattern04 = "Label = 'ReadyDir4'";
                DataRow[] dRowGatheredPattern04 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern04);

                string sSearchPattern05 = "Label = 'SendHereFile'";
                DataRow[] dRowGatheredPattern05 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern05);

                if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0 && dRowGatheredPattern04.Length > 0 && dRowGatheredPattern05.Length > 0)
                {
                    sReadyDir1 = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                    sReadyDir2 = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                    sReadyDir3 = Convert.ToString(dRowGatheredPattern03[0]["Value"]).Trim();
                    sReadyDir4 = Convert.ToString(dRowGatheredPattern04[0]["Value"]).Trim();
                    sSendHereFile = Convert.ToString(dRowGatheredPattern05[0]["Value"]).Trim();

                    bSuccess = true;
                }
                else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0 || dRowGatheredPattern03.Length == 0 || dRowGatheredPattern04.Length == 0 || dRowGatheredPattern05.Length == 0)
                {
                    bSuccess = false;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
                bSuccess = false;
            }
        }

        #endregion

        #region Processing methods.

        public void QueryDiscOrdersForAllFrames(DataTable dTblDiscOrders, string sDiscType, bool bSittingBased, string sCustNum, string sPackageTag, string sProdNum, string sFrameNum, string sRefNum, string sImageLocation, int iQuantity, string sSitting, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                DataTable dTblStyles = new DataTable("Styles");
                DataTable dTblGSBackgrounds = new DataTable("dTblGSBackgrounds");

                DataTable dTblStylesAndGSBackgrounds = new DataTable("dTblStylesAndGSBackgrounds");
                dTblStylesAndGSBackgrounds.Columns.Add("Style", typeof(string));
                dTblStylesAndGSBackgrounds.Columns.Add("GSBkGrnd", typeof(string));
                dTblStylesAndGSBackgrounds.Columns.Add("Itype", typeof(string));
                dTblStylesAndGSBackgrounds.Columns.Add("Otype", typeof(string));
                dTblStylesAndGSBackgrounds.Columns.Add("Alt_data", typeof(string));
                dTblStylesAndGSBackgrounds.Columns.Add("Alt_type", typeof(string));
                dTblStylesAndGSBackgrounds.Columns.Add("MultiRenderGS", typeof(bool));

                string sStyle = string.Empty;
                string sGreenscreenBackground = string.Empty;
                string sItype = string.Empty;
                string sOtype = string.Empty;
                string sAltData = string.Empty;
                string sAltType = string.Empty;
                bool bHaveAltData = false;
                bool bMultiRenderGS = false;
                int iIDsNeeded = 0;
                string sGSBG = string.Empty;
                string sBG = string.Empty;
                int iUnique = 0;
                bool bInitialPass01 = true;
                
                string sCommText = "SELECT * FROM Custtrans WHERE (Itype = 'S' AND Otype = 'S') AND Customer = '" // this query will get styles or styles with 3d bits
                            + sCustNum + "' AND Packagetag = '" + sPackageTag + "' ORDER BY Alt_data ASC";

                dbConns03.CDSQuery(sCDSConnString, sCommText, dTblStyles);

                sCommText = "SELECT * FROM Custtrans WHERE (Itype = 'B' AND Otype = 'B') AND Customer = '" // this query will get all gs bkgrnds
                            + sCustNum + "' AND Packagetag = '" + sPackageTag + "'";

                dbConns03.CDSQuery(sCDSConnString, sCommText, dTblGSBackgrounds);

                if (dTblStyles.Rows.Count > 0)
                {
                    if (dTblGSBackgrounds.Rows.Count > 0)
                    {
                        // I have styles and green screen backgrounds.

                        bMultiRenderGS = false;

                        foreach (DataRow dRowStyles in dTblStyles.Rows)
                        {
                            sStyle = Convert.ToString(dRowStyles["Labdata"]).Trim();
                            sItype = Convert.ToString(dRowStyles["Itype"]).Trim();
                            sOtype = Convert.ToString(dRowStyles["Otype"]).Trim();
                            sAltData = Convert.ToString(dRowStyles["Alt_data"]).Trim();
                            sAltType = Convert.ToString(dRowStyles["Alt_type"]).Trim();

                            foreach (DataRow dRowGSBackgrounds in dTblGSBackgrounds.Rows)
                            {
                                sGSBG = Convert.ToString(dRowGSBackgrounds["Labdata"]).Trim();

                                dTblStylesAndGSBackgrounds.Rows.Add(sStyle, sGSBG, sItype, sOtype, sAltData, sAltType, bMultiRenderGS);
                            }
                        }
                    }
                    else if (dTblGSBackgrounds.Rows.Count == 0)
                    {
                        // I have styles but no individual green screen backgrounds. Can still have green screen backgrounds associated with styles.

                        foreach (DataRow dRowStyles in dTblStyles.Rows)
                        {
                            sAltData = Convert.ToString(dRowStyles["Alt_data"]).Trim();
                            sAltType = Convert.ToString(dRowStyles["Alt_type"]).Trim();

                            if (sAltData.Length > 0)
                            {
                                if (sAltType == "B")
                                {
                                    bHaveAltData = true;
                                }
                            }
                        }

                        foreach (DataRow dRowStyles2 in dTblStyles.Rows)
                        {
                            sStyle = Convert.ToString(dRowStyles2["Labdata"]).Trim();
                            sItype = Convert.ToString(dRowStyles2["Itype"]).Trim();
                            sOtype = Convert.ToString(dRowStyles2["Otype"]).Trim();
                            sAltData = Convert.ToString(dRowStyles2["Alt_data"]).Trim();
                            sAltType = Convert.ToString(dRowStyles2["Alt_type"]).Trim();
                            sGSBG = Convert.ToString(dRowStyles2["Alt_data"]).Trim();

                            bMultiRenderGS = bHaveAltData;

                            dTblStylesAndGSBackgrounds.Rows.Add(sStyle, sGSBG, sItype, sOtype, sAltData, sAltType, bMultiRenderGS);
                        }
                    }
                }
                else if (dTblStyles.Rows.Count == 0)
                {
                    if (dTblGSBackgrounds.Rows.Count > 0)
                    {
                        // I have green screen backgrounds but no styles.

                        foreach (DataRow dRowGSBackgrounds in dTblGSBackgrounds.Rows)
                        {
                            sGSBG = Convert.ToString(dRowGSBackgrounds["Labdata"]).Trim();
                            sItype = Convert.ToString(dRowGSBackgrounds["Itype"]).Trim();
                            sOtype = Convert.ToString(dRowGSBackgrounds["Otype"]).Trim();
                            sAltData = Convert.ToString(dRowGSBackgrounds["Alt_data"]).Trim();
                            sAltType = Convert.ToString(dRowGSBackgrounds["Alt_type"]).Trim();
                            sStyle = "NY";

                            dTblStylesAndGSBackgrounds.Rows.Add(sStyle, sGSBG, sItype, sOtype, sAltData, sAltType, bMultiRenderGS);
                        }
                    }
                    else if (dTblGSBackgrounds.Rows.Count == 0)
                    {
                        sBreakPoint = string.Empty;
                    }
                }

                if (dTblStylesAndGSBackgrounds.Rows.Count > 0)
                {
                    taskMethods03.StylesAndBackgroundCount(ref iIDsNeeded, bMultiRenderGS, dTblStyles, dTblGSBackgrounds, sDiscType);

                    bool bSuccess = false;
                    sCommText = "UPDATE [DiscOrders] Set [JobIDsNeeded] = '" + iIDsNeeded + "' WHERE [UniqueID] = '" + sProdNum + sFrameNum + sDiscType + "'";

                    dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                    if(bSuccess == true)
                    {
                        foreach (DataRow dRowStylesAndGSBackgrounds in dTblStylesAndGSBackgrounds.Rows)
                        {
                            sBG = Convert.ToString(dRowStylesAndGSBackgrounds["Style"]).Trim(); //sBG = Style
                            sGSBG = Convert.ToString(dRowStylesAndGSBackgrounds["GSBkGrnd"]).Trim(); //sGSBG = GS Background

                            if (sGSBG.Length == 0)
                            {
                                sGSBG = "NONE";
                            }

                            this.GatherFrameData(sProdNum, sFrameNum, sRefNum, sCustNum, sPackageTag, sImageLocation, iQuantity, sBG, ref iUnique, sGSBG, sDiscType, sSitting, bMultiRenderGS, bSittingBased, dataSetMain, mForm);
                        }
                    }
                    else if (bSuccess != true)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (dTblStylesAndGSBackgrounds.Rows.Count == 0)
                {
                    sBG = "NY";
                    sGSBG = "NONE";
                    bMultiRenderGS = false;

                    iIDsNeeded = 6;

                    sCommText = "UPDATE [DiscOrders] Set [JobIDsNeeded] = '" + iIDsNeeded + "' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [Disctype] = '" + sDiscType + "'";
                    bool bSuccess = false;

                    dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                    if (bSuccess == true)
                    {                        
                        this.GatherFrameData(sProdNum, sFrameNum, sRefNum, sCustNum, sPackageTag, sImageLocation, iQuantity, sBG, ref iUnique, sGSBG, sDiscType, sSitting, bMultiRenderGS, bSittingBased, dataSetMain, mForm);
                    }
                    else if (bSuccess != true)
                    {
                        sBreakPoint = string.Empty;
                    }
                }

                // Update DiscOrders.Status = 20 (Frame data processed)
                string sStatus = "20";

                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);

            }
            catch(Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void GatherFrameData(string sProdNum, string sFrameNum, string sRefNum, string sCustNum, string sPackageTag, string sImageLocation, int iQuantity, string sBG, ref int iUnique, string sGSBG, string sDiscType, string sSitting, bool bMultiRenderGS, bool bSittingBased, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                string sExportDef = string.Empty;
                string sUnique = string.Empty;
                bool bHaveExportDeffile = false;
                string sYearOn = string.Empty;
                string sNameOn = string.Empty;
                bool bGSFound = false;

                string sSearchPattern01 = "Label = 'ExportDefPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    string sExportDefPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                    DataTable dTblDocStyle = new DataTable("dTblDocStyle");
                    string sCommText = "SELECT * FROM Docstyle WHERE Docstyle = '" + sBG + "'";

                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblDocStyle);

                    if (dTblDocStyle.Rows.Count > 0)
                    {
                        string sDP2Bord = Convert.ToString(dTblDocStyle.Rows[0]["dp2bord"]).Trim();
                        string sDP2Bkgrnd = Convert.ToString(dTblDocStyle.Rows[0]["dp2bkgrd"]).Trim();
                        string sDP2Color = Convert.ToString(dTblDocStyle.Rows[0]["dp2color"]).Trim();
                        string sDP2Text = Convert.ToString(dTblDocStyle.Rows[0]["dp2text"]).Trim();
                        string sDP2Mask = Convert.ToString(dTblDocStyle.Rows[0]["dp2mask"]).Trim();

                        if (sDP2Color == "NONE" && sDP2Bord == "NONE" && sDP2Bkgrnd == "NONE") //000
                        {
                            iUnique += +1;

                            if (sDP2Mask != "NONE")
                            {
                                sExportDef = sDP2Mask;
                            }
                            else if (sDP2Mask == "NONE")
                            {
                                sExportDef = sDP2Text;
                            }

                            sUnique = iUnique + sExportDef;
                        }
                        else if (sDP2Color == "NONE" && sDP2Bord == "NONE" && sDP2Bkgrnd != "NONE") //001
                        {
                            iUnique += +1;
                            sExportDef = sDP2Text + sDP2Bkgrnd;
                            sUnique = iUnique + sExportDef;
                        }
                        else if (sDP2Color == "NONE" && sDP2Bord != "NONE" && sDP2Bkgrnd == "NONE") //010
                        {
                            // Encountered some ExportDef file names that need DP2Bord + Dp2Text that meet the above condition.
                            // I stored these in a table for easy real time handling.

                            bool bUniqueFound = false;
                            DataTable dTblUniqueExportDefDP2Bords = new DataTable("dTblUniqueExportDefDP2Bords");
                            sCommText = "SELECT * FROM [Unique ExportDef DP2Bords]";                            

                            dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblUniqueExportDefDP2Bords);

                            if (dTblUniqueExportDefDP2Bords.Rows.Count > 0)
                            {
                                foreach (DataRow dRowUniqueExportDefDP2Bords in dTblUniqueExportDefDP2Bords.Rows)
                                {
                                    string sBord = Convert.ToString(dRowUniqueExportDefDP2Bords["DP2Bord"]).Trim();

                                    if (sDP2Bord == sBord)
                                    {
                                        iUnique += +1;
                                        sExportDef = sDP2Bord + sDP2Text;
                                        sUnique = iUnique + sExportDef;

                                        bUniqueFound = true;
                                        break;
                                    }
                                }

                                if (bUniqueFound != true)
                                {
                                    iUnique += +1;
                                    sExportDef = sDP2Bord;
                                    sUnique = iUnique + sExportDef;
                                }
                            }
                            else if (dTblUniqueExportDefDP2Bords.Rows.Count == 0)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                        else if (sDP2Color == "NONE" && sDP2Bord != "NONE" && sDP2Bkgrnd != "NONE") //011
                        {
                            iUnique += +1;
                            sExportDef = sDP2Bord + sDP2Bkgrnd;
                            sUnique = iUnique + sExportDef;
                        }
                        else if (sDP2Color != "NONE" && sDP2Bord == "NONE" && sDP2Bkgrnd == "NONE") //100
                        {
                            iUnique += +1;

                            if (sDP2Mask != "NONE")
                            {
                                sExportDef = sDP2Color + sDP2Mask;
                            }
                            else if (sDP2Mask == "NONE")
                            {
                                sExportDef = sDP2Color + sDP2Text;
                            }

                            sUnique = iUnique + sExportDef;
                        }
                        else if (sDP2Color != "NONE" && sDP2Bord == "NONE" && sDP2Bkgrnd != "NONE") //101
                        {
                            iUnique += +1;
                            sExportDef = sDP2Color + sDP2Text + sDP2Bkgrnd;
                            sUnique = iUnique + sExportDef;
                        }
                        else if (sDP2Color != "NONE" && sDP2Bord != "NONE" && sDP2Bkgrnd == "NONE") //110
                        {
                            iUnique += +1;
                            sExportDef = sDP2Color + sDP2Bord;
                            sUnique = iUnique + sExportDef;
                        }
                        else if (sDP2Color != "NONE" && sDP2Bord != "NONE" && sDP2Bkgrnd != "NONE") //111
                        {
                            iUnique += +1;
                            sExportDef = sDP2Color + sDP2Bord + sDP2Bkgrnd;
                            sUnique = iUnique + sExportDef;
                        }

                        bHaveExportDeffile = false;
                        taskMethods03.CheckForExportFileExistence(ref sExportDef, ref bHaveExportDeffile, sExportDefPath);

                        if (bHaveExportDeffile == true)
                        {
                            if (dataSetMain.Tables.Contains("dTblPFDIDPkgdetails") && dataSetMain.Tables["dTblPFDIDPkgdetails"].Rows.Count > 0)
                            {
                                sYearOn = Convert.ToString(dataSetMain.Tables["dTblPFDIDPkgdetails"].Rows[0]["Year"]).Trim();
                            }                            

                            if (sYearOn.Length == 0)
                            {
                                sYearOn = DateTime.Now.Year.ToString();
                            }

                            this.GatherNameOnData(sProdNum, sFrameNum, ref sNameOn, dataSetMain);

                            if (sGSBG != "NONE")
                            {
                                taskMethods03.VerifyGS(sGSBG, ref bGSFound);

                                if (bGSFound == true)
                                {
                                    // Save data at this point to the FrameData table.
                                    sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, DP2Bord, DP2BkGrnd, DP2Color, DP2Text, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, GSBackground, DiscType, Sitting, DP2Mask)" +
                                        " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + sUnique + sDiscType + "', '" + sDP2Bord + "', '" + sDP2Bkgrnd + "', '" +
                                        sDP2Color + "', '" + sDP2Text + "', '" + sExportDef + "', '" + bMultiRenderGS + "', '1', '" + DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sGSBG + "', '" + sDiscType + "', '" + sSitting + "', '" + sDP2Mask + "')";
                                }
                                else if (bGSFound != true)
                                {
                                    // Save data at this point to the FrameData table.
                                    sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, DP2Bord, DP2BkGrnd, DP2Color, DP2Text, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, GSBackground, DiscType, Sitting, DP2Mask)" +
                                        " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + sUnique + sDiscType + "', '" + sDP2Bord + "', '" + sDP2Bkgrnd + "', '" +
                                        sDP2Color + "', '" + sDP2Text + "', '" + sExportDef + "', '" + bMultiRenderGS + "', '1', '" + DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sGSBG + "', '" + sDiscType + "', '" + sSitting + "', '" + sDP2Mask + "')";

                                    string sCommText2 = "UPDATE [DiscOrders] SET [Error] = '1', [ErrorDate] = '" + DateTime.Now.ToString() + "', [ErrorDescription] = 'Greenscreen " + sGSBG + " not located.'" +
                                        " WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                                    bool bSuccess2 = false;

                                    dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText2, ref bSuccess2);

                                    if (bSuccess2 == true)
                                    {

                                    }
                                    else if (bSuccess2 != true)
                                    {
                                        sBreakPoint = string.Empty;
                                    }
                                }
                            }
                            else if (sGSBG == "NONE")
                            {
                                string sCDSFrameNum = sFrameNum.TrimStart('0');
                                string sCodesGSBKGRD = string.Empty;

                                DataTable dTblCodes = new DataTable("dTblCodes");
                                sCommText = "SELECT Gs_bkgrd FROM Codes WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sCDSFrameNum + " GROUP BY Gs_bkgrd ORDER BY gs_bkgrd desc"; //Note: fix this for multiple gs_bkgrnds. if more than 1 gs_bkgrnd is gathered will need to query for package containing ICD and get gs_bkgrnd from ICD framenum

                                dbConns03.CDSQuery(sCDSConnString, sCommText, dTblCodes);

                                if (dTblCodes.Rows.Count > 0)
                                {
                                    sCodesGSBKGRD = Convert.ToString(dTblCodes.Rows[0]["Gs_bkgrd"]).Trim();

                                    if (sCodesGSBKGRD.Length > 0 && sCodesGSBKGRD != "")
                                    {
                                        sGSBG = sCodesGSBKGRD;

                                        taskMethods03.VerifyGS(sGSBG, ref bGSFound);

                                        if (bGSFound == true)
                                        {
                                            sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, DP2Bord, DP2BkGrnd, DP2Color, DP2Text, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, DiscType, Sitting, DP2Mask, GSBackground)" +
                                                " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + sUnique + sDiscType + "', '" +
                                                sDP2Bord + "', '" + sDP2Bkgrnd + "', '" + sDP2Color + "', '" + sDP2Text + "', '" + sExportDef + "', '" + bMultiRenderGS + "', '1', '" +
                                                DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sDiscType + "', '" + sSitting + "', '" + sDP2Mask + "', '" + sGSBG + "')";
                                        }
                                        else if (bGSFound != true)
                                        {
                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to locate gsbkgrnd: " + sGSBG + " ]";

                                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                            string sStatus = "90";

                                            if (bSittingBased != true)
                                            {
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                            }
                                            else if (bSittingBased == true)
                                            {
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                            }
                                        }
                                    }
                                    else if (sCodesGSBKGRD.Length == 0 || sCodesGSBKGRD == "")
                                    {
                                        sGSBG = "brightgreen";

                                        taskMethods03.VerifyGS(sGSBG, ref bGSFound);

                                        if (bGSFound == true)
                                        {
                                            sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, DP2Bord, DP2BkGrnd, DP2Color, DP2Text, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, DiscType, Sitting, DP2Mask, GSBackground)" +
                                                " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + sUnique + sDiscType + "', '" +
                                                sDP2Bord + "', '" + sDP2Bkgrnd + "', '" + sDP2Color + "', '" + sDP2Text + "', '" + sExportDef + "', '" + bMultiRenderGS + "', '1', '" +
                                                DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sDiscType + "', '" + sSitting + "', '" + sDP2Mask + "', '" + sGSBG + "')";
                                        }
                                        else if (bGSFound != true)
                                        {
                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to locate gsbkgrnd: brightgreen.]";

                                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                            string sStatus = "90";

                                            if (bSittingBased != true)
                                            {
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                            }
                                            else if (bSittingBased == true)
                                            {
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                            }
                                        }
                                    }
                                }
                                else if (dTblCodes.Rows.Count == 0)
                                {
                                    sGSBG = "brightgreen";

                                    taskMethods03.VerifyGS(sGSBG, ref bGSFound);

                                    if (bGSFound == true)
                                    {
                                        sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, DP2Bord, DP2BkGrnd, DP2Color, DP2Text, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, DiscType, Sitting, DP2Mask, GSBackground)" +
                                            " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + sUnique + sDiscType + "', '" +
                                            sDP2Bord + "', '" + sDP2Bkgrnd + "', '" + sDP2Color + "', '" + sDP2Text + "', '" + sExportDef + "', '" + bMultiRenderGS + "', '1', '" +
                                            DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sDiscType + "', '" + sSitting + "', '" + sDP2Mask + "', '" + sGSBG + "')";
                                    }
                                    else if (bGSFound != true)
                                    {
                                        string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to locate gsbkgrnd: brightgreen.]";

                                        insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                        string sStatus = "90";

                                        if (bSittingBased != true)
                                        {
                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                        }
                                        else if (bSittingBased == true)
                                        {
                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                        }
                                    }
                                }
                            }

                            // Save data at this point to the FrameData table.
                            bool bSuccess = false;
                            dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                            if (bSuccess == true)
                            {

                            }
                            else if (bSuccess != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                        else if (bHaveExportDeffile != true)
                        {
                            string sExportDefErrorMsg = "NOT FOUND";

                            sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, DP2Bord, DP2BkGrnd, DP2Color, DP2Text, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, GSBackground, DiscType, Sitting, DP2Mask)" +
                            " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + sUnique + sDiscType + "', '" + sDP2Bord + "', '" + sDP2Bkgrnd + "', '" +
                            sDP2Color + "', '" + sDP2Text + "', '" + sExportDefErrorMsg + "', '" + bMultiRenderGS + "', '1', '" + DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sGSBG + "', '" + sDiscType + "', '" + sSitting + "', '" + sDP2Mask + "')";
                            bool bSuccess = false;

                            dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                            if (bSuccess == true)
                            {
                                string sCommText2 = "UPDATE [DiscOrders] SET [Error] = '1', [ErrorDate] = '" + DateTime.Now.ToString() + "', [ErrorDescription] = 'ExportDef file ( " + sExportDef + " ) not located.'" +
                                " WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                                bool bSuccess2 = false;

                                dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText2, ref bSuccess2);

                                if (bSuccess2 == true)
                                {
                                    string sStatus = "90";

                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                                else if (bSuccess2 != true)
                                {
                                    sBreakPoint = string.Empty;
                                }
                            }
                            else if (bSuccess != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                    }
                    else if (dTblDocStyle.Rows.Count == 0) // If no DocStyle data then use the gsbkgrnd table.
                    {
                        DataTable dTblGsbkgrd = new DataTable("dTblGsbkgrd");
                        sCommText = "SELECT * FROM gsbkgrd WHERE Gs_bkgrd = '" + sBG + "'";
                        dbConns03.CDSQuery(sCDSConnString, sCommText, dTblGsbkgrd);

                        if (dTblGsbkgrd.Rows.Count > 0)
                        {
                            sYearOn = Convert.ToString(dataSetMain.Tables["dTblPFDIDPkgdetails"].Rows[0]["Year"]).Trim();

                            this.GatherNameOnData(sProdNum, sFrameNum, ref sNameOn, dataSetMain);

                            iUnique += +1;

                            sExportDef = "4UPW.txt";

                            sCommText = "INSERT INTO FrameData (ProdNum, RefNum, FrameNum, UniqueID, GSBackground, ExportDefFile, MultiRenderGS, Processed, ProcessDate, PkgTag, YearOn, NameOn, DiscType, Sitting)" +
                                " VALUES ('" + sProdNum + "', '" + sRefNum + "', '" + sFrameNum + "', '" + sRefNum + sFrameNum + iUnique + sBG + "', '" +
                                sBG + "', '" + sExportDef + "', '" + bMultiRenderGS + "', '1', '" + DateTime.Now.ToString() + "', '" + sPackageTag + "', '" + sYearOn + "', '" + sNameOn + "', '" + sDiscType + "', '" + sSitting + "')";

                            bool bSuccess = true;

                            dbConns03.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                            if (bSuccess == true)
                            {

                            }
                            else if (bSuccess != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                        else if (dTblGsbkgrd.Rows.Count == 0)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                }
                else if (dRowGatheredPattern01.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void ExportDefProcessing(string sProdNum, bool bGoodResults, DataTable dTblJob, string sDiscType, bool bSittingBased, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                int iJobsBatchID = 0;
                string sRefNum = string.Empty;
                string sFrameNum = string.Empty;
                string sStatus = string.Empty;
                string sLastSitting = string.Empty;
                bool bGotNWPFileData = false;
                string sLastSitting2 = string.Empty;
                bool bInsertFailedWarned = false;
                string sLastFrame = string.Empty;
                string sSitting = string.Empty;

                if (bGoodResults == true)
                {
                    if (dTblJob.Rows.Count > 0)
                    {
                        if (bSittingBased != true)
                        {
                            sRefNum = Convert.ToString(dTblJob.Rows[0]["RefNum"]).Trim();
                            sFrameNum = Convert.ToString(dTblJob.Rows[0]["FrameNum"]).Trim();

                            // Update the FrameData table for current record that exportdef file has been generated
                            bool bUpdateFrameDataSuccess = false;
                            insertsOrUpdates03.UpdateFrameDataForExportDefGenerated(sRefNum, sFrameNum, sDiscType, sProdNum, ref bUpdateFrameDataSuccess);
                        }

                        bool bInserted = true;

                        // Foreach through dTblJobs and insert required data from dTblJobs into dp2.jobqueue and set at 0 (HOLD)
                        foreach (DataRow dRowJobs in dTblJob.Rows)
                        {
                            string sJobsExportDefFile = Convert.ToString(dRowJobs["SavedExportDefPath"]).Trim();
                            sRefNum = Convert.ToString(dRowJobs["RefNum"]).Trim();
                            sFrameNum = Convert.ToString(dRowJobs["FrameNum"]).Trim();
                            int iJobsJobsID = Convert.ToInt32(dRowJobs["JobID"]);
                            iJobsBatchID = Convert.ToInt32(dRowJobs["BatchID"]);
                            sSitting = Convert.ToString(dRowJobs["Sitting"]).Trim();

                            insertsOrUpdates03.JobQueueInsert(sJobsExportDefFile, sRefNum, sFrameNum, iJobsJobsID, iJobsBatchID, ref bInserted, bSittingBased, sSitting);

                            sLastFrame = sFrameNum;
                        }

                        if (bInserted == true)
                        {
                            if (bSittingBased != true)
                            {
                                // Update DiscOrders.Status = 30 (Records added to the JobQueue table on HOLD)
                                sStatus = "30";
                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                            }
                            else if (bSittingBased == true)
                            {
                                var vSittings = dTblJob.AsEnumerable()
                                    .Select(row => new
                                    {
                                        Sitting = row.Field<string>("Sitting")
                                    })
                                .Distinct();

                                foreach (var v in vSittings)
                                {
                                    sSitting = Convert.ToString(v.Sitting);

                                    // Update DiscOrders.Status = 30 (Records added to the JobQueue table on HOLD)
                                    sStatus = "30";
                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                }
                            }

                            string sCustNum = Convert.ToString(dTblJob.Rows[0]["CustNum"]).Trim();
                            string sImageName = Convert.ToString(dTblJob.Rows[0]["ImageName"]).Trim();
                            string sMrgFilePath = string.Empty;

                            bool bGotMergeFileData = false;

                            if (bSittingBased == true)
                            {
                                var vSittings2 = dTblJob.AsEnumerable()
                                    .Select(row => new
                                    {
                                        Sitting = row.Field<string>("Sitting"),
                                        FrameNum = row.Field<string>("FrameNum")
                                    })
                                .Distinct();

                                foreach (var v in vSittings2)
                                {
                                    sSitting = Convert.ToString(v.Sitting);
                                    sFrameNum = Convert.ToString(v.FrameNum);

                                    if (sSitting != sLastSitting)
                                    {
                                        this.GatherMergeFileData(sProdNum, sCustNum, sFrameNum, sImageName, sRefNum, ref sMrgFilePath, ref bGotMergeFileData, sDiscType, sSitting, bSittingBased, dataSetMain);

                                        sLastSitting = sSitting;
                                    }
                                    else if (sSitting == sLastSitting)
                                    {
                                        continue;
                                    }
                                }
                            }
                            else if (bSittingBased != true)
                            {
                                this.GatherMergeFileData(sProdNum, sCustNum, sFrameNum, sImageName, sRefNum, ref sMrgFilePath, ref bGotMergeFileData, sDiscType, sSitting, bSittingBased, dataSetMain);
                            }

                            if (bGotMergeFileData == true)
                            {
                                if (bSittingBased != true)
                                {
                                    bGotNWPFileData = false;
                                    this.GatherNWPFileData(sProdNum, sMrgFilePath, sFrameNum, sRefNum, ref bGotNWPFileData, sDiscType, sCustNum, sSitting, bSittingBased, sSitting, dataSetMain);
                                }
                                else if (bSittingBased == true)
                                {
                                    var vSittings3 = dTblJob.AsEnumerable()
                                        .Select(row => new
                                        {
                                            Sitting = row.Field<string>("Sitting"),
                                            FrameNum = row.Field<string>("FrameNum")
                                        })
                                    .Distinct();

                                    foreach (var v in vSittings3)
                                    {
                                        sSitting = Convert.ToString(v.Sitting);
                                        sFrameNum = Convert.ToString(v.FrameNum);

                                        if (sSitting != sLastSitting2)
                                        {
                                            this.GatherNWPFileData(sProdNum, sMrgFilePath, sFrameNum, sRefNum, ref bGotNWPFileData, sDiscType, sCustNum, sSitting, bSittingBased, sSitting, dataSetMain);

                                            sLastSitting2 = sSitting;
                                        }
                                        else if (sSitting == sLastSitting2)
                                        {
                                            continue;
                                        }
                                    }
                                }

                                if (bGotNWPFileData == true)
                                {
                                    if (bSittingBased != true)
                                    {
                                        bool bJobQueueStatusUpdated = false;
                                        // Update all records associated with the BatchID to 1 (READY)                                    
                                        insertsOrUpdates03.JobQueuePrintStatusUpdateFrameBased(sRefNum, sFrameNum, ref iJobsBatchID, ref bJobQueueStatusUpdated);

                                        // Update DiscOrders.Status = 35 (Records added to the JobQueue table previously now flagged as READY)
                                        sStatus = "35";
                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);

                                        string sText = "[Finished processing]:";
                                        taskMethods03.LogText(sText, mForm);
                                        sText = "[Disc type]: " + sDiscType;
                                        taskMethods03.LogText(sText, mForm);
                                        sText = "[ProdNum]: " + sProdNum;
                                        taskMethods03.LogText(sText, mForm);
                                        sText = "[RefNum]: " + sRefNum;
                                        taskMethods03.LogText(sText, mForm);
                                        sText = "[FrameNum]: " + sFrameNum;
                                        taskMethods03.LogText(sText, mForm);
                                        sText = "[Sitting]: " + sSitting.Trim();
                                        taskMethods03.LogText(sText + Environment.NewLine, mForm);
                                    }
                                    else if (bSittingBased == true)
                                    {
                                        bool bJobQueueStatusUpdated = false;
                                        // Update all records associated with the BatchID to 1 (READY) 
                                        insertsOrUpdates03.JobQueuePrintStatusUpdateSittingBased(sRefNum, ref iJobsBatchID, ref bJobQueueStatusUpdated);

                                        if (bJobQueueStatusUpdated == true)
                                        {
                                            // Update DiscOrders.Status = 35 (Records added to the JobQueue table previously now flagged as READY)
                                            sStatus = "35";
                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);

                                            string sText = "[Finished processing]:";
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[Disc type]: " + sDiscType;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[ProdNum]: " + sProdNum;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[RefNum]: " + sRefNum;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[FrameNum]: " + sFrameNum;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[Sitting]: " + sSitting.Trim();
                                            taskMethods03.LogText(sText + Environment.NewLine, mForm);
                                        }
                                        else if (bJobQueueStatusUpdated != true)
                                        {
                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to update Jobqueue status.]";

                                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                            sStatus = "90";

                                            if (bSittingBased != true)
                                            {
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                            }
                                            else if (bSittingBased == true)
                                            {
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                            }
                                        }
                                    }
                                }
                                else if (bGotNWPFileData != true)
                                {
                                    string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to gather NWP file data.]";

                                    insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    sStatus = "90";

                                    if (bSittingBased != true)
                                    {
                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }
                                    else if (bSittingBased == true)
                                    {
                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                    }

                                    taskMethods03.RemoveOrphanedExportDefFiles(dTblJob);
                                }
                            }
                            else if (bGotMergeFileData != true)
                            {
                                string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to gather merge file data.]";

                                insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                sStatus = "90";

                                if (bSittingBased != true)
                                {
                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                                else if (bSittingBased == true)
                                {
                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                }

                                taskMethods03.RemoveOrphanedExportDefFiles(dTblJob);
                            }
                        }
                        else if (bInserted != true && bInsertFailedWarned != true)
                        {
                            bInsertFailedWarned = true;

                            string sErrorDescription = "[" + DateTime.Now.ToString() + "][Record failed to be inserted in the DP2.JobQueue table.]";

                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                            sStatus = "90";

                            if (bSittingBased != true)
                            {
                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                            }
                            else if (bSittingBased == true)
                            {
                                insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                            }

                            taskMethods03.RemoveOrphanedExportDefFiles(dTblJob);
                        }
                    }
                    else if (dTblJob.Rows.Count == 0)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (bGoodResults != true)
                {
                    sBreakPoint = string.Empty;

                    taskMethods03.RemoveOrphanedExportDefFiles(dTblJob);
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void CheckForFrameBasedRenderedImages(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                string sUniqueID = string.Empty;
                string sRenderedPath = string.Empty;
                string sDiscType = string.Empty;
                bool bSittingBased = false;
                string sAction = string.Empty;

                string sSearchPattern01 = "(Gather = 1 AND SittingBased = 0 AND OrderItemBased = 0)";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    foreach (DataRow dRow01 in dRowGatheredPattern01)
                    {
                        sDiscType = Convert.ToString(dRow01["GatherDiscType"]).Trim();

                        sAction = sDiscType + " Saved";

                        DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                        string sCommText = "SELECT * FROM [DiscOrders] WHERE [Status] = '35' AND [DiscType] = '" + sDiscType + "'";

                        dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                        if (dTblDiscOrders.Rows.Count > 0)
                        {
                            foreach (DataRow dRow in dTblDiscOrders.Rows)
                            {
                                string sRefNum = Convert.ToString(dRow["RefNum"]).Trim();
                                string sFrameNum = Convert.ToString(dRow["FrameNum"]).Trim();
                                string sProdNum = Convert.ToString(dRow["ProdNum"]).Trim();
                                string sSitting = Convert.ToString(dRow["Sitting"]);
                                int iNeededRenderCount = Convert.ToInt32(dRow["JobIDsNeeded"]);

                                string sText = "[Current order being checked for rendered images]";
                                taskMethods03.LogText(sText, mForm);
                                sText = "[Disc type]: " + sDiscType;
                                taskMethods03.LogText(sText, mForm);
                                sText = "[ProdNum]: " + sProdNum;
                                taskMethods03.LogText(sText, mForm);
                                sText = "[RefNum]: " + sRefNum;
                                taskMethods03.LogText(sText, mForm);
                                sText = "[FrameNum]: " + sFrameNum;
                                taskMethods03.LogText(sText, mForm);
                                sText = "[Sitting]: " + sSitting.Trim();
                                taskMethods03.LogText(sText + Environment.NewLine, mForm);

                                string sSearchPattern02 = "Label = '" + sDiscType + "RenderedPath'";
                                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                                if (dRowGatheredPattern02.Length > 0)
                                {
                                    sRenderedPath = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                                    sRenderedPath += sProdNum + sFrameNum + @"\";

                                    if (Directory.Exists(sRenderedPath))
                                    {
                                        int iDirRenderedCount = Directory.GetFiles(sRenderedPath, "*.jpg", SearchOption.TopDirectoryOnly).Length;
                                        int iMergeFileCount = Directory.GetFiles(sRenderedPath, "Merge.txt", SearchOption.TopDirectoryOnly).Length;

                                        if (iMergeFileCount == 1)
                                        {
                                            if (iDirRenderedCount == iNeededRenderCount)
                                            {
                                                // Update DiscOrders.Status = 40 (Images have been rendered.)
                                                string sStatus = "40";
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                
                                                // Generate the Copyright Release.txt
                                                bool bCRGenerated = false;
                                                taskMethods03.GenerateCopyrightRelease(sRenderedPath, ref bCRGenerated, dataSetMain);

                                                if (bCRGenerated == true)
                                                {
                                                    // Update DiscOrders.Status = 50 (Copyright and Merge text files are with the rendered images.)
                                                    sStatus = "50";
                                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);

                                                    // Moves NWP file to the rimage for disc production.
                                                    bool bNWPMoved = false;
                                                    bool bGenerateNWPFiles = false;
                                                    this.MoveNWPtoReady(sRefNum, sFrameNum, sRenderedPath, ref bNWPMoved, sDiscType, sSitting, bSittingBased, ref bGenerateNWPFiles, mForm, dataSetMain, sProdNum);

                                                    if (bGenerateNWPFiles == true)
                                                    {
                                                        if (bNWPMoved == true)
                                                        {
                                                            // Update DiscOrders.Status = 60 (NWP files moved to initiate processing of the disc. Completed.)
                                                            sStatus = "60";
                                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);

                                                            bool bStamped = false;

                                                            insertsOrUpdates03.UpdateStamps(sProdNum, sAction, ref bStamped);

                                                            if (bStamped == true)
                                                            {
                                                                sText = "[Moved NWP file]:";
                                                                taskMethods03.LogText(sText, mForm);
                                                                sText = "[Disc type]: " + sDiscType;
                                                                taskMethods03.LogText(sText, mForm);
                                                                sText = "[ProdNum]: " + sProdNum;
                                                                taskMethods03.LogText(sText, mForm);
                                                                sText = "[RefNum]: " + sRefNum;
                                                                taskMethods03.LogText(sText, mForm);
                                                                sText = "[FrameNum]: " + sFrameNum;
                                                                taskMethods03.LogText(sText, mForm);
                                                                sText = "[Sitting]: " + sSitting.Trim();
                                                                taskMethods03.LogText(sText + Environment.NewLine, mForm);
                                                            }
                                                            else if (bStamped != true)
                                                            {
                                                                sBreakPoint = string.Empty;
                                                            }
                                                        }
                                                        else if (bNWPMoved != true)
                                                        {
                                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to move NWP file.]";

                                                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                            sStatus = "90";

                                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                        }
                                                    }
                                                    else if (bGenerateNWPFiles != true)
                                                    {
                                                        sText = "[NWP files not flagged for moving to Rimage]:";
                                                        taskMethods03.LogText(sText, mForm);
                                                        sText = "[Disc type]: " + sDiscType;
                                                        taskMethods03.LogText(sText, mForm);
                                                        sText = "[ProdNum]: " + sProdNum;
                                                        taskMethods03.LogText(sText, mForm);
                                                        sText = "[RefNum]: " + sRefNum;
                                                        taskMethods03.LogText(sText, mForm);
                                                        sText = "[FrameNum]: " + sFrameNum;
                                                        taskMethods03.LogText(sText, mForm);
                                                        sText = "[Sitting]: " + sSitting.Trim();
                                                        taskMethods03.LogText(sText + Environment.NewLine, mForm);

                                                        sStatus = "60";
                                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                    }
                                                }
                                                else if (bCRGenerated != true)
                                                {
                                                    string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to generate Copyright.]";

                                                    insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    sStatus = "90";

                                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (iDirRenderedCount != iNeededRenderCount)
                                            {
                                                sBreakPoint = string.Empty;
                                            }
                                        }
                                        else if (iMergeFileCount == 0)
                                        {
                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ No merge file in rendered directory.]";

                                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                            string sStatus = "90";

                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                        }
                                    }
                                    else if (!Directory.Exists(sRenderedPath))
                                    {
                                        string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Rendered image path does not exist.]";

                                        insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                        string sStatus = "90";

                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }
                                }
                                else if (dRowGatheredPattern02.Length == 0)
                                {
                                    sBreakPoint = string.Empty;
                                }
                            }
                        }
                        else if (dTblDiscOrders.Rows.Count == 0)
                        {
                            // Continue.
                        }
                    }
                }
                else if (dRowGatheredPattern01.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        public void CheckForSittingBasedRenderedImages(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                string sDiscType = string.Empty;
                string sRenderedPath2 = string.Empty;
                string sOriginalRenderedPath = string.Empty;
                bool bAllGood = false;
                bool bSittingBased = true;
                string sRefNum = string.Empty;
                string sFrameNum = string.Empty;
                string sProdNum = string.Empty;
                string sSitting = string.Empty;

                string sSearchPattern01 = "(Gather = 1 AND SittingBased = 1 AND OrderItemBased = 0)";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    foreach (DataRow dRowGatherDiscTypes in dRowGatheredPattern01)
                    {
                        sDiscType = Convert.ToString(dRowGatherDiscTypes["GatherDiscType"]).Trim();

                        string sSearchPattern02 = "Label = '" + sDiscType + "RenderedPath'";
                        DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                        if (dRowGatheredPattern02.Length > 0)
                        {
                            string sRenderedPath = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                            sOriginalRenderedPath = sRenderedPath;
                            sRenderedPath2 = sRenderedPath;

                            DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                            string sCommText = "SELECT * FROM [DiscOrders] WHERE [Status] = '35' AND [DiscType] = '" + sDiscType + "'";

                            dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                            if (dTblDiscOrders.Rows.Count > 0)
                            {
                                foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                                {
                                    sRenderedPath = sOriginalRenderedPath;
                                    sRenderedPath2 = sOriginalRenderedPath;

                                    sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                                    sFrameNum = Convert.ToString(dRowDiscOrders["FrameNum"]).Trim();
                                    sProdNum = Convert.ToString(dRowDiscOrders["ProdNum"]).Trim();
                                    sSitting = Convert.ToString(dRowDiscOrders["Sitting"]);

                                    string sText = "[Current order being checked for rendered images]:";
                                    taskMethods03.LogText(sText, mForm);
                                    sText = "[Disc type]: " + sDiscType;
                                    taskMethods03.LogText(sText, mForm);
                                    sText = "[ProdNum]: " + sProdNum;
                                    taskMethods03.LogText(sText, mForm);
                                    sText = "[RefNum]: " + sRefNum;
                                    taskMethods03.LogText(sText, mForm);
                                    sText = "[FrameNum]: " + sFrameNum;
                                    taskMethods03.LogText(sText, mForm);
                                    sText = "[Sitting]: " + sSitting.Trim();
                                    taskMethods03.LogText(sText + Environment.NewLine, mForm);

                                    DataTable dTblFrames = new DataTable("dTblFrames");
                                    sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "' AND Sitting = '" + sSitting + "'";

                                    dbConns03.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                                    if (dTblFrames.Rows.Count > 0)
                                    {
                                        int iNeededRenderedImageCount = 0;
                                        int idTblFramesRowCount = dTblFrames.Rows.Count;

                                        int iRendersPerFrame = 0;

                                        string sSearchPattern03 = "Label = '" + sDiscType + "RendersPerFrame'";
                                        DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                                        if (dRowGatheredPattern03.Length > 0)
                                        {
                                            iRendersPerFrame = Convert.ToInt32(dRowGatheredPattern03[0]["Value"]);

                                            iNeededRenderedImageCount = idTblFramesRowCount * iRendersPerFrame;

                                            sRenderedPath += sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\";

                                            if (Directory.Exists(sRenderedPath))
                                            {
                                                string sMergeFile = sRenderedPath2 + sRefNum + @"\" + sProdNum + @"\" + sProdNum + "_" + sSitting.Trim() + "_" + sDiscType + "Merge.txt";

                                                if (File.Exists(sMergeFile))
                                                {
                                                    int iDirRenderedCount = Directory.GetFiles(sRenderedPath, "*.jpg", SearchOption.TopDirectoryOnly).Length;

                                                    if (iDirRenderedCount == iNeededRenderedImageCount || iDirRenderedCount == iNeededRenderedImageCount + 1)
                                                    {
                                                        // Update DiscOrders.Status = 40 (Images have been rendered.)
                                                        string sStatus = "40";
                                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);

                                                        // Check directory size here.
                                                        long lSize = 0;
                                                        taskMethods03.DirSize(new DirectoryInfo(sRenderedPath), ref lSize);

                                                        if (lSize >= 737000000)
                                                        {
                                                            //update nwp file data for this record to "media = DVD" from "media = CDR"
                                                        }

                                                        bool bIncludeCopyrightImage = false;

                                                        string sSearchPattern04 = "GatherDiscType = '" + sDiscType + "'";
                                                        DataRow[] dRowGatheredPattern04 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern04);

                                                        if (dRowGatheredPattern04.Length > 0)
                                                        {
                                                            bIncludeCopyrightImage = Convert.ToBoolean(dRowGatheredPattern04[0]["GetsCopyrightReleaseImage"]);

                                                            if (bIncludeCopyrightImage == true)
                                                            {
                                                                string sSearchPattern05 = "Label = 'GenericCopyrightRelease'";
                                                                DataRow[] dRowGatheredPattern05 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern05);

                                                                if (dRowGatheredPattern05.Length > 0)
                                                                {
                                                                    string sGenericCopyrightRelease = Convert.ToString(dRowGatheredPattern05[0]["Value"]).Trim();
                                                                    string sDestFilePath = sRenderedPath + Path.GetFileName(sGenericCopyrightRelease).Trim();

                                                                    if (!File.Exists(sDestFilePath))
                                                                    {
                                                                        File.Copy(sGenericCopyrightRelease, sDestFilePath);
                                                                    }
                                                                }
                                                                else if (dRowGatheredPattern05.Length == 0)
                                                                {
                                                                    sBreakPoint = string.Empty;
                                                                }
                                                            }
                                                            else if (bIncludeCopyrightImage != true)
                                                            {
                                                                sBreakPoint = string.Empty;
                                                            }
                                                        }
                                                        else if (dRowGatheredPattern04.Length == 0)
                                                        {
                                                            sBreakPoint = string.Empty;
                                                        }

                                                        string sSearchPattern06 = "Label = 'CopyrightRelease'";
                                                        DataRow[] dRowGatheredPattern06 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern06);

                                                        if (dRowGatheredPattern06.Length > 0)
                                                        {
                                                            string sRightsReleaseText = Convert.ToString(dRowGatheredPattern06[0]["Value"]).Trim();
                                                            string sRightsReleasePath = sRenderedPath + "rights-release.txt";

                                                            if (!File.Exists(sRightsReleasePath))
                                                            {
                                                                File.WriteAllText(sRightsReleasePath, sRightsReleaseText);
                                                            }

                                                            bAllGood = true;
                                                        }
                                                        else if (dRowGatheredPattern06.Length == 0)
                                                        {
                                                            bAllGood = false;

                                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Rights release data not gathered from table.]";

                                                            insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                            sStatus = "90";

                                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                        }
                                                    }
                                                    else if (iDirRenderedCount != iNeededRenderedImageCount)
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else if (!File.Exists(sMergeFile))
                                                {
                                                    bAllGood = false;

                                                    string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Merge file could not be located.]";

                                                    insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    string sStatus = "90";

                                                    insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (!Directory.Exists(sRenderedPath))
                                            {
                                                bAllGood = false;

                                                Directory.CreateDirectory(sRenderedPath);
                                            }
                                        }
                                        else if (dRowGatheredPattern03.Length == 0)
                                        {
                                            sBreakPoint = string.Empty;
                                        }
                                    }
                                    else if (dTblFrames.Rows.Count == 0)
                                    {
                                        bAllGood = false;

                                        string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ No frame data.]";

                                        insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                        string sStatus = "90";

                                        insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }

                                    if (bAllGood == true)
                                    {
                                        bool bNWPMoved = false;
                                        bool bGenerateNWPFiles = false;
                                        this.MoveNWPtoReady(sRefNum, sFrameNum, sRenderedPath, ref bNWPMoved, sDiscType, sSitting, bSittingBased, ref bGenerateNWPFiles, mForm, dataSetMain, sProdNum);

                                        if (bGenerateNWPFiles == true)
                                        {
                                            if (bNWPMoved == true)
                                            {
                                                sText = "[Moved NWP file]:";
                                                taskMethods03.LogText(sText, mForm);
                                                sText = "[Disc type]: " + sDiscType;
                                                taskMethods03.LogText(sText, mForm);
                                                sText = "[ProdNum]: " + sProdNum;
                                                taskMethods03.LogText(sText, mForm);
                                                sText = "[RefNum]: " + sRefNum;
                                                taskMethods03.LogText(sText, mForm);
                                                sText = "[FrameNum]: " + sFrameNum;
                                                taskMethods03.LogText(sText, mForm);
                                                sText = "[Sitting]: " + sSitting.Trim();
                                                taskMethods03.LogText(sText + Environment.NewLine, mForm);

                                                string sStatus = "60";
                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                            }
                                            else if (bNWPMoved != true)
                                            {
                                                string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Failed to move NWP file to initiate disc production.]";

                                                insertsOrUpdates03.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                string sStatus = "90";

                                                insertsOrUpdates03.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                            }
                                        }
                                        else if (bGenerateNWPFiles != true)
                                        {
                                            string sStatus = "60";
                                            insertsOrUpdates03.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);

                                            sText = "[NWP files not flagged for moving to Rimage]:";
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[Disc type]: " + sDiscType;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[ProdNum]: " + sProdNum;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[RefNum]: " + sRefNum;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[FrameNum]: " + sFrameNum;
                                            taskMethods03.LogText(sText, mForm);
                                            sText = "[Sitting]: " + sSitting.Trim();
                                            taskMethods03.LogText(sText + Environment.NewLine, mForm);
                                        }
                                    }
                                    else if (bAllGood != true)
                                    {
                                        sBreakPoint = string.Empty;
                                    }
                                }

                                // End of foreach through DiscOrders for status = 35
                                sBreakPoint = string.Empty;
                            }
                            else if (dTblDiscOrders.Rows.Count == 0)
                            {
                                continue;
                            }
                        }
                        else if (dRowGatheredPattern02.Length == 0)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                }
                else if (dRowGatheredPattern01.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
            }
        }

        private void MoveNWPtoReady(string sRefNum, string sFrameNum, string sRenderedPath, ref bool bNWPMoved, string sDiscType, string sSitting, bool bSittingBased, ref bool bGenerateNWPFiles, Main_Form mForm, DataSet dataSetMain, string sProdNum)
        {
            try
            {
                mainForm = mForm;
                string sLogText = string.Empty;
                string sCommText = string.Empty;

                string sSearchPattern01 = "GatherDiscType = '" + sDiscType + "'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    bGenerateNWPFiles = Convert.ToBoolean(dRowGatheredPattern01[0]["GenerateNWPFiles"]);

                    if (bGenerateNWPFiles == true)
                    {
                        string sNWPFile = string.Empty;
                        string sReadyDirFilePath = string.Empty;
                        string sReadyDir1 = string.Empty;
                        string sReadyDir2 = string.Empty;
                        string sReadyDir3 = string.Empty;
                        string sReadyDir4 = string.Empty;
                        string sSendHereFile = string.Empty;
                        bool bSuccess = true;

                        this.GatherNWPFileVariables(ref sReadyDir1, ref sReadyDir2, ref sReadyDir3, ref sReadyDir4, ref sSendHereFile, ref bSuccess, dataSetMain);

                        if (bSuccess == true)
                        {
                            bool bDirFound = false;

                            if (bSittingBased != true)
                            {
                                sNWPFile = sDiscType + "_" + sRefNum + "_" + sFrameNum + ".NWP";
                            }
                            else if (bSittingBased == true)
                            {
                                sNWPFile = sDiscType + "_" + sRefNum + "_" + sSitting.Trim() + ".NWP";
                            }

                            sRenderedPath += sNWPFile;

                            DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");

                            if (bSittingBased != true)
                            {
                                sCommText = "SELECT [NWPFileData] FROM [DiscOrders] WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                            }
                            else if (bSittingBased == true)
                            {
                                sCommText = "SELECT [NWPFileData] FROM [DiscOrders] WHERE [RefNum] = '" + sRefNum + "' AND [Sitting] = '" + sSitting.Trim() + "' AND [DiscType] = '" + sDiscType + "'";
                            }

                            dbConns03.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                            if (dTblDiscOrders.Rows.Count > 0)
                            {
                                string sNWPFileData = Convert.ToString(dTblDiscOrders.Rows[0]["NWPFileData"]).Trim();

                                File.WriteAllText(sRenderedPath, sNWPFileData);

                                if (File.Exists(sRenderedPath))
                                {
                                    if (bDirFound == false && (File.Exists(sReadyDir1 + sSendHereFile)))
                                    {
                                        sReadyDirFilePath = sReadyDir1 += sNWPFile;

                                        if (File.Exists(sReadyDirFilePath))
                                        {
                                            File.Delete(sReadyDirFilePath);
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                        else if (!File.Exists(sReadyDirFilePath))
                                        {
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                    }
                                    else if (bDirFound == false && File.Exists(sReadyDir2 + sSendHereFile))
                                    {
                                        sReadyDirFilePath = sReadyDir2 += sNWPFile;

                                        if (File.Exists(sReadyDirFilePath))
                                        {
                                            File.Delete(sReadyDirFilePath);
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                        else if (!File.Exists(sReadyDirFilePath))
                                        {
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                    }
                                    else if (bDirFound == false && File.Exists(sReadyDir3 + sSendHereFile))
                                    {
                                        sReadyDirFilePath = sReadyDir3 += sNWPFile;

                                        if (File.Exists(sReadyDirFilePath))
                                        {
                                            File.Delete(sReadyDirFilePath);
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                        else if (!File.Exists(sReadyDirFilePath))
                                        {
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                    }
                                    else if (bDirFound == false && File.Exists(sReadyDir4 + sSendHereFile))
                                    {
                                        sReadyDirFilePath = sReadyDir4 += sNWPFile;

                                        if (File.Exists(sReadyDirFilePath))
                                        {
                                            File.Delete(sReadyDirFilePath);
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                        else if (!File.Exists(sReadyDirFilePath))
                                        {
                                            File.Copy(sRenderedPath, sReadyDirFilePath);
                                            File.Delete(sRenderedPath);

                                            bDirFound = true;
                                            bNWPMoved = true;
                                        }
                                    }
                                    else if (bDirFound == false)
                                    {
                                        string sSearchPattern02 = "DiscType = '" + sDiscType + "'";
                                        DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblDiscTypes"].Select(sSearchPattern02);

                                        if (dRowGatheredPattern02.Length > 0)
                                        {
                                            string sDefaultDir = Convert.ToString(dRowGatheredPattern02[0]["DefaultReadyDir"]).Trim();

                                            sReadyDirFilePath = sDefaultDir += sNWPFile;

                                            if (File.Exists(sReadyDirFilePath))
                                            {
                                                File.Delete(sReadyDirFilePath);
                                                File.Copy(sRenderedPath, sReadyDirFilePath);
                                                File.Delete(sRenderedPath);

                                                bDirFound = true;
                                                bNWPMoved = true;
                                            }
                                            else if (!File.Exists(sReadyDirFilePath))
                                            {
                                                File.Copy(sRenderedPath, sReadyDirFilePath);
                                                File.Delete(sRenderedPath);

                                                bDirFound = true;
                                                bNWPMoved = true;
                                            }
                                        }
                                        else if (dRowGatheredPattern02.Length == 0)
                                        {
                                            bNWPMoved = false;
                                        }
                                    }
                                }
                                else if (!File.Exists(sRenderedPath))
                                {
                                    bNWPMoved = false;
                                }
                            }
                            else if (dTblDiscOrders.Rows.Count == 0)
                            {
                                bNWPMoved = false;
                            }
                        }
                        else if (bSuccess != true)
                        {
                            bNWPMoved = false;
                        }
                    }
                    else if (bGenerateNWPFiles != true)
                    {

                    }
                }
                else if (dRowGatheredPattern01.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns03.SaveExceptionToDB(ex);
                bNWPMoved = false;
            }
        } //tm

        #endregion
    }
}
