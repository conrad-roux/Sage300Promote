using CRMIntegration;
using Sage.CRM.Data;
using System;
using System.Net.Http;
using System.Text;

namespace Common
{
    public class CommonPage
    {
        public const string BaseUrl = "/Sage300/OnPremise/";

        public const string ProxyUrl = "/WebScreenProxy/Home/ProxyRequest/?appUrl=";

        public const string Parameters = "&companyId={0}&userId={1}";

        public const string PostParameters = "&companyId={0}&userId={1}&userKey={2}&timeout=120";

        public const string LoginUserCookieName = "LoginProductUserId";

        public const string OpportunityFormat = "<input id='txtOpportunityID' name='txtOpportunityID' type='hidden' value='{0}'>";

        public const string HostFormat = "<input id='txtHostNameID' name='txtHostNameID' type='hidden' value='{0}'>";

        public const string HtmliFrame = "<iframe id='Sage300PageFrame' sandbox='allow-forms allow-popups allow-pointer-lock allow-same-origin allow-scripts allow-top-navigation' width='100%' height='700' frameBorder='0'></iframe>";

        public const string HtmlLoading = "<div id='sage300Loading' style='width:100%; text-align:center'><img src='../Themes/img/ergonomic/sage300loading.gif'></div>";

        public static HttpClient Sage300HttpClinet;

        public CommonPage()
        {
        }

        public static string Base64Decode(string base64EncodedData)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }

        public static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public static string BuildDatabaseDropDwon(bool bSynchronized)
        {
            string str = "<SELECT CLASS=EDIT SIZE=1 ID='dropdownDatabaseId' NAME='database' style='width:200px; height:25px'>";
            string str1 = " Select * From AccPacConfig where AccP_Deleted is NULL order by AccP_database";
            QuerySelect querySelect = new QuerySelect();
            querySelect.SQLCommand = str1;
            querySelect.ExecuteReader();
            while (!querySelect.Eof())
            {
                string str2 = (querySelect.FieldValue("accp_default") == "Y" ? " SELECTED" : "");
                string str3 = querySelect.FieldValue("accp_description");
                string str4 = querySelect.FieldValue("AccP_database");
                string str5 = "<OPTION VALUE='{0}'{1}>{2}</OPTION>";
                str = string.Concat(str, string.Format(str5, str4, str2, str3));
                querySelect.Next();
            }
            str = string.Concat(str, "</SELECT></br>");
            return str;
        }

        public static string GetConfigField(string field, string companyDatabase)
        {
            string str = string.Format("SELECT * \n                                      FROM AccpacConfig \n                                      WHERE accp_deleted IS NULL \n                                      AND accp_database='{0}'", companyDatabase);
            QuerySelect querySelect = new QuerySelect();
            querySelect.SQLCommand =str;
            QuerySelect querySelect1 = querySelect;
            querySelect1.ExecuteReader();
            if (querySelect1.Eof())
            {
                return string.Empty;
            }
            return querySelect1.FieldValue(field);
        }

        public static string GetOpportunityTabInfo(string logonUser, string companyDb, string compId, string emailSubject)
        {
            string configField = CommonPage.GetConfigField("AccP_CompEmail", companyDb);
            CommonPage.GetConfigField("AccP_ServerName", companyDb);
            string str = "";
            string str1 = string.Concat("Select emai_emailaddress as email from Email e inner join EmailLink el on e.Emai_EmailId = el.ELink_EmailId where el.ELink_EntityId = 5 and el.ELink_RecordID = ", Convert.ToInt32(compId));
            QuerySelect querySelect = new QuerySelect();
            querySelect.SQLCommand = str1;
            querySelect.ExecuteReader();
            if (!querySelect.Eof())
            {
                str = querySelect.FieldValue("email");
            }
            string str2 = string.Concat("Select Oppo_OpportunityId, Oppo_Description, Oppo_PrimaryCompanyId, Oppo_PrimaryPersonId From Opportunity where Oppo_Status = 'In Progress' and Oppo_PrimaryCompanyId = ", Convert.ToInt32(compId));
            string str3 = string.Format("{0}|{1}|{2}|{3}&", new object[] { logonUser, str, configField, emailSubject });
            QuerySelect querySelect1 = new QuerySelect();
            querySelect1.SQLCommand = str2;
            querySelect1.ExecuteReader();
            while (!querySelect1.Eof())
            {
                string str4 = querySelect1.FieldValue("Oppo_OpportunityId");
                string str5 = querySelect1.FieldValue("Oppo_Description");
                string str6 = querySelect1.FieldValue("Oppo_PrimaryCompanyId");
                string str7 = querySelect1.FieldValue("Oppo_PrimaryPersonId");
                if (string.IsNullOrEmpty(str7))
                {
                    str7 = "0";
                }
                string str8 = string.Format("{0},{1},{2},{3}", new object[] { str4, str5, str6, str7 });
                str3 = string.Concat(str3, str8, "|");
                querySelect1.Next();
            }
            str3 = str3.Substring(0, str3.Length - 1);
            return string.Format("<input id='txtOpportunityID' name='txtOpportunityID' type='hidden' value='{0}'>", str3);
        }

        public static string GetRequestUrl(string companyId, string userId, string appUrl)
        {
            return string.Concat(CommonPage.GetConfigField("AccP_ServerName", companyId), "/WebScreenProxy/Home/ProxyRequest/?appUrl=", Uri.EscapeDataString(appUrl));
        }

        public static string GetUniqueDatabaseId()
        {
            string str = "Select AccP_Database From AccPacConfig where AccP_Deleted is NULL";
            QuerySelect querySelect = new QuerySelect();
            querySelect.SQLCommand = str;
            querySelect.ExecuteReader();
            string str1 = "";
            int num = 0;
            while (!querySelect.Eof())
            {
                num++;
                str1 = querySelect.FieldValue("AccP_database");
                if (num > 1)
                {
                    break;
                }
                querySelect.Next();
            }
            if (num != 1)
            {
                return "";
            }
            return str1;
        }

        public static string NewOpportunity(string companyId, int userId, string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                personId = "0";
            }
            Record record = new Record("Opportunity");
            record.SetField("Oppo_Stage", "Lead");
            record.SetField("Oppo_Status", "In Progress");
            record.SetField("Oppo_PrimaryCompanyId", companyId);
            record.SetField("Oppo_PrimaryPersonId", personId);
            record.SetField("Oppo_AssignedUserId", userId);
            record.SetField("Oppo_Source", "Telephone");
            record.SetField("Oppo_Certainty", 0);
            int num = 1;
            string str = "SELECT * FROM Custom_SysParams WHERE Parm_Name='BaseCurrency' AND Parm_Deleted IS NULL";
            QuerySelect querySelect = new QuerySelect();
            querySelect.SQLCommand = str;
            QuerySelect querySelect1 = querySelect;
            querySelect1.ExecuteReader();
            if (!querySelect1.Eof())
            {
                num = Convert.ToInt32(querySelect1.FieldValue("Parm_Value"));
            }
            record.SetField("oppo_forecast_CID", num);
            record.SetField("oppo_forecast", 0);
            record.SetField("oppo_total_CID", num);
            record.SetField("oppo_total", 0);
            record.SetField("oppo_QuotePending_CID", num);
            record.SetField("oppo_QuotePending", 0);
            record.SetField("oppo_totalorders_CID", num);
            record.SetField("oppo_totalorders", 0);
            record.SetField("oppo_totalshipments_CID", num);
            record.SetField("oppo_totalshipments", 0);
            record.SetField("oppo_totalinvoices_CID", num);
            record.SetField("oppo_totalinvoices", 0);
            record.SetField("Oppo_Opened", DateTime.Now);
            record.SetField("oppo_targetclose", DateTime.Now.AddDays(30));
            record.SaveChanges();
            record.SetWorkflowInfo("Opportunity Workflow", "Lead");
            record.SetField("Oppo_Description", record.RecordId);
            record.SetField("oppo_note", record.RecordId);
            record.SaveChanges();
            return string.Format("{0},{1},{2},{3}", new object[] { record.RecordId, record.RecordId, companyId, personId });
        }

        public static string PostRequestUrl(string companyId, string userId, string encryptedUserkey)
        {
            string str = CRMCrypto.Encrypt(CRMCheckFormat.CheckFormat(encryptedUserkey), companyId, userId);
            string str1 = string.Concat(CommonPage.GetConfigField("AccP_ServerName", companyId), "/WebScreenProxy/Home/ProxyRequest/?appUrl=");
            string str2 = CommonPage.Base64Encode(str);
            string str3 = string.Format("&companyId={0}&userId={1}&userKey={2}&timeout=120", companyId, userId, str2);
            return string.Concat(str1, str3);
        }

        public static string SqlEscapeQuotes(string s)
        {
            return s.Replace("'", "''");
        }
    }
}