using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace Atol
{
    public class MyNotifyIcon
    {

        private NotifyIcon mainNofityIcon;
        private ContextMenu notifyIconContextMenu;
        private MenuItem menuItemShow;
        private MenuItem menuItemClose;
        private MenuItem menuItemStart;
        private MenuItem menuItemStop;
        private MainForm frm;

        public void initIcon(MainForm f)
        {
            this.frm = f;

            frm.Resize += new EventHandler(frm_Resize);
            frm.Shown += new EventHandler(frm_Shown);

            IContainer aa = frm.getComponents();
            aa = new System.ComponentModel.Container();
            this.notifyIconContextMenu = new ContextMenu();
            this.menuItemShow = new MenuItem();
            this.menuItemClose = new MenuItem();
            this.menuItemStart = new MenuItem();
            this.menuItemStop = new MenuItem();
            this.notifyIconContextMenu.MenuItems.AddRange(new MenuItem[] { this.menuItemShow, this.menuItemClose });
            this.menuItemShow.Index = 0;
            this.menuItemShow.Text = "Hide";
            this.menuItemShow.Click += new EventHandler(menuItemShow_Click);
            this.menuItemStart.Index = 1;
            this.menuItemStart.Text = "Start";
            this.menuItemStart.Click += new EventHandler(menuItemStart_Click);
            this.notifyIconContextMenu.MenuItems.Add(this.menuItemStart);
            this.menuItemStop.Index = 2;
            this.menuItemStop.Text = "Stop";
            this.menuItemStop.Click += new EventHandler(menuItemStop_Click);
            this.notifyIconContextMenu.MenuItems.Add(this.menuItemStop);
            this.menuItemClose.Index = 3;
            this.menuItemClose.Text = "E&xit";
            this.menuItemClose.Click += new EventHandler(menuItemClose_Click);
            this.notifyIconContextMenu.MenuItems.Add(this.menuItemClose);
            this.mainNofityIcon = new NotifyIcon(aa);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(frm.GetType());
            this.mainNofityIcon.Icon = frm.Icon;
            this.mainNofityIcon.ContextMenu = this.notifyIconContextMenu;
            this.mainNofityIcon.Text = frm.Text;
            this.mainNofityIcon.Visible = true;
            this.mainNofityIcon.DoubleClick += new EventHandler(mainNofityIcon_DoubleClick);

            this.disableStop();
        }

        void frm_Shown(object sender, EventArgs e)
        {
            //this.stop();
            //if (this.readSettingsFromFile())
            //{
            //    manager = new TaskManager(this, this.settings);
            //    this.addToLog("set params");
            //    this.setParams();
            //    if (!this.isFormValid())
            //    {
            //        this.addToLog("error: form is not valid");
            //    }
            //    if (this.settings.StartAfterRun)
            //    {
            //        this.start();
            //        this.WindowState = FormWindowState.Minimized;
            //        this.Hide();
            //        this.ShowInTaskbar = false;
            //        this.menuItemShow.Text = "Show";
            //    }
            //}
            //else
            //{
            //    this.addToLog("error: cant load config file");
            //    this.stop();
            //}
        }

        public void showHideForm()
        {
            if (frm.WindowState == FormWindowState.Minimized)
            {
                frm.Show();
                frm.ShowInTaskbar = true;
                frm.WindowState = FormWindowState.Normal;
                this.menuItemShow.Text = "Hide";
            }
            else
            {
                frm.WindowState = FormWindowState.Minimized;
                frm.Hide();
                frm.ShowInTaskbar = false;
                this.menuItemShow.Text = "Show";
            }
        }

        void frm_Resize(object sender, EventArgs e)
        {
            if (this.frm.WindowState == FormWindowState.Minimized)
            {
                this.frm.Hide();
                this.menuItemShow.Text = "Show";
            }
        }

        void mainNofityIcon_DoubleClick(object sender, EventArgs e)
        {
            this.showHideForm();
        }

        void menuItemClose_Click(object sender, EventArgs e)
        {
            frm.Close();
        }

        void menuItemStop_Click(object sender, EventArgs e)
        {
            frm.StopProgram();
            this.disableStop();
        }

        void menuItemStart_Click(object sender, EventArgs e)
        {
            frm.StartProgram();
            this.disableStart();
        }

        void menuItemShow_Click(object sender, EventArgs e)
        {
            this.showHideForm();
        }

        internal void destroy()
        {
            this.mainNofityIcon.Dispose();
        }

        public void disableStart()
        {
            this.menuItemStart.Enabled = false;
            this.menuItemStop.Enabled = true;
        }

        public void disableStop()
        {
            this.menuItemStart.Enabled = true;
            this.menuItemStop.Enabled = false;

        }
    }   
}
