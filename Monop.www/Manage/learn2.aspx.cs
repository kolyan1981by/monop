using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GameLogic;
using System.IO;
using GameLogic;

namespace Monop.www.Manage
{
    public partial class learn2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Bind();

        }

        private void Bind()
        {

            GridView1.DataSource = lands;
            GridView1.DataBind();

            GridView2.DataSource = lands;
            GridView2.DataBind();

        }

        string[] lands = new[] { "Spain", "Greece", "Italy", "England", "Russia", "France", "Usa", "Japan" };

        protected void ButtonClear_Click(object sender, EventArgs e)
        {
            Bind();
            TextBoxMyMoney.Text = "";
            TextBoxYourMoney.Text = "";
        }

        protected void ButtonLearn_Click(object sender, EventArgs e)
        {

            var st = new TradeRule();

            foreach (GridViewRow row in GridView1.Rows)
            {
                var ind = row.RowIndex;
                for (int i = 1; i <= 4; i++)
                {
                    var ch = (CheckBox)row.FindControl("ch" + i);
                    if (ch.Checked)
                    {
                        if (i == 1 || i == 2)
                            st.MyCount = i;

                        if (i == 3 || i == 4)
                        {
                            st.GetLand = ind + 1;
                            st.GetCount = i - 2;
                        }
                    }
                }
            }


            var res2 = "";
            foreach (GridViewRow row in GridView2.Rows)
            {
                var ind = row.RowIndex;
                for (int i = 1; i <= 4; i++)
                {
                    var ch = (CheckBox)row.FindControl("ch" + i);
                    if (ch.Checked)
                    {
                        if (i == 1 || i == 2)
                            st.YourCount = i;

                        if (i == 3 || i == 4)
                        {
                            st.GiveLand = ind + 1;
                            st.GiveCount = i - 2;
                        }
                    }
                }

            }
            //get money
            var m1 = TextBoxMyMoney.Text;
            if (!string.IsNullOrEmpty(m1)) st.GetMoney = Convert.ToInt32(m1);
            //give money
            var m2 = TextBoxYourMoney.Text;
            if (!string.IsNullOrEmpty(m2)) st.GiveMoney = Convert.ToInt32(m2);

            //money factor(for ex, if i have 3M, he has 2M, factor = 1.5)
            var m3 = tbMoneyFactor.Text;
            if (!string.IsNullOrEmpty(m3)) st.MoneyFactor = Convert.ToDouble(m3);

            if (!IsValid(st))
            {
                LabelRes.Text = "exchange isn't valid";
            }
            else
            {
                if (!ExistNewState(st))
                {
                    ManualLogicHelper.SaveToFile(st, ServerManageFolder);
                    LabelRes.Text = "new exch created successfully";
                }
                else
                    LabelRes.Text = "exchange already exist";
            }
        }

        private bool IsValid(TradeRule st)
        {
            if (st.GetLand == st.GiveLand) return false;
            return true;
        }

        private bool ExistNewState(TradeRule newSt)
        {
            if (newSt.GetLand == 0 || newSt.GiveLand == 0) return true;
            foreach (var st in ManualLogicHelper.LoadExchanges(ServerManageFolder))
            {
                var q = (st.GetLand == newSt.GetLand
                    && st.GetCount == newSt.GetCount
                    && st.MyCount == newSt.MyCount
                    && st.GetMoney >= newSt.GetMoney
                    && st.GiveLand == newSt.GiveLand
                     && st.GiveCount == newSt.GiveCount
                      && st.YourCount == newSt.YourCount
                       && st.GiveMoney <= newSt.GiveMoney
                       && st.MoneyFactor >= newSt.MoneyFactor
                    );
                if (q) return true;
            }
            return false;
        }

        string fileName = "exchange.txt";

        public string ServerManageFolder { get { return Server.MapPath("~/manage"); } }

        protected void ButtonRead_Click(object sender, EventArgs e)
        {
            //ReSave();
            LoadFromFile();

        }

        private void LoadFromFile()
        {
            var lst = ManualLogicHelper.LoadExchanges(ServerManageFolder, "exchange.txt")
                .OrderBy(x => x.GetLand).ToList();
            Session["lst"] = lst;
            BindToGV(lst);
        }

        private void BindToGV(List<TradeRule> lst)
        {
            var lst2 = lst.Select(st =>
                new
                {
                    id = st.Id,
                    toMe = ToMe(st),
                    toYou = ToYou(st),
                    mFactor = st.MoneyFactor,
                    Disabled = st.Disabled,
                }
            );

            GridViewRes.DataSource = lst2;
            GridViewRes.DataBind();
        }


        protected void ButtonDel_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbRecId.Text))
            {
                var id = Int32.Parse(tbRecId.Text);
                Delete(id);
                //LoadFromFile();
            }
        }
        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        private void Delete(int delId)
        {
            var lst = (List<TradeRule>)Session["lst"];

            lst[delId].Disabled = true;
            var exch = lst.FirstOrDefault(x => x.Id == delId);
            if (exch != null) exch.Disabled = true;
        }

        private void Save()
        {
            var lst = (List<TradeRule>)Session["lst"];

            var fpath = Path.Combine(ServerManageFolder, "exchange.txt");

            File.WriteAllText(fpath, "///" + DateTime.Now + Environment.NewLine);
            foreach (var rec in lst)//.Where(x => !x.Disabled))
            {
                ManualLogicHelper.SaveToFile(rec, ServerManageFolder);
            }
        }

        public string ToMe(TradeRule st)
        {
            //my 
            var m1 = string.Format("{0} ({1}+{2}) m={3}",
                lands[st.GetLand - 1], st.MyCount, st.GetCount, st.GetMoney);
            return m1;
        }

        public string ToYou(TradeRule st)
        {
            //your
            var m2 = string.Format("{0} ({1}+{2}) m={3}",
                lands[st.GiveLand - 1], st.YourCount, st.GiveCount, st.GiveMoney);
            return m2;
        }

    }
}