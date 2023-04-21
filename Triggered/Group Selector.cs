using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using static Triggered.Our;

namespace Triggered
{
    public partial class GroupSelector : Form
    {
        public GroupSelector()
        {
            InitializeComponent();
        }

        public List<string> GetGroups()
        {
            List<object> jsonData;
            List<string> availableGroups = new List<string>();
            // Deserialize the JSON data into a list of objects
            string content = File.ReadAllText("example.json");
            jsonData = JSON.Obj(content);
            // Iterate through each object and check if it has a GroupName attribute
            foreach (object item in jsonData)
            {
                if (item is JsonElement jsonElement && jsonElement.TryGetProperty("GroupName", out JsonElement groupName))
                {
                    availableGroups.Add(groupName.ToString());
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

        private void GroupSelector_Load(object sender, EventArgs e)
        {
            PopulateList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}