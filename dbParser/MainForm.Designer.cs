namespace dbParser
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
            this.RecordsProgressBar = new System.Windows.Forms.ProgressBar();
            this.RecordsLabel = new System.Windows.Forms.Label();
            this.SPCButton = new System.Windows.Forms.Button();
            this.SpeedMeasureLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RecordsProgressBar
            // 
            this.RecordsProgressBar.Location = new System.Drawing.Point(12, 12);
            this.RecordsProgressBar.Name = "RecordsProgressBar";
            this.RecordsProgressBar.Size = new System.Drawing.Size(339, 35);
            this.RecordsProgressBar.Step = 1;
            this.RecordsProgressBar.TabIndex = 0;
            // 
            // RecordsLabel
            // 
            this.RecordsLabel.AutoSize = true;
            this.RecordsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RecordsLabel.Location = new System.Drawing.Point(177, 63);
            this.RecordsLabel.Name = "RecordsLabel";
            this.RecordsLabel.Size = new System.Drawing.Size(18, 20);
            this.RecordsLabel.TabIndex = 1;
            this.RecordsLabel.Text = "0";
            // 
            // SPCButton
            // 
            this.SPCButton.Location = new System.Drawing.Point(94, 119);
            this.SPCButton.Name = "SPCButton";
            this.SPCButton.Size = new System.Drawing.Size(192, 31);
            this.SPCButton.TabIndex = 2;
            this.SPCButton.Text = "Start";
            this.SPCButton.UseVisualStyleBackColor = true;
            this.SPCButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // SpeedMeasureLabel
            // 
            this.SpeedMeasureLabel.AutoSize = true;
            this.SpeedMeasureLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SpeedMeasureLabel.Location = new System.Drawing.Point(116, 96);
            this.SpeedMeasureLabel.Name = "SpeedMeasureLabel";
            this.SpeedMeasureLabel.Size = new System.Drawing.Size(154, 20);
            this.SpeedMeasureLabel.TabIndex = 3;
            this.SpeedMeasureLabel.Text = "0 records per minute";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 181);
            this.Controls.Add(this.SpeedMeasureLabel);
            this.Controls.Add(this.SPCButton);
            this.Controls.Add(this.RecordsLabel);
            this.Controls.Add(this.RecordsProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar RecordsProgressBar;
        private System.Windows.Forms.Label RecordsLabel;
        private System.Windows.Forms.Button SPCButton;
        private System.Windows.Forms.Label SpeedMeasureLabel;
    }
}

