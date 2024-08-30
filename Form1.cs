using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewsFinder
{
    public partial class Form1 : Form
    {
        const string _apiUrl = "https://openapi.naver.com/v1/search/news.json";
        const string _clientId = "afLSbSaFUgjmmHgmMg15";
        const string _clientSecret = "9XSwBrg1jD";

        //DB 경로
        string strConn = string.Format("Data Source={0}", Application.StartupPath + @"\newsDB.db");

        public Form1()
        {
            InitializeComponent();
            label3.Text = trackBar1.Value.ToString();
            Log.LogCreate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = trackBar1.Value.ToString();
        }

        private string getResults()
        {
            string keyword = textBoxKeyword.Text;
            string display = trackBar1.Value.ToString();
            string sort = "sim";
            if(radioButton2.Checked == true)
            {
                sort = "date";
            }

            string query = string.Format("?query={0}&display={1}&sort={2}", keyword, display, sort);

            WebRequest request = WebRequest.Create(_apiUrl + query);
            request.Headers.Add("X-Naver-Client-Id", _clientId);
            request.Headers.Add("X-Naver-Client-Secret", _clientSecret);

            string requestResult = "";

            using (var response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(dataStream))
                    {
                        requestResult = reader.ReadToEnd();
                    }
                }
            }

                return requestResult;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string results = getResults();

                results = results.Replace("<b>", "");
                results = results.Replace("</b>", "");
                results = results.Replace("&lt;", "<");
                results = results.Replace("&gt;", ">");

                var parseJson = JObject.Parse(results);
                var countsOfDisplay = Convert.ToInt32(parseJson["display"]);
                var countsOfResults = Convert.ToInt32(parseJson["total"]);

                listView1.Items.Clear();

                for(int i=0; i<countsOfDisplay; i++)
                {
                    ListViewItem item = new ListViewItem((i + 1).ToString());

                    var title = parseJson["items"][i]["title"].ToString();
                    title = title.Replace("&quot;", "\"");
                    title = title.Replace("</b>", "");

                    var description = parseJson["items"][i]["description"].ToString();
                    description = description.Replace("&quot;", "\"");
                    description = description.Replace("</b>", "");

                    var link = parseJson["items"][i]["link"].ToString();

                    var dateTime = Convert.ToDateTime(parseJson["items"][i]["pubDate"]).ToString("MM/dd/yyyy HH:MM:ss");

                    item.SubItems.Add(title);
                    item.SubItems.Add(description);                  
                    item.SubItems.Add(link);
                    item.SubItems.Add(dateTime);

                    listView1.Items.Add(item);

                    string temp = title + "|" + description + "|" + link + "|" + dateTime;
                    Log.LogWrite("기사조회 " + temp);
                }

            }
            catch(Exception ex)
            {

            }
        } 

        private void newsFinderDouble(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection item = listView1.SelectedItems;
                ListViewItem lvItem = item[0];

                try
                {
                    string urlAddress = lvItem.SubItems[3].Text;
                    Console.WriteLine("urlAddress : " + urlAddress);

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        urlAddress = urlAddress.Replace("&", "^&");
                        System.Diagnostics.Process p = new Process();
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.FileName = urlAddress;
                        p.Start();
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string title = "";
            string des = "";
            string url = "";
            string date = "";

            using (SQLiteConnection conn = new SQLiteConnection(strConn))
            {
                conn.Open();
                
                foreach (ListViewItem item in this.listView1.CheckedItems)
                {
                    if (item.Checked)
                    {
                        title = item.SubItems[1].Text;
                        des = item.SubItems[2].Text;
                        url = item.SubItems[3].Text;
                        date = item.SubItems[4].Text;

                        string sql = string.Format("Insert into news_table(news_title, news_des, news_url, news_date, insertDate)" +
                            " values ('{0}', '{1}', '{2}', '{3}', date('now') )", title, des, url, date);
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();

                        string temp = title + "|" + des + "|" + url + "|" + date;
                        Log.LogWrite("DB 입력 " + temp);
                    }
                }
                
                conn.Close();
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if(e.ColumnIndex == 0)
            {
                e.DrawBackground();
                bool val = false;
                try
                {
                    val = Convert.ToBoolean(e.Header.Tag);
                }
                catch (Exception)
                {

                }
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 4, e.Bounds.Top + 4), val ?
                    System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if(e.Column == 0)
            {
                bool val = false;
                try
                {
                    val = Convert.ToBoolean(this.listView1.Columns[e.Column].Tag);
                }
                catch (Exception)
                {

                }
                this.listView1.Columns[e.Column].Tag = !val;
                foreach(ListViewItem item in this.listView1.Items)
                {
                    item.Checked = !val;
                }
                this.listView1.Invalidate();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //기존 항목들 비우기
            listView1.Items.Clear();
            string startDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string endDate = dateTimePicker2.Value.ToString("yyyy-MM-dd");
            ListViewItem lvi;
            string title;
            string des;
            string link;
            string originDate;


            using (var conn = new SQLiteConnection(strConn))
            {
                conn.Open();
                string sql = string.Format("select * from news_table where insertDate >= '{0}' and insertDate <= '{1}'", startDate, endDate);
                int idx = 0;
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    idx++;
                    title = rdr["news_title"].ToString();
                    des = rdr["news_des"].ToString();
                    link = rdr["news_url"].ToString();
                    originDate = rdr["news_date"].ToString();

                    lvi = new ListViewItem(new String[] {Convert.ToString(idx), title, des, link, originDate});
                    this.listView1.Items.Add(lvi);

                    string temp = title + "|" + des + "|" + link + "|" + originDate;
                    Log.LogWrite("DB 조회 " + temp);
                }
                rdr.Close();
            }

            

        }
    }
}
