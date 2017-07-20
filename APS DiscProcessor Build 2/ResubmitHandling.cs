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
    class ResubmitHandling
    {
        // Common class suffix = 20
        string sDiscProcessorConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.DiscProcessorConnString.ToString();
        string sCDSConnString = APS_DiscProcessor_Build_2.Properties.Settings.Default.CDSConnString.ToString();
        DBConnections dbConns20 = new DBConnections();
        string sBreakPoint = string.Empty;
        Main_Form mainForm = null;
        InsertsOrUpdates insertsOrUpdates20 = new InsertsOrUpdates();
        TaskMethods taskMethods20 = new TaskMethods();

        string sEmailServer = string.Empty;
        string sCCSendTo = string.Empty;
        string sSendTo = string.Empty;
        string sTextMsgSendTo = string.Empty;
        bool bSendNotifications = false;
        bool bSendLocalEmailNotification = false;
        bool bSendRemoteEmailNotification = false;
        bool bSendPhoneTextNotification = false;

        public void GatherResubmits(DataSet dataSetMain, Main_Form mForm)
        {
            try
            {
                mainForm = mForm;
                bool bSuccess = false;
                this.GatherNotificationVariables(ref sEmailServer, ref sCCSendTo, ref sSendTo, ref sTextMsgSendTo, ref bSuccess, dataSetMain, ref bSendNotifications, ref bSendLocalEmailNotification, ref bSendRemoteEmailNotification, ref bSendPhoneTextNotification);

                if (bSuccess == true)
                {
                    string sCommText = string.Empty;
                    DataTable dTblResubmits = new DataTable("dTblResubmits");
                    DataTable dTblDiscOrders = new DataTable("dTblDiscOrders");
                    int iResubmitCount = 0;
                    string sProdNum = string.Empty;
                    string sFrameNum = string.Empty;
                    string sRefNum = string.Empty;

                    sCommText = "SELECT * FROM DP_Resubmits WHERE Status = 80";
                    dbConns20.CDSQuery(sCDSConnString, sCommText, dTblResubmits);

                    if (dTblResubmits.Rows.Count > 0)
                    {
                        foreach (DataRow dRowResubmits in dTblResubmits.Rows)
                        {
                            sProdNum = Convert.ToString(dRowResubmits["Lookupnum"]).Trim();
                            string sSequence = Convert.ToString(dRowResubmits["Sequence"]).Trim();
                            sFrameNum = sSequence.PadLeft(4, '0');
                            string sSitting = Convert.ToString(dRowResubmits["Sitting"]);
                            string sDiscType = Convert.ToString(dRowResubmits["Disctype"]).Trim();

                            string sSearchPattern01 = "GatherDiscType = '" + sDiscType + "'";
                            DataRow[] dRowGatheredPattern01 = dataSetMain.Tables["dTblGatherDiscTypes"].Select(sSearchPattern01);

                            if (dRowGatheredPattern01.Length > 0)
                            {
                                bool bSittingBased = Convert.ToBoolean(dRowGatheredPattern01[0]["SittingBased"]);

                                iResubmitCount = 0;

                                sCommText = "SELECT * FROM [DiscOrders] WHERE ([ProdNum] = '" + sProdNum + "' AND [DiscType] = '" + sDiscType + "') AND " +
                                    "([FrameNum] = '" + sFrameNum + "' OR [Sitting] = '" + sSitting + "')";

                                dbConns20.SQLQuery(sDiscProcessorConnString, sCommText, dTblDiscOrders);

                                if (dTblDiscOrders.Rows.Count > 0)
                                {
                                    sRefNum = Convert.ToString(dTblDiscOrders.Rows[0]["RefNum"]).Trim();
                                    iResubmitCount = Convert.ToInt32(dTblDiscOrders.Rows[0]["ResubmitCount"]);
                                    iResubmitCount += 1;

                                    DataTable dTblFrameData = new DataTable("dTblFrameData");
                                    sCommText = "SELECT * FROM [FrameData] WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";

                                    dbConns20.SQLQuery(sDiscProcessorConnString, sCommText, dTblFrameData);

                                    if (dTblFrameData.Rows.Count > 0)
                                    {
                                        sCommText = "DELETE FROM [FrameData] WHERE [ProdNum] = '" + sProdNum + "' AND [FrameNum] = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                                        bool bSuccess1 = false;

                                        dbConns20.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess1);

                                        if (bSuccess1 == true)
                                        {
                                            bool bDeleted = false;
                                            bool bDirExists = false;
                                            taskMethods20.DeleteRenderedDirectoryAndFiles(sProdNum, sFrameNum, ref bDeleted, sDiscType, ref bDirExists, dataSetMain, sSitting, sRefNum);

                                            if (bDirExists == false || bDeleted == true)
                                            {
                                                foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                                                {
                                                    string sDOPackagtag = Convert.ToString(dRowDiscOrders["Packagetag"]).Trim();
                                                    string sDOSitting = Convert.ToString(dRowDiscOrders["Sitting"]);
                                                    string sDORefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();

                                                    DataTable dTblItems = new DataTable("dTblItems");
                                                    sCommText = "SELECT Packagetag FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                                                    dbConns20.CDSQuery(sCDSConnString, sCommText, dTblItems);

                                                    if (dTblItems.Rows.Count > 0)
                                                    {
                                                        string sItemsPackagetag = Convert.ToString(dTblItems.Rows[0]["Packagetag"]).Trim();

                                                        if (sItemsPackagetag.Length > 0 && sItemsPackagetag != "")
                                                        {
                                                            DataTable dTblFrames = new DataTable("dTblFrames");
                                                            sCommText = "SELECT Sitting FROM Frames WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sFrameNum.TrimStart('0').Trim();

                                                            dbConns20.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                                                            if (dTblFrames.Rows.Count > 0)
                                                            {
                                                                string sFramesSitting = Convert.ToString(dTblFrames.Rows[0]["Sitting"]);

                                                                if (sFramesSitting.Length > 0 && sFramesSitting != "")
                                                                {
                                                                    sCommText = "UPDATE [DiscOrders] SET [Status] = '10', [LastCheck] = '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() +
                                                                        "', [ResubmitCount] = '" + iResubmitCount + "', [Error] = '0', [ErrorChecked] = '0', [ErrorDescription] = '', [Packagetag] = '" +
                                                                        sItemsPackagetag + "', [Sitting] = '" + sFramesSitting + "' WHERE ProdNum = '" +
                                                                        sProdNum + "' AND FrameNum = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                                                                    bool bSuccess2 = false;

                                                                    dbConns20.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess2);

                                                                    if (bSuccess2 == true)
                                                                    {
                                                                        sCommText = "UPDATE DP_Resubmits SET Status = 81 WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sFrameNum + " AND Disctype = '" + sDiscType + "'";
                                                                        bSuccess = false;

                                                                        dbConns20.CDSNonQuery(sCDSConnString, sCommText, ref bSuccess);

                                                                        if (bSuccess == true)
                                                                        {

                                                                        }
                                                                        else if (bSuccess != true)
                                                                        {
                                                                            sBreakPoint = string.Empty;
                                                                        }
                                                                    }
                                                                    else if (bSuccess2 != true)
                                                                    {
                                                                        string sBody = "A resubmit was submitted but the updating of the DiscOrders table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                                        this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                                    }
                                                                }
                                                                else if (sFramesSitting.Length == 0)
                                                                {
                                                                    string sBody = "A resubmit was submitted but the gathering of sitting data from the Frames table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                                    this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                                }
                                                            }
                                                            else if (dTblFrames.Rows.Count == 0)
                                                            {
                                                                string sBody = "A resubmit was submitted but the gathering of data from the Frames table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                                this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                            }
                                                        }
                                                        else if (sItemsPackagetag.Length == 0)
                                                        {
                                                            string sBody = "A resubmit was submitted but the gathering of packagetag data from the Items table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                            this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                        }
                                                    }
                                                    else if (dTblItems.Rows.Count == 0)
                                                    {
                                                        string sBody = "A resubmit was submitted but the gathering of data from the Items table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                        this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                    }
                                                }
                                            }
                                            else if (bDirExists != false && bDeleted != true)
                                            {
                                                string sBody = "A resubmit was submitted but the rendered directory could not be deleted for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                            }
                                        }
                                        else if (bSuccess1 != true)
                                        {
                                            string sBody = "A resubmit was submitted but the deleting of the FrameData records failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                            this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                        }
                                    }
                                    else if (dTblFrameData.Rows.Count == 0)
                                    {
                                        bool bDeleted = false;
                                        bool bDirExists = false;
                                        taskMethods20.DeleteRenderedDirectoryAndFiles(sProdNum, sFrameNum, ref bDeleted, sDiscType, ref bDirExists, dataSetMain, sSitting, sRefNum);

                                        foreach (DataRow dRowDiscOrders in dTblDiscOrders.Rows)
                                        {
                                            string sDOPackagtag = Convert.ToString(dRowDiscOrders["Packagetag"]).Trim();
                                            string sDOSitting = Convert.ToString(dRowDiscOrders["Sitting"]);
                                            string sDORefNum = Convert.ToString(dRowDiscOrders["RefNum"]).Trim();

                                            DataTable dTblItems = new DataTable("dTblItems");
                                            sCommText = "SELECT Packagetag FROM Items WHERE Lookupnum = '" + sProdNum + "'";

                                            dbConns20.CDSQuery(sCDSConnString, sCommText, dTblItems);

                                            if (dTblItems.Rows.Count > 0)
                                            {
                                                string sItemsPackagetag = Convert.ToString(dTblItems.Rows[0]["Packagetag"]).Trim();

                                                if ((sItemsPackagetag.Length > 0 && sItemsPackagetag != "") || bSittingBased == true)
                                                {
                                                    DataTable dTblFrames = new DataTable("dTblFrames");
                                                    sCommText = "SELECT Sitting FROM Frames WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sFrameNum.TrimStart('0').Trim();

                                                    dbConns20.CDSQuery(sCDSConnString, sCommText, dTblFrames);

                                                    if (dTblFrames.Rows.Count > 0)
                                                    {
                                                        string sFramesSitting = Convert.ToString(dTblFrames.Rows[0]["Sitting"]);

                                                        if (sFramesSitting.Length > 0 && sFramesSitting != "")
                                                        {
                                                            sCommText = "UPDATE [DiscOrders] SET [Status] = '10', [LastCheck] = '" + DateTime.Now.ToString("MM/dd/yy H:mm:ss").Trim() +
                                                                "', [ResubmitCount] = '" + iResubmitCount + "', [Error] = '0', [ErrorChecked] = '0', [ErrorDescription] = '', [Packagetag] = '" +
                                                                sItemsPackagetag + "', [Sitting] = '" + sFramesSitting + "' WHERE ProdNum = '" +
                                                                sProdNum + "' AND FrameNum = '" + sFrameNum + "' AND [DiscType] = '" + sDiscType + "'";
                                                            bool bSuccess2 = false;

                                                            dbConns20.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess2);

                                                            if (bSuccess2 == true)
                                                            {
                                                                sCommText = "UPDATE DP_Resubmits SET Status = 81 WHERE Lookupnum = '" + sProdNum + "' AND Sequence = " + sFrameNum + " AND Disctype = '" + sDiscType + "'";
                                                                bSuccess = false;

                                                                dbConns20.CDSNonQuery(sCDSConnString, sCommText, ref bSuccess);
                                                            }
                                                            else if (bSuccess2 != true)
                                                            {
                                                                string sBody = "A resubmit was submitted but the updating of the DiscOrders table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                                this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                            }
                                                        }
                                                        else if (sFramesSitting.Length == 0)
                                                        {
                                                            string sBody = "A resubmit was submitted but the gathering of sitting data from the Frames table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                            this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                        }
                                                    }
                                                    else if (dTblFrames.Rows.Count == 0)
                                                    {
                                                        string sBody = "A resubmit was submitted but the gathering of data from the Frames table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                        this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                    }
                                                }
                                                else if ((sItemsPackagetag.Length == 0 || sItemsPackagetag == "") && bSittingBased != true)
                                                {
                                                    string sBody = "A resubmit was submitted but the gathering of packagetag data from the Items table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                    this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                                }
                                            }
                                            else if (dTblItems.Rows.Count == 0)
                                            {
                                                string sBody = "A resubmit was submitted but the gathering of data from the Items table failed for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                                this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                            }
                                        }
                                    }
                                }
                                else if (dTblDiscOrders.Rows.Count == 0)
                                {
                                    string sBody = "A resubmit was submitted but the was not located in the DiscOrders table for ProdNum: " + sProdNum + " FrameNum: " + sFrameNum + ".";

                                    this.EmailAndStatusUpdatesForResubmits(sProdNum, sFrameNum, sBody, dataSetMain);
                                }
                            }
                            else if (dRowGatheredPattern01.Length == 0)
                            {
                                sBreakPoint = string.Empty;
                            }
                        }
                    }
                    else if (dTblResubmits.Rows.Count == 0)
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
                dbConns20.SaveExceptionToDB(ex);
            }
        }

        private void GatherNotificationVariables(ref string sEmailServer, ref string sCCSendTo, ref string sSendTo, ref string sTextMsgSendTo, ref bool bSuccess, DataSet dataSetMain, ref bool bSendNotifications, ref bool bSendLocalEmailNotification, ref bool bSendRemoteEmailNotification, ref bool bSendPhoneTextNotification)
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

                dbConns20.SaveExceptionToDB(ex);
            }
        }

        private void SendNotification(string sEmailServer, string sCCSendTo, string sSendTo, string sSubject, string sBody, string sTextMsgSendTo, DataSet dataSetMain)
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
                dbConns20.SaveExceptionToDB(ex);
            }
        }

        private void EmailAndStatusUpdatesForResubmits(string sProdNum, string sFrameNum, string sBody, DataSet dataSetMain)
        {
            try
            {
                string sSubject = "DiscProcessor error notification.";

                this.SendNotification(sEmailServer, sCCSendTo, sSendTo, sSubject, sBody, sTextMsgSendTo, dataSetMain);

                string sCommText = "UPDATE [DiscOrders] SET [Status] = '91' WHERE ProdNum = '" + sProdNum + "' AND FrameNum = '" + sFrameNum + "'";
                bool bSuccess4 = false;

                dbConns20.SQLNonQuery(sDiscProcessorConnString, sCommText, ref bSuccess4);

                if (bSuccess4 == true)
                {
                    sCommText = "UPDATE DP_Resubmits SET Status = 82 WHERE lookupnum = '" + sProdNum + "' AND sequence = " + sFrameNum + "";
                    bool bSuccess5 = false;

                    dbConns20.CDSNonQuery(sCDSConnString, sCommText, ref bSuccess5);

                    if (bSuccess5 == true)
                    {
                        // Done.
                    }
                    else if (bSuccess5 != true)
                    {
                        sBreakPoint = string.Empty;
                    }
                }
                else if (bSuccess4 != true)
                {
                    sBreakPoint = string.Empty;
                }
            }
            catch (Exception ex)
            {
                dbConns20.SaveExceptionToDB(ex);
            }
        }                

    }
}
