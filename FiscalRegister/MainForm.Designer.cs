namespace Atol
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnSettings = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAtolSettings = new System.Windows.Forms.Button();
            this.lblApiKey = new System.Windows.Forms.Label();
            this.lblRegisterModel = new System.Windows.Forms.Label();
            this.lblDomain = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.lblMemory = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(580, 16);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(141, 23);
            this.btnSettings.TabIndex = 0;
            this.btnSettings.Text = "Изменить настройки...";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAtolSettings);
            this.groupBox1.Controls.Add(this.lblApiKey);
            this.groupBox1.Controls.Add(this.lblRegisterModel);
            this.groupBox1.Controls.Add(this.lblDomain);
            this.groupBox1.Controls.Add(this.btnSettings);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(727, 95);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Настройки";
            // 
            // btnAtolSettings
            // 
            this.btnAtolSettings.Location = new System.Drawing.Point(583, 46);
            this.btnAtolSettings.Name = "btnAtolSettings";
            this.btnAtolSettings.Size = new System.Drawing.Size(138, 24);
            this.btnAtolSettings.TabIndex = 6;
            this.btnAtolSettings.Text = "Настройки АТОЛ";
            this.btnAtolSettings.UseVisualStyleBackColor = true;
            this.btnAtolSettings.Click += new System.EventHandler(this.btnAtolSettings_Click);
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(6, 57);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(51, 13);
            this.lblApiKey.TabIndex = 4;
            this.lblApiKey.Text = "API KEY:";
            // 
            // lblRegisterModel
            // 
            this.lblRegisterModel.AutoSize = true;
            this.lblRegisterModel.Location = new System.Drawing.Point(264, 26);
            this.lblRegisterModel.Name = "lblRegisterModel";
            this.lblRegisterModel.Size = new System.Drawing.Size(75, 13);
            this.lblRegisterModel.TabIndex = 2;
            this.lblRegisterModel.Text = "Модель ККМ:";
            // 
            // lblDomain
            // 
            this.lblDomain.AutoSize = true;
            this.lblDomain.Location = new System.Drawing.Point(6, 26);
            this.lblDomain.Name = "lblDomain";
            this.lblDomain.Size = new System.Drawing.Size(45, 13);
            this.lblDomain.TabIndex = 3;
            this.lblDomain.Text = "Домен:";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(229, 115);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(127, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Старт";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(362, 115);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(117, 23);
            this.btnDisconnect.TabIndex = 4;
            this.btnDisconnect.Text = "Стоп";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(13, 144);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(727, 280);
            this.logTextBox.TabIndex = 5;
            // 
            // lblMemory
            // 
            this.lblMemory.AutoSize = true;
            this.lblMemory.Location = new System.Drawing.Point(593, 120);
            this.lblMemory.Name = "lblMemory";
            this.lblMemory.Size = new System.Drawing.Size(0, 13);
            this.lblMemory.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Aquamarine;
            this.ClientSize = new System.Drawing.Size(752, 438);
            this.Controls.Add(this.lblMemory);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VetManager и АТОЛ 1.0.6";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.Label lblDomain;
        private System.Windows.Forms.Label lblRegisterModel;
        private System.Windows.Forms.TextBox logTextBox;
        public System.Windows.Forms.Button btnConnect;
        public System.Windows.Forms.Button btnDisconnect;
        public System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Label lblMemory;
        public System.Windows.Forms.Button btnAtolSettings;
    }
}