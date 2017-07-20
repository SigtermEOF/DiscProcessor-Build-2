using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace APS_DiscProcessor_Build_2
{


    class GatherWorld
    {
        // Common class suffix = 21
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        DBConnections dbConns21 = new DBConnections();
        string sBreakpoint = string.Empty;
        Main_Form mainForm = null;
        TaskMethods taskMethods21 = new TaskMethods();

        public void GatherAllReadyWorkInPackagesNoSearchDays(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                DataTable dTblCollectedPackageRecords = new DataTable("dTblCollectedPackageRecords");

                string sGetDiscTypesToGather = "[Gather] = 1 AND [InDevelopment] = 0";
                DataRow[] dRowGetDisctypesToGather = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sGetDiscTypesToGather);

                if (dRowGetDisctypesToGather.Length > 0)
                {
                    foreach (DataRow dRow in dRowGetDisctypesToGather)
                    {
                        dTblCollectedPackageRecords.Clear();
                        dTblCollectedPackageRecords.Dispose();

                        string sDiscType = Convert.ToString(dRow["GatherDiscType"]).Trim();

                        string sText = "[Gathering orders containing " + sDiscType + " discs.]";
                        taskMethods21.LogText(sText, mForm);

                        string sCommText = "SELECT Lookupnum, Packagetag, Order FROM ITEMS WHERE ITEMS.PACKAGETAG IN" +
                                    " (SELECT PACKAGETAG FROM LABELS WHERE LABELS.CODE = '" + sDiscType + "' AND LABELS.PACKAGETAG <> '    ') ORDER BY items.d_dueout";

                        dbConns21.CDSQuery(sCDSConnString, sCommText, dTblCollectedPackageRecords);

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

                            taskMethods21.LogText(sText, mForm);

                            this.GatherAllALaCarteBasedReadyWorkNoSearchDays(sDiscType, dTblCollectedPackageRecords, mForm);
                        }
                        else if (dTblCollectedPackageRecords.Rows.Count == 0)
                        {
                            sText = "[Gathered 0 package records containing " + sDiscType + " discs for processing.]";

                            taskMethods21.LogText(sText, mForm);

                            this.GatherAllALaCarteBasedReadyWorkNoSearchDays(sDiscType, dTblCollectedPackageRecords, mForm);
                        }
                    }
                }
                else if (dRowGetDisctypesToGather.Length == 0)
                {
                    sBreakpoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns21.SaveExceptionToDB(ex);
            }
        }

        private void GatherAllALaCarteBasedReadyWorkNoSearchDays(string sDiscType, DataTable dTblCollectedPackageRecords, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;

                DataTable dTblCollectedALaCarteRecords = new DataTable("dTblCollectedALaCarteRecords");
                string sCommText = "SELECT Lookupnum, Packagetag, Order FROM ITEMS WHERE ITEMS.Lookupnum IN" +
                    " (SELECT Lookupnum FROM CODES WHERE Codes.CODE = '" + sDiscType + "') ORDER BY items.d_dueout";

                dbConns21.CDSQuery(sCDSConnString, sCommText, dTblCollectedALaCarteRecords);

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

                    taskMethods21.LogText(sText, mForm);

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

                        mForm.ScanForTriggerPoints(sDiscType, dTblTotalCollectedRecords);
                    }
                    else if (dTblTotalCollectedRecords.Rows.Count == 0)
                    {
                        sText = "[No " + sDiscType + " disc records gathered this cycle.]";

                        taskMethods21.LogText(sText, mForm);
                        return;
                    }
                }
                else if (dTblCollectedALaCarteRecords.Rows.Count == 0)
                {
                    string sText = "[Gathered 0 a la carte records containing " + sDiscType + " discs for processing.]";

                    taskMethods21.LogText(sText, mForm);

                    DataTable dTblTotalCollectedRecords = new DataTable("dTblTotalCollectedRecords");

                    if (dTblCollectedPackageRecords.Rows.Count > 0)
                    {
                        dTblTotalCollectedRecords = dTblCollectedPackageRecords.Copy();

                        int iTotalCollectedRecordsRowCount = dTblCollectedPackageRecords.Rows.Count;

                        mForm.ScanForTriggerPoints(sDiscType, dTblTotalCollectedRecords);
                    }
                    else if (dTblCollectedPackageRecords.Rows.Count == 0)
                    {
                        sText = "[No " + sDiscType + " disc records gathered this cycle.]";

                        taskMethods21.LogText(sText, mForm);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                dbConns21.SaveExceptionToDB(ex);
            }
        }
    }
}
