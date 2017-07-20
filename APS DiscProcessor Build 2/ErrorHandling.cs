using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net.Mail;
using System.Net;

namespace APS_DiscProcessor_Build_2
{
    class ErrorHandling
    {
        // Common class suffix = 19
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        DBConnections dbConns19 = new DBConnections();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        InsertsOrUpdates insertsOrUpdates19 = new InsertsOrUpdates();

        string sEmailServer = string.Empty;
        string sCCSendTo = string.Empty;
        string sSendTo = string.Empty;
        string sTextMsgSendTo = string.Empty;
        bool bSendNotifications = false;
        bool bSendLocalEmailNotification = false;
        bool bSendRemoteEmailNotification = false;
        bool bSendPhoneTextNotification = false;

        public void GatherErrors(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                bool bSuccess = false;
                this.GatherEmailVariables(ref sEmailServer, ref sCCSendTo, ref sSendTo, ref sTextMsgSendTo, ref bSuccess, dataSetMain, ref bSendNotifications, ref bSendLocalEmailNotification, ref bSendRemoteEmailNotification, ref bSendPhoneTextNotification);

                if (bSuccess == true)
                {
                    // Check the DiscOrders table for errors.

                    string sProdNum = string.Empty;
                    string sFrameNum = string.Empty;
                    string sRefNum = string.Empty;
                    string sDiscType = string.Empty;
                    string sError = string.Empty;
                    string sErrorDescription = string.Empty;
                    string sUniqueID = string.Empty;
                    string sSitting = string.Empty;

                    StringBuilder sBuilder = new StringBuilder();

                    DataTable dTblDiscOrders = new DataTable("DiscOrders");
                    string sCommText = "SELECT * FROM [DiscOrders] WHERE [Status] = '90' AND ([ErrorChecked] != '1' OR [ErrorChecked] IS NULL)";

                    dbConns19.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                    if (dTblDiscOrders.Rows.Count <= 5 && dTblDiscOrders.Rows.Count != 0)
                    {
                        foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                        {
                            sBuilder.Clear();

                            sProdNum = Convert.ToString(dRowDiscOrders["ProdNum"]).Trim();
                            sFrameNum = Convert.ToString(dRowDiscOrders["FrameNum"]).Trim();
                            sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                            sDiscType = Convert.ToString(dRowDiscOrders["DiscType"]).Trim();
                            sError = Convert.ToString(dRowDiscOrders["Error"]).Trim();
                            sErrorDescription = Convert.ToString(dRowDiscOrders["ErrorDescription"]).Trim();
                            sUniqueID = Convert.ToString(dRowDiscOrders["UniqueID"]).Trim();
                            sSitting = Convert.ToString(dRowDiscOrders["Sitting"]);

                            sBuilder.AppendFormat("Production #: " + sProdNum);
                            sBuilder.Append(Environment.NewLine);
                            sBuilder.AppendFormat("Reference #: " + sRefNum);
                            sBuilder.Append(Environment.NewLine);
                            sBuilder.AppendFormat("Frame #: " + sFrameNum);
                            sBuilder.Append(Environment.NewLine);
                            sBuilder.AppendFormat("UniqueID : " + sUniqueID);
                            sBuilder.Append(Environment.NewLine);
                            sBuilder.AppendFormat("Error : " + sError);
                            sBuilder.Append(Environment.NewLine);
                            sBuilder.AppendFormat("Error description : " + sErrorDescription);
                            sBuilder.Append(Environment.NewLine);

                            string sSubject = "DiscProcessor error notification.";
                            string sBody = "An error was recorded in the DiscProcessor.DiscOrders table: " + Environment.NewLine + Environment.NewLine + sBuilder.ToString().Trim();

                            this.EmailError(sEmailServer, sCCSendTo, sSendTo, sSubject, sBody, sTextMsgSendTo, dataSetMain);

                            sCommText = "UPDATE [DiscOrders] SET [ErrorChecked] = '1' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                            bool bSuccess1 = false;

                            dbConns19.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess1);

                            if (bSuccess1 == true)
                            {
                                string sStatus = "91";
                                insertsOrUpdates19.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                            }
                            else if (bSuccess1 != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                    }
                    else if (dTblDiscOrders.Rows.Count > 5)
                    {
                        string sSubject = "DiscProcessor error notification.";
                        string sBody = "Multiple errors were recorded in the DiscProcessor.DiscOrders table.";

                        this.EmailError(sEmailServer, sCCSendTo, sSendTo, sSubject, sBody, sTextMsgSendTo, dataSetMain);

                        foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                        {
                            sProdNum = Convert.ToString(dRowDiscOrders["ProdNum"]).Trim();
                            sFrameNum = Convert.ToString(dRowDiscOrders["FrameNum"]).Trim();
                            sRefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();
                            sDiscType = Convert.ToString(dRowDiscOrders["DiscType"]).Trim();
                            sError = Convert.ToString(dRowDiscOrders["Error"]).Trim();
                            sErrorDescription = Convert.ToString(dRowDiscOrders["ErrorDescription"]).Trim();
                            sUniqueID = Convert.ToString(dRowDiscOrders["UniqueID"]).Trim();

                            sCommText = "UPDATE [DiscOrders] SET [ErrorChecked] = '1' WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                            bool bSuccess1 = false;

                            dbConns19.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess1);

                            if (bSuccess1 == true)
                            {
                                string sStatus = "91";

                                string sSearchPattern02 = "GatherDiscType = '" + sDiscType + "'";
                                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern02);

                                if (dRowGatheredPattern02.Length > 0)
                                {
                                    bool bSittingBased = Convert.ToBoolean(dRowGatheredPattern02[0]["SittingBased"]);

                                    if (bSittingBased != true)
                                    {
                                        insertsOrUpdates19.UpdateDiscOrdersTableStatusFrameBased(sRefNum, sFrameNum, sStatus, sDiscType, sProdNum);
                                    }
                                    else if (bSittingBased == true)
                                    {
                                        insertsOrUpdates19.UpdateDiscOrdersTableStatusSittingBased(sRefNum, sStatus, sDiscType, sSitting, sProdNum);
                                    }
                                }
                                else if (dRowGatheredPattern02.Length == 0)
                                {
                                    sBreakPoint = string.Empty;
                                }
                            }
                            else if (bSuccess1 != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                    }
                    else if (dTblDiscOrders.Rows.Count == 0)
                    {

                    }

                    // Check the Errors table for exceptions.

                    DataTable dTblErrorsTable = new DataTable("ErrorsTable");
                    sCommText = "SELECT * FROM [Errors] WHERE [Exception_Email_Sent] = '0'";

                    dbConns19.SQLQuery(sDiscProcessorConnString, sCommText, dTblErrorsTable);

                    if (dTblErrorsTable.Rows.Count <= 5 && dTblErrorsTable.Rows.Count != 0)
                    {
                        foreach (DataRow dRowErrorsTable in dTblErrorsTable.Rows)
                        {
                            string sException = Convert.ToString(dRowErrorsTable["Exception"]).Trim();
                            string sExceptionDateTime = Convert.ToString(dRowErrorsTable["Exception_DateTime"]).Trim();

                            string sSubject = "DiscProcessor error notification.";
                            string sBody = "An exception was recorded in the DiscProcessor.Errors table at " + sExceptionDateTime + " : " + Environment.NewLine + Environment.NewLine + sException;

                            this.EmailError(sEmailServer, sCCSendTo, sSendTo, sSubject, sBody, sTextMsgSendTo, dataSetMain);

                            sCommText = "UPDATE [Errors] SET [Exception_Email_Sent] = '1' WHERE [Exception] = '" + sException + "' AND [Exception_DateTime] = '" + sExceptionDateTime + "'";
                            bool bSuccess1 = false;

                            dbConns19.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess1);

                            if (bSuccess1 == true)
                            {
                                continue;
                            }
                            else if (bSuccess1 != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                    }
                    else if (dTblErrorsTable.Rows.Count > 5)
                    {
                        string sSubject = "DiscProcessor error notification.";
                        string sBody = "Multiple exceptions were recorded in the DiscProcessor.Errors table.";

                        this.EmailError(sEmailServer, sCCSendTo, sSendTo, sSubject, sBody, sTextMsgSendTo, dataSetMain);

                        foreach (DataRow dRowErrorsTable in dTblErrorsTable.Rows)
                        {
                            string sException = Convert.ToString(dRowErrorsTable["Exception"]).Trim();
                            string sExceptionDateTime = Convert.ToString(dRowErrorsTable["Exception_DateTime"]).Trim();

                            sCommText = "UPDATE [Errors] SET [Exception_Email_Sent] = '1' WHERE [Exception] = '" + sException + "' AND [Exception_DateTime] = '" + sExceptionDateTime + "'";
                            bool bSuccess1 = false;

                            dbConns19.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess1);

                            if (bSuccess1 == true)
                            {
                                continue;
                            }
                            else if (bSuccess1 != true)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                    }
                    else if (dTblErrorsTable.Rows.Count == 0)
                    {

                    }
                }
                else if (bSuccess != true)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns19.SaveExceptionToDB(ex);
            }
        }

        private void GatherEmailVariables(ref string sEmailServer, ref string sCCSendTo, ref string sSendTo, ref string sTextMsgSendTo, ref bool bSuccess, DataSet dataSetMain, ref bool bSendNotifications, ref bool bSendLocalEmailNotification, ref bool bSendRemoteEmailNotification, ref bool bSendPhoneTextNotification)
        {
            try
            {
                string sSearchPattern01 = "Label = 'EmailServer'";
                DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern01);

                string sSearchPattern02 = "Label = 'EmailCCSendTo'";
                DataRow[] dRowGatheredPattern02 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern02);

                string sSearchPattern03 = "Label = 'EmailSendTo'";
                DataRow[] dRowGatheredPattern03 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern03);

