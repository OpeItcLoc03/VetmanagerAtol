using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Atol
{
    public partial class SettingsForm : Form
    {
        MainForm mainForm = null;

        public SettingsForm(MainForm frm)
        {
            mainForm = frm;

            InitializeComponent();
        }

        internal void SetValues()
        {
            urlFld.Text = mainForm.settings.Url;
            apiKeyFld.Text = mainForm.settings.ApiKey;
            checkStartAfterRun.Checked = mainForm.settings.StartAfterRun;
        }

        private void DisableFields()
        {
            urlFld.Enabled = false;
            apiKeyFld.Enabled = false;
            checkStartAfterRun.Enabled = false;
            checkConnectionBtn.Enabled = false;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
        }

        private void EnableFields()
        {
            if (urlFld.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate{ urlFld.Enabled = true; }));
            }
            else
            {
                urlFld.Enabled = true;
            }

            if (apiKeyFld.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate{ apiKeyFld.Enabled = true; }));
            }
            else
            {
                apiKeyFld.Enabled = true;
            }


            if (checkStartAfterRun.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate { checkStartAfterRun.Enabled = true; }));
            }
            else
            {
                checkStartAfterRun.Enabled = true;
            }

            if (checkConnectionBtn.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate { checkConnectionBtn.Enabled = true; }));
            }
            else
            {
                checkConnectionBtn.Enabled = true;
            }

            if (btnSave.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate { btnSave.Enabled = true; }));
            }
            else
            {
                btnSave.Enabled = true;
            }

            if (btnCancel.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate { btnCancel.Enabled = true; }));
            }
            else
            {
                btnCancel.Enabled = true;
            }
        }

        private void checkConnectionBtn_Click(object sender, EventArgs e)
        {
            MyWebProxy wp = new MyWebProxy();
            wp.url = mainForm.settings.GetFullUrl(urlFld.Text);
            wp.api = apiKeyFld.Text;
            DisableFields();
            WebResult res = wp.checkConnection();
            EnableFields();

            if (!res.isError && res.data != "")
            {
                if (res.data.IndexOf("connection_succesfull") != -1)
                {
                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }

                    mainForm.AddToLog("Соединено успешно");
                    MessageBox.Show("Соединено успешно", "Системное сообщение");
                }
            }
            else
            {
                mainForm.AddToLog("Ошибка соединения");
                MessageBox.Show("Ошибка соединения", "Системное сообщение");
            }

            wp = null;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }
    }
}
