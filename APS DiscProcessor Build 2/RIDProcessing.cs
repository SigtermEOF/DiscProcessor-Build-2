using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class RIDProcessing
    {
        // Common class suffix = 15
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns15 = new DBConnections();
        DataGatheringAndProcessing dataGatheringAndProcessing15 = new DataGatheringAndProcessing();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods15 = new TaskMethods();
        InsertsOrUpdates insertsOrUpdates15 = new InsertsOrUpdates();

        private void PushRIDToRender(string sProdNum, DataSet dataSetMain, Main_Form mForm, ref bool bCreated, ref int iRenderedCount)
        {
            //query dp2.images for ref num
            //render every image in order in generic layout
            //no name on
            //no year
            //use merge file data and nwp file data method from rcd processing class

            try
            {
                mainForm = mForm;
                string sDiscType = "RID";
                string sSitting = "";
                bool bSittingBased = false;
                string sStatus = string.Empty;

                DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                string sCommText = "SELECT * FROM [DiscOrders] WHERE [ProdNum] = '" + sProdNum + "'";

                dbConns15.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                if (dTblDiscOrders.Rows.Count > 0)
                {
                    string sRefNum = Convert.ToString(dTblDiscOrders.Rows[0]["RefNum"]).Trim();
                    string sFrameNum = Convert.ToString(dTblDiscOrders.Rows[0]["FrameNum"]).Trim();

                    string sSearchPattern01 = "Label = 'RIDTemplateFile'";
                    DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                    string sSearchPattern02 = "Label = 'RIDRenderedPath'";
                    DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                    string sSearchPattern03 = "Label = 'ExportDefPathDone'";
                    DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                    if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0)
                    {                                
                        string sExportDefFile = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim(); //check for file existance
                        string sRIDRenderedPath = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim(); // check for dir existance
                        string sExportDefPathDone = Convert.ToString(dRowGatheredPattern03[0]["Value"]).Trim(); // check for dir existance

                        string sRenderedImageLocation = sRIDRenderedPath + sProdNum + sFrameNum + @"\" + sProdNum + sFrameNum + "-" + iRenderedCount + ".JPG";
                        string sSavedExportDefPath = sExportDefPathDone + "DiscProcessor" + "_" + sRefNum + "_" + sProdNum + "_" + sFrameNum + "-" + iRenderedCount + ".txt";

                        DataTable dTblDP2Images = new DataTable("dTblDP2Images");
                        sCommText = "SELECT * FROM [Images] WHERE [OrderID] = '" + sRefNum + "'";

                        dbConns15.SQLQuery(sDP2ConnString, sCommText, dTblDP2Images);

                        if (dTblDP2Images.Rows.Count > 0)
                        {
                            foreach (DataRow dRowDP2Images in dTblDP2Images.Rows)
                            {
                                string sImagePath = Convert.ToString(dRowDP2Images["Path"]).Trim();

                                string sExportDefFileText = File.ReadAllText(sExportDefFile);
                                sExportDefFileText = sExportDefFileText.Replace("APSPATH", sImagePath);
                                sExportDefFileText = sExportDefFileText.Replace("APSBGID", string.Empty);
                                sExportDefFileText = sExportDefFileText.Replace("APSTEXT %60 APSYEAR", string.Empty);
                                sExportDefFileText = sExportDefFileText.Replace("APSDEST", sRenderedImageLocation);
                            }
                        }
                        else if (dTblDP2Images.Rows.Count == 0)
                        {
                            sBreakPoint = string.Empty;
                        }
                    }
                    else if (dRowGatheredPattern01.Length == 0 || dRowGatheredPattern02.Length == 0 || dRowGatheredPattern03.Length == 0)
                    {
                        string sErrorDescription = "[" + DateTime.Now.ToString() + "][Failed to gather RIDTemplateFile data from the Variables table.]";

                        insertsOrUpdates15.UpdateDiscOrdersForErrors(sErrorDescription, sRefNum, sFrameNum, sDiscType, sSitting, bSittingBased);

                        sStatus = "90";

                        insertsOrUpdates15.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                    }
                }
                else if (dTblDiscOrders.Rows.Count == 0)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch(Exception ex)
            {
                dbConns15.SaveExceptionToDB(ex);
            }
        }
    }
}
