<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExpenseTransfer.aspx.cs" Inherits="VDCS_Expenses.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            text-align: center;
        }
        .auto-style2 {
            font-size: xx-large;
        }
        #Expenses {
            text-align: center;
        }
        .auto-style3 {
            width: 222px;
        }
        .auto-style4 {
            width: 325px;
        }
    </style>
</head>
<body>
    <div class="auto-style1">

        <br />
        <br />
    </div>
    <form id="DisplayExpenses" runat="server">
    <div style="margin-left: 40px" class="auto-style1">
                <span class="auto-style2"><strong>PeopleHR Expenses to Xero
    </strong></span><br /><br />
        <center><table>
            <tr><td class="auto-style4" style="text-align: left">1 - Authenicate to Xero</td>
                <td class="auto-style3" ><asp:ImageButton ID="XeroAuthenicateButton" runat="server" ImageUrl="~/images/connect_xero_button_blue1.png" OnClick="XeroAuthenicateButton_Click" ></asp:ImageButton>
                    <br />
                    <asp:Label ID="XeroConnection" runat="server" Text=""></asp:Label>
                </td></tr>
            <tr><td class="auto-style4" style="text-align: left" >2 - Get Expense Data from PeopleHR Key<br />&nbsp;&nbsp;&nbsp; (you will need your PeopleHR application Key)<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="PeopleHRKey" ErrorMessage="You must enter this key" BackColor="Red" ForeColor="#CCFFFF"></asp:RequiredFieldValidator>
                </td>
                <td class="auto-style3"><asp:TextBox ID="PeopleHRKey" runat="server" Width="193px"></asp:TextBox><br>
                    <asp:Button ID="GetPeopleHRExpenses" runat="server" Text="Get Expenses" OnClick="GetPeopleHRExpenses_Click" /></td></tr>
            <tr><td class="auto-style4" style="text-align: left">3 - Enter default expense Nominal Code</td><td class="auto-style3"><asp:TextBox ID="DefaultExpenseNominalCode" text="2103" runat="server"></asp:TextBox></td></tr>
                        <tr><td class="auto-style4" style="text-align: left">4 - Send Expense Journals to Xero</td>
                <td class="auto-style3"><asp:Button ID="Button1" runat="server" Text="Send to Xero" OnClick="Button1_Click"  />
                    <br />
            <asp:Label ID="Completed" runat="server" Text="" style="font-weight: 700"></asp:Label>
                            </td></tr>
        </table></center>
        <br />
    </div>
        <div style="margin-left: 40px" runat="server" id="Expenses">
        </div> 

     

    </form>
    <p>
        Instructions</p>
    <p>
        The page will transfer expenses from peopleHR to Xero, and create a draft journal that can be posted.</p>
    <p>
        1 - Create a query in peopleHR to select the expenses call it ExpenseToXero.</p>
    <p>
        2 - Select the expense area for your query and add the following items :</p>
    <ul>
        <li>Employee ID</li>
        <li>First Name</li>
        <li>Last Name</li>
        <li>Expense Report Description</li>
        <li>Expense Date Submitted</li>
        <li>Expense Paid\Unpaid</li>
        <li>Expense Report Approved Date</li>
        <li>Expense Line Title</li>
        <li>Expense Line Date</li>
        <li>Expense Line Taxable Amount</li>
        <li>Expense Line Category</li>
        <li>Deleted Expense</li>
    </ul>
    <p>
        3 - Create an API Key in settings/API. Call it expense and Expenses and give it access to the querybuilder (The Key generaled should be entered above)</p>
    <p>
        4 - In the setting/expenses area, create expense categories with brackets after to hold the nominal code for expenses in xero i.e. Transport (2342)</p>
    <p>
        &nbsp;</p>
    <p>
        &nbsp;</p>
    <p>
        <br />

    </p>
    <p>
        &nbsp;</p>
</body>
</html>
