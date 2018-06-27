using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Web.UI;
using Xero.Api.Core;
using Xero.Api.Core.Model;
using Xero.Api.Core.Model.Status;
using Xero.Api.Example.Applications.Public;
using Xero.Api.Infrastructure.Interfaces;
using Xero.Api.Infrastructure.OAuth;

namespace VDCS_Expenses
{   
    public partial class WebForm1 : System.Web.UI.Page
    {
 
        public XeroCoreApi myxeroapi = default(XeroCoreApi);
        protected void Page_Load(object sender, EventArgs e)
        {

            if (IsPostBack == false)
            {
                if (Page.Request.QueryString["oauth_verifier"] != null)
                {
                    myxeroapi = XeroAuthenticate(this, true, "");
                    if ((myxeroapi != null))
                    {
                        XeroConnection.Text = myxeroapi.Organisation.Name.ToString();      
                    }
                }
            }
        }

        // Public Application Authenication - a46c26f6-7874-4bf4-b5b0-d221eb2c54f9

        protected XeroCoreApi XeroAuthenticate(Page inpage, bool authpagereturnedauthentication, string incode)
        {
            // This procedure will return w working XeroCoreApi if successful and nothing otherwise
            // Set client secrets for general use and then load any override ones from the web.config file
            Consumer myxeroconsumer = new Consumer("EQG3PH0JMUA2OYTGWXDN67BJDCVNFF", "YWTHX3SAJUUV2FV96IMRW1OJ2E06HH");
            dynamic myxerouser = new ApiUser { Name = "richard.may@virtualdcs.co.uk" };

            // If user defined ID and Secrets exist then use those instead

            if ((System.Configuration.ConfigurationManager.AppSettings.Get("XeroApiConsumerKey") != null))
            {
                if ((System.Configuration.ConfigurationManager.AppSettings.Get("XeroApiConsumerSecret") != null))
                {
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings.Get("XeroApiConsumerKey").ToString()))
                    {
                        myxeroconsumer = new Consumer(System.Configuration.ConfigurationManager.AppSettings.Get("XeroApiConsumerKey").ToString(), System.Configuration.ConfigurationManager.AppSettings.Get("XeroApiConsumerSecret").ToString());
                    }
                }
            }
            // Define Page Call Information
            dynamic myxerocallbackurl = "http://peoplehr.virtualdcs.co.uk/ExpenseTransfer.aspx";
            dynamic myxeromemorystore = new MemoryAccessTokenStore();
            dynamic myxerorequestTokenStore = new MemoryRequestTokenStore();
            dynamic myxerobaseapiurl = "https://api.xero.com";

            // Authenticate with Xero
            PublicMvcAuthenticator myxeroauthenticator = new PublicMvcAuthenticator(myxerobaseapiurl, myxerobaseapiurl, myxerocallbackurl, myxeromemorystore, myxeroconsumer, myxerorequestTokenStore);

