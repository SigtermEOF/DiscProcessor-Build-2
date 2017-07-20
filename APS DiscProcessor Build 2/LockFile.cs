using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Threading;

namespace APS_DiscProcessor_Build_2
{
    class LockFile
    {
        // Common class suffix = 17
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        DBConnections dbConns17 = new DBConnections();
        string sBreakPoint = string.Empty;
        FileStream fsLock;


        public void LockTheFile(ref int iBatchID, ref int iJobID, ref int iJobIDsNeeded, ref bool bLockedFile, ref bool bGatherIDsSuccess, DataSet dataSetMain)
        {
            try
            {
                string sSearchPattern01 = "Label = 'LockFile'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                if (dRowGatheredPattern01.Length > 0)
                {
                    string sLockFile = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();

                    fsLock = new FileStream(sLockFile, FileMode.Open, FileAccess.Read, FileShare.None);

                    try
                    {
                        // Lock the file.
                        fsLock.Lock(0, fsLock.Length);
                    }
                    catch (IOException)
                    {
                        try
                        {
                            TimeSpan tSpan = new TimeSpan(0, 0, 5); // Sleep the thread for 5 seconds if the file is currently locked.
                            Thread.Sleep(tSpan);

                            // Lock the file.
                            fsLock.Lock(0, fsLock.Length);
                        }
                        catch (IOException)
                        {
                            try
                            {
                                TimeSpan tSpan = new TimeSpan(0, 0, 10); // Sleep the thread for 10 seconds if the file is currently locked.
                                Thread.Sleep(tSpan);

                                // Lock the file.
                                fsLock.Lock(0, fsLock.Length);
                            }
                            catch (IOException)
                            {
                                bLockedFile = true;
                            }
                        }
                    }

                    if (bLockedFile != true)
                    {
                        bool bGetSuccess = true;

                        this.GetBatchID(ref iBatchID, ref bGetSuccess, ref fsLock);

                        if (bGetSuccess == true)
                        {
                            bool bUpdateSuccess = true;
                            this.UpdateBatchID(ref bUpdateSuccess, ref fsLock);

                            if (bUpdateSuccess == true)
                            {
                                this.GetJobID(ref iJobID, ref bGetSuccess, ref fsLock);

                                if (bGetSuccess == true)
                                {
                                    this.UpdateJobID(iJobIDsNeeded, ref bUpdateSuccess, ref fsLock);

                                    if (bUpdateSuccess == true)
                                    {
                                        // Unlock the file.
                                        fsLock.Unlock(0, fsLock.Length);
                                        fsLock.Close();

                                        bGatherIDsSuccess = true;
                                    }
                                    else if (bUpdateSuccess != true)
                                    {
                                        bGatherIDsSuccess = false;
                                    }
                                }
                                else if (bGetSuccess != true)
                                {
                                    bGatherIDsSuccess = false;
                                }
                            }
                            else if (bUpdateSuccess != true)
                            {
                                bGatherIDsSuccess = false;
                            }
                        }
                        else if (bGetSuccess != true)
                        {
                            bGatherIDsSuccess = false;
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
                bGatherIDsSuccess = false;

                fsLock.Unlock(0, fsLock.Length);
                fsLock.Close();

                dbConns17.SaveExceptionToDB(ex);
            }
        }

        private void GetBatchID(ref int iBatchID, ref bool bGetSuccess, ref FileStream fsLock)
        {
            try
            {
                DataTable dt = new DataTable();
                string sCommText = "SELECT * FROM [IDs] WHERE [NAME] = 'PrintBatchID'";

                dbConns17.SQLQuery(sDP2ConnString, sCommText, dt);

                if (dt.Rows.Count > 0)
                {
                    iBatchID = Convert.ToInt32(dt.Rows[0]["ID"]) + 2;
                    bGetSuccess = true;
                }
                else if (dt.Rows.Count == 0)
                {
                    bGetSuccess = false;
                }
            }
            catch (Exception ex)
            {
                fsLock.Unlock(0, fsLock.Length);
                fsLock.Close();

                dbConns17.SaveExceptionToDB(ex);
                bGetSuccess = false;
            }
        }

        private void UpdateBatchID(ref bool bUpdateSuccess, ref FileStream fsLock)
        {
            try
            {
                string sCommText = "UPDATE [IDs] SET [ID] = ID+4 WHERE [NAME] = 'PrintBatchID'";

                bool bSuccess = true;

                dbConns17.SQLNonQuery(sDP2ConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {
                    bUpdateSuccess = true;
                }
                else if (bSuccess != true)
                {
                    bUpdateSuccess = false;
                }
            }
            catch (Exception ex)
            {
                fsLock.Unlock(0, fsLock.Length);
                fsLock.Close();

                dbConns17.SaveExceptionToDB(ex);
                bUpdateSuccess = false;
            }
        }

        private void GetJobID(ref int iJobID, ref bool bGetSuccess, ref FileStream fsLock)
        {
            try
            {
                DataTable dt = new DataTable();
                string sCommText = "SELECT * FROM [IDs] WHERE [NAME] = 'PrintJobID'";

                dbConns17.SQLQuery(sDP2ConnString, sCommText, dt);

                if (dt.Rows.Count > 0)
                {
                    iJobID = Convert.ToInt32(dt.Rows[0]["ID"]) + 2;
                    bGetSuccess = true;
                }
                else if (dt.Rows.Count == 0)
                {
                    bGetSuccess = false;
                }
            }
            catch (Exception ex)
            {
                fsLock.Unlock(0, fsLock.Length);
                fsLock.Close();

                dbConns17.SaveExceptionToDB(ex);
                bGetSuccess = false;
            }
        }

        private void UpdateJobID(int iIDsNeeded, ref bool bUpdateSuccess, ref FileStream fsLock)
        {
            try
            {
                string sCommText = "UPDATE [IDs] SET [ID] = ID+" + (iIDsNeeded + 5) + " WHERE [NAME] = 'PrintJobID'";

                bool bSuccess = true;

                dbConns17.SQLNonQuery(sDP2ConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {
                    bUpdateSuccess = true;
                }
                else if (bSuccess != true)
                {
                    bUpdateSuccess = false;
                }
            }
            catch (Exception ex)
            {
                fsLock.Unlock(0, fsLock.Length);
                fsLock.Close();

                dbConns17.SaveExceptionToDB(ex);
                bUpdateSuccess = false;
            }
        }
    }
}
