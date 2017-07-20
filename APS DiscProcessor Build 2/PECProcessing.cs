using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class PECProcessing
    {
        // Common class suffix = 04
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns04 = new DBConnections();
        string sBreakPoint = string.Empty;
        DataGatheringAndProcessing dataGatheringAndProcessing04 = new DataGatheringAndProcessing();
        Main_Form mainForm = null;
        TaskMethods taskMethods04 = new TaskMethods();
        LockFile lockFile04 = new LockFile();
        InsertsOrUpdates insertsOrUpdates04 = new InsertsOrUpdates();

        public void PECGatherRenderInfo(string sProdNum, ref bool bInitialPassPEC01, ref bool bCreated, DataTable dTblOrder, string sSitting, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                string sDiscType = "PEC";
                bool bSittingBased = true;

                DataTable dTblSitting = new DataTable("dTblSitting");
                List<string> lLayouts = new List<string>();
                int iRenderedCount = 0;
                bool bHaveFile = false;
                string sExportDef = string.Empty;
                bool bInitialJobIDAssigned = false;
                string sExportDefPath = string.Empty;
                string sExportDefPathDone = string.Empty;
                string sRenderedImageLocation = string.Empty;
                string sExportDefRenderFile = string.Empty;
                bool bGoodResults = true;
                string sCustNum = string.Empty;
                string sRefNum = string.Empty;
                string sFrameNum = string.Empty;

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
                    lLayouts.Add("BW.txt");
                    lLayouts.Add("SEP.txt");
                    lLayouts.Add("CES.txt");

                    bCreated = true;
                }
                else if (bCreated == true)
                {

                }

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                string sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "' AND [DiscType] = 'PEC' AND [Sitting] = '" + sSitting + "'";

                dbConns04.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                if (dTblDiscOrders.Rows.Count > 0)
                {
                    try
                    {
                        DataTable dTblFrames = new DataTable("dTblFrames");
                        sCommText = "SELECT * FROM Frames WHERE Lookupnum = '" + sProdNum + "'";

                        dbConns04.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                        DataTable dTblDP2Image = new DataTable("dTblDP2Image");
                        sCommText = "SELECT * FROM DP2Image WHERE Lookupnum = '" + sProdNum + "'";

                        dbConns04.CDSQuery(sCDSConnString, sCommText, dTblDP2Image);

                        foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                        {
                            bInitialPassPEC01 = true;

                            sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                            sCustNum = Convert.ToString(dRowDiscOrders["CustNum"]).Trim();

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
                                    int iJobIDsNeeded = idTblFramesRowCount * 4;
                                    bool bLockedFile = false;
                                    bool bGatherIDsSuccess = false;

                                    if (bIDsGathered != true)
                                    {
                                        lockFile04.LockTheFile(ref iBatchID, ref iJobID, ref iJobIDsNeeded, ref bLockedFile, ref bGatherIDsSuccess, dataSetMain);
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
                                                    string sImageName = Convert.ToString(dRowGatheredPattern05[0]["Path"]).Trim();
                                                    bool bExists = false;

                                                    taskMethods04.ImageExists(sImageName, ref bExists);

                                                    if (bExists == true)
                                                    {
                                                        string sSearchPattern01 = "Label = 'PECRenderedPath'";
                                                        DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                                                        string sSearchPattern02 = "Label = 'ExportDefPath'";
                                                        DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                                                        string sSearchPattern03 = "Label = 'ExportDefPathDone'";
                                                        DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                                                        if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0)
                                                        {
                                                            string sPECRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                                                            sExportDefPath = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                                                            sExportDefPathDone = Convert.ToString(dRowGatheredPattern03[0]["Value"]).Trim();

                                                            if (!Directory.Exists(sExportDefPathDone))
                                                            {
                                                                Directory.CreateDirectory(sExportDefPathDone);
                                                            }

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

                                                                    taskMethods04.CheckForExportFileExistence(ref sExportDef, ref bHaveFile, sExportDefPath);

                                                                    if (bHaveFile == true)
                                                                    {
                                                                        iRenderedCount += 1;

                                                                        sRenderedImageLocation = sPECRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\" + sProdNum + sSitting.Trim() + sSequence + "-" + iRenderedCount + "-Color.JPG";
                                                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sSitting.Trim() + "_" + sSequence + "-" + iRenderedCount + "-Color.txt";

                                                                        string text = File.ReadAllText(sExportDefPath + sExportDef);

                                                                        text = text.Replace("APSPATH", sImageName);
                                                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                                                        string sGSBG = string.Empty;
                                                                        taskMethods04.GetGSFromCodes(sProdNum, sFrameNum, ref sGSBG, sSitting);

                                                                        if (sGSBG.Length > 0)
                                                                        {
                                                                            bool bGSFound = false;
                                                                            taskMethods04.VerifyGS(sGSBG, ref bGSFound);

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

                                                                        dTblSitting.Rows.Add(iRenderedCount, iBatchID, iJobID, sRefNum, sSitting, sSequence, sImageName, sExportDef, sExportDefRenderFile, sRenderedImageLocation, sCustNum);
                                                                    }
                                                                    else if (bHaveFile != true)
                                                                    {
                                                                        sBreakPoint = string.Empty;
                                                                        bGoodResults = false;
                                                                    }
                                                                }
                                                                else if (s == "BW.txt")
                                                                {
                                                                    sExportDef = s;

                                                                    taskMethods04.CheckForExportFileExistence(ref sExportDef, ref bHaveFile, sExportDefPath);

                                                                    if (bHaveFile == true)
                                                                    {
                                                                        iRenderedCount += 1;

                                                                        sRenderedImageLocation = sPECRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\" + sProdNum + sSitting.Trim() + sSequence + "-" + iRenderedCount + "-BW.JPG";
                                                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sSitting.Trim() + "_" + sSequence + "-" + iRenderedCount + "-BW.txt";

                                                                        string text = File.ReadAllText(sExportDefPath + sExportDef);

                                                                        text = text.Replace("APSPATH", sImageName);
                                                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                                                        string sGSBG = string.Empty;
                                                                        taskMethods04.GetGSFromCodes(sProdNum, sFrameNum, ref sGSBG, sSitting);

                                                                        if (sGSBG.Length > 0)
                                                                        {
                                                                            bool bGSFound = false;
                                                                            taskMethods04.VerifyGS(sGSBG, ref bGSFound);

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

                                                                        dTblSitting.Rows.Add(iRenderedCount, iBatchID, iJobID, sRefNum, sSitting, sSequence, sImageName, sExportDef, sExportDefRenderFile, sRenderedImageLocation, sCustNum);
                                                                    }
                                                                    else if (bHaveFile != true)
                                                                    {
                                                                        sBreakPoint = string.Empty;
                                                                        bGoodResults = false;
                                                                    }
                                                                }
                                                                else if (s == "SEP.txt")
                                                                {
                                                                    sExportDef = s;

                                                                    taskMethods04.CheckForExportFileExistence(ref sExportDef, ref bHaveFile, sExportDefPath);

                                                                    if (bHaveFile == true)
                                                                    {
                                                                        iRenderedCount += 1;

                                                                        sRenderedImageLocation = sPECRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\" + sProdNum + sSitting.Trim() + sSequence + "-" + iRenderedCount + "-SEP.JPG";
                                                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sSitting.Trim() + "_" + sSequence + "-" + iRenderedCount + "-SEP.txt";

                                                                        string text = File.ReadAllText(sExportDefPath + sExportDef);

                                                                        text = text.Replace("APSPATH", sImageName);
                                                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                                                        string sGSBG = string.Empty;
                                                                        taskMethods04.GetGSFromCodes(sProdNum, sFrameNum, ref sGSBG, sSitting);

                                                                        if (sGSBG.Length > 0)
                                                                        {
                                                                            bool bGSFound = false;
                                                                            taskMethods04.VerifyGS(sGSBG, ref bGSFound);

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

                                                                        dTblSitting.Rows.Add(iRenderedCount, iBatchID, iJobID, sRefNum, sSitting, sSequence, sImageName, sExportDef, sExportDefRenderFile, sRenderedImageLocation, sCustNum);
                                                                    }
                                                                    else if (bHaveFile != true)
                                                                    {
                                                                        sBreakPoint = string.Empty;
                                                                        bGoodResults = false;
                                                                    }
                                                                }
                                                                else if (s == "CES.txt")
                                                                {
                                                                    sExportDef = s;

                                                                    taskMethods04.CheckForExportFileExistence(ref sExportDef, ref bHaveFile, sExportDefPath);

                                                                    if (bHaveFile == true)
                                                                    {
                                                                        iRenderedCount += 1;

                                                                        sRenderedImageLocation = sPECRenderedPath + sRefNum + @"\" + sProdNum + @"\" + sSitting.Trim() + @"\" + sProdNum + sSitting.Trim() + sSequence + "-" + iRenderedCount + "-CES.JPG";
                                                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sSitting.Trim() + "_" + sSequence + "-" + iRenderedCount + "-CES.txt";

                                                                        string text = File.ReadAllText(sExportDefPath + sExportDef);

                                                                        text = text.Replace("APSPATH", sImageName);
                                                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                                                        string sGSBG = string.Empty;
                                                                        taskMethods04.GetGSFromCodes(sProdNum, sFrameNum, ref sGSBG, sSitting);

                                                                        if (sGSBG.Length > 0)
                                                                        {
                                                                            bool bGSFound = false;
                                                                            taskMethods04.VerifyGS(sGSBG, ref bGSFound);

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

                                                                        dTblSitting.Rows.Add(iRenderedCount, iBatchID, iJobID, sRefNum, sSitting, sSequence, sImageName, sExportDef, sExportDefRenderFile, sRenderedImageLocation, sCustNum);
                                                                    }
                                                                    else if (bHaveFile != true)
                                                                    {
                                                                        sBreakPoint = string.Empty;
                                                                        bGoodResults = false;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0 || dRowGatheredPattern03.Length == 0)
                                                        {
                                                            sBreakPoint = string.Empty;
                                                        }
                                                    }
                                                    else if (bExists != true)
                                                    {
                                                        sBreakPoint = string.Empty;
                                                        bGoodResults = false;

                                                        string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Image(s) could not be located.]";

                                                        insertsOrUpdates04.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sSequence, sDiscType, sSitting, bSittingBased);

                                                        string sStatus = "90";

                                                        insertsOrUpdates04.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sSequence, sStatus, sDiscType, sProdNum);

                                                        taskMethods04.RemoveOrphanedExportDefFiles(dTblSitting);
                                                        taskMethods04.RemoveOrphanedExportDefFiles(dTblOrder);

                                                        string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                                                        taskMethods04.LogText(sText, mForm);
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

                                                insertsOrUpdates04.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sSequence, sDiscType, sSitting, bSittingBased);

                                                string sStatus = "90";

                                                insertsOrUpdates04.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sSequence, sStatus, sDiscType, sProdNum);

                                                taskMethods04.RemoveOrphanedExportDefFiles(dTblSitting);
                                                taskMethods04.RemoveOrphanedExportDefFiles(dTblOrder);

                                                string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                                                taskMethods04.LogText(sText, mForm);
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
                        sBreakPoint = string.Empty;

                        taskMethods04.RemoveOrphanedExportDefFiles(dTblSitting);
                        taskMethods04.RemoveOrphanedExportDefFiles(dTblOrder);

                        string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                        taskMethods04.LogText(sText, mForm);

                        bool bSuccess = true;
                        sCommText = "UPDATE [DiscOrders] SET [Status] = '10' WHERE [RefNum] = '" + sRefNum + "' AND [DiscType] = '" + sDiscType + "'";

                        dbConns04.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                        if (bSuccess == true)
                        {
                            bGoodResults = false;
                        }
                        else if (bSuccess != true)
                        {
                            sBreakPoint = string.Empty;
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
                    dataGatheringAndProcessing04.ExportDefProcessing(sProdNum, bGoodResults, dTblOrder, sDiscType, bSittingBased, dataSetMain, mForm);
                }
                else if (bGoodResults != true)
                {
                    dTblSitting.Clear();
                    dTblOrder.Clear();

                    // Go back into foreach, order with issue will be picked up next cycle or flagged as stalled if not.
                }
            }
            catch (Exception ex)
            {
                dbConns04.SaveExceptionToDB(ex);
            }
        }
    }
}