            if (authpagereturnedauthentication == false)
            {
                // Redirect to the authentication page
                string requri = myxeroauthenticator.GetRequestTokenAuthorizeUrl(myxerouser.Name);
                if (!string.IsNullOrEmpty(requri))
                    Response.Redirect(requri, true);
                return null;
            }
            else
            {
                // Validate token using querystrings that have been returned from the authentication process
                IToken myxerotoken = myxeroauthenticator.RetrieveAndStoreAccessToken(myxerouser.Name, Request.QueryString["oauth_token"].ToString(), Request.QueryString["oauth_verifier"].ToString(), Request.QueryString["org"].ToString());
                if ((myxerotoken != null))
                {
                    XeroCoreApi myxeroapi = new XeroCoreApi("https://api.xero.com", myxeroauthenticator, myxeroconsumer, myxerouser);
                    return myxeroapi;
                }
                else
                {
                    return null;
                }
            }
        }


        public class MemoryAccessTokenStore : ITokenStore
        {
            private static readonly IDictionary<string, IToken> _tokens = new ConcurrentDictionary<string, IToken>();

            public IToken Find(string userId)
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                IToken token;

                _tokens.TryGetValue(userId, out token);
                return token;
            }

            public void Add(IToken token)
            {
                _tokens[token.UserId] = token;
            }

            public void Delete(IToken token)
            {
                if (_tokens.ContainsKey(token.UserId))
                {
                    _tokens.Remove(token.UserId);
                }
            }
        }

        public class MemoryRequestTokenStore : ITokenStore
        {
            private static readonly IDictionary<string, IToken> _tokens = new ConcurrentDictionary<string, IToken>();

            public IToken Find(string userId)
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                IToken token;

                _tokens.TryGetValue(userId, out token);
                return token;
            }

            public void Add(IToken token)
            {
                _tokens[token.UserId] = token;
            }

            public void Delete(IToken token)
            {
                if (_tokens.ContainsKey(token.UserId))
                {
                    _tokens.Remove(token.UserId);
                }
            }
        }

        protected string GetPeopleHRData()
        {
            try
            {
                string ExpenseResults = "";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.peoplehr.net/Query");
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";

                // a46c26f6-7874-4bf4-b5b0-d221eb2c54f9

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"APIKey\":\"" + PeopleHRKey.Text + "\",\"Action\":\"GetQueryResultByQueryName\",\"QueryName\":\"ExpenseToXero\"}";
                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string ExpData = streamReader.ReadToEnd();


                    var Result = JObject.Parse(ExpData);

                    ExpenseResults = String.Concat("<center><table border=1><tr><td>First Name</td >",
                        "<td>Last Name</td >",
                        "<td>Description</td >",
                        "<td>Date Submitted</td >",
                        "<td>Amount</td >",
                        "<td>Taxable Amount</td >",
                        "<td>Category", "</td></tr>");

                    var ResultLine = Result["Result"].Children();
                    foreach (JToken Expense in ResultLine)

                    {
                        double LineValue = (double)Expense["Expense Line Amount"];
                        decimal VATValue = 0;
                        if (Expense["Expense Line Taxable Amount"].ToString() == "")
                        {
                            VATValue = 0;
                        }
                        else
                        {
                            VATValue = (decimal)Math.Round(LineValue - (LineValue / 1.2), 2);
                        }
                        ExpenseResults = ExpenseResults + String.Concat("<tr><td>", Expense["First Name"].ToString(), "</td>",
                            "<td>", Expense["Last Name"].ToString(), "</td>",
                            "<td>", Expense["Expense Report Description"].ToString(), "</td>",
                            "<td>", Expense["Expense Date Submitted"].ToString(), "</td>",
                            "<td>", Expense["Expense Line Amount"].ToString(), "</td>",
                            "<td>", VATValue, "</td>",
                            "<td>", Expense["Expense Line Category"].ToString(), "</td></tr>");

                    }
                    ExpenseResults = ExpenseResults + "</table></center>";
                    Expenses.InnerHtml = ExpenseResults;
                    return ExpData;
                }
            }

            catch (Exception exc)
            {
                throw new Exception("Problem Connecting to xero" + exc.ToString());

            }
        }

        public void UploadExpensestoXero()
        {
            string Narration = "";
            string FootCheck = "";
            decimal ExpenseValueTotal = 0;
            decimal ExpenseValue = 0;
            string Nominal = "000";
            int ExpenseLineCount = 0;
            string DefaultExpenseCode = DefaultExpenseNominalCode.Text;
            string DefaultBSCode = DefaultExpenseNominalCode.Text;
            string TaxType = "NONE";
            decimal VATValue = 0;


            try { 
            myxeroapi = XeroAuthenticate(this, true, "");
            }
            catch
            {
             
                throw new Exception("Not connected to Xero");
            }


            XeroConnection.Text = myxeroapi.Organisation.Name.ToString();

            dynamic stuff = JsonConvert.DeserializeObject(GetPeopleHRData());


            try { 
            ExpenseLineCount = Convert.ToInt32(stuff.Result.Count);

            }
            catch 
            {
                Completed.Text = "No Expenses to upload" ;
                throw new Exception("No Expenses to upload");
            }

            try
            {
                var Journal = new ManualJournal { };
                int counter = 0;
                foreach (var x in stuff.Result)
                {

                    if (Narration != string.Concat("Expense Claim", " - ", x["Expense Report Description"], " - ", x["First Name"], " ", x["Last Name"]))
                    {
                        Journal = new ManualJournal
                        {
                            Date = x["Expense Line Date"],
                            Narration = string.Concat("Expense Claim", " - ", x["Expense Report Description"], " - ", x["First Name"], " ", x["Last Name"]),
                        };
                        Journal.Status = ManualJournalStatus.Draft;
                        Journal.LineAmountTypes = Xero.Api.Core.Model.Types.LineAmountType.Inclusive;
                        Journal.Lines = new List<Line>();
                        Narration = string.Concat("Expense Claim", " - ", x["Expense Report Description"], " - ", x["First Name"], " ", x["Last Name"]);
                        ExpenseValueTotal = 0;
                    }
                    String St = x["Expense Line Category"];


                    if (St == null)
                    {
                        Nominal = "";
                    }
                    else
                    {
                        int pFrom = St.IndexOf("(") + "(".Length;
                        int pTo = St.LastIndexOf(")");
                        Nominal = St.Substring(pFrom, pTo - pFrom);
                    }

                    // Tax Calculation

                    if (x["Expense Line Taxable Amount"] == null)
                    {
                        TaxType = "NONE";
                        VATValue = 0;
                    }
                    else
                    {
                        TaxType = "INPUT2";
                        double LineValue = (double)x["Expense Line Amount"];
                        VATValue = (decimal)Math.Round(LineValue - (LineValue / 1.2), 2);
                    }

                    // Expense Journal
                    Journal.Lines.Add(
                                        new Line
                                        {
                                            AccountCode = Nominal,
                                            TaxType = TaxType,
                                            Description = string.Concat(x["Expense Line Title"], " - ", x["Expense Report Description"], " - ", x["First Name"], " ", x["Last Name"]),
                                            Amount = x["Expense Line Amount"],
                                            TaxAmount = VATValue
                                        }
                                        );
                    ExpenseValue = x["Expense Line Amount"];
                    ExpenseValueTotal = ExpenseValueTotal + ExpenseValue;
                    if (counter == ExpenseLineCount - 1)
                    { FootCheck = ""; }
                    else
                    { FootCheck = string.Concat("Expense Claim", " - ", stuff.Result[counter + 1]["Expense Report Description"], " - ", stuff.Result[counter + 1]["First Name"], " ", stuff.Result[counter + 1]["Last Name"]); }
                    counter++;
                    if (Narration != FootCheck)
                    {
                        // Employee Expense Balance Sheet Account
                        Journal.Lines.Add(
                                          new Line
                                          {
                                              AccountCode = DefaultBSCode,
                                              TaxType = "NONE",
                                              Description = string.Concat(x["Expense Report Description"], " - ", x["First Name"], " ", x["Last Name"]),
                                              Amount = -ExpenseValueTotal,
                                          }
                                          );

                        myxeroapi.Create(Journal);
                        Completed.Text = "Journals Written";
                    }
                }
            }
            catch (Exception e)
            {
                Completed.Text = e.ToString();
            }

            }



        protected void GetPeopleHRExpenses_Click(object sender, EventArgs e)
        {
            string a  = GetPeopleHRData();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try { 
            UploadExpensestoXero();
            }
            catch (Exception ex)
            {
                Completed.Text = ex.Message;
            }
        }

        protected void XeroAuthenicateButton_Click(object sender, ImageClickEventArgs e)
        {
            try { 
            myxeroapi = XeroAuthenticate(this, false, "");

            }
            catch (Exception exc)
            {
                Completed.Text = "Problem Connecting to xero" + exc.Message ;
            }

        }

    }
}