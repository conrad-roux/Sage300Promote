using Common;
using CRMIntegration;
using Sage.CRM.Data;
using Sage.CRM.HTML;
using Sage.CRM.UI;
using Sage.CRM.Utils;
using Sage.CRM.WebObject;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DebugPromote
{
    public class PromoteCompany : Web
    {
        private const string EndPointUrl = "{0}/Sage300WebApi/v1.0/-/{1}/";

        public const string PromoteRecord = "<input id='txtPromoteRecord' name='txtPromoteRecord' type='hidden' value='{0}'>";

        public static HttpClientHandler Sage300HttpClientHandler;

        public static HttpClient Sage300HttpClient;

        private string _database = "";

        private string _comp_type = "";

        private string _comp_companyid = "";

        private string _comp_name = "";

        private string option = "";

        private string _Sage300Field = "comp_idcust";

        private string sage300HostName = "http://localhost";

        private string userId = "";

        private string userKey = "";

        private string encryptedUserkey = "";

        static PromoteCompany()
        {
        }

        public PromoteCompany()
        {
            this.option = base.Dispatch.EitherField("Option");
        }

        public override void BuildContents()
        {
            LogMessage("ConradDebug","In Build",1);

            string upper = (base.Dispatch.EitherField("Option") ?? "Customer").ToUpper();
            string str = base.Dispatch.EitherField("HiddenMode");
            this._comp_companyid = base.GetContextInfo("Company", "comp_companyid");
            this._comp_name = base.GetContextInfo("Company", "Comp_Name");
            this.userId = base.GetContextInfo("User", "User_AccpacID");
            this.encryptedUserkey = base.GetContextInfo("User", "User_AccpacPSWD");
            this.userKey = CRMCheckFormat.CheckFormat(this.encryptedUserkey);
            base.AddContent("<div id='sage300Loading' style='width:100%; text-align:center'><img src='../Themes/img/ergonomic/sage300loading.gif'></div>");
            if (upper == "FINALIZELINK")
            {
                Record record = base.FindRecord("Company", string.Format("Comp_CompanyId = {0}", this._comp_companyid));
                bool flag = false;
                string fieldAsString = record.GetFieldAsString("comp_idcust");
                string fieldAsString1 = record.GetFieldAsString("comp_idvend");
                string str1 = "";
                if (string.IsNullOrEmpty(fieldAsString) || !string.IsNullOrEmpty(fieldAsString1))
                {
                    str1 = fieldAsString1;
                }
                else
                {
                    flag = true;
                    str1 = fieldAsString;
                }
                record.GetFieldAsString("Comp_Name");
                this._database = record.GetFieldAsString("comp_database");
                this.sage300HostName = CommonPage.GetConfigField("AccP_ServerName", this._database);
                PromoteCompany.Sage300HttpClientHandler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(this.userId, this.userKey)
                };
                if (this.CheckExist(str1, (flag ? "ARCustomers" : "APVendors")))
                {
                    record.SetField("Comp_Database", "");
                    record.SetField("Comp_IdCust", "");
                    record.SetField("Comp_IdVend", "");
                    record.SaveChanges();
                    string str2 = this.Url("200");
                    base.Dispatch.Redirect(str2);
                    return;
                }
                record.SetField("Comp_IdGrp", base.Dispatch.ContentField("GroupCode"));
                record.SetField("Comp_CodeTaxGrp", base.Dispatch.ContentField("TaxGroup"));
                record.SetField("Comp_CodeTerm", base.Dispatch.ContentField("TermsCode"));
                record.SetField("Comp_Status", base.Dispatch.ContentField("Status"));
                record.SetField("Comp_AmtCrLimt", base.Dispatch.ContentField("CreditLimit"));
                record.SaveChanges();
                string str3 = this.Url("200");
                base.Dispatch.Redirect(str3);
                return;
            }
            if (upper.ToUpper() == "PROMOTE" && string.IsNullOrEmpty(str))
            {
                this.DisplayPromoteForm(upper);
            }
            this.GetTabs();
            if (str == "PromoteCustomer" || str == "PromoteVendor")
            {
                this._database = base.Dispatch.ContentField("database");
                bool flag1 = str == "PromoteCustomer";
                string str4 = base.Dispatch.ContentField("txtId");
                this._comp_type = (flag1 ? "Customer" : "Vendor");
                this._Sage300Field = (flag1 ? "comp_idcust" : "comp_idvend");
                string contextInfo = base.GetContextInfo("Company", "Comp_CompanyId");
                Record record1 = base.FindRecord("Company", string.Concat("Comp_CompanyId=", contextInfo));
                if (string.IsNullOrEmpty(record1.GetFieldAsString("Comp_database")))
                {
                    record1.SetField("Comp_database", this._database);
                    record1.SaveChanges();
                }
                string str5 = CommonPage.PostRequestUrl(this._database, this.userId, this.encryptedUserkey);
                base.AddContent(base.HTML.InputHidden("postRequestUrl", str5));
                this.sage300HostName = CommonPage.GetConfigField("AccP_ServerName", this._database);
                PromoteCompany.Sage300HttpClientHandler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(this.userId, this.userKey)
                };
                if (string.IsNullOrEmpty(str4))
                {
                    base.Dispatch.Redirect(base.UrlDotNet("PromoteCompany", "GetPromotePage&Option=PROMOTE&T=Company&ErrCode=1"));
                    return;
                }
                if (!this.CheckExist(str4, (flag1 ? "ARCustomers" : "APVendors")))
                {
                    base.Dispatch.Redirect(base.UrlDotNet("PromoteCompany", string.Format("GetPromotePage&Option=PROMOTE&T=Company&ErrCode=2&id={0}", str4)));
                    return;
                }
                string str6 = string.Concat("Select ainq_url From AccpacInquiry where ainq_module = '", (flag1 ? "AR1399" : "AP1299"), "'");
                QuerySelect querySelect = new QuerySelect();
                querySelect.SQLCommand = str6;
                QuerySelect querySelect1 = querySelect;
                querySelect1.ExecuteReader();
                string str7 = "";
                if (!querySelect1.Eof())
                {
                    str7 = querySelect1.FieldValue("ainq_url");
                }
                base.AddContent("<iframe id='Sage300PageFrame' sandbox='allow-forms allow-popups allow-pointer-lock allow-same-origin allow-scripts allow-top-navigation' width='100%' height='700' frameBorder='0'></iframe>");
                string str8 = string.Concat(str7, str4);
                string requestUrl = CommonPage.GetRequestUrl(this._database, this.userId, str8);
                base.AddContent(string.Format("<a id='Sage300PageLinkId' href={0}></a>", requestUrl));
                string str9 = base.UrlDotNet("PromoteCompany", "GetPromotePage&Option=FinalizeLink");
                base.AddContent(string.Concat("<a href=", str9, " target='_blank' id='linkFinalizeLink' style='display: none;'><a>"));
                string str10 = string.Format("<input id='txtHostNameID' name='txtHostNameID' type='hidden' value='{0}'>", string.Concat("http://", base.Dispatch.Host, "|", this.sage300HostName));
                base.AddContent(str10);
                string customerOrVendor = this.GetCustomerOrVendor(this._comp_companyid, flag1, this._database, str4);
                base.AddContent(customerOrVendor);
                string str11 = this.Url("200");
                base.AddContent(base.HTML.InputHidden("SummaryPageUrl", str11));
            }
            else if (str == "UnlinkCustomer" || str == "UnlinkVendor")
            {
                this._database = base.GetContextInfo("Company", "comp_database");
                bool flag2 = str == "UnlinkCustomer";
                this._comp_type = base.GetContextInfo("Company", "comp_type");
                this._Sage300Field = (flag2 ? "comp_idcust" : "comp_idvend");
                if (!flag2)
                {
                    base.Metadata.GetTranslation("ainq_selection", "apVendor");
                }
                else
                {
                    base.Metadata.GetTranslation("ainq_selection", "arCustomer");
                }
                this.sage300HostName = CommonPage.GetConfigField("AccP_ServerName", this._database);
                PromoteCompany.Sage300HttpClientHandler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(this.userId, this.userKey)
                };
                if (this.CheckExist(this._comp_companyid, "OEOrders"))
                {
                    this.UnlinkCompany(Convert.ToInt32(this._comp_companyid));
                    string str12 = string.Format(base.Metadata.GetTranslation("Company_Promote", "ConfirmMessage"), this._comp_companyid);
                    base.AddContent(string.Format("<h3>{0}</h3>", str12));
                    string str13 = this.Url("200");
                    base.Dispatch.Redirect(str13);
                }
                else
                {
                    this.DisplayCustomerOrVendorExist();
                }
            }
            string str14 = string.Format("{0}/WebScreenProxy/Home/CreateSage300Cookies?userId={1}&companyId={2}", this.sage300HostName, this.userId, this._database);
            base.AddContent(string.Format("<iframe id='Sage300ProxyCookieFrame' src='{0}' height='0' frameBorder='0'></iframe>", str14));
            base.AddContent("<script type='text/javascript' src='../CustomPages/Sage300Integration/PromoteCompany.js'></script>");
        }

        private string BuildDatabaseDropDown(string companyid)
        {
            string str = "<SELECT CLASS=EDIT SIZE=1 ID='database' NAME='database'>";
            string contextInfo = base.GetContextInfo("Company", "Comp_CompanyId");
            string fieldAsString = base.FindRecord("Company", string.Concat("Comp_CompanyId=", contextInfo)).GetFieldAsString("Comp_database");
            if (!string.IsNullOrEmpty(fieldAsString))
            {
                string str1 = "<OPTION VALUE='{0}'{1}>{2}</OPTION>";
                string fieldAsString1 = base.FindRecord("AccPacConfig", string.Concat("AccP_database='", fieldAsString, "'")).GetFieldAsString("accp_description");
                str = string.Concat(str, string.Format(str1, fieldAsString, "", fieldAsString1));
            }
            else
            {
                Record record = base.FindRecord("AccPacConfig", "accp_synchronized='Y'");
                record.OrderBy = "AccP_database";
                while (!record.Eof())
                {
                    string str2 = "";
                    if (companyid != string.Empty)
                    {
                        str2 = (record.GetFieldAsString("AccP_database") == companyid ? " SELECTED" : "");
                    }
                    else
                    {
                        str2 = (record.GetFieldAsString("accp_default") == "Y" ? " SELECTED" : "");
                    }
                    string fieldAsString2 = record.GetFieldAsString("accp_description");
                    string fieldAsString3 = record.GetFieldAsString("AccP_database");
                    string str3 = "<OPTION VALUE='{0}'{1}>{2}</OPTION>";
                    str = string.Concat(str, string.Format(str3, fieldAsString3, str2, fieldAsString2));
                    record.GoToNext();
                }
            }
            str = string.Concat(str, "</SELECT> </br>");
            return str;
        }

        private bool CheckExist(string id, string option = "OEOrders")
        {
            string str = string.Format("{0}/Sage300WebApi/v1.0/-/{1}/", this.sage300HostName, this._database);
            string str1 = "";
            if (option == "OEOrders")
            {
                str1 = string.Concat(str, string.Format("OE/OEOrders?$count=true&$top=1&$filter=SageCRMCompanyID eq {0}", id));
            }
            else if (option == "ARCustomers")
            {
                str1 = string.Concat(str, string.Format("AR/ARCustomers?$count=true&$top=1&$filter=CustomerNumber eq '{0}'", id));
            }
            else if (option == "APVendors")
            {
                str1 = string.Concat(str, string.Format("AP/APVendors?$count=true&$top=1&$filter=VendorNumber eq '{0}'", id));
            }
            else if (option == "CRMCompany")
            {
                str1 = string.Concat(str, string.Format("OE/OEOrders?$count=true&$top=1&$filter=SageCRMCompanyID eq {0} and CustomerNumber eq 'CRM999999999'", id));
            }
            string str2 = "";
            dynamic obj = this.SendRequest(new HttpMethod("GET"), str1, ref str2);
            if (obj == (dynamic)null)
            {
                base.AddContent(string.Format("<br/><br/><h4>{0}</h4><br/>", str2));
                base.LogMessage(string.Format("Sage300_Promote_{0}_Log", this._comp_type), str2, 5);
                return false;
            }
            if (obj["@odata.count"] > 0)
            {
                return false;
            }
            return true;
        }

        private void DisplayCustomerOrVendorExist()
        {
            string translation = base.Metadata.GetTranslation("Accpac_Messages", "ErrUnLink");
            translation = string.Format(translation, this._comp_type, this._database);
            base.LogMessage(string.Format("Sage300_Promote_{0}_Log", this._comp_type), translation, 5);
            base.AddContent("<TABLE WIDTH=100% BORDER=0 CELLPADDING=0 CELLSPACING=0 RULES=NONE>");
            base.AddContent("<TR class='GridHeader'><TD COLSPAN=3 class='TABLEHEADBLANK'>");
            base.AddContent("<TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH='100%'><TR>");
            base.AddContent("<TD VALIGN=BOTTOM class='PanelCorners'>");
            base.AddContent("<IMG SRC='../Themes/img/ergonomic/backgrounds/paneleftcorner.jpg' HSPACE=0 BORDER=0 ALIGN=TOP></TD>");
            base.AddContent(string.Format("<TD NOWRAP = TRUE VALIGN = BOTTOM CLASS = PANEREPEAT>{0}</TD >", base.Metadata.GetTranslation("Accpac_Messages", "ApplicationStatus")));
            base.AddContent("<TD class='PanelCorners' VALIGN=BOTTOM>");
            base.AddContent("<IMG SRC='../Themes/img/ergonomic/backgrounds/panerightcorner.gif' HSPACE=0 BORDER=0 ALIGN=TOP></TD>");
            base.AddContent("</TR></TABLE>");
            base.AddContent("</TD></TR>");
            base.AddContent("<TR CLASS=CONTENT>");
            base.AddContent("<TD WIDTH=1px CLASS=TABLEBORDERLEFT><IMG SRC='../Themes/img/ergonomic/backgrounds/tabletopborder.gif' HSPACE=0 BORDER=0 ALIGN=TOP></TD>");
            base.AddContent("<TD HEIGHT=100% WIDTH=100% CLASS=VIEWBOXCAPTION>");
            base.AddContent("<TABLE CLASS=CONTENTGRID cellspacing=0 cellpadding=1 width=100%><TR>");
            base.AddContent(string.Format("<TD CLASS='GRIDHEAD WIDTH=200'>{0}</TD>", base.Metadata.GetTranslation("ColNames", "StatusType")));
            base.AddContent(string.Format("<TD CLASS='GRIDHEAD WIDTH=200'>{0}</TD>", base.Metadata.GetTranslation("ColNames", "ApplicationMessage")));
            base.AddContent("</TR>");
            string str = "ROW1";
            base.AddContent("<TR>");
            base.AddContent(string.Format("<TD CLASS={0}>{1}</TD>", str, base.Metadata.GetTranslation("Errors", "Error")));
            base.AddContent(string.Format("<TD CLASS={0}>{1}</TD>", str, translation));
            base.AddContent("</TR>");
            base.AddContent("</TABLE>");
            base.AddContent("</TD>");
            base.AddContent("<TD WIDTH=1px CLASS=TABLEBORDERRIGHT><IMG SRC='../Themes/img/ergonomic/backgrounds/tabletopborder.gif' HSPACE=0 BORDER=0 ALIGN=TOP></TD>");
            base.AddContent("</TR>");
        }

        private void DisplayPromoteForm(string option)
        {
            string str = base.Dispatch.EitherField("ErrCode");
            string str1 = base.Dispatch.EitherField("id");
            string str2 = "";
            if (str == "1")
            {
                str2 = string.Format(base.Metadata.GetTranslation("Company_Promote", "EnterId"), base.Metadata.GetTranslation("Company_Promote", this._comp_type));
            }
            else if (str == "2")
            {
                str2 = string.Format(base.Metadata.GetTranslation("Company_Promote", "Exists"), this._comp_type, str1, this._database);
            }
            if (!string.IsNullOrEmpty(str2))
            {
                base.AddContent(string.Format("<br/><br/><h4>{0}</h4><br/>", str2));
            }
            base.AddContent(base.HTML.Form());
            string contextInfo = base.GetContextInfo("Company", "comp_database");
            string contextInfo1 = base.GetContextInfo("Company", "comp_CompanyId");
            Record record = base.FindRecord("Company", string.Format("comp_CompanyId = '{0}'", contextInfo1));
            string fieldAsString = record.GetFieldAsString("Comp_IdCust");
            string fieldAsString1 = record.GetFieldAsString("Comp_IdVend");
            bool empty = fieldAsString != string.Empty;
            bool flag = fieldAsString1 != string.Empty;
            string str3 = "";
            base.AddContent(base.HTML.InputHidden("HiddenMode", ""));
            if (!(empty | flag))
            {
                string translation = base.Metadata.GetTranslation("Company_Promote", "SelectAccpacDatabase");
                base.AddContent(string.Concat("<h3>", translation, "</h3>"));
                string str4 = string.Concat(base.Metadata.GetTranslation("Company_Promote", "CompanyName"), ":");
                str3 = string.Concat("<h4>", str4, "<h4>");
                str3 = string.Concat(str3, this.BuildDatabaseDropDown(contextInfo));
                string str5 = "";
                string str6 = string.Format("<h4>{0} / {1}</h4><div><input type ='textbox' id='txtId' name='txtId' value='{2}' maxlength='12'></div><br />", base.Metadata.GetTranslation("ColNames", "Comp_IdCust"), base.Metadata.GetTranslation("ColNames", "Comp_IdVend"), str5);
                str3 = string.Concat(str3, str6);
                string str7 = string.Format(base.Metadata.GetTranslation("Company_Promote", "PromoteEntity"), base.Metadata.GetTranslation("ainq_selection", "arCustomer"));
                string str8 = string.Format(base.Metadata.GetTranslation("Company_Promote", "PromoteEntity"), base.Metadata.GetTranslation("ainq_selection", "apVendor"));
                PromoteCompany.Sage300HttpClientHandler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(this.userId, this.userKey)
                };
                bool flag1 = true;
                if (!string.IsNullOrEmpty(contextInfo))
                {
                    this._database = contextInfo;
                    flag1 = this.CheckExist(contextInfo1, "CRMCompany");
                }
                base.AddSubmitButton(str7, "Save.gif", "javascript:document.EntryForm.HiddenMode.value='PromoteCustomer';$('#sage300Loading').show();");
                if (flag1)
                {
                    base.AddSubmitButton(str8, "Save.gif", "javascript:document.EntryForm.HiddenMode.value='PromoteVendor';$('#sage300Loading').show();");
                }
            }
            else
            {
                string str9 = (empty ? base.Metadata.GetTranslation("ainq_selection", "arCustomer") : base.Metadata.GetTranslation("ainq_selection", "apVendor"));
                string str10 = (empty ? fieldAsString : fieldAsString1);
                string fieldAsString2 = record.GetFieldAsString("Comp_IdGrp");
                string fieldAsString3 = record.GetFieldAsString("Comp_CodeTaxGrp");
                string fieldAsString4 = record.GetFieldAsString("Comp_CodeTerm");
                string str11 = (empty ? base.Metadata.GetTranslation("ColNames", "Comp_IdCust") : base.Metadata.GetTranslation("ColNames", "Comp_IdVend"));
                string str12 = string.Format(base.Metadata.GetTranslation("Company_Promote", "Integration"), str9);
                base.AddContent(string.Concat("<h3>", str12, "</h3>"));
                str3 = string.Concat(str3, string.Format("<h4>{0}</h4><input type ='textbox' id='{1}' name='{2}' value='{3}' readonly><br />", new object[] { str11, "id", "id", str10 }));
                str3 = string.Concat(str3, string.Format("<h4>{0}</h4><input type ='textbox' id='{1}' name='{2}' value='{3}' readonly><br />", new object[] { base.Metadata.GetTranslation("ColNames", "Comp_IdGrp"), "groupcode", "groupcode", fieldAsString2 }));
                str3 = string.Concat(str3, string.Format("<h4>{0}</h4><input type ='textbox' id='{1}' name='{2}' value='{3}' readonly><br />", new object[] { base.Metadata.GetTranslation("ColNames", "Comp_CodeTaxGrp"), "taxgroup", "taxgroup", fieldAsString3 }));
                str3 = string.Concat(str3, string.Format("<h4>{0}</h4><input type ='textbox' id='{1}' name='{2}' value='{3}' readonly><br />", new object[] { base.Metadata.GetTranslation("ColNames", "Comp_CodeTerm"), "termscode", "termscode", fieldAsString4 }));
                string str13 = (empty ? "UnlinkCustomer" : "UnlinkVendor");
                base.AddConfirmButton(string.Format(base.Metadata.GetTranslation("Company_Promote", "UnlinkEntity"), str9), "Save.gif", base.Metadata.GetTranslation("Company_Promote", "ConfirmUnlink"), "HiddenMode", str13);
            }
            HorizontalPanel horizontalPanel = new HorizontalPanel();
            horizontalPanel.HTMLId = "ImportPageId";
            HorizontalPanel horizontalPanel1 = horizontalPanel;
            horizontalPanel1.AddAttribute("width", "100%");
            horizontalPanel1.AddAttribute("valign", "bottom");
            ContentBox contentBox = new ContentBox();
            HTMLString hTMLString = new HTMLString();
            hTMLString.Html = str3;
            contentBox.Inner = hTMLString;
            horizontalPanel1.Add(contentBox);
            base.AddContent(horizontalPanel1);
        }

        private string GetCustomerOrVendor(string compId, bool isCustomer, string database, string id)
        {
            string str = (isCustomer ? "AR" : "AP");
            string str1 = (isCustomer ? "Customer" : "Vendor");
            string contextInfo = base.GetContextInfo("Company", "Comp_Name");
            string str2 = contextInfo.Substring(0, Math.Min(10, contextInfo.Length));
            string contextInfo1 = base.GetContextInfo("Company", "Comp_EmailAddress");
            string contextInfo2 = base.GetContextInfo("Company", "Comp_PhoneAreaCode");
            string contextInfo3 = base.GetContextInfo("Company", "Comp_PhoneNumber");
            contextInfo3 = string.Concat(contextInfo2, contextInfo3);
            contextInfo3 = new string(contextInfo3.Where<char>(new Func<char, bool>(char.IsDigit)).ToArray<char>());
            string str3 = base.GetContextInfo("Company", "Comp_FaxCountryCode");
            string contextInfo4 = base.GetContextInfo("Company", "Comp_FaxAreaCode");
            string str4 = base.GetContextInfo("Company", "Comp_FaxNumber");
            str4 = string.Concat(str3, contextInfo4, str4);
            str4 = new string(str4.Where<char>(new Func<char, bool>(char.IsDigit)).ToArray<char>());
            string contextInfo5 = base.GetContextInfo("Company", "Comp_WebSite");
            string str5 = "";
            string str6 = "";
            string str7 = "";
            string str8 = "";
            string str9 = "";
            string str10 = "";
            string str11 = "";
            string str12 = "";
            string str13 = "";
            string str14 = "";
            string str15 = "";
            string str16 = "";
            string str17 = string.Format("SELECT * FROM vListPerson WHERE Pers_CompanyId = {0} AND PeLi_Type='{1}Contact'", compId, str);
            QuerySelect querySelect = new QuerySelect();
            querySelect.SQLCommand = str17;
            QuerySelect querySelect1 = querySelect;
            querySelect1.ExecuteReader();
            if (!querySelect1.Eof())
            {
                str5 = querySelect1.FieldValue("Pers_FullName");
                str6 = querySelect1.FieldValue("Pers_EmailAddress");
                str7 = string.Concat(querySelect1.FieldValue("Pers_PhoneAreaCode"), querySelect1.FieldValue("Pers_PhoneNumber"));
                str7 = new string(str7.Where<char>(new Func<char, bool>(char.IsDigit)).ToArray<char>());
                str8 = string.Concat(querySelect1.FieldValue("Pers_FaxAreaCode"), querySelect1.FieldValue("Pers_FaxNumber"));
                str8 = new string(str8.Where<char>(new Func<char, bool>(char.IsDigit)).ToArray<char>());
            }
            else
            {
                string contextInfo6 = base.GetContextInfo("Company", "Comp_PrimaryPersonId");
                string str18 = string.Format("SELECT * FROM vListPerson WHERE Pers_PersonId = {0}", contextInfo6);
                QuerySelect querySelect2 = new QuerySelect();
                querySelect2.SQLCommand = str18;
                QuerySelect querySelect3 = querySelect2;
                querySelect3.ExecuteReader();
                if (!querySelect3.Eof())
                {
                    str5 = querySelect3.FieldValue("Pers_FullName");
                    str6 = querySelect3.FieldValue("Pers_EmailAddress");
                    str7 = string.Concat(querySelect3.FieldValue("Pers_PhoneAreaCode"), querySelect3.FieldValue("Pers_PhoneNumber"));
                    str7 = new string(str7.Where<char>(new Func<char, bool>(char.IsDigit)).ToArray<char>());
                    str8 = string.Concat(querySelect3.FieldValue("Pers_FaxAreaCode"), querySelect3.FieldValue("Pers_FaxNumber"));
                    str8 = new string(str8.Where<char>(new Func<char, bool>(char.IsDigit)).ToArray<char>());
                    Record record = new Record("Person_Link");
                    record.SetField("PeLi_CompanyID", compId);
                    record.SetField("PeLi_PersonId", contextInfo6);
                    record.SetField("PeLi_Type", string.Format("{0}Contact", str));
                    record.SaveChanges();
                }
            }
            string str19 = string.Format("SELECT * FROM vListAddress WHERE AdLi_CompanyId = {0} AND AdLi_Type='{1}Address'", compId, str);
            QuerySelect querySelect4 = new QuerySelect();
            querySelect4.SQLCommand = str19;
            QuerySelect querySelect5 = querySelect4;
            querySelect5.ExecuteReader();
            if (!querySelect5.Eof())
            {
                str9 = querySelect5.FieldValue("Addr_Address1");
                str10 = querySelect5.FieldValue("Addr_Address2");
                str11 = querySelect5.FieldValue("Addr_Address3");
                str12 = querySelect5.FieldValue("Addr_Address4");
                str13 = querySelect5.FieldValue("Addr_City");
                str16 = querySelect5.FieldValue("Addr_State");
                str15 = querySelect5.FieldValue("Addr_PostCode");
                str14 = querySelect5.FieldValue("Addr_Country");
            }
            else
            {
                string contextInfo7 = base.GetContextInfo("Company", "Comp_PrimaryAddressId");
                string str20 = string.Format("SELECT * FROM vListAddress WHERE Addr_AddressId = {0}", contextInfo7);
                QuerySelect querySelect6 = new QuerySelect();
                //querySelect6.set_SQLCommand(str20);
                querySelect6.SQLCommand = str20;
                QuerySelect querySelect7 = querySelect6;
                querySelect7.ExecuteReader();
                if (!querySelect7.Eof())
                {
                    str9 = querySelect7.FieldValue("Addr_Address1");
                    str10 = querySelect7.FieldValue("Addr_Address2");
                    str11 = querySelect7.FieldValue("Addr_Address3");
                    str12 = querySelect7.FieldValue("Addr_Address4");
                    str13 = querySelect7.FieldValue("Addr_City");
                    str16 = querySelect7.FieldValue("Addr_State");
                    str15 = querySelect7.FieldValue("Addr_PostCode");
                    str14 = querySelect7.FieldValue("Addr_Country");
                    Record record1 = new Record("Address_Link");
                    record1.SetField("AdLi_CompanyID", compId);
                    record1.SetField("AdLi_AddressId", contextInfo7);
                    record1.SetField("AdLi_Type", string.Format("{0}Address", str));
                    record1.SaveChanges();
                }
            }
            Record record2 = base.FindRecord("Company", string.Format("Comp_CompanyId = {0}", compId));
            record2.SetField("Comp_Type", str1);
            record2.SetField("comp_database", database);
            record2.SetField((isCustomer ? "Comp_IdCust" : "Comp_IdVend"), id);
            record2.SaveChanges();
            string str21 = string.Concat(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|", new object[] { contextInfo, str2, contextInfo1, contextInfo3, str4, contextInfo5 }), string.Format("{0}|{1}|{2}|{3}|", new object[] { str5, str6, str7, str8 }), string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", new object[] { str9, str10, str11, str12, str13, str16, str15, str14, compId }));
            return string.Format("<input id='txtPromoteRecord' name='txtPromoteRecord' type='hidden' value='{0}'>", str21);
        }

        private object SendRequest(HttpMethod method, string requestUri, ref string webapimessage)
        {
            if (PromoteCompany.Sage300HttpClient == null)
            {
                PromoteCompany.Sage300HttpClient = new HttpClient(PromoteCompany.Sage300HttpClientHandler);
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, requestUri)
            {
                Content = null
            };
            HttpResponseMessage result = PromoteCompany.Sage300HttpClient.SendAsync(httpRequestMessage).Result;
            string str = result.Content.ReadAsStringAsync().Result;
            HttpStatusCode statusCode = result.StatusCode;
            dynamic obj = (new JavaScriptSerializer()).DeserializeObject(str);
            if (statusCode != HttpStatusCode.OK)
            {
                webapimessage = (string)(string.Format("Error Code: {0}, Error Message: Sage 300 WebApi ", statusCode) + obj["error"]["message"]["value"]);
            }
            if (statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.MultipleChoices)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }
                return obj;
            }
            base.LogMessage(string.Format("Sage300_WebAPI_Log_{0}", this._database), webapimessage, 5);
            return null;
        }

        private void UnlinkCompany(int companyId)
        {
            Record record = base.FindRecord("Company", string.Format("Comp_CompanyId={0}", companyId));
            record.SetField(this._Sage300Field, "");
            record.SetField("Comp_IdGrp", "");
            record.SetField("Comp_CodeTaxGrp", "");
            record.SetField("Comp_CodeTerm", "");
            record.SetField("Comp_AmtCrLimt", "");
            record.SetField("comp_database", "");
            record.SetField("comp_status", "Active");
            record.SetField("Comp_Type", "Dormant");
            record.SaveChanges();
        }
    }
}