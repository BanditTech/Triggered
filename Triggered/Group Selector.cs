using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using NLog;
using System;
using Newtonsoft.Json.Linq;

namespace Triggered
{
    public partial class GroupSelector : Form
    {
        private static Logger log;
        public GroupSelector()
        {
            InitializeComponent();
            log = LogManager.GetCurrentClassLogger();
        }
        public List<string> GetGroups()
        {
            List<object> jsonData;
            List<string> availableGroups = new List<string>();
            // Deserialize the JSON data into a list of objects
            string content = File.ReadAllText("C:\\Users\\thebb\\Desktop\\example.json");
            jsonData = JsonConvert.DeserializeObject<List<object>>(content);
            // Iterate through each object and check if it has a GroupName attribute
            foreach (object item in jsonData)
            {
                if (item is JObject jObject && jObject.ContainsKey("GroupName"))
                {
                    string groupName = jObject["GroupName"].ToString();
                    availableGroups.Add(groupName);
                }
            }
            // Now you have a list of available groups that you can use to create the menu
            MessageBox.Show("Resulting Groups: " + string.Join(",", availableGroups));
            return availableGroups;
        }
        public void PopulateList()
        {
            List<string> groups = GetGroups();
            foreach (string group in groups)
            {
                ListViewItem item = new ListViewItem(group);
                DisplayList.Items.Add(item);
            }
        }

        private void GroupSelector_Load(object sender, System.EventArgs e)
        {
            PopulateList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