                string sSearchPattern04 = "Label = 'TextMsgSendTo'";
                DataRow[] dRowGatheredPattern04 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern04);

                string sSearchPattern05 = "Label = 'SendNotifications'";
                DataRow[] dRowGatheredPattern05 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern05);

                string sSearchPattern06 = "Label = 'SendLocalEmailNotification'";
                DataRow[] dRowGatheredPattern06 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern06);

                string sSearchPattern07 = "Label = 'SendRemoteEmailNotification'";
                DataRow[] dRowGatheredPattern07 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern07);

                string sSearchPattern08 = "Label = 'SendPhoneTextNotification'";
                DataRow[] dRowGatheredPattern08 = dataSetMain.Tables["dTblVariables"].Select(sSearchPattern08);

                if (dRowGatheredPattern01.Length > 0 && dRowGatheredPattern02.Length > 0 && dRowGatheredPattern03.Length > 0 && dRowGatheredPattern04.Length > 0 && dRowGatheredPattern05.Length > 0 && dRowGatheredPattern06.Length > 0 && dRowGatheredPattern07.Length > 0 && dRowGatheredPattern08.Length > 0)
                {
                    sEmailServer = Convert.ToString(dRowGatheredPattern01[0]["Value"]).Trim();
                    sCCSendTo = Convert.ToString(dRowGatheredPattern02[0]["Value"]).Trim();
                    sSendTo = Convert.ToString(dRowGatheredPattern03[0]["Value"]).Trim();
                    sTextMsgSendTo = Convert.ToString(dRowGatheredPattern04[0]["Value"]).Trim();
                    bSendNotifications = Convert.ToBoolean(dRowGatheredPattern05[0]["Value"]);
                    bSendLocalEmailNotification = Convert.ToBoolean(dRowGatheredPattern06[0]["Value"]);
                    bSendRemoteEmailNotification = Convert.ToBoolean(dRowGatheredPattern07[0]["Value"]);
                    bSendPhoneTextNotification = Convert.ToBoolean(dRowGatheredPattern08[0]["Value"]);

                    bSuccess = true;
                }
                else
                {
                    bSuccess = false;
                }
            }
            catch (Exception ex)
            {
                bSuccess = false;

                dbConns19.SaveExceptionToDB(ex);
            }
        }

        private void EmailError(string sEmailServer, string sCCSendTo, string sSendTo, string sSubject, string sBody, string sTextMsgSendTo, DataSet dataSetMain)
        {
            try
            {
                if (bSendNotifications == true)
                {
                    MailAddress from = new MailAddress("APSAUTO@ADVANCEDPHOTO.COM", "APS");
                    MailAddress to = new MailAddress(sSendTo); // In house email notification
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Subject = sSubject;
                    mailMessage.Body = sBody;

                    if (bSendRemoteEmailNotification == true)
                    {
                        MailAddress cc = new MailAddress(sCCSendTo); // Remote gmail email notification
                        mailMessage.CC.Add(cc);
                    }
                    if (bSendPhoneTextNotification == true)
                    {
                        MailAddress txt = new MailAddress(sTextMsgSendTo); // Phone text notification                        
                        mailMessage.CC.Add(txt);
                    }

                    SmtpClient stmpClient = new SmtpClient(sEmailServer);
                    stmpClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                    stmpClient.Send(mailMessage);
                }
                else if (bSendNotifications != true)
                {
                    // Do nothing.
                }
            }
            catch (Exception ex)
            {
                dbConns19.SaveExceptionToDB(ex);
            }
        }
    }
}
