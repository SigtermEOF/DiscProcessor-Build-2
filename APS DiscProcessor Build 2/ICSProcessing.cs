using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class ICSProcessing
    {
        // Common class suffix = 09
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns09 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing09 = new DataGatheringAndProcessing();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods09 = new TaskMethods();
        LockFile lockFile09 = new LockFile();
        InsertsOrUpdates insertsOrUpdates09 = new InsertsOrUpdates();

        public void ICSGatherRenderInfo(string sProdNum, ref bool bInitialPass01, ref bool bCreated, DataTable dTblOrder, string sSitting, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                string sDiscType = "ICS";
                bool bSittingBased = true;

                string sSearchPattern01 = "Label = 'ICSRenderedPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                string sSearchPattern02 = "Label = 'ExportDefPath'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                string sSearchPattern03 = "Label = 'ExportDefPathDone'";
                DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0)
                {
                    string sICSRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                    string sExportDefPath = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                    string sExportDefPathDone = Convert.ToString(dRowGatheredPattern03[0]["Value"]).Trim();

                    if (!Directory.Exists(sICSRenderedPath))
                    {
                        Directory.CreateDirectory(sICSRenderedPath);
                    }

                    DataTable dTblSitting = new DataTable("dTblSitting");
                    List<string> lLayouts = new List<string>();
                    int iRenderedCount = 0;
                    bool bHaveFile = false;
                    string sExportDef = string.Empty;
                    bool bInitialJobIDAssigned = false;
                    string sExportDefRenderFile = string.Empty;
                    bool bGoodResults = true;
                    string sExportDefSitting = string.Empty;
                    string sCustNum = string.Empty;
                    string sRefNum = string.Empty;
                    string sFrameNum = string.Empty;
                    string sRenderedImageLocation = string.Empty;

                    if (bCreated != true)
                    {
                        dTblSitting.Columns.Add("Count", typeof(int)); // iRenderedCount
                        dTblSitting.Columns.Add("BatchID", typeof(int)); // iBatchID
                        dTblSitting.Columns.Add("JobID", typeof(int)); // iJobID
                        dTblSitting.Columns.Add("RefNum", typeof(string)); // sRefNum
                        dTblSitting.Columns.Add("Sitting", typeof(string)); // sSitting
                        dTblSitting.Columns.Add("FrameNum", typeof(string)); // sFrameNum
                        dTblSitting.Columns.Add("ImageName", typeof(string)); // sImageName
                        dTblSitting.Columns.Add("ExportDefFile", typeof(string)); // sExportDef
                        dTblSitting.Columns.Add("SavedExportDefPath", typeof(string)); // sExportDefRenderFile
                        dTblSitting.Columns.Add("RenderedImageLocation", typeof(string)); // sRenderedImageLocation
                        dTblSitting.Columns.Add("CustNum", typeof(string)); // sCustNum

                        lLayouts.Add("COLOR.txt");

                        bCreated = true;
                    }
                    else if (bCreated == true)
                    {
                        // Continue.
                    }

                    DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                    string sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "' AND [DiscType] = 'ICS' AND [Sitting] = '" + sSitting + "' ORDER BY [FrameNum]";

                    dbConns09.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                    if (dTblDiscOrders.Rows.Count > 0)
                    {
                        try
                        {
                            DataTable dTblFrames = new DataTable("dTblFrames");
                            sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "' ORDER BY Sequence";

                            dbConns09.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                            DataTable dTblDP2Image = new DataTable("dTblDP2Image");
                            sCommText = "SELECT Path, Frame FROM DP2Image WHERE Lookupnum = '" + sProdNum + "' ORDER BY Frame";

                            dbConns09.CDSQuery(sCDSConnString, sCommText, dTblDP2Image);

                            foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                            {
                                bInitialPass01 = true;

                                sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                                sExportDefSitting = sSitting;
                                sCustNum = Convert.ToString(dRowDiscOrders["CustNum"]).Trim();

                                if (bInitialPass01 == true)
                                {
                                    string sText = "[Gathering image name and path for reference number " + sRefNum + " and sitting number " + sSitting.Trim() + ".]";
                                    taskMethods09.LogText(sText, mForm);

                                    bInitialPass01 = false;
                                }

                                if (dTblFrames.Rows.Count > 0)
                                {
                                    string sSearchPattern04 = "Sitting = '" + sSitting + "'";
                                    string sSearchOrderBy04 = "Sequence ASC";
                                    DataRow[] dRowGatheredPattern04 = dTblFrames.Select(sSearchPattern04, sSearchOrderBy04);

                                    if (dRowGatheredPattern04.Length > 0)
                                    {
                                        int idTblFramesRowCount = dRowGatheredPattern04.Length;

                                        bool bIDsGathered = false;
                                        int iBatchID = 0;
                                        int iJobID = 0;
                                        int iJobIDsNeeded = idTblFramesRowCount * 1;
                                        bool bLockedFile = false;
                                        bool bGatherIDsSuccess = false;

                                        if (bIDsGathered != true)
                                        {
                                            lockFile09.LockTheFile(ref iBatchID, ref iJobID, ref iJobIDsNeeded, ref bLockedFile, ref bGatherIDsSuccess, dataSetMain);
                                        }
                                        else if (bIDsGathered == true)
                                        {
                                            // ID's previously gathered, continue.
                                        }

                                        int iLastJobID = iJobID + iJobIDsNeeded;

                                        if (bLockedFile != true && bGatherIDsSuccess == true)
                                        {
                                            dTblSitting.Clear();

                                            foreach (DataRow dRowFrames in dRowGatheredPattern04)
                                            {
                                                string sSequence = Convert.ToString(dRowFrames["Sequence"]).Trim();
                                                sFrameNum = sSequence;

                                                if (dTblDP2Image.Rows.Count > 0)
                                                {
                                                    string sSearchPattern05 = "Frame = '" + sSequence + "'";
                                                    DataRow[] dRowGatheredPattern05 = dTblDP2Image.Select(sSearchPattern05);

                                                    if (dRowGatheredPattern05.Length > 0)
                                                    {
                                                        string sPath = Convert.ToString(dRowGatheredPattern05[0]["Path"]).Trim();
                                                        string sImageName = Path.GetFileNameWithoutExtension(sPath);
                                                        bool bExists = false;

                                                        taskMethods09.ImageExists(sPath, ref bExists);

                                                        if (bExists == true)
                                                        {
                                                            foreach (string s in lLayouts)
                                                            {
                                                                if (bInitialJobIDAssigned == true)
                                                                {
                                                                    // This will prevent skipping the initial gathered JobID when assigning to a rendered product.
                                                                    iJobID += +1;
                                                                }
                                                                else if (bInitialJobIDAssigned != true)
                                                                {
                                                                    bInitialJobIDAssigned = true;
                                                                }

                                                                if (s == "COLOR.txt")
                                                                {
                                                                    sExportDef = s;

                                                                    taskMethods09.CheckForExportFileExistence(ref sExportDef, ref bHaveFile, sExportDefPath);

                                                                    if (bHaveFile == true)
                                                                    {
                                                                        iRenderedCount += 1;

                                                                        sRenderedImageLocation = sICSRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\" + iRenderedCount + "-" + sImageName + ".JPG";
                                                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sSitting.Trim() + "_" + sSequence + "-" + iRenderedCount + sImageName + ".txt";

                                                                        string text = File.ReadAllText(sExportDefPath + sExportDef);

                                                                        text = text.Replace("APSPATH", sPath);
                                                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                                                        string sGSBG = string.Empty;
                                                                        taskMethods09.GetGSFromCodes(sProdNum, sFrameNum, ref sGSBG, sSitting);

                                                                        if (sGSBG.Length > 0)
                                                                        {
                                                                            bool bGSFound = false;
                                                                            taskMethods09.VerifyGS(sGSBG, ref bGSFound);

                                                                            if (bGSFound == true)
                                                                            {
                                                                                text = text.Replace("APSBGID", sGSBG);
                                                                            }
                                                                            else if (bGSFound != true)
                                                                            {

                                                                            }
                                                                        }
                                                                        else if (sGSBG.Length == 0)
                                                                        {
                                                                            text = text.Replace("APSBGID", "");
                                                                        }

                                                                        File.WriteAllText(sExportDefRenderFile, text);

                                                                        dTblSitting.Rows.Add(iRenderedCount, iBatchID, iJobID, sRefNum, sSitting, sSequence, sPath, sExportDef, sExportDefRenderFile, sRenderedImageLocation, sCustNum);
                                                                    }
                                                                    else if (bHaveFile != true)
                                                                    {
                                                                        sBreakPoint = string.Empty;
                                                                        bGoodResults = false;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else if (bExists != true)
                                                        {
                                                            sBreakPoint = string.Empty;
                                                            bGoodResults = false;

                                                            string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Image(s) could not be located.]";

                                                            insertsOrUpdates09.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sSequence, sDiscType, sExportDefSitting, bSittingBased);

                                                            string sStatus = "90";

                                                            insertsOrUpdates09.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sSequence, sStatus, sDiscType, sProdNum);

                                                            taskMethods09.RemoveOrphanedExportDefFiles(dTblSitting);
                                                            taskMethods09.RemoveOrphanedExportDefFiles(dTblOrder);

                                                            string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                                                            taskMethods09.LogText(sText, mForm);
                                                        }
                                                    }
                                                    else if (dRowGatheredPattern05.Length == 0)
                                                    {
                                                        sBreakPoint = string.Empty;
                                                    }
                                                }
                                                else if (dTblDP2Image.Rows.Count == 0)
                                                {
                                                    sBreakPoint = string.Empty;
                                                    bGoodResults = false;

                                                    string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ No Dp2Image records.]";

                                                    insertsOrUpdates09.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sSequence, sDiscType, sExportDefSitting, bSittingBased);

                                                    string sStatus = "90";

                                                    insertsOrUpdates09.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sSequence, sStatus, sDiscType, sProdNum);

                                                    taskMethods09.RemoveOrphanedExportDefFiles(dTblSitting);
                                                    taskMethods09.RemoveOrphanedExportDefFiles(dTblOrder);

                                                    string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                                                    taskMethods09.LogText(sText, mForm);
                                                }
                                            }
                                        }
                                        else if (bLockedFile == true || bGatherIDsSuccess != true)
                                        {
                                            sBreakPoint = string.Empty;
                                            bGoodResults = false;
                                        }

                                        dTblOrder.Merge(dTblSitting);
                                    }
                                    else if (dRowGatheredPattern04.Length == 0)
                                    {
                                        sBreakPoint = string.Empty;
                                    }
                                }
                                else if (dTblFrames.Rows.Count == 0)
                                {
                                    sBreakPoint = string.Empty;
                                }
                            }
                        }
                        catch (ObjectDisposedException odex)
                        {
                            taskMethods09.RemoveOrphanedExportDefFiles(dTblSitting);
                            taskMethods09.RemoveOrphanedExportDefFiles(dTblOrder);

                            string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                            taskMethods09.LogText(sText, mForm);

                            bool bSuccess = true;
                            sCommText = "UPDATE [DiscOrders] SET [Status] = '10' WHERE [RefNum] = '" + sRefNum + "' AND [DiscType] = '" + sDiscType + "'";

                            dbConns09.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                            if (bSuccess == true)
                            {
                                bGoodResults = false;
                            }
                            else if (bSuccess != true)
                            {
                                bGoodResults = false;
                            }
                        }

                        // End of foreach through dTblDiscOrders.

                        sBreakPoint = string.Empty;

                    }
                    else if (dTblDiscOrders.Rows.Count == 0)
                    {

                    }

                    // End of try block.

                    if (bGoodResults == true)
                    {
                        dataGatheringAndProcessing09.ExportDefProcessing(sProdNum, bGoodResults, dTblOrder, sDiscType, bSittingBased, dataSetMain, mForm);
                    }
                    else if (bGoodResults != true)
                    {
                        dTblSitting.Clear();
                        dTblOrder.Clear();

                        // Go back into foreach, order with issue will be picked up next cycle or flagged as stalled if not.
                    }
                }
                else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0 || dRowGatheredPattern03.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns09.SaveExceptionToDB(ex);
            }
        }
    }
}
