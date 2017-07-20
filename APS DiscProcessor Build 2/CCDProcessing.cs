using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class CCDProcessing
    {
        // Common class suffix = 07
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns07 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing07 = new DataGatheringAndProcessing();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods07 = new TaskMethods();
        LockFile lockFile07 = new LockFile();
        InsertsOrUpdates insertsOrUpdates07 = new InsertsOrUpdates();

        public void CCDExportDefModifying(string sImagePath, string sRefNum, string sExportDefFile, string sProdNum, string sFrameNum, int iJobIDsNeeded, string sYearOn, string sNameOn, string sUniqueID, ref DataTable dTblJob, ref bool bCreated, ref int iRenderedCount, ref bool bIDsGathered, ref int iJobID, ref int iBatchID, ref int iLoops, int iCount, ref bool bInitialJobIDAssigned, string sCustNum, ref bool bGoodResults, ref bool bInitialPass01, string sGSBkGrnd, DataRow[] dRowGatheredFrameDataForCurrentFrame, string sDP2Mask, string sSitting, DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                string sDiscType = "CCD";
                bool bSittingBased = false;

                string sSearchPattern01 = "Label = 'CCDRenderedPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                string sSearchPattern02 = "Label = 'ExportDefPath'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                string sSearchPattern03 = "Label = 'ExportDefPathDone'";
                DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0)
                {
                    string sCCDRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
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
                    bool bLockedFile = false;
                    int iLastJobID = 0;
                    bool bHaveFile = false;
                    bool bGatherIDsSuccess = false;

                    if (bInitialPass01 == true)
                    {
                        //string sText = "[Generating ExportDef files for reference number " + sRefNum + " and frame number " + sFrameNum + ".]";
                        //mForm.LogText(sText);

                        bInitialPass01 = false;
                    }

                    if (bIDsGathered == false)
                    {
                        lockFile07.LockTheFile(ref iBatchID, ref iJobID, ref iJobIDsNeeded, ref bLockedFile, ref bGatherIDsSuccess, dataSetMain);
                    }
                    else if (bIDsGathered != false)
                    {

                    }

                    iLastJobID = iJobID + iJobIDsNeeded;

                    if (bLockedFile != true && bGatherIDsSuccess == true)
                    {
                        bIDsGathered = true;

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
                        else if (bCreated != false)
                        {

                        }

                        List<string> lLayouts = new List<string>();
                        lLayouts.Add("8x10wName");
                        lLayouts.Add("8x10NoName");
                        lLayouts.Add("5x7");
                        lLayouts.Add("Color8x10");
                        lLayouts.Add("BW8x10");
                        lLayouts.Add("Calendar");
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

                                sExportDefFile = "4UPW.txt";

                                bHaveFile = false;
                                taskMethods07.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;

                                    sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-8x10wname.JPG";
                                    sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-8x10wname.txt";

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

                                        taskMethods07.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "8x10NoName")
                            {
                                sExportDefFile = "4UPW.txt";

                                bHaveFile = false;
                                taskMethods07.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-8x10noname.JPG";
                                    sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-8x10noname.txt";

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    if (text.Contains("APSBGID"))
                                    {
                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                    }
                                    if (text.Contains("APSTEXT"))
                                    {
                                        text = text.Replace("APSTEXT", string.Empty);
                                    }
                                    if (text.Contains("APSYEAR"))
                                    {
                                        text = text.Replace("APSYEAR", string.Empty);
                                    }
                                    if (text.Contains("APSCROP"))
                                    {
                                        string sCrop = string.Empty;

                                        taskMethods07.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "5x7")
                            {
                                sExportDefFile = "044UPW.txt";

                                bHaveFile = false;
                                taskMethods07.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-5x7.JPG";
                                    sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-5x7.txt";

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

                                        taskMethods07.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "Color8x10")
                            {
                                sExportDefFile = "COLOR.txt";

                                bHaveFile = false;
                                taskMethods07.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-Color8x10.JPG";
                                    sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-Color8x10.txt";

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    if (text.Contains("APSBGID"))
                                    {
                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                    }
                                    if (text.Contains("APSTEXT"))
                                    {
                                        text = text.Replace("APSTEXT", string.Empty);
                                    }
                                    if (text.Contains("APSYEAR"))
                                    {
                                        text = text.Replace("APSYEAR", string.Empty);
                                    }
                                    if (text.Contains("APSCROP"))
                                    {
                                        string sCrop = string.Empty;

                                        taskMethods07.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "BW8x10")
                            {
                                sExportDefFile = "BW.txt";

                                bHaveFile = false;
                                taskMethods07.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-BW8x10.JPG";
                                    sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-BW8x10.txt";

                                    string text = File.ReadAllText(sExportDefPath + sExportDefFile);

                                    text = text.Replace("APSPATH", sImagePath);
                                    text = text.Replace("APSDEST", sRenderedImageLocation);

                                    if (text.Contains("APSBGID"))
                                    {
                                        text = text.Replace("APSBGID", sGSBkGrnd);
                                    }
                                    if (text.Contains("APSTEXT"))
                                    {
                                        text = text.Replace("APSTEXT", string.Empty);
                                    }
                                    if (text.Contains("APSYEAR"))
                                    {
                                        text = text.Replace("APSYEAR", string.Empty);
                                    }
                                    if (text.Contains("APSCROP"))
                                    {
                                        string sCrop = string.Empty;

                                        taskMethods07.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "Calendar")
                            {
                                sExportDefFile = "CD_GC_Calendar.txt";

                                bHaveFile = false;
                                taskMethods07.CheckForExportFileExistence(ref sExportDefFile, ref bHaveFile, sExportDefPath);

                                if (bHaveFile == true)
                                {
                                    iRenderedCount += +1;
                                    iJobID += +1;

                                    sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-Calendar.JPG";
                                    sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-Calendar.txt";

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

                                        taskMethods07.GetCrop(ref sCrop, sUniqueID, sProdNum, sFrameNum, sRefNum);

                                        text = text.Replace("APSCROP", sCrop);
                                    }

                                    iLoops += +1;

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bHaveFile != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No ExportDef file located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                }
                            }
                            else if (s == "Copyright")
                            {
                                if (sCustNum == "58241")
                                {
                                    dataGatheringAndProcessing07.GatherOriginalCustNumFromIMQOrder(sProdNum, ref sCustNum);
                                }

                                iRenderedCount += +1;
                                iJobID += +1;

                                sRenderedImageLocation = sCCDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + "-Copyright.JPG";
                                sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + "-Copyright.txt";

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

                                    File.WriteAllText(sSavedExportDefPath, text);

                                    dTblJob.Rows.Add(iRenderedCount, sUniqueID, iBatchID, iJobID, sRefNum, sFrameNum, sExportDefFile, sSavedExportDefPath, sRenderedImageLocation, sNameOn, sYearOn, sImageName, sCustNum, sSitting);
                                }
                                else if (bCopyrightFound != true)
                                {
                                    bGoodResults = false;

                                    string sErrorDescription = "[ " + DateTime.Now.ToString() + " ][ No copyright located. ]";

                                    insertsOrUpdates07.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                                    string sStatus = "90";

                                    insertsOrUpdates07.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);

                                    taskMethods07.RemoveOrphanedExportDefFiles(dTblJob);

                                    string sText = "[Removed orphaned exportdef files for  " + sRefNum + ".]";
                                    taskMethods07.LogText(sText, mForm);
                                }
                            }
                        }

                        // End of foreach.
                        sBreakPoint = string.Empty;
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
                dbConns07.SaveExceptionToDB(ex);
                bGoodResults = false;
            }
        }
    }
}
