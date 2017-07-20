using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace APS_DiscProcessor_Build_2
{
    class TaskMethods
    {
        // Common class suffix = 16
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns16 = new DBConnections();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        InsertsOrUpdates insertsOrUpdates16 = new InsertsOrUpdates();

        public long DirSize(DirectoryInfo dirInfo, ref long lSize)
        {
            try
            {
                FileInfo[] fis = dirInfo.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    lSize += fi.Length;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }

            return lSize;
        }

        public void Clear(Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                string sLogText = string.Empty;
                mForm.rtxtboxLog.Clear();
                mForm.rtxtboxLog.Refresh();
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }

        public void LogText(string sText, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                string sDTime1 = DateTime.Now.ToString("MM-dd-yy").Trim();
                string sDTime2 = DateTime.Now.ToString("HH:mm:ss").Trim();
                string sDTime3 = "[" + sDTime1 + "][" + sDTime2 + "]";
                string sLogText = sDTime3 + sText;
                mForm.rtxtboxLog.AppendText(sLogText + Environment.NewLine);
                mForm.Refresh();
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }

        public bool IsDigitsOnly(string sString, ref bool bDigitOnly)
        {
            foreach (char c in sString)
            {
                if (c < '0' || c > '9')
                {
                    bDigitOnly = false;
                }
                else
                {
                    bDigitOnly = true;
                }
            }
            return bDigitOnly;
        }

        public void CleanDPTables(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                int iRemovedRecords = 0;

                DataTable dTblDiscOrdersDistinctProdNums = new DataTable("dTblDiscOrdersDistinctProdNums");
                string sCommText = "SELECT DISTINCT [ProdNum] FROM [DiscOrders]";

                dbConns16.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrdersDistinctProdNums);

                if (dTblDiscOrdersDistinctProdNums.Rows.Count > 0)
                {
                    int iDiscOrdersDistinctProdNums = dTblDiscOrdersDistinctProdNums.Rows.Count;

                    string sText = " " + iDiscOrdersDistinctProdNums + " records currently exist in the DiscOrders table.";
                    this.LogText(sText, mForm);

                    foreach (DataRow dRowDiscOrdersDistinctProdNums in dTblDiscOrdersDistinctProdNums.Rows)
                    {
                        string sProdNum = Convert.ToString(dRowDiscOrdersDistinctProdNums["ProdNum"]);

                        DataTable dTblItems = new DataTable("dTblItems");
                        sCommText = "SELECT * FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                        dbConns16.CDSQuery(sCDSConnString, sCommText, dTblItems);

                        if (dTblItems.Rows.Count > 0)
                        {
                            continue;
                        }
                        else if (dTblItems.Rows.Count == 0)
                        {
                            // Delete records from the DiscOrders table as well as the FrameData table if frame based order.

                            sText = "Production number " + sProdNum + " no longer exists in the Items table.";
                            this.LogText(sText, mForm);

                            DataTable dTblFrameData = new DataTable("dTblFrameData");
                            sCommText = "SELECT * FROM [FrameData] WHERE [ProdNum] = '" + sProdNum + "'";

                            dbConns16.SQLQuery(sDiscProcessorConnString, sCommText, dTblFrameData);

                            if (dTblFrameData.Rows.Count > 0)
                            {
                                // Delete FrameData record prior to deleting DiscOrders records.

                                sText = "Deleting FrameData records for production number " + sProdNum + " .";
                                this.LogText(sText, mForm);

                                sCommText = "DELETE FROM [FrameData] WHERE [ProdNum] = '" + sProdNum + "'";
                                bool bFrameDataDeleteSuccess = false;
                                dbConns16.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bFrameDataDeleteSuccess);

                                if (bFrameDataDeleteSuccess == true)
                                {
                                    // Delete DiscOrders records.

                                    sText = "Deleting DiscOrders records for production number " + sProdNum + " .";
                                    this.LogText(sText, mForm);

                                    sCommText = "DELETE FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "'";
                                    bool bDiscOrdersDeleteSuccess = false;
                                    dbConns16.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bDiscOrdersDeleteSuccess);

                                    if (bDiscOrdersDeleteSuccess == true)
                                    {
                                        iRemovedRecords += 1;
                                        continue;
                                    }
                                    else if (bDiscOrdersDeleteSuccess != true)
                                    {
                                        iRemovedRecords -= 1;
                                        sBreakPoint = string.Empty;
                                    }
                                }
                                else if (bFrameDataDeleteSuccess != true)
                                {
                                    sBreakPoint = string.Empty;
                                }
                            }
                            else if (dTblFrameData.Rows.Count == 0)
                            {
                                // Delete DiscOrders records.

                                sText = "Deleting DiscOrders records for production number " + sProdNum + " .";
                                this.LogText(sText, mForm);

                                sCommText = "DELETE FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "'";
                                bool bDiscOrdersDeleteSuccess = false;
                                dbConns16.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bDiscOrdersDeleteSuccess);

                                if (bDiscOrdersDeleteSuccess == true)
                                {
                                    iRemovedRecords += 1;
                                    continue;
                                }
                                else if (bDiscOrdersDeleteSuccess != true)
                                {
                                    iRemovedRecords -= 1;
                                    sBreakPoint = string.Empty;
                                }
                            }
                        }
                    }

                    // End of foreach.
                    sText = "Cleaning of tables complete.";
                    this.LogText(sText, mForm);
                    int iCurrentCount = iDiscOrdersDistinctProdNums - iRemovedRecords;
                    sText = "Distinct DiscOrders records prior to cleaning: " + iDiscOrdersDistinctProdNums + ".";
                    this.LogText(sText, mForm);
                    sText = "DiscOrders records removed: " + iRemovedRecords + ".";
                    this.LogText(sText, mForm);
                    sText = "Current distinct DiscOrders records: " + iCurrentCount + ".";
                    this.LogText(sText, mForm);
                }
                else if (dTblDiscOrdersDistinctProdNums.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }

        public bool ImageExists(string sPath, ref bool bExists)
        {
            try
            {
                if (File.Exists(sPath))
                {
                    bExists = true;
                }
                else if (!File.Exists(sPath))
                {
                    bExists = false;
                }
            }
            catch (Exception ex)
            {
                bExists = false;
                dbConns16.SaveExceptionToDB(ex);
            }
            return bExists;
        }

        public bool DeleteRenderedDirectoryAndFiles(string sProdNum, string sFrameNum, ref bool bDeleted, string sDiscType, ref bool bDirExists, DataSet dataSetMain, string sSitting, string sRefNum)
        {
            try
            {
                string sSearchPattern01 = "Label = '" + sDiscType + "RenderedPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                string sSearchPattern02 = "GatherDiscType = '" + sDiscType + "'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern02);

                if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0)
                {
                    string sRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                    bool bSittingBased = Convert.ToBoolean(dRowGatheredPattern02[0]["SittingBased"]);

                    if (bSittingBased != true)
                    {
                        if (sDiscType != "PCD" && sDiscType != "RCD")
                        {
                            sRenderedPath += sProdNum + sFrameNum;
                        }
                        else if (sDiscType == "PCD" || sDiscType == "RCD")
                        {
                            sRenderedPath += sRefNum;
                        }
                    }
                    else if (bSittingBased == true)
                    {
                        sRenderedPath += sRefNum + @"\" + sProdNum + @"\" + sSitting;
                    }

                    if (Directory.Exists(sRenderedPath))
                    {
                        bDirExists = true;

                        string[] sFiles = Directory.GetFiles(sRenderedPath, "*.*", SearchOption.AllDirectories);
                        string[] sDirs = Directory.GetDirectories(sRenderedPath);

                        foreach (string s in sFiles)
                        {
                            File.SetAttributes(s, FileAttributes.Normal);
                            File.Delete(s);
                        }

                        foreach (string s in sDirs)
                        {
                            Directory.Delete(s);
                        }

                        Directory.Delete(sRenderedPath, true);

                        bDeleted = true;
                    }
                    else if (!Directory.Exists(sRenderedPath))
                    {
                        bDirExists = false;
                    }
                }
                else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
                bDeleted = false;
            }
            return bDeleted;
        }

        public void GenerateCopyrightRelease(string sRenderedPath, ref bool bCRGenerated, DataSet dataSetMain)
        {
            try
            {
                string sCopyrightRelease = "Copyright Release.txt";
                string sCRPath = sRenderedPath + sCopyrightRelease;

                int iHaveCopyright = Directory.GetFiles(sRenderedPath, "Copyright Release.txt", SearchOption.TopDirectoryOnly).Length;

                if (iHaveCopyright == 0)
                {
                    string sSearchPattern01 = "Label = 'CopyrightRelease'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                    if (dRowGatheredPattern01.Length > 0)
                    {
                        string sCopyright = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                        File.WriteAllText(sCRPath, sCopyright);

                        bCRGenerated = true;
                    }
                    else if (dRowGatheredPattern01.Length == 0)
                    {
                        bCRGenerated = false;
                    }
                }
                else if (iHaveCopyright != 0)
                {
                    bCRGenerated = true;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
                bCRGenerated = false;
            }
        }

        public void SetUpPreInsertionDataPrepTable(DataTable dTblPreInsertionDataPrep, ref bool bPreInsertionDataPrepSetUp)
        {
            try
            {
                dTblPreInsertionDataPrep.Columns.Add("ProdNum", typeof(string));
                dTblPreInsertionDataPrep.Columns.Add("RefNum", typeof(string));
                dTblPreInsertionDataPrep.Columns.Add("DiscType", typeof(string));
                dTblPreInsertionDataPrep.Columns.Add("PackageTag", typeof(string));

                bPreInsertionDataPrepSetUp = true;

            }
            catch (Exception ex)
            {
                bPreInsertionDataPrepSetUp = false;

                dbConns16.SaveExceptionToDB(ex);
            }
        }        

        public void StylesAndBackgroundCount(ref int iIDsNeeded, bool bMultiRenderGS, DataTable dTbl01, DataTable dTbl02, string sDiscType)
        {
            // if bMultiRenderGS = true
            // 3 renders per style (8x10 w/ name, 8x10 w/out name and a 5x7)
            // 14 gs bgs
            // 2 renders per gs bg (color 8x10 and a b&w 8x10)
            // 1 rendered copyright
            // (14 styles * 3) + (14 gs bgs * 2) + 1 = 71

            // if bMultiRenderGS = false
            // 14 styles
            // 3 renders per style (8x10 w/ name, 8x10 w/out name and a 5x7)
            // 3 renders per disc (color 8x10, b&w 8x10 and a copyright)
            // (14 styles * 3) + 3 = 45

            try
            {
                int iRowsCount01 = dTbl01.Rows.Count;
                int iRowsCount02 = dTbl02.Rows.Count;

                if (sDiscType == "ICD" || sDiscType == "ICDW")
                {
                    if (bMultiRenderGS == false)
                    {
                        if (iRowsCount01 != 0 && iRowsCount02 != 0)
                        {
                            iIDsNeeded = ((iRowsCount01 * iRowsCount02) * 3) + 3;
                        }
                        else if (iRowsCount01 == 0 && iRowsCount02 != 0)
                        {
                            iIDsNeeded = (iRowsCount02 * 3) + 3;
                        }
                        else if (iRowsCount01 != 0 && iRowsCount02 == 0)
                        {
                            iIDsNeeded = (iRowsCount01 * 3) + 3;
                        }

                    }
                    else if (bMultiRenderGS != false)
                    {
                        DataTable dTblAltData = new DataTable("dTblAltData");
                        dTblAltData.Columns.Add("Alt_Data", typeof(string));

                        string sPreviousAltData = string.Empty;

                        foreach (DataRow dRow01 in dTbl01.Rows)
                        {
                            string sAltData = Convert.ToString(dRow01["Alt_data"]).Trim();

                            if (sAltData.Length > 0)
                            {
                                if (sAltData != sPreviousAltData) // This is to prevent the same green screen background getting a color/b&w 8x10 render.
                                {
                                    sPreviousAltData = sAltData;

                                    dTblAltData.Rows.Add(sAltData);
                                }
                            }
                        }

                        int iRowsCount03 = dTblAltData.Rows.Count;

                        iIDsNeeded = (iRowsCount01 * 3) + (iRowsCount03 * 2) + 1;
                    }
                }
                else if (sDiscType == "MEG" || sDiscType == "MEGW")
                {
                    //Meg discs need:
                    //(Styles * 3 renders) + (20 additional renders + 12 renders for calendar + 1 copyright rendered).
                    if (iRowsCount01 != 0 && iRowsCount02 != 0)
                    {
                        iIDsNeeded = ((iRowsCount01 * iRowsCount02) * 3) + (20 + 12 + 1);
                    }
                    else if (iRowsCount01 != 0 && iRowsCount02 == 0)
                    {
                        iIDsNeeded = (iRowsCount01 * 3) + (20 + 12 + 1);
                    }
                    else if (iRowsCount01 == 0 && iRowsCount02 != 0)
                    {
                        iIDsNeeded = (iRowsCount02 * 3) + (20 + 12 + 1);
                    }
                }
                else if (sDiscType == "CCD")
                {
                    //CCD discs need:
                    // 7 renders (1-8x10 w/ name, 1-8x10 w/out name, 1-5x7 w/ name, 1-color 8x10, 1-b&w 8x10, 1-calendar, 1-copyright)
                    iIDsNeeded = 7;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }

        public void CheckForExportFileExistence(ref string sExportDefFile, ref bool bHaveFile, string sExportDefPath)
        {
            try
            {
                string sExportDefWithoutExtension = Path.GetFileNameWithoutExtension(sExportDefFile);

                string[] sFiles = Directory.GetFiles(sExportDefPath);

                foreach (string s in sFiles)
                {
                    string sFile = Path.GetFileNameWithoutExtension(s);
                    string sExtension = Path.GetExtension(s);

                    if (sExportDefWithoutExtension == sFile)
                    {
                        bHaveFile = true;
                        sExportDefFile = sExportDefWithoutExtension += sExtension;
                        break;
                    }
                    else if (sExportDefWithoutExtension != sFile)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                bHaveFile = false;

                dbConns16.SaveExceptionToDB(ex);                
            }
        }

        public bool VerifyGS(string sGSBG, ref bool bGSFound)
        {
            try
            {
                if (sGSBG == "brightgreen")
                {
                    sGSBG = "BRIGHTGREE";
                }

                DataTable dTblGsbkgrd = new DataTable("dTblGsbkgrd");
                string sCommText = "SELECT * FROM Gsbkgrd WHERE Gs_bkgrd = '" + sGSBG + "'";

                dbConns16.CDSQuery(sCDSConnString, sCommText, dTblGsbkgrd);

                if (dTblGsbkgrd.Rows.Count > 0)
                {
                    bGSFound = true;
                }
                else if (dTblGsbkgrd.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                    bGSFound = false;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
                bGSFound = false;
            }

            return bGSFound;
        }

        public void CheckForImages(string sRefNum, string sFrameNum, string sDiscType, ref bool bSuccess, bool bSittingBased, DataTable dTblDiscOrders, DataTable dTblFrames, string sSitting, string sProdNum)
        {
            try
            {
                string sSequence = sFrameNum.TrimStart('0');

                string sSearchPattern01 = "FrameNum = '" + sFrameNum + "'";
                DataRow[] dRowGatheredPattern01 = dTblDiscOrders.Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    string sImageLocation = Path.GetDirectoryName(Convert.ToString(dRowGatheredPattern01[0]["ImageLocation"])).Trim();

                    if (Directory.Exists(sImageLocation))
                    {
                        string sSearchPattern02 = "Sequence = '" + sSequence + "'";
                        DataRow[] dRowGatheredPattern02 = dTblFrames.Select(sSearchPattern02);

                        if (dRowGatheredPattern02.Length > 0)
                        {
                            string sImageID = Convert.ToString(dRowGatheredPattern02[0]["Image_id"]).Trim();

                            List<string> lImageTypes = new List<string>();
                            lImageTypes.Add("*.jpg");
                            lImageTypes.Add("*.png");

                            foreach (string s in lImageTypes)
                            {
                                string[] sFiles = Directory.GetFiles(sImageLocation, s);

                                foreach (string file in sFiles)
                                {
                                    string sFile = Path.GetFileName(Convert.ToString(file));
                                    string sImagelowercase = sImageID.ToLower().Trim();

                                    if (sFile == sImageID || sFile == sImagelowercase)
                                    {
                                        bSuccess = true;
                                        break;
                                    }
                                    else if (sFile != sImageID || sFile != sImagelowercase)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        else if (dRowGatheredPattern02.Length == 0)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                    else if (!Directory.Exists(sImageLocation))
                    {
                        string sErrorDescription = "[" + DateTime.Now.ToString() + " ][ Image directory does not exist.]";

                        insertsOrUpdates16.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                        string sStatus = "90";

                        if (bSittingBased != true)
                        {
                            insertsOrUpdates16.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                        }
                        else if (bSittingBased == true)
                        {
                            insertsOrUpdates16.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
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
                dbConns16.SaveExceptionToDB(ex);
            }
        }

        public void GetCrop(ref string sCrop, string sUniqueID, string sProdNum, string sFrameNum, string sRefNum)
        {
            try
            {
                string sDP2Bord = string.Empty;
                string sZone = string.Empty;
                sFrameNum = sFrameNum.TrimStart('0');

                DataTable dTblFrameData = new DataTable("dTblFrameData");
                string sCommText = "SELECT [DP2Bord] FROM [FrameData] WHERE [UniqueID] = '" + sUniqueID + "'";

                dbConns16.SQLQuery(sDiscProcessorConnString, sCommText, dTblFrameData);

                if (dTblFrameData.Rows.Count > 0)
                {
                    sDP2Bord = Convert.ToString(dTblFrameData.Rows[0]["DP2Bord"]).Trim();

                    // SELECT * FROM cds.dp2crop WHERE DP2Bord = sDP2Bord
                    // if no returned results then sCrop = "50 50 50 50 100 100"

                    // if sCrop = string.empty
                    // select * from cds.dp2image where lookupnum = sProdNum and frame = sFrameNum
                    // if results then sZone = dp2image.zone
                    // sCrop = "50 50 50 50 100 100"

                    // if sZone = string.empty
                    // select * from cds.items where lookupnum = sProdNum
                    // if results then sZone = items.special

                    // if sZone != string.empty
                    // select * from cds.dp2crop where dp2crop.DP2Bord = sDP2Bord and dp2crop.zone = sZone
                    // if results then sCrop = dp2crop.cropovr

                    DataTable dTblDp2crop = new DataTable("dTblDp2crop");
                    sCommText = "SELECT * FROM DP2Crop WHERE DP2Bord = '" + sDP2Bord + "'";

                    dbConns16.CDSQuery(sCDSConnString, sCommText, dTblDp2crop);

                    if (dTblDp2crop.Rows.Count > 0)
                    {
                        if (sZone.Length == 0)
                        {
                            DataTable dTblDp2image = new DataTable("dTblDp2image");
                            sCommText = "SELECT * FROM DP2Image WHERE Lookupnum = '" + sProdNum + "' AND Frame = " + sFrameNum + "";

                            dbConns16.CDSQuery(sCDSConnString, sCommText, dTblDp2image);

                            if (dTblDp2image.Rows.Count > 0)
                            {
                                sZone = Convert.ToString(dTblDp2image.Rows[0]["Zone"]).Trim();

                                if (sZone.Length == 0)
                                {
                                    DataTable dTblItems = new DataTable("dTblItems");
                                    sCommText = "SELECT * FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                                    dbConns16.CDSQuery(sCDSConnString, sCommText, dTblItems);

                                    if (dTblItems.Rows.Count > 0)
                                    {
                                        sZone = Convert.ToString(dTblItems.Rows[0]["Special"]).Trim();
                                    }
                                    else if (dTblItems.Rows.Count == 0)
                                    {
                                        sBreakPoint = string.Empty;
                                    }
                                }
                                else if (sZone.Length > 0)
                                {

                                }
                            }
                            else if (dTblDp2image.Rows.Count == 0)
                            {
                                sCrop = "50 50 50 50 100 100";
                            }
                        }
                        else if (sZone.Length == 0)
                        {

                        }
                    }
                    else if (dTblDp2crop.Rows.Count == 0)
                    {
                        sCrop = "50 50 50 50 100 100";
                    }
                }
                else if (dTblFrameData.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                }

                if (sZone.Length > 0)
                {
                    DataTable dTblDp2crop = new DataTable();
                    sCommText = "SELECT Cropovr FROM DP2Crop WHERE DP2Bord = '" + sDP2Bord + "' AND Zone = '" + sZone + "'";

                    dbConns16.CDSQuery(sCDSConnString, sCommText, dTblDp2crop);

                    if (dTblDp2crop.Rows.Count > 0)
                    {
                        sCrop = Convert.ToString(dTblDp2crop.Rows[0]["Cropovr"]).Trim();
                    }
                    else if (dTblDp2crop.Rows.Count == 0)
                    {

                    }
                }
                else if (sZone.Length == 0)
                {
                    sCrop = "50 50 50 50 100 100";
                }

            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }

        public void RemoveOrphanedExportDefFiles(DataTable dTblJob)
        {
            try
            {
                if (dTblJob.Rows.Count > 0)
                {
                    foreach (DataRow dRow in dTblJob.Rows)
                    {
                        string sFileToRemove = Convert.ToString(dRow["SavedExportDefPath"]).Trim();

                        if (File.Exists(sFileToRemove))
                        {
                            File.Delete(sFileToRemove);
                        }
                    }
                }
                else if (dTblJob.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }        

        public void GetGSFromCodes(string sProdNum, string sFrameNum, ref string sGSBG, string sSitting)
        {
            try
            {
                sFrameNum = sFrameNum.TrimStart('0');

                DataTable dTblCodes = new DataTable("dTblCodes");
                string sCommText = "SELECT * FROM Codes WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sFrameNum;

                dbConns16.CDSQuery(sCDSConnString, sCommText, dTblCodes);

                if (dTblCodes.Rows.Count > 0)
                {
                    foreach (DataRow dRowCodes in dTblCodes.Rows)
                    {
                        if (dRowCodes["Gs_bkgrd"].ToString().Trim() != "")
                        {
                            sGSBG = Convert.ToString(dRowCodes["Gs_bkgrd"]).Trim();
                        }
                    }
                }
                else if (dTblCodes.Rows.Count == 0)
                {

                }
            }
            catch (Exception ex)
            {
                dbConns16.SaveExceptionToDB(ex);
            }
        }                 
    }
}
