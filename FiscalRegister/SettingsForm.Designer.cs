namespace Atol
{
    partial class SettingsForm
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
            this.checkStartAfterRun = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.checkConnectionBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.apiKeyFld = new System.Windows.Forms.TextBox();
            this.urlFld = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // checkStartAfterRun
            // 
            this.checkStartAfterRun.Location = new System.Drawing.Point(12, 79);
            this.checkStartAfterRun.Name = "checkStartAfterRun";
            this.checkStartAfterRun.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkStartAfterRun.Size = new System.Drawing.Size(271, 19);
            this.checkStartAfterRun.TabIndex = 55;
            this.checkStartAfterRun.Text = "Установить связь после старта программы";
            this.checkStartAfterRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkStartAfterRun.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(253, 113);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 53;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnSave.Location = new System.Drawing.Point(164, 113);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 52;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // checkConnectionBtn
            // 
            this.checkConnectionBtn.Location = new System.Drawing.Point(378, 9);
            this.checkConnectionBtn.Name = "checkConnectionBtn";
            this.checkConnectionBtn.Size = new System.Drawing.Size(109, 57);
            this.checkConnectionBtn.TabIndex = 51;
            this.checkConnectionBtn.Text = "Проверить соединение с VetManager";
            this.checkConnectionBtn.UseVisualStyleBackColor = true;
            this.checkConnectionBtn.Click += new System.EventHandler(this.checkConnectionBtn_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 50;
            this.label4.Text = "API KEY";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 49;
            this.label3.Text = "Clinic URL";
            // 
            // apiKeyFld
            // 
            this.apiKeyFld.Location = new System.Drawing.Point(77, 42);
            this.apiKeyFld.Name = "apiKeyFld";
            this.apiKeyFld.Size = new System.Drawing.Size(295, 20);
            this.apiKeyFld.TabIndex = 48;
            // 
            // urlFld
            // 
            this.urlFld.Location = new System.Drawing.Point(77, 5);
            this.urlFld.Name = "urlFld";
            this.urlFld.Size = new System.Drawing.Size(294, 20);
            this.urlFld.TabIndex = 47;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Aquamarine;
            this.ClientSize = new System.Drawing.Size(497, 149);
            this.Controls.Add(this.checkStartAfterRun);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.checkConnectionBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.apiKeyFld);
            this.Controls.Add(this.urlFld);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Настройки";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button checkConnectionBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckBox checkStartAfterRun;
        public System.Windows.Forms.TextBox apiKeyFld;
        public System.Windows.Forms.TextBox urlFld;
    }
}

