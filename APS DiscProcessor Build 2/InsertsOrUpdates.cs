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
    class InsertsOrUpdates
    {
        // Common class suffix = 18
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns18 = new DBConnections();
        string sBreakPoint = string.Empty;

        #region Updates.

        public void UpdateDiscOrdersForErrors(string sErrorDescription, string sRefNum, string sFrameNum, string sDiscType, string sSitting, bool bSittingBased)
        {
            try
            {
                bool bSuccess = false;
                string sCommText = string.Empty;

                if (bSittingBased != true)
                {
                    sCommText = "UPDATE [DiscOrders] SET [Status] = '90', [Error] = '1', [ErrorDescription] = '" + sErrorDescription + "', [ErrorDate] = '"
                        + DateTime.Now.ToString().Trim() + "' WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                }
                else if (bSittingBased == true)
                {
                    sCommText = "UPDATE [DiscOrders] SET [Status] = '90', [Error] = '1', [ErrorDescription] = '" + sErrorDescription + "', [ErrorDate] = '"
                        + DateTime.Now.ToString().Trim() + "' WHERE [RefNum] = '" + sRefNum + "' AND [Sitting] = '" + sSitting + "' AND [DiscType] = '" + sDiscType + "'";
                }

                dbConns18.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {

                }
                else if (bSuccess != true)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns18.SaveExceptionToDB(ex);
            }
        }

        public void UpdateDiscOrdersTableStatusFrameBased(string sRefNum, string sFrameNum, string sStatus, string sDiscType, string sProdNum)
        {
            try
            {
                string sCommText = "UPDATE [DiscOrders] SET [Status] = '" + sStatus + "', [LastCheck] = '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim()
                    + "' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";

                bool bSuccess = false;

                dbConns18.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {

                }
                else if (bSuccess != true)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns18.SaveExceptionToDB(ex);
            }
        }

        public void UpdateDiscOrdersTableStatusSittingBased(string sRefNum, string sStatus, string sDiscType, string sSitting, string sProdNum)
        {
            try
            {
                string sCommText = "UPDATE [DiscOrders] SET [Status] = '" + sStatus + "', [LastCheck] = '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim()
                    + "' WHERE [UniqueID] = '" + sProdNum + sSitting.Trim() + sDiscType + "'";

                bool bSuccess = false;

                dbConns18.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {

                }
                else if (bSuccess != true)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns18.SaveExceptionToDB(ex);
            }
        }

        public void UpdateFrameDataForExportDefGenerated(string sRefNum, string sFrameNum, string sDiscType, string sProdNum, ref bool bUpdateFrameDataSuccess)
        {
            try
            {
                bool bUpdateSuccess = true;
                string sCommText = "UPDATE [FrameData] SET [ExportDefGenerated] = '1', [ExportDefGeneratedDate] = '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() + "' WHERE [RefNum] = '" + sRefNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";

                dbConns18.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bUpdateSuccess);

                if (bUpdateSuccess == true)
                {
                    bUpdateFrameDataSuccess = true;
                }
                else if (bUpdateSuccess != true)
                {
                    bUpdateFrameDataSuccess = false;
                }
            }
            catch (Exception ex)
            {
                bUpdateFrameDataSuccess = false;

                dbConns18.SaveExceptionToDB(ex);
            }
        }

        public void UpdateStamps(string sProdNum, string sAction, ref bool bStamped)
        {
            try
            {
                DataTable dTblStamps = new DataTable("dTblStamps");
                string sCommText = "SELECT * FROM Stamps WHERE Lookupnum = '" + sProdNum + "' AND Action = '" + sAction + "'";

                dbConns18.CDSQuery(sCDSConnString, sCommText, dTblStamps);

                if (dTblStamps.Rows.Count > 0)
                {
                    bStamped = true;
                }
                else if (dTblStamps.Rows.Count == 0)
                {
                    bool bSuccess = false;
                    sCommText = "INSERT INTO Stamps (User_id, Stationid, Lookupnum, Date, Time, Action, Wbs_task, Sequence, Framenum, Count," +
                    " Seconds, Wbs_plan, Wbs_track, Wbs_status, App_level, Processed) VALUES " +
                    "('DISCPROC', 'DProcessor', '" + sProdNum + "'," + " DATE(" + DateTime.Now.Date.ToString("yyyy,MM,dd").Trim() + "), '" + DateTime.Now.ToString("H:mm:ss").Trim() + "', '" + sAction + "', '" + sAction + "', 0, ' ', ' ', ' ', ' '," +
                    " .F., ' ', ' ', ' ' )";

                    dbConns18.CDSNonQuery(sCDSConnString, sCommText, ref bSuccess);

                    if (bSuccess == true)
                    {
                        bStamped = true;
                    }
                    else if (bSuccess != true)
                    {
                        bStamped = false;
                    }
                }
            }
            catch (Exception ex)
            {
                bStamped = false;

                dbConns18.SaveExceptionToDB(ex);
            }
        }

        public void JobQueuePrintStatusUpdateFrameBased(string sRefNum, string sFrameNum, ref int iBatchID, ref bool bJobQueueStatusUpdated)
        {
            try
            {
                string sCommText = "UPDATE [JobQueue] SET [PrintStatus] = '1' WHERE [OrderID] = '" + sRefNum + "' AND [BatchID] = '" + iBatchID + "'";

                bool bSuccess = true;

                dbConns18.SQLNonQuery(sDP2ConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {
                    bJobQueueStatusUpdated = true;
                }
                else if (bSuccess != true)
                {
                    bJobQueueStatusUpdated = false;
                }
            }
            catch (Exception ex)
            {
                bJobQueueStatusUpdated = false;

                dbConns18.SaveExceptionToDB(ex);
            }
        }

        public void JobQueuePrintStatusUpdateSittingBased(string sRefNum, ref int iBatchID, ref bool bJobQueueStatusUpdated)
        {
            try
            {
                string sCommText = "UPDATE [JobQueue] SET [PrintStatus] = '1' WHERE [OrderID] = '" + sRefNum + "' AND [BatchID] = '" + iBatchID + "'";

                bool bSuccess = true;

                dbConns18.SQLNonQuery(sDP2ConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {
                    bJobQueueStatusUpdated = true;
                }
                else if (bSuccess != true)
                {
                    bJobQueueStatusUpdated = false;
                }
            }
            catch (Exception ex)
            {
                bJobQueueStatusUpdated = false;

                dbConns18.SaveExceptionToDB(ex);
            }
        }

        #endregion

        #region Inserts.

        public void InsertIntoCDSDiscOrders(DataSet dataSetMain, string sRefNum, string sProdNum, string sSequence, string sSitting, string sDiscType, ref bool bInsertSuccess, string sFrameNum)
        {
            try
            {
                string sSearchPattern01 = "Label = 'RenderedPath'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    string sRenderedPath = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                    sRenderedPath += sDiscType + @"\" + sProdNum + sFrameNum + @"\";

                    string sCommText = "INSERT INTO DiscOrders (Cust_ref, Lookupnum, Sequence, Sitting, DiscType, Rendpath) VALUES ('" + sRefNum + "', '" + sProdNum + "', " +
                        sSequence + ", '" + sSitting + "', '" + sDiscType + "', '" + sRenderedPath + "')";

                    dbConns18.CDSNonQuery(sCDSConnString, sCommText, ref bInsertSuccess);

                    if (bInsertSuccess == true)
                    {

                    }
                    else if (bInsertSuccess != true)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (dRowGatheredPattern01.Length == 0)
                {
                    sBreakPoint = string.Empty;

                    bInsertSuccess = false;
                }
            }
            catch (Exception ex)
            {
                bInsertSuccess = false;

                dbConns18.SaveExceptionToDB(ex);                
            }
        }

        public void JobQueueInsert(string sExportDefFile, string sRefNum, string sFrameNum, int iJobID, int iBatchID, ref bool bInserted, bool bSittingBased, string sSitting)
        {
            // jobqueue print status
            //  0 - HOLD........-
            //  1 - READY.......-
            //  2 - RESERVED....-
            //  3 - PRINTING....-
            //  4 - COMPLETED...-
            //  5 - SAVED.......-
            //  6 - ERROR.......-
            //  7 - CANCELLED...-
            //  8 - PENDING.....-
            //  9 - LOADED......-
            // 10 - PARSED......-

            try
            {
                string sCommText = "INSERT INTO JOBQUEUE (QUEUENAME, BATCHID, ORDERID, ORDERSEQUENCE, ORDERITEMID, ORDERITEMQTY," +
                        " ORDERITEMSEQUENCE, PRIORITY, SUBMITDATE, JOBID, PRINTSTATUS, OWNER, JOBPATH) " +
                        "VALUES ('AUTOGEN', '" + iBatchID + "', '" + sRefNum + "', " + "1, 1, 1, 1, 50, '" +
                        Convert.ToString(DateTime.Now.Year).Trim().PadLeft(4, '0') + Convert.ToString(DateTime.Now.Month).Trim().PadLeft(2, '0') + Convert.ToString(DateTime.Now.Day).Trim().PadLeft(2, '0') +
                        Convert.ToString(DateTime.Now.Hour).Trim().PadLeft(2, '0') + Convert.ToString(DateTime.Now.Minute).Trim().PadLeft(2, '0') + Convert.ToString(DateTime.Now.Second).Trim().PadLeft(5, '0') +
                        "', '" + iJobID + "', 0, '" + SystemInformation.ComputerName + "', '" + sExportDefFile + "')";

                bool bSuccess = true;

                dbConns18.SQLNonQuery(sDP2ConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {

                }
                else if (bSuccess != true)
                {
                    bInserted = false;
                }
            }
            catch (Exception ex)
            {
                bInserted = false;

                dbConns18.SaveExceptionToDB(ex);
            }
        }

        #endregion
    }
}
