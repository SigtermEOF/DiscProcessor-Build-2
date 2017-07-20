using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class PCDProcessing
    {
        // Common class suffix = 11
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns11 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing11 = new DataGatheringAndProcessing();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods11 = new TaskMethods();
        InsertsOrUpdates insertsOrUpdates11 = new InsertsOrUpdates();

        public void PushPCDToRender(string sProdNum, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                string sDiscType = "PCD";
                string sSitting = "";
                bool bSittingBased = false;
                string sStatus = string.Empty;

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                string sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "'";

                dbConns11.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                if (dTblDiscOrders.Rows.Count > 0)
                {
                    string sRefNum = Convert.ToString(dTblDiscOrders.Rows[0]["RefNum"]).Trim();
                    string sFrameNum = Convert.ToString(dTblDiscOrders.Rows[0]["FrameNum"]).Trim();

                    string sSearchPattern01 = "Label = 'PCDTemplateFile'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        string sExportDefFile = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                        string sExportDefFileText = File.ReadAllText(sExportDefFile);

                        sExportDefFileText = sExportDefFileText.Replace("ORDERID", sRefNum);

                        string sSearchPattern02 = "Label = 'PCDExportFileDropDir'";
                        DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                        if (dRowGatheredPattern02.Length > 0)
                        {
                            string sPCDExportFileDropDir = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                            string sPathToSave = sPCDExportFileDropDir + sRefNum + @"_" + sProdNum + ".txt";

                            File.WriteAllText(sPathToSave, sExportDefFileText);

                            bool bDiscOrderUpdateSuccess = false;
                            sCommText = "UPDATE [DiscOrders] SET [Status] = '35' WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = 'PCD'";

                            dbConns11.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bDiscOrderUpdateSuccess);

                            if (bDiscOrderUpdateSuccess == true)
                            {

                            }
                            else if (bDiscOrderUpdateSuccess != true)
                            {
                                string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to update status to 35 in the DiscOrders table.]";

                                insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                sStatus = "90";

                                insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                            }
                        }
                        else if (dRowGatheredPattern02.Length == 0)
                        {
                            string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to gather PCDExportFileDropDir from the Variables table.]";

                            insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                            sStatus = "90";

                            insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                        }
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to gather PCDTemplateFile data from the Variables table.]";

                        insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                        sStatus = "90";

                        insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                    }
                }
                else if (dTblDiscOrders.Rows.Count == 0)
                {

                }
            }
            catch (Exception ex)
            {
                dbConns11.SaveExceptionToDB(ex);
            }
        }

        public void CheckForRenderedOrderItems(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                string sDiscType = "PCD";
                string sSitting = "";
                bool bSittingBased = false;
                string sStatus = string.Empty;

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                string sCommText = "SELECT * FROM [DiscOrders] WHERE [Status] = '35' AND [DiscType] = 'PCD'";

                dbConns11.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                if (dTblDiscOrders.Rows.Count > 0)
                {
                    string sRefNum = string.Empty;
                    string sCustNum = string.Empty;
                    string sProdNum = string.Empty;
                    string sFrameNum = string.Empty;
                    int iQuantity = 0;
                    string sUniqueID = string.Empty;
                    int iOrderItemCount = 0;

                    foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                    {
                        sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                        sCustNum = Convert.ToString(dRowDiscOrders["CustNum"]).Trim();
                        sProdNum = Convert.ToString(dRowDiscOrders["ProdNum"]).Trim();
                        sFrameNum = Convert.ToString(dRowDiscOrders["FrameNum"]).Trim();
                        iQuantity = Convert.ToInt32(dRowDiscOrders["Quantity"]);
                        sUniqueID = Convert.ToString(dRowDiscOrders["UniqueID"]).Trim();

                        DataTable dTblOrderItems = new DataTable("dTblOrderItems");
                        sCommText = "SELECT * FROM [OrderItems] WHERE [OrderID] = '" + sRefNum + "'";

                        dbConns11.SQLQuery(sDP2ConnString, sCommText, dTblOrderItems);

                        if (dTblOrderItems.Rows.Count > 0)
                        {
                            iOrderItemCount = dTblOrderItems.Rows.Count;

                            string sTempRenderedPath = @"\\hp2\vol1\jobs\cdsburn\ShootCD\" + sRefNum + @"\"; //Note: set this as a variable

                            if (!Directory.Exists(sTempRenderedPath))
                            {
                                Directory.CreateDirectory(sTempRenderedPath);
                            }

                            int iDirRenderedCount = Directory.GetFiles(sTempRenderedPath, "*.jpg", SearchOption.TopDirectoryOnly).Length;

                            if (iDirRenderedCount == iOrderItemCount)
                            {
                                string sDPRenderedPath = @"\\hp2\vol1\jobs\cdsburn\DiscProcessor Renders\PCD\" + sRefNum + @"\";

                                if (!Directory.Exists(sDPRenderedPath))
                                {
                                    Directory.CreateDirectory(sDPRenderedPath);
                                }

                                string[] sFiles = Directory.GetFiles(sTempRenderedPath);
                                string sFullPathToDPRenderedPath = string.Empty;
                                string sFullPathToCurrentRenderedPath = string.Empty;

                                foreach (string sFile in sFiles)
                                {
                                    if (Path.GetExtension(sFile) == ".jpg") //filter only jpg's here
                                    {
                                        string sFilePath = Path.GetFileName(sFile);

                                        sFullPathToDPRenderedPath = sDPRenderedPath + sFilePath;
                                        sFullPathToCurrentRenderedPath = sTempRenderedPath + sFilePath;

                                        if (!File.Exists(sFullPathToDPRenderedPath))
                                        {
                                            File.Copy(sFullPathToCurrentRenderedPath, sFullPathToDPRenderedPath);
                                        }
                                    }
                                }

                                int iMovedFileCount = Directory.GetFiles(Path.GetDirectoryName(sFullPathToDPRenderedPath), "*.jpg", SearchOption.TopDirectoryOnly).Length;

                                if (iMovedFileCount == iOrderItemCount)
                                {
                                    // gather merge data
                                    bool bUpdateMergeDataSuccess = false;
                                    this.GatherAndSavePCDMergeData(sRefNum, sCustNum, sProdNum, ref bUpdateMergeDataSuccess, sFrameNum);

                                    if (bUpdateMergeDataSuccess == true)
                                    {
                                        //gather nwp data
                                        bool bUpdateNWPDataSuccess = false;
                                        this.GatherAndSavePCDNWPData(sDPRenderedPath, iQuantity, sUniqueID, sRefNum, sFrameNum, ref bUpdateNWPDataSuccess);

                                        if (bUpdateNWPDataSuccess == true)
                                        {
                                            // write out that data to files
                                            DataTable dTblGatherMergeAndNWPFileDataFromDiscOrders = new DataTable("dTblGatherMergeAndNWPFileDataFromDiscOrders");
                                            sCommText = "SELECT [MergeFileData], [NWPFileData] FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum +
                                                "' AND DiscType = 'PCD'";

                                            dbConns11.SQLQuery(sDiscProcessorConnString, sCommText, dTblGatherMergeAndNWPFileDataFromDiscOrders);

                                            if (dTblGatherMergeAndNWPFileDataFromDiscOrders.Rows.Count > 0)
                                            {
                                                string sMergeFileData = Convert.ToString(dTblGatherMergeAndNWPFileDataFromDiscOrders.Rows[0]["MergeFileData"]);
                                                string sNWPFileData = Convert.ToString(dTblGatherMergeAndNWPFileDataFromDiscOrders.Rows[0]["NWPFileData"]);

                                                string sMergeFilePath = sDPRenderedPath + sRefNum + "_Merge.txt";
                                                string sNWPFilePath = sDPRenderedPath + "PCD" + "_" + sRefNum + "_" + sFrameNum + ".NWP";

                                                File.WriteAllText(sMergeFilePath, sMergeFileData);

                                                string sSearchPattern01 = "GatherDiscType = 'PCD'";
                                                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                                                if (dRowGatheredPattern01.Length > 0)
                                                {
                                                    bool bGenerateNWPFiles = Convert.ToBoolean(dRowGatheredPattern01[0]["GenerateNWPFiles"]);

                                                    if (bGenerateNWPFiles == true)
                                                    {
                                                        File.WriteAllText(sNWPFilePath, sNWPFileData);

                                                        string sReadyDir1 = string.Empty;
                                                        string sReadyDir2 = string.Empty;
                                                        string sReadyDir3 = string.Empty;
                                                        string sReadyDir4 = string.Empty;
                                                        string sSendHereFile = string.Empty;
                                                        bool bGatheredNWPFileVariablesSuccess = true;
                                                        string sReadyDirFilePath = string.Empty;
                                                        string sRenderedPath = sDPRenderedPath;

                                                        dataGatheringAndProcessing11.GatherNWPFileVariables(ref sReadyDir1, ref sReadyDir2, ref sReadyDir3, ref sReadyDir4, ref sSendHereFile, ref bGatheredNWPFileVariablesSuccess, dataSetMain);

                                                        if (bGatheredNWPFileVariablesSuccess == true)
                                                        {
                                                            // move nwp to rimage
                                                            bool bDirFound = false;
                                                            bool bNWPMoved = false;
                                                            string sFile = Path.GetFileName(sNWPFilePath);

                                                            if (File.Exists(sNWPFilePath))
                                                            {
                                                                if (bDirFound == false && (File.Exists(sReadyDir1 + sSendHereFile)))
                                                                {
                                                                    sReadyDirFilePath = sReadyDir1 += sFile;

                                                                    if (File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Delete(sReadyDirFilePath);
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                    else if (!File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                }
                                                                else if (bDirFound == false && File.Exists(sReadyDir2 + sSendHereFile))
                                                                {
                                                                    sReadyDirFilePath = sReadyDir2 += sFile;

                                                                    if (File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Delete(sReadyDirFilePath);
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                    else if (!File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                }
                                                                else if (bDirFound == false && File.Exists(sReadyDir3 + sSendHereFile))
                                                                {
                                                                    sReadyDirFilePath = sReadyDir3 += sFile;

                                                                    if (File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Delete(sReadyDirFilePath);
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                    else if (!File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                }
                                                                else if (bDirFound == false && File.Exists(sReadyDir4 + sSendHereFile))
                                                                {
                                                                    sReadyDirFilePath = sReadyDir4 += sFile;

                                                                    if (File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Delete(sReadyDirFilePath);
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                    else if (!File.Exists(sReadyDirFilePath))
                                                                    {
                                                                        File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                        File.Delete(sRenderedPath + sFile);

                                                                        bDirFound = true;
                                                                        bNWPMoved = true;
                                                                    }
                                                                }
                                                                else if (bDirFound == false)
                                                                {
                                                                    string sSearchPattern02 = "DiscType = 'PCD'";
                                                                    DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblDiscTypes"].Select(sSearchPattern02);

                                                                    if (dRowGatheredPattern02.Length > 0)
                                                                    {
                                                                        string sDefaultDir = Convert.ToString(dRowGatheredPattern02[0]["DefaultReadyDir"]).Trim();

                                                                        sReadyDirFilePath = sDefaultDir += sFile;

                                                                        if (File.Exists(sReadyDirFilePath))
                                                                        {
                                                                            File.Delete(sReadyDirFilePath);
                                                                            File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                            File.Delete(sRenderedPath + sFile);

                                                                            bDirFound = true;
                                                                            bNWPMoved = true;
                                                                        }
                                                                        else if (!File.Exists(sReadyDirFilePath))
                                                                        {
                                                                            File.Copy(sRenderedPath + sFile, sReadyDirFilePath);
                                                                            File.Delete(sRenderedPath + sFile);

                                                                            bDirFound = true;
                                                                            bNWPMoved = true;
                                                                        }
                                                                    }
                                                                    else if (dRowGatheredPattern02.Length == 0)
                                                                    {
                                                                        bNWPMoved = false;
                                                                    }
                                                                }

                                                                bool bUpdateDiscOrdersStatus = false;
                                                                sCommText = "UPDATE [DiscOrders] SET [Status] = '60' WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "'" +
                                                                    " AND [DiscType] = 'PCD'";

                                                                dbConns11.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bUpdateDiscOrdersStatus);

                                                                if (bUpdateDiscOrdersStatus == true)
                                                                {

                                                                }
                                                                else if (bUpdateDiscOrdersStatus != true)
                                                                {
                                                                    string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to update status to 60 in the DiscOrders table.]";

                                                                    insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                                    sStatus = "90";

                                                                    insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                                }
                                                            }
                                                            else if (!File.Exists(sNWPFilePath))
                                                            {
                                                                string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to locate NWP file to move to the Rimage.]";

                                                                insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                                sStatus = "90";

                                                                insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                            }
                                                        }
                                                        else if (bGatheredNWPFileVariablesSuccess != true)
                                                        {
                                                            string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to gather NWP file data.]";

                                                            insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                            sStatus = "90";

                                                            insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                        }
                                                    }
                                                    else if (bGenerateNWPFiles != true)
                                                    {
                                                        bool bUpdateDiscOrdersStatus = false;
                                                        sCommText = "UPDATE [DiscOrders] SET [Status] = '60' WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "'" +
                                                            " AND [DiscType] = 'PCD'";

                                                        dbConns11.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bUpdateDiscOrdersStatus);

                                                        if (bUpdateDiscOrdersStatus == true)
                                                        {

                                                        }
                                                        else if (bUpdateDiscOrdersStatus != true)
                                                        {
                                                            string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to update status to 60 in the DiscOrders table.]";

                                                            insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                            sStatus = "90";

                                                            insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                        }

                                                        string sText = "[NWP files are not flagged for moving to Rimage for reference # " + sRefNum + " and frame # " + sFrameNum + ".]";
                                                        taskMethods11.LogText(sText, mForm);                                                        
                                                    }
                                                }
                                                else if (dRowGatheredPattern01.Length == 0)
                                                {
                                                    string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to gather GenerateNWPFiles value from the GatherDiscTypes table.]";

                                                    insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    sStatus = "90";

                                                    insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (dTblGatherMergeAndNWPFileDataFromDiscOrders.Rows.Count == 0)
                                            {
                                                string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to gather merge and/or NWP file data from the DiscOrders table.]";

                                                insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                sStatus = "90";

                                                insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                            }
                                        }
                                        else if (bUpdateNWPDataSuccess != true)
                                        {
                                            string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to update NWPFileData in the DiscOrders table.]";

                                            insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                            sStatus = "90";

                                            insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                        }
                                    }
                                    else if (bUpdateMergeDataSuccess != true)
                                    {
                                        string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to update MergeFileData in the DiscOrders table.]";

                                        insertsOrUpdates11.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                        sStatus = "90";

                                        insertsOrUpdates11.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }
                                }
                            }
                            else if (iDirRenderedCount != iOrderItemCount)
                            {
                                // Continue.
                            }
                        }
                        else if (dTblOrderItems.Rows.Count == 0)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                }
                else if (dTblDiscOrders.Rows.Count == 0)
                {

                }
            }
            catch (Exception ex)
            {
                dbConns11.SaveExceptionToDB(ex);
            }
        }

        private void GatherAndSavePCDMergeData(string sRefNum, string sCustNum, string sProdNum, ref bool bUpdateMergeDataSuccess, string sFrameNum)
        {
            try
            {
                string sCustName = string.Empty;
                string sCustID = string.Empty;

                DataTable dTblCustomer = new DataTable("dTblCustomer");
                string sCommText = "SELECT Name FROM Customer WHERE Customer = '" + sCustNum + "'";

                dbConns11.CDSQuery(sCDSConnString, sCommText, dTblCustomer);

                if (dTblCustomer.Rows.Count > 0)
                {
                    sCustName = Convert.ToString(dTblCustomer.Rows[0]["Name"]).Trim();
                }
                else if (dTblCustomer.Rows.Count == 0)
                {

                }

                DataTable dTblItems = new DataTable("dTblItems");
                sCommText = "SELECT Custid FROM Items WHERE Order = '" + sRefNum + "'";

                dbConns11.CDSQuery(sCDSConnString, sCommText, dTblItems);

                if (dTblItems.Rows.Count > 0)
                {
                    sCustID = Convert.ToString(dTblItems.Rows[0]["Custid"]).Trim();
                }
                else if (dTblItems.Rows.Count == 0)
                {

                }

                sCustName = sCustName.Replace("'", "''");
                sCustID = sCustID.Replace("'", "''");

                string sMergeText = "\"" + sRefNum + "\",\"" + sProdNum + "\",\"" + sCustName + "\",\"" + sCustID + "\"";

                bUpdateMergeDataSuccess = false;
                sCommText = "UPDATE [DiscOrders] Set [MergeFileData] = '" + sMergeText + "' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = 'PCD'";

                dbConns11.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bUpdateMergeDataSuccess);

                if (bUpdateMergeDataSuccess == true)
                {
                    bUpdateMergeDataSuccess = true;
                }
                else if (bUpdateMergeDataSuccess != true)
                {
                    bUpdateMergeDataSuccess = false;
                }
            }
            catch (Exception ex)
            {
                bUpdateMergeDataSuccess = false;

                dbConns11.SaveExceptionToDB(ex);
            }
        }

        private void GatherAndSavePCDNWPData(string sDPRenderedPath, int iQuantity, string sUniqueID, string sRefNum, string sFrameNum, ref bool bUpdateNWPDataSuccess)
        {
            try
            {
                string sLabelFile = string.Empty;
                string sMergeFilePath = sDPRenderedPath + sRefNum + "_Merge.txt";

                DataTable dTblVariables01 = new DataTable("dTblVariables01");
                string sCommText = "SELECT [Value] FROM [Variables] WHERE [Label] = 'PCDLabelFile'";

                dbConns11.SQLQuery(sDiscProcessorConnString, sCommText, dTblVariables01);

                if (dTblVariables01.Rows.Count > 0)
                {
                    sLabelFile = Convert.ToString(dTblVariables01.Rows[0]["Value"]).Trim();
                }
                else if (dTblVariables01.Rows.Count == 0)
                {

                }

                StringBuilder sBuilder = new StringBuilder();
                sBuilder.AppendFormat("file = {0}", sDPRenderedPath + Environment.NewLine);
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("FileType = Parent");
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("copies = {0}", iQuantity);
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("Label = {0}", sLabelFile);
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("Priority = 1");
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("media = CDR");
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("Merge = {0}", sMergeFilePath);
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("OrderID = {0}", sUniqueID);
                sBuilder.Append(Environment.NewLine);
                sBuilder.AppendFormat("volume = {0}", sUniqueID);
                sBuilder.Append(Environment.NewLine);

                string sBuilderText = Convert.ToString(sBuilder);

                bUpdateNWPDataSuccess = false;
                sCommText = "UPDATE [DiscOrders] SET [NWPFileData] = '" + sBuilder.ToString() + "' WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "'";

                dbConns11.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bUpdateNWPDataSuccess);

                if (bUpdateNWPDataSuccess == true)
                {
                    bUpdateNWPDataSuccess = true;
                }
                else if (bUpdateNWPDataSuccess != true)
                {
                    bUpdateNWPDataSuccess = false;
                }
            }
            catch (Exception ex)
            {
                bUpdateNWPDataSuccess = false;

                dbConns11.SaveExceptionToDB(ex);
            }
        }
    }
}
