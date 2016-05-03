using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostsHandler
{
    public partial class Form1 : Form
    {
        public Form1()
        {           InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string[] hostFileString;
            using (StreamReader sr = new StreamReader("c:\\windows\\system32\\drivers\\etc\\hosts"))
            {

                hostFileString = sr.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                sr.Close();
            }
            foreach (string line in hostFileString)
            {
                string entry = line;
                var match = Regex.Match(line, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                if (match.Success)
                {
                    entry = line.Replace("\t", "`").Replace("``", "`");
                    string ip = match.Value;
                    string domain = line.Substring((match.Index + match.Length), line.Length - (match.Index + match.Length)).Replace("\t", "");
                    string description = "";
                    bool included = !(line.Replace("\t", "").Replace(" ", "").Substring(0, 1).Replace(" ", "") == "#");
                    if (domain.Contains("#"))
                    {
                        description = domain.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                        domain = domain.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                    Console.WriteLine(match.Captures[0]);
                    upsertDomain(domain.Replace(" ", ""), ip, included, description);
                }
            }
            listView1.Sort();
        }

        private bool upsertDomain(string domain, string ip, bool included, string descriptor)
        {
            if (listView1.Groups[domain] == null)
            {
                listView1.Groups.Add(new ListViewGroup(domain, domain + " - unset."));
            }
            if (descriptor == "") { descriptor = "Unknown"; }

            ListViewItem lvi = new ListViewItem(descriptor + " - " + ip, domain, listView1.Groups[domain]);
            if (included)
            {
                lvi.Checked = true;
                lvi.BackColor = Color.LightGreen;
                listView1.Groups[domain].Header = lvi.Text;
            }
            listView1.Items.Add(lvi);

            return true;
        }

        private void saveHostFile()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            bool originalState = e.Item.Checked;
            foreach (ListViewItem item in e.Item.Group.Items)
            {
                item.BackColor = Color.Transparent;
            }
            if (e.Item.Checked)
            {
                e.Item.BackColor = Color.Transparent;
                e.Item.Group.Header = e.Item.Group.Name = " - unset.";
            }
            else
            {
                e.Item.BackColor = Color.LightGreen;
                e.Item.Group.Header = e.Item.Group.Name + " - " + e.Item.Text;
            }
        }
    }
}
