<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="learn2.aspx.cs" Inherits="Monop.www.Manage.learn2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .style1
        {
            width: 20%;
        }
        .style2
        {
            width: 40%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <a href="/">Home</a> <a href="/sim">Sim</a> <a href="learn2.aspx">learn2</a>
        <a href="/learn/auc">Auc</a><br />
    </div>
    <div>
        <table style="width: 100%;">
            <tr>
                <td style="width: 50%;" valign="top">
                    <table style="width: 600px;">
                        <tr>
                            <td class="style2" valign="top">
                                to me
                                <asp:GridView ID="GridView1" runat="server" BackColor="White" BorderColor="#336666"
                                    BorderStyle="Double" BorderWidth="3px" CellPadding="4" EnableModelValidation="True"
                                    GridLines="Horizontal" AutoGenerateColumns="false">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Country">
                                            <ItemTemplate>
                                                <%# Container.DataItem %>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="i have">
                                            <ItemTemplate>
                                                1<asp:CheckBox runat="server" ID="ch1" />
                                                2<asp:CheckBox runat="server" ID="ch2" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="you give">
                                            <ItemTemplate>
                                                <asp:CheckBox runat="server" ID="ch3" />
                                                <asp:CheckBox runat="server" ID="ch4" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <FooterStyle BackColor="White" ForeColor="#333333" />
                                    <HeaderStyle BackColor="#336666" Font-Bold="True" ForeColor="White" />
                                    <PagerStyle BackColor="#336666" ForeColor="White" HorizontalAlign="Center" />
                                    <RowStyle BackColor="White" ForeColor="#333333" />
                                    <SelectedRowStyle BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                </asp:GridView>
                            </td>
                            <td class="style1">
                                <asp:Button ID="ButtonLearn" runat="server" Text="Learn" OnClick="ButtonLearn_Click"
                                    Height="53px" Width="98px" />
                                <br />
                                <asp:Button ID="ButtonClear" runat="server" Text="Clear" Height="53px" Width="98px"
                                    OnClick="ButtonClear_Click" />
                            </td>
                            <td valign="top">
                                to you
                                <asp:GridView ID="GridView2" runat="server" BackColor="White" BorderColor="#336666"
                                    BorderStyle="Double" BorderWidth="3px" CellPadding="4" EnableModelValidation="True"
                                    GridLines="Horizontal" AutoGenerateColumns="false">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Country">
                                            <ItemTemplate>
                                                <%# Container.DataItem %>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="you have">
                                            <ItemTemplate>
                                                1<asp:CheckBox runat="server" ID="ch1" />
                                                2<asp:CheckBox runat="server" ID="ch2" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="i give">
                                            <ItemTemplate>
                                                <asp:CheckBox runat="server" ID="ch3" />
                                                <asp:CheckBox runat="server" ID="ch4" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <FooterStyle BackColor="White" ForeColor="#333333" />
                                    <HeaderStyle BackColor="#336666" Font-Bold="True" ForeColor="White" />
                                    <PagerStyle BackColor="#336666" ForeColor="White" HorizontalAlign="Center" />
                                    <RowStyle BackColor="White" ForeColor="#333333" />
                                    <SelectedRowStyle BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                </asp:GridView>
                            </td>
                        </tr>
                        <tr>
                            <td class="style2">
                                to me money<asp:TextBox ID="TextBoxMyMoney" runat="server"></asp:TextBox>
                                <br />
                                to you money<asp:TextBox ID="TextBoxYourMoney" runat="server"></asp:TextBox>
                                <br />
                                change if my money > his money
                                <asp:TextBox ID="tbMoneyFactor" runat="server" Text="1"></asp:TextBox>
                            </td>
                            <td class="style1">
                            </td>
                            <td>
                            </td>
                        </tr>
                    </table>
                    <asp:Label ID="LabelRes" runat="server" Text=""></asp:Label>
                </td>
                <td valign="top">
                    <asp:Button ID="ButtonRead" runat="server" Text="Read" OnClick="ButtonRead_Click" />
                    <asp:Button ID="ButtonDel" runat="server" Text="Del" OnClick="ButtonDel_Click" /><asp:TextBox
                        ID="tbRecId" runat="server" />
                    <br />
                    <asp:Button ID="ButtonSave" runat="server" Text="Save" OnClick="ButtonSave_Click" />
                    <asp:GridView ID="GridViewRes" runat="server">
                    </asp:GridView>
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
