namespace LinkCRequest
{
    partial class CertAutoInstall_form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CertAutoInstall_form));
            this.ProgBar_AutoInstall = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ProgBar_AutoInstall
            // 
            this.ProgBar_AutoInstall.Location = new System.Drawing.Point(26, 35);
            this.ProgBar_AutoInstall.MarqueeAnimationSpeed = 20;
            this.ProgBar_AutoInstall.Name = "ProgBar_AutoInstall";
            this.ProgBar_AutoInstall.Size = new System.Drawing.Size(457, 11);
            this.ProgBar_AutoInstall.Step = 1;
            this.ProgBar_AutoInstall.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgBar_AutoInstall.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(24, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Идет поиск контейнеров ...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(271, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Выполняется поиск сертификатов в контейнерах ...";
            // 
            // CertAutoInstall_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 81);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ProgBar_AutoInstall);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CertAutoInstall_form";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Автоматическая установка сертификатов УЦ Линк-сервис";
            this.Load += new System.EventHandler(this.CertAutoInstall_form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar ProgBar_AutoInstall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}