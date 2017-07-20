using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace APS_DiscProcessor_Build_2
{
    class DBConnections
    {
        // Common class suffix = 02
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        string sDP2ConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DP2ConnString.ToString();
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();

        public bool SQLNonQuery(string sConnString, string sCommText, ref bool bSuccess)
        {
            try
            {
                SqlConnection sqlConn = new SqlConnection(sConnString);

                SqlCommand sqlComm = sqlConn.CreateCommand();

                sqlComm.CommandTimeout = 120;

                sqlComm.CommandText = sCommText;

                sqlConn.Open();

                sqlComm.ExecuteNonQuery();

                sqlComm.Dispose();

                sqlConn.Close();
                sqlConn.Dispose();

                bSuccess = true;
            }
            catch (Exception ex)
            {
                bSuccess = false;
                this.SaveExceptionToDBwithCommText(ex, sCommText);
            }
            return bSuccess;
        }

        public void SQLQuery(string sConnString, string sCommText, DataTable dTbl)
        {
            try
            {
                SqlConnection sqlConn = new SqlConnection(sConnString);

                SqlCommand sqlComm = sqlConn.CreateCommand();

                sqlComm.CommandTimeout = 120;

                sqlComm.CommandText = sCommText;

                sqlConn.Open();

                SqlDataReader sqlDReader = sqlComm.ExecuteReader();

                if (sqlDReader.HasRows)
                {
                    dTbl.Clear();
                    dTbl.Load(sqlDReader);
                }

                sqlDReader.Close();
                sqlDReader.Dispose();

                sqlComm.Dispose();

                sqlConn.Close();
                sqlConn.Dispose();
            }
            catch (Exception ex)
            {
                this.SaveExceptionToDBwithCommText(ex, sCommText);
            }
        }

        public void CDSQuery(string sConnString, string sCommText, DataTable dTbl)
        {
            try
            {
                OleDbConnection olDBConn = new OleDbConnection(sConnString);

                OleDbCommand oleDBComm = olDBConn.CreateCommand();

                oleDBComm.CommandTimeout = 120;

                oleDBComm.CommandText = sCommText;

                olDBConn.Open();

                oleDBComm.CommandTimeout = 0;

                OleDbDataReader oleDBDReader = oleDBComm.ExecuteReader();

                if (oleDBDReader.HasRows)
                {
                    dTbl.Clear();
                    dTbl.Load(oleDBDReader);
                }

                oleDBComm.Dispose();

                oleDBDReader.Close();
                oleDBDReader.Dispose();

                olDBConn.Close();
                olDBConn.Dispose();
            }
            catch (Exception ex)
            {
                this.SaveExceptionToDBwithCommText(ex, sCommText);
            }
        }

        public bool CDSNonQuery(string sConnString, string sCommText, ref bool bSuccess)
        {
            try
            {
                OleDbConnection oleDBConn = new OleDbConnection(sConnString);

                OleDbCommand oleDBComm = oleDBConn.CreateCommand();

                oleDBComm.CommandTimeout = 120;

                oleDBComm.CommandText = sCommText;

                oleDBConn.Open();

                oleDBComm.CommandTimeout = 0;

                oleDBComm.ExecuteNonQuery();

                oleDBComm.Dispose();

                oleDBConn.Close();
                oleDBConn.Dispose();

                bSuccess = true;
            }
            catch (Exception ex)
            {
                bSuccess = false;
                this.SaveExceptionToDBwithCommText(ex, sCommText);
            }
            return bSuccess;
        }

        public bool CDSNonQueryNULLOFFThenON(string sConnString, string sCommText, ref bool bSuccess)
        {
            try
            {
                OleDbConnection oleDBConn = new OleDbConnection(sConnString);

                OleDbCommand oleDBCommNULLOFF = oleDBConn.CreateCommand();
                oleDBCommNULLOFF.CommandText = "SET NULL OFF";
                oleDBConn.Open();
                oleDBCommNULLOFF.CommandTimeout = 0;
                oleDBCommNULLOFF.ExecuteNonQuery();

                OleDbCommand oleDBComm = oleDBConn.CreateCommand();
                oleDBComm.CommandText = sCommText;
                oleDBComm.CommandTimeout = 0;
                oleDBComm.ExecuteNonQuery();

                OleDbCommand oleDBCommNULLON = oleDBConn.CreateCommand();
                oleDBCommNULLON.CommandText = "SET NULL ON";
                oleDBCommNULLON.CommandTimeout = 0;
                oleDBCommNULLON.ExecuteNonQuery();

                oleDBCommNULLOFF.Dispose();
                oleDBComm.Dispose();
                oleDBConn.Close();
                oleDBConn.Dispose();

                bSuccess = true;
            }
            catch (Exception ex)
            {
                bSuccess = false;
                this.SaveExceptionToDB(ex);
            }
            return bSuccess;
        }

        public void SaveExceptionToDB(Exception ex)
        {
            string sException = ex.ToString().Trim();
            sException = sException.Replace(@"'", "");
            string sCommText = string.Empty;

            try
            {
                sCommText = "INSERT INTO [Errors] VALUES ('" + sException + "', '" + DateTime.Now.ToString() + "', '0')";
                bool bSuccess = false;

                this.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {

                }
                else if (bSuccess != true)
                {
                    MessageBox.Show("Insertion of data into the Errors table failed." + Environment.NewLine + Environment.NewLine + sCommText);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + sCommText);
            }
        }

        public void SaveExceptionToDBwithCommText(Exception ex, string sText)
        {
            string sException = ex.ToString().Trim();
            sException = sException.Replace(@"'", "");
            sText = sText.Replace(@"'", "");
            string sCommText = string.Empty;

            try
            {
                sCommText = "INSERT INTO [Errors] VALUES ('" + sException + Environment.NewLine + sText + "', '" + DateTime.Now.ToString() + "', '0')";
                bool bSuccess = false;

                this.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess);

                if (bSuccess == true)
                {

                }
                else if (bSuccess != true)
                {
                    MessageBox.Show("Insertion of data into the Errors table failed." + Environment.NewLine + Environment.NewLine + sCommText);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + sCommText);
            }
        }
    }
}
