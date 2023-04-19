using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NLog;
using NLog.Windows.Forms;

namespace Triggered
{
    public partial class App_ParentFrame : Form
    {
        #region Init
        private static Logger log;
        private int borderSize = 2;
        private Size formSize;
        public App_ParentFrame()
        {
            InitializeComponent();
            DesignAdjustment();
            this.Padding = new Padding(borderSize); //Border size 
            this.BackColor = Color.FromArgb(20, 20, 20);
            // Get the logger instance
            log = LogManager.GetCurrentClassLogger();
            log.Info("App Initiated\n----------------------------------------------------------------------");
        }
        private void App_ParentFrame_Load(object sender, EventArgs e)
        {
            RichTextBoxTarget.ReInitializeAllTextboxes(this);
            formSize = this.ClientSize;
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
            {
                activeForm.Close();
                SideMenu_Panel_Viewpane.Controls.Remove(activeForm);
            }

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            // Disable the SideMenu_Panel_BottomLog panel
            SideMenu_Panel_BottomLog.Visible = false;
            SideMenu_Panel_LogTextbox.Visible = false;

            SideMenu_Panel_Viewpane.Controls.Add(childForm);
            SideMenu_Panel_Viewpane.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

            // Subscribe to FormClosed event
            childForm.FormClosed += ChildForm_FormClosed;
        }
        private void ChildForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Unsubscribe from FormClosed event
            ((Form)sender).FormClosed -= ChildForm_FormClosed;

            SideMenu_Panel_BottomLog.Visible = true;
            SideMenu_Panel_LogTextbox.Visible = true;
        }
        #endregion

        #region Event Functions
        //DragForm
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        //Override border creating and snapping
        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083; //Standard Title Bar - Snap Window 
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020; //Minimize form (Before) 
            const int SC_RESTORE = 0xF120; //Restore form (Before) 
            const int WM_NCHITTEST = 0x0084; //Win32, Mouse Input Notification: Determine what part of the window corresponds to a point, allows to resize the form. 
            const int resizeAreaSize = 10;
            #region Form Resize
            // Resize/WM_NCHITTEST values
            const int HTCLIENT = 1; //Represents the client area of ​​the window 
            const int HTLEFT = 10;  //Left border of a window, allows resize horizontally to the left 
            const int HTRIGHT = 11; //Right border of a window, allows resize horizontally to the right 
            const int HTTOP = 12;   //Upper-horizontal border of a window, allows resize vertically up 
            const int HTTOPLEFT = 13; //Upper-left corner of a window border, allows resize diagonally to the left 
            const int HTTOPRIGHT = 14; //Upper-right corner of a window border, allows resize diagonally to the right 
            const int HTBOTTOM = 15; //Lower-horizontal border of a window, allows resize vertically down 
            const int HTBOTTOMLEFT = 16; //Lower-left corner of a window border, allows resize diagonally to the left 
            const int HTBOTTOMRIGHT = 17; //Lower-right corner of a window border, allows resize diagonally to the right 
            ///<Doc> More Information: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest </Doc>
            if (m.Msg == WM_NCHITTEST)
            { //If the windows m is WM_NCHITTEST 
                base.WndProc(ref m);
                if (this.WindowState == FormWindowState.Normal) //Resize the form if it is in normal state 
                {
                    if ((int)m.Result == HTCLIENT) //If the result of the m (mouse pointer) is in the client area of ​​the window 
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32()); //Gets screen point coordinates(X and Y coordinate of the pointer)                            
                        Point clientPoint = this.PointToClient(screenPoint); //Computes the location of the screen point into client coordinates                          
                        if (clientPoint.Y <= resizeAreaSize) //If the pointer is at the top of the form (within the resize area- X coordinate)  
                        {
                            if (clientPoint.X <= resizeAreaSize) //If the pointer is at the coordinate X=0 or less than the resizing area(X=10) in   
                                m.Result = (IntPtr)HTTOPLEFT; //Resize diagonally to the left
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize)) //If the pointer is at the coordinate X=11 or less than the width of the form(X=Form.Width-resizeArea)    
                                m.Result = (IntPtr)HTTOP; //Resize vertically up
                            else //Resize diagonally to the right 
                                m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (clientPoint.Y <= (this.Size.Height - resizeAreaSize)) // If the pointer is inside the form at the Y coordinate(discounting the resize area size )    
                        {
                            if (clientPoint.X <= resizeAreaSize) //Resize horizontally to the left  
                                m.Result = (IntPtr)HTLEFT;
                            else if (clientPoint.X > (this.Width - resizeAreaSize)) //Resize horizontally to the right    
                                m.Result = (IntPtr)HTRIGHT;
                        }
                        else
                        {
                            if (clientPoint.X <= resizeAreaSize) //Resize diagonally to the left  
                                m.Result = (IntPtr)HTBOTTOMLEFT;
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize)) //Resize vertically down     
                                m.Result = (IntPtr)HTBOTTOM;
                            else //Resize diagonally to the right 
                                m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                    }
                }
                return;
            }
            #endregion
            //Remove border and keep snap window
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                return;
            }
            //Keep form size when it is minimized and restored. Since the form is resized because it takes into account the size of the title bar and borders.
            if (m.Msg == WM_SYSCOMMAND)
            {
                /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
                /// Quote:
                /// In WM_SYSCOMMAND messages, the four low-order bits of the wParam parameter
                /// are used internally by the system. To obtain the correct result when testing
                /// the value of wParam, an application must combine the value 0xFFF0 with the
                /// wParam value by using the bitwise AND operator.
                int wParam = (m.WParam.ToInt32() & 0xFFF0);
                if (wParam == SC_MINIMIZE) //Before   
                    formSize = this.ClientSize;
                if (wParam == SC_RESTORE) // Restored form(Before) 
                    this.Size = formSize;
            }
            base.WndProc(ref m);
        }
        // Adjust the form when Maximizing and Restoring

        private void AdjustForm()
        {
            switch (this.WindowState)
            {
                case FormWindowState.Maximized : //Maximized form (After)
                    this.Padding = new Padding(8, 8, 8, 0);
                    break;
                case FormWindowState.Normal : //Restored form (After)
                    if (this.Padding.Top != borderSize)
                        this.Padding = new Padding(borderSize);
                    break;
            }
        }
        private string Get_Tag_or_Text(object sender)
        {
            // Cast the sender object to a Button, assuming it is a Button
            if (sender is Button button)
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
                    log.Info("Test Message - This is a test"); // informational message
                    log.Warn("Test Message - This is a test"); // warning message
                    log.Error("Test Message - This is a test"); // error message
                    log.Fatal("Test Message - This is a test"); // highest level, critical error message
                    break;
                default:
                    break;
            }
            log.Debug(str);
        }

        private void Window_Button_Clicks(object sender, EventArgs e)
        {
            string str = Get_Tag_or_Text(sender);
            if (str == "")
            {
                return;
            }
            switch (str)
            {
                case "Minimize":
                    WindowState = FormWindowState.Minimized;
                    break;
                case "Maximize":
                    MaximizeLogic();
                    break;
                case "Close":
                    Close();
                    break;
                default:
                    break;
            }
        }

        private void MaximizeLogic()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }
            ToggleMaximizeRestoreButton();
        }
        private void ToggleMaximizeRestoreButton()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Window_Button_Max.ImageIndex = 3;
            }
            else
            {
                Window_Button_Max.ImageIndex = 1;
            }
        }

        private void App_ParentFrame_Resize(object sender, EventArgs e)
        {
            ToggleMaximizeRestoreButton();
            AdjustForm();
        }
        #endregion
    }
}
