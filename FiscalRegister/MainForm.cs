using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Net;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Atol
{
    public partial class MainForm : Form
    {
        public VMSettings settings = new VMSettings();
        public const string configFile = "config.xml";
        public const string logFile = "log.txt";
        public const long maxLogSize = 10 * 1024 * 1024; // 10 MB
        public bool LastError = false;
        public System.Windows.Forms.Timer memoryTimer = new System.Windows.Forms.Timer();
        public System.Windows.Forms.Timer garbageTimer = new System.Windows.Forms.Timer();
        public System.Windows.Forms.Timer remoteVersionChecker = new System.Windows.Forms.Timer();
        public Worker worker = new Worker();
        Thread workerThread = null;
        private MyNotifyIcon iconClass = new MyNotifyIcon();

        public IContainer getComponents()
        {
            return this.components;
        }

        public MainForm()
        {
            InitializeComponent();
            this.Text = "VetManager и АТОЛ " + Version.version;

            Version.ReadVmVersionFromFile();

            AddToLog("Текущая версия программы: " + Version.version);

            this.iconClass.initIcon(this);

            Label.CheckForIllegalCrossThreadCalls = false;
            TextBox.CheckForIllegalCrossThreadCalls = false;
            Button.CheckForIllegalCrossThreadCalls = false;

            btnDisconnect.Enabled = false;

            if (LoadSettingsFromFile())
            {
                SetSettingsInfo();
                worker.settings = settings;
                worker.mainForm = this;
            }
            else
            {
                throw new Exception("Ошибка чтения данных конфига");
            }

            memoryTimer.Interval = 5000;
            memoryTimer.Tick += new EventHandler(memoryTimer_Tick);
            memoryTimer.Start();

            garbageTimer.Interval = 60000;
            garbageTimer.Tick += new EventHandler(garbageTimer_Tick);
            garbageTimer.Start();

            remoteVersionChecker.Interval = 1000 * 60 * 10; // 10 mins
            //remoteVersionChecker.Interval = 1000 * 10; // 10 mins // TODO remove
            remoteVersionChecker.Tick += new EventHandler(remoteVersionChecker_Tick);
            remoteVersionChecker.Start();

            if (this.settings.StartAfterRun)
            {
                StartProgram();
                this.iconClass.disableStart();
                this.iconClass.showHideForm();
            }
        }

        void remoteVersionChecker_Tick(object sender, EventArgs e)
        {
            remoteVersionChecker.Stop();

            AddToLog("Остановка рабочего потока для проверки версии");

            StopProgram();
            this.iconClass.disableStop();

            AddToLog("Проверка новой версии на удаленном сервере");
            
            VersionController.Response resp = VersionController.VersionChecker.UpdateRemote(Version.vmVersion, Version.version);
            
            remoteVersionChecker.Start();

            if (!resp.isError)
            {
                if (resp.isFileLoadeed)
                {
                    AddToLog("Новая версия загружена с удаленного домена");

                    if (VersionController.VersionChecker.UnzipProgram())
                    {
                        AddToLog("Обновление распаковано!");
                        // file unzipped
                        AddToLog("Попытка начать обновление, текущая (старая) версия: " + Version.version);
                        if (VersionController.VersionChecker.StartUpdator())
                        {
                            this.Close();
                        }
                        else
                        {
                            AddToLog("Ошибка обновления!!!");
                        }
                    }
                    else
                    {
                        // error unzipping
                        AddToLog("Ошибка распаковки обновления!!!");
                    }
                }
                else if (resp.message.IndexOf('.') != -1 && resp.message == Version.version)
                {
                    AddToLog("Клиент не нуждается в обновлении");
                }
            }
            else
            {
                AddToLog("Ошибка проверки версии: " + resp.message);
            }

            StartProgram();
            this.iconClass.disableStart();
        }

        void garbageTimer_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }

        void memoryTimer_Tick(object sender, EventArgs e)
        {
            lblMemory.Text = "Память: " + Convert.ToString(Process.GetCurrentProcess().WorkingSet64 / 1048576) + " МБ";
        }

        private void SetSettingsInfo()
        {
            lblDomain.Text = "Домен: " + this.settings.Url;
            lblApiKey.Text = "API KEY: " + this.settings.ApiKey;
            if (this.settings.Device.CurrentDeviceName != "")
            {
                lblRegisterModel.Text = "Модель ККМ: " + this.settings.Device.CurrentDeviceName;
            }
            else
            {
                lblRegisterModel.Text = "Модель ККМ: не указана";
            }
            
        }

        private bool LoadSettingsFromFile()
        {
            if (!File.Exists(configFile))
            {
                return false;
            }
            TextReader reader = new StreamReader(configFile);
            bool result = false;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(VMSettings));
                this.settings = (VMSettings)serializer.Deserialize(reader);
                result = true;
            }
            catch
            {

            }
            finally
            {
                reader.Close();
            }

            reader = null;

            return result;
        }

        private bool SaveSettingsToFile()
        {
            TextWriter writter = new StreamWriter(configFile);
            bool result = false;

            try
            {
                XmlSerializer serializer = new XmlSerializer(this.settings.GetType());
                serializer.Serialize(writter, this.settings);
                result = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                writter.Flush();
                writter.Close();
            }

            writter = null;

            return result;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SettingsForm frm = new SettingsForm(this);
            frm.SetValues();

            if (frm.ShowDialog() == DialogResult.Yes)
            {
                if (frm.urlFld.Text != "")
                {
                    string lastChar = frm.urlFld.Text.Substring(frm.urlFld.Text.Length - 1, 1);

                    if (lastChar == "/")
                    {
                        frm.urlFld.Text = frm.urlFld.Text.Substring(0, frm.urlFld.Text.Length - 1);
                    }

                    if (frm.urlFld.Text.IndexOf("http") == -1)
                    {
                        frm.urlFld.Text = "https://" + frm.urlFld.Text;
                    }
                }

                this.settings.Url = frm.urlFld.Text;
                this.settings.ApiKey = frm.apiKeyFld.Text;
                this.settings.StartAfterRun = frm.checkStartAfterRun.Checked;

                if (SaveSettingsToFile())
                {
                    SetSettingsInfo();
                    this.AddToLog("Данные сохранены");
                }
            }
        }

        public void AddToLog(string p)
        {
            if (p.Trim() == "")
            {
                return;
            }

            string date = DateTime.Now.ToString("dd.MM HH:mm:ss");
            p = "[" + date + "] " + p;

            if (logTextBox.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate {
                    logTextBox.Text = p + "\r\n" + logTextBox.Text;
                }));
            }
            else
            {
                logTextBox.Text = p + "\r\n" + logTextBox.Text;
            }

            using (StreamWriter w = File.AppendText(logFile))
            {
                w.WriteLine(p);
            }
            
            FileInfo fi = new FileInfo(logFile);
            if (fi.Length >= maxLogSize)
            {
                File.WriteAllText(logFile, String.Empty);
            }

            if (logTextBox.Text.Length > 20000)
            {
                logTextBox.Text = "";
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            StartProgram();
            this.iconClass.disableStart();
        }

        public void StartProgram()
        {
            worker.isNeedIterrupt = false;
            this.workerThread = new Thread(new ThreadStart(worker.StartWork));
            this.workerThread.Start();

            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;
            btnSettings.Enabled = false;
            btnAtolSettings.Enabled = false;
        }

        public void StopProgram()
        {
            if (this.workerThread != null)
            {
                worker.isNeedIterrupt = true;
                this.workerThread.Abort();
                AddToLog("Stop worker thread");

                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
                btnSettings.Enabled = true;
                btnAtolSettings.Enabled = true;

                this.workerThread = null;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            StopProgram();
            this.iconClass.disableStop();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopProgram();

            this.iconClass.destroy();
            
        }

        private void btnAtolSettings_Click(object sender, EventArgs e)
        {
            string atolSettings = worker.ShowProperties();
            this.settings.Device = null;
            this.settings.Device = new AtolSettings();

            if (atolSettings != "")
            {
                string[] separators = { "\r\n" };
                string[] lines = atolSettings.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string[] res = line.Split('=');

                    this.settings.Device[res[0]] = res[1];
                }
                
                lines = null;
            }

            this.SaveSettingsToFile();
        }
    }
}
