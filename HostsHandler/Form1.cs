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
        public bool handleByHostsHandler = false;
        public Form1()
        {
            InitializeComponent();
            listView1.Groups.Clear();
            listView1.Items.Clear();
        }

        public bool warnOrSave()
        {
            if (handleByHostsHandler)
            {
                save();
            }
            else
            {
                DialogResult ret = MessageBox.Show("By manipuating your host file here, you acknowledge that HostsManager will write to your hostfile.", "Are you sure?", MessageBoxButtons.YesNo);
                if (ret == System.Windows.Forms.DialogResult.Yes)
                {
                    handleByHostsHandler = true;
                    save();
                }
            }
            return handleByHostsHandler;
        }

        public void LoadFile()
        {
            string[] hostFileString;
            // backup hosts file.

            using (StreamReader sr = new StreamReader("c:\\windows\\system32\\drivers\\etc\\hosts"))
            {
                hostFileString = sr.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                sr.Close();
            }
            LoadingProgress.Maximum = hostFileString.Length;

            foreach (string line in hostFileString)
            {
                LoadingProgress.Value = (int)LoadingProgress.Value + 1;
                string entry = line;
                if (line.ToLower().Contains("handlebyhostsmanager"))
                {
                    handleByHostsHandler = (bool)line.ToLower().Contains("true");
                }

                Match match = HostsHandler.Helper.getIPMatchFromString(line);
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
            LoadingProgress.Visible = false;
            listView1.Visible = true;
            listView1.Sort(); // TODO: better sorting and grouping. domains should be grouped with subdomains below them

            // TODO: Deduplicate entries

            if (handleByHostsHandler)
            {
                save();
            }

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
            lvi.Name = ip;
            listView1.Items.Add(lvi);

            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            LoadFile();
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
            warnOrSave();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            warnOrSave();
        }

        private void save()
        {

            var outputFilename = "c:\\hostsOut";
            using (System.IO.StreamWriter sw = new StreamWriter(outputFilename, false))
            {
                sw.WriteLine("# ManageByHostsHander=" + handleByHostsHandler);
                foreach (ListViewGroup item in listView1.Groups)
                {
                    // output section header
                    sw.WriteLine("");
                    sw.WriteLine(("# " + item.Name + " ").PadLeft(10).PadRight(80, '-'));

                    foreach (ListViewItem childItem in item.Items)
                    {
                        sw.WriteLine(new outputLine(childItem).ToLineString(1));
                    }
                    //sw.WriteLine(("# end of " + item.Name).PadLeft(10).PadRight(80, '-'));
                }
            }
            SaveBtn.Visible = false;
        }


        public class outputLine
        {
            public bool IsIncluded
            {
                get
                {
                    return (bool)(lvItem.BackColor == Color.LightGreen);
                }
            }

            public string IsIncludedString
            {
                get
                {
                    return (IsIncluded ? "" : "#");
                }
            }

            public string ip
            {
                get
                {
                    return HostsHandler.Helper.getIPMatchFromString(lvItem.Text).Value;
                }
            }

            public string Description
            {
                get
                {
                    return lvItem.Text.Split('-')[0];
                }
            }

            public ListViewItem lvItem;

            public outputLine(ListViewItem lvi)
            {
                lvItem = lvi;
            }

            public string ToLineString(int indent)
            {
                string line = IsIncludedString.PadLeft(indent * 10).PadRight(10 * indent) + ip; ;

                return line.PadRight(50) + "# " + Description.Trim();
            }
        }
    }
}
