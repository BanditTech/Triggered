using System;
using System.Windows.Forms;
using NLog;
using NLog.Windows.Forms;

namespace Triggered
{
    public partial class App_ParentFrame : Form
    {
        #region Init
        private static Logger log;
        public App_ParentFrame()
        {
            InitializeComponent();
            DesignAdjustment();

            // Get the logger instance
            log = LogManager.GetCurrentClassLogger();
            log.Info("App Initiated\n----------------------------------------------------------------------");
        }
        private void App_ParentFrame_Load(object sender, EventArgs e)
        {
            RichTextBoxTarget.ReInitializeAllTextboxes(this);
        }
        #endregion

        #region Sidebar Visibility Functions
        private void DesignAdjustment()
        {
            SideMenu_Panel_FirstMenu.Visible = false;
            SideMenu_Button_FirstMenu_Submenu.ImageIndex = 0;
            SideMenu_Panel_SecondMenu.Visible = false;
            SideMenu_Button_SecondMenu_Submenu.ImageIndex = 0;
            SideMenu_Panel_ThirdMenu.Visible = false;
            SideMenu_Button_ThirdMenu_Submenu.ImageIndex = 0;
        }
        private void HideSubMenu()
        {
            if (SideMenu_Panel_FirstMenu.Visible == true)
            {
                SideMenu_Button_FirstMenu_Submenu.ImageIndex = 0;
                SideMenu_Panel_FirstMenu.Visible = false;
            }
            if (SideMenu_Panel_SecondMenu.Visible == true)
            {
                SideMenu_Button_SecondMenu_Submenu.ImageIndex = 0;
                SideMenu_Panel_SecondMenu.Visible = false;
            }
            if (SideMenu_Panel_ThirdMenu.Visible == true)
            {
                SideMenu_Button_ThirdMenu_Submenu.ImageIndex = 0;
                SideMenu_Panel_ThirdMenu.Visible = false;
            }
        }
        private void ToggleSubMenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
            {
                HideSubMenu();
                subMenu.Visible = true;
            }
            else
                subMenu.Visible = false;
        }
        private void ToggleButtonImage(Button button)
        {
            if (button.ImageIndex == 1)
            {
                button.ImageIndex = 0;
            }
            else
                button.ImageIndex = 1;
        }
        #endregion

        #region Embed Child Form
        private Form activeForm = null;
        private void OpenChildForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            SideMenu_Panel_Viewpane.Controls.Add(childForm);
            SideMenu_Panel_Viewpane.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
        #endregion

        #region Button Clicks
        private string Get_Tag_or_Text(object sender)
        {
            // Cast the sender object to a Button, assuming it is a Button
            Button button = sender as Button;
            if (button != null)
            {
                // Check if the Tag property is null
                if (button.Tag != null)
                {
                    string tag = button.Tag.ToString();
                    return tag;
                }
                else if (button.Text != null)
                {
                    string text = button.Text.ToString();
                    return text;
                }
                else
                {
                    return "Null";
                }
            }
            else
                return "";
        }

        private void CatchAll(object sender, EventArgs e)
        {
            string str = Get_Tag_or_Text(sender);
            if (str == "")
            {
                return;
            }
            switch (str)
            {
                case "General":
                    ToggleButtonImage(SideMenu_Button_FirstMenu_Submenu);
                    ToggleSubMenu(SideMenu_Panel_FirstMenu);
                    break;
                case "Locations":
                    ToggleButtonImage(SideMenu_Button_SecondMenu_Submenu);
                    ToggleSubMenu(SideMenu_Panel_SecondMenu);
                    break;
                case "Configuration":
                    ToggleButtonImage(SideMenu_Button_ThirdMenu_Submenu);
                    ToggleSubMenu(SideMenu_Panel_ThirdMenu);
                    break;
                case "Program":
                    OpenChildForm(new Program_Settings());
                    break;
                case "Resolution":
                    OpenChildForm(new FirstOpt2());
                    break;
                case "Game":
                    // Do something when the "Game" button is clicked
                    break;
                case "Debug":
                    // Do something when the "Debug" button is clicked
                    break;
                case "Status":
                    // Do something when the "Status" button is clicked
                    break;
                case "Item Grids":
                    // Do something when the "Item Grids" button is clicked
                    break;
                case "Resources":
                    // Do something when the "Resources" button is clicked
                    break;
                case "Flasks":
                    // Do something when the "Flasks" button is clicked
                    break;
                case "Abilities":
                    // Do something when the "Abilities" button is clicked
                    break;
                case "Option":
                    // Do something when the "Option" button is clicked
                    break;
                case "Test Button":
                    log.Trace("Test Message - This is a test"); // lowest level, detailed message for debugging purposes
                    log.Debug("Test Message - This is a test"); // message for debugging purposes
                    log.Info ("Test Message - This is a test"); // informational message
                    log.Warn ("Test Message - This is a test"); // warning message
                    log.Error("Test Message - This is a test"); // error message
                    log.Fatal("Test Message - This is a test"); // highest level, critical error message
                    break;
                default:
                    break;
            }
            log.Debug(str);
        }
        #endregion
    }
}
