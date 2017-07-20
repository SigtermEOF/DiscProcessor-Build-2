using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class ICDProcessing
    {
        // Common class suffix = 05
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns05 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing05 = new DataGatheringAndProcessing();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods05 = new TaskMethods();
        LockFile lockFile05 = new LockFile();
        InsertsOrUpdates insertsOrUpdates05 = new InsertsOrUpdates();

        public void ICDExportDefModifying(string sImagePath, string sRefNum, string sExportDefFile, string sProdNum, string sFrameNum, int iJobIDsNeeded, string sYearOn, string sNameOn, string sUniqueID, ref DataTable dTblJob, ref bool bCreated, ref int iRenderedCount, ref bool bIDsGathered, ref int iJobID, ref int iBatchID, ref int iLoops, int iCount, ref bool bInitialJobIDAssigned, string sCustNum, ref bool bGoodResults, string sGSBkGrnd, bool bStylesAndBGs, DataRow[] dRowGatheredFrameDataForCurrentFrame, string sDP2Mask, string sSitting, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                string sDiscType = "ICD";
                bool bSittingBased = false;

                string sSearchPattern01 = "Label = 'ICDRenderedPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                string sSearchPattern02 = "Label = 'ExportDefPath'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                string sSearchPattern03 = "Label = 'ExportDefPathDone'";
                DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0)
                {
                    string sICDRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                    string sExportDefPath = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                    string sExportDefPathDone = Convert.ToString(dRowGatheredPattern03[0]["Value"]).Trim();

                    if (!Directory.Exists(sExportDefPathDone))
                    {
                        Directory.CreateDirectory(sExportDefPathDone);
                    }

                    string sImageName = Path.GetFileName(sImagePath);
                    string sSavedExportDefPath = string.Empty;
                    string sRenderedImageLocation = string.Empty;
                    string sExportDefOriginal = sExportDefFile;
                    string sExportDefRenderFile = string.Empty;
                    bool bLockedFile = false;
                    int iLastJobID = 0;
                    bool bHaveFile = false;
                    bool bGatherIDsSuccess = true;

                    if (bIDsGathered == false)
                    {
                        lockFile05.LockTheFile(ref iBatchID, ref iJobID, ref iJobIDsNeeded, ref bLockedFile, ref bGatherIDsSuccess, dataSetMain);
                    }

                    iLastJobID = iJobID + iJobIDsNeeded;

                    if (bLockedFile != true && bGatherIDsSuccess == true)
                    {
                        bIDsGathered = true;

                        // Create a datatable to store the entire records worth of needed exportdef files which then is looped through and pushed into the dp2.jobqueue table.

                        if (bCreated == false)
                        {
                            dTblJob.Columns.Add("Count", typeof(int)); // iRenderedCount
                            dTblJob.Columns.Add("UniqueID", typeof(string)); // sUniqueID
                            dTblJob.Columns.Add("BatchID", typeof(int)); // iBatchID
                            dTblJob.Columns.Add("JobID", typeof(int)); // iJobID
                            dTblJob.Columns.Add("RefNum", typeof(string)); // sRefNum
                            dTblJob.Columns.Add("FrameNum", typeof(string)); // sFrameNum
                            dTblJob.Columns.Add("ExportDefFile", typeof(string)); // sExportDefFile
                            dTblJob.Columns.Add("SavedExportDefPath", typeof(string)); // sSavedExportDefPath                        
                            dTblJob.Columns.Add("RenderedImageLocation", typeof(string)); // sRenderedImageLocation
                            dTblJob.Columns.Add("NameOn", typeof(string)); // sNameOn
                            dTblJob.Columns.Add("YearOn", typeof(string)); // sYearOn
                            dTblJob.Columns.Add("ImageName", typeof(string)); // sImageName
                            dTblJob.Columns.Add("CustNum", typeof(string)); // sCustNum
                            dTblJob.Columns.Add("Sitting", typeof(string)); // sSitting

                            bCreated = true;
                        }

                        List<string> lLayouts = new List<string>();
                        lLayouts.Add("8x10wName");
                        lLayouts.Add("8x10NoName");
                        lLayouts.Add("5x7");
                        lLayouts.Add("Additional");
                        lLayouts.Add("Copyright");

                        foreach (string s in lLayouts)
                        {
                            if (s == "8x10wName")
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

                                // Some layouts need swapped due to artwork that is present with text but not with no text.
                                // I added these layouts to a table for easy real time handling.

                                DataTable dTblExportDefSwaps = new DataTable("dTblExportDefSwaps");
                                string sCommText = "SELECT * FROM [ExportDef Swaps] WHERE [WithText] = '1'";
                                bool bEDSwapFound = false;

                                dbConns05.SQLQuery(sDiscProcessorConnString, sCommText, dTblExportDefSwaps);

                                if (dTblExportDefSwaps.Rows.Count > 0)
                                {
                                    foreach (DataRow dRowExportDefSwaps in dTblExportDefSwaps.Rows)
                                    {
                                        string sOriginalED = Convert.ToString(dRowExportDefSwaps["OriginalExportDef"]).Trim();
                                        string sSwappedED = Convert.ToString(dRowExportDefSwaps["SwappedExportDef"]).Trim();

                                        if (sExportDefOriginal == sOriginalED)
                                        {
                                            sExportDefFile = sSwappedED;

                                            bEDSwapFound = true;

                                            bHaveFile = false;
                                            taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);
                                        }
                                    }

                                    if (bEDSwapFound != true)
                                    {
                                        sExportDefFile = sExportDefOriginal;

                                        bHaveFile = false;
                                        taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);
                                    }
                                }
                                else if (dTblExportDefSwaps.Rows.Count == 0)
                                {
                                    sBreakPoint = string.Empty;
                                }

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;

                                    sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-8x10wname.JPG";
                                    sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-8x10wname.txt";

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    if (text.Contains("APSBGID"))
                                    {
                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                    }
                                    if (text.Contains("APSTEXT"))
                                    {
                                        text = text.Replace("APSTEXT", sNameOn);
                                    }
                                    if (text.Contains("APSYEAR"))
                                    {
                                        text = text.Replace("APSYEAR", sYearOn);
                                    }
                                    if (text.Contains("APSCROP"))
                                    {
                                        string sCrop = string.Empty;

                                        taskMethods05.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sExportDefRenderFile, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "8x10NoName")
                            {
                                // Some layouts need swapped due to artwork that is present with text but not with no text.
                                // I added these layouts to a table for easy real time handling.

                                DataTable dTblExportDefSwaps = new DataTable("dTblExportDefSwaps");
                                string sCommText = "SELECT * FROM [ExportDef Swaps] WHERE [WithText] = '0'";
                                bool bEDSwapFound = false;

                                dbConns05.SQLQuery(sDiscProcessorConnString, sCommText, dTblExportDefSwaps);

                                if (dTblExportDefSwaps.Rows.Count > 0)
                                {
                                    foreach (DataRow dRowExportDefSwaps in dTblExportDefSwaps.Rows)
                                    {
                                        string sOriginalED = Convert.ToString(dRowExportDefSwaps["OriginalExportDef"]).Trim();
                                        string sSwappedED = Convert.ToString(dRowExportDefSwaps["SwappedExportDef"]).Trim();

                                        if (sExportDefOriginal == sOriginalED)
                                        {
                                            sExportDefFile = sSwappedED;

                                            bEDSwapFound = true;

                                            bHaveFile = false;
                                            taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);
                                        }
                                    }

                                    if (bEDSwapFound != true)
                                    {
                                        sExportDefFile = sExportDefOriginal;

                                        bHaveFile = false;
                                        taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);
                                    }
                                }
                                else if (dTblExportDefSwaps.Rows.Count == 0)
                                {
                                    sBreakPoint = string.Empty;
                                }

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-8x10noname.JPG";
                                    sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-8x10noname.txt";

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    if (text.Contains("APSBGID"))
                                    {
                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                    }
                                    if (text.Contains("APSTEXT"))
                                    {
                                        text = text.Replace("APSTEXT", "");
                                    }
                                    if (text.Contains("APSYEAR"))
                                    {
                                        text = text.Replace("APSYEAR", "");
                                    }
                                    if (text.Contains("APSCROP"))
                                    {
                                        string sCrop = string.Empty;

                                        taskMethods05.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sExportDefRenderFile, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "5x7")
                            {
                                sExportDefFile = "04" + sExportDefOriginal;

                                bHaveFile = false;
                                taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-5x7.JPG";
                                    sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-5x7.txt";

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    if (text.Contains("APSBGID"))
                                    {
                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                    }
                                    if (text.Contains("APSTEXT"))
                                    {
                                        text = text.Replace("APSTEXT", sNameOn);
                                    }
                                    if (text.Contains("APSYEAR"))
                                    {
                                        text = text.Replace("APSYEAR", sYearOn);
                                    }
                                    if (text.Contains("APSCROP"))
                                    {
                                        string sCrop = string.Empty;

                                        taskMethods05.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sExportDefRenderFile, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "Additional" && bStylesAndBGs == true)
                            {
                                if (iLoops == iCount)
                                {
                                    string sPreviousGSBkGrnd01 = string.Empty;
                                    string sPreviousGSBkGrnd02 = string.Empty;

                                    foreach (DataRow dRow in dRowGatheredFrameDataForCurrentFrame)
                                    {
                                        sGSBkGrnd = Convert.ToString(dRow["GSBackground"]).Trim(); 

                                        if (sGSBkGrnd.Length > 0)
                                        {
                                            if (sGSBkGrnd != sPreviousGSBkGrnd01)
                                            {
                                                sPreviousGSBkGrnd01 = sGSBkGrnd;

                                                sExportDefFile = "COLOR.txt";

                                                bHaveFile = false;
                                                taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                                if (bHaveFile == true)
                                                {
                                                    iRenderedCount += +1;
                                                    iJobID += +1;

                                                    sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-Color8x10.JPG";
                                                    sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-Color8x10.txt";

                                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                                    text = text.Replace("APSPATH", sImagePath);
                                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                                    if (text.Contains("APSBGID"))
                                                    {
                                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                                    }

                                                    File.WriteAllText(sExportDefRenderFile, text);

                                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                                }
                                                else if (bHaveFile != true)
                                                {
                                                    bGoodResults = false;

                                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                                    insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    string sStatus = "90";

                                                    insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (sGSBkGrnd == sPreviousGSBkGrnd01)
                                            {

                                            }
                                        }
                                        else if (sGSBkGrnd.Length == 0)
                                        {
                                            sBreakPoint = string.Empty;
                                        }

                                        sGSBkGrnd = Convert.ToString(dRow["GSBackground"]).Trim();

                                        if (sGSBkGrnd.Length > 0)
                                        {
                                            if (sGSBkGrnd != sPreviousGSBkGrnd02)
                                            {
                                                sPreviousGSBkGrnd02 = sGSBkGrnd;

                                                sExportDefFile = "BW.TXT";

                                                bHaveFile = false;
                                                taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                                if (bHaveFile == true)
                                                {
                                                    iRenderedCount += +1;
                                                    iJobID += +1;

                                                    sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-BW8x10.JPG";
                                                    sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-BW8x10.txt";

                                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                                    text = text.Replace("APSPATH", sImagePath);
                                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                                    if (text.Contains("APSBGID"))
                                                    {
                                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                                    }

                                                    File.WriteAllText(sExportDefRenderFile, text);

                                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                                }
                                                else if (bHaveFile != true)
                                                {
                                                    bGoodResults = false;

                                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                                    insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                                    string sStatus = "90";

                                                    insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                                }
                                            }
                                            else if (sGSBkGrnd == sPreviousGSBkGrnd02)
                                            {

                                            }
                                        }
                                        else if (sGSBkGrnd.Length == 0)
                                        {
                                            sBreakPoint = string.Empty;
                                        }
                                    }
                                }
                                else if (iLoops != iCount)
                                {

                                }
                            }
                            else if (s == "Additional" && bStylesAndBGs != true)
                            {
                                if (iLoops == iCount)
                                {
                                    sExportDefFile = "COLOR.txt";

                                    bHaveFile = false;
                                    taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                    if (bHaveFile == true)
                                    {
                                        iRenderedCount += +1;
                                        iJobID += +1;

                                        sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-Color8x10.JPG";
                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-Color8x10.txt";

                                        string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                        text = text.Replace("APSPATH", sImagePath);
                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                        File.WriteAllText(sExportDefRenderFile, text);

                                        dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                    }
                                    else if (bHaveFile != true)
                                    {
                                        bGoodResults = false;

                                        string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                        insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                        string sStatus = "90";

                                        insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }

                                    sExportDefFile = "BW.TXT";

                                    bHaveFile = false;
                                    taskMethods05.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                    if (bHaveFile == true)
                                    {
                                        iRenderedCount += +1;
                                        iJobID += +1;

                                        sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-BW8x10.JPG";
                                        sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-BW8x10.txt";

                                        string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                        text = text.Replace("APSPATH", sImagePath);
                                        text = text.Replace("APSDEST", sRenderedImageLocation);

                                        File.WriteAllText(sExportDefRenderFile, text);

                                        dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                    }
                                    else if (bHaveFile != true)
                                    {
                                        bGoodResults = false;

                                        string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                        insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                        string sStatus = "90";

                                        insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }
                                }
                                else if (iLoops != iCount)
                                {

                                }
                            }
                            else if (s == "Copyright" && (iLoops == iCount))
                            {
                                if (sCustNum == "58241")
                                {
                                    dataGatheringAndProcessing05.GatherOriginalCustNumFromIMQOrder(sProdNum, ref sCustNum);
                                }

                                iRenderedCount += +1;
                                iJobID += +1;

                                sRenderedImageLocation = sICDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-Copyright.JPG";
                                sExportDefRenderFile = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-Copyright.txt";

                                string[] sFiles = Directory.GetFiles(sExportDefPath);
                                string sFilePath = string.Empty;
                                bool bCopyrightFound = false;

                                foreach (string sFile in sFiles)
                                {
                                    sFilePath = Path.GetFileName(sFile);

                                    if (sFilePath == sCustNum + " Copyright.txt")
                                    {
                                        sExportDefFile = sFilePath;
                                        bCopyrightFound = true;
                                    }
                                    else if (sFilePath == sCustNum + " Copyright.TXT")
                                    {
                                        sExportDefFile = sFilePath;
                                        bCopyrightFound = true;
                                    }
                                }
                                if (bCopyrightFound == true)
                                {
                                    bGoodResults = true;

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    File.WriteAllText(sExportDefRenderFile, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sExportDefRenderFile, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bCopyrightFound != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No copyright located. ]";

                                    insertsOrUpdates05.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates05.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);

                                    taskMethods05.RemoveOrphanedExportDefFiles(dTblJob);

                                    string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                                    taskMethods05.LogText(sText, mForm);
                                }
                            }
                        }

                        // End of foreach.
                    }
                    else if (bLockedFile == true || bGatherIDsSuccess != true)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0 || dRowGatheredPattern03.Length == 0)
                {
                    sBreakPoint = string.Empty;
                }                
            }
            catch (Exception ex)
            {
                dbConns05.SaveExceptionToDB(ex);
                bGoodResults = false;
            }
        }
    }
}
