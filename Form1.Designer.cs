namespace PetrolimexWidget
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dgvPrices = new DataGridView();
            lblTitle = new Label();
            timerRefresh = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)dgvPrices).BeginInit();
            SuspendLayout();
            // 
            // dgvPrices
            // 
            dgvPrices.AllowUserToAddRows = false;
            dgvPrices.BackgroundColor = Color.FromArgb(30, 30, 30);
            dgvPrices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPrices.Location = new Point(4, 4);
            dgvPrices.Name = "dgvPrices";
            dgvPrices.ReadOnly = true;
            dgvPrices.Size = new Size(342, 200);
            dgvPrices.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.ForeColor = SystemColors.ButtonHighlight;
            lblTitle.Location = new Point(4, 210);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(97, 15);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Petrolimex Prices";
            // 
            // timerRefresh
            // 
            timerRefresh.Enabled = true;
            timerRefresh.Interval = 3600000;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(350, 231);
            Controls.Add(lblTitle);
            Controls.Add(dgvPrices);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Form1";
            TopMost = true;
            ((System.ComponentModel.ISupportInitialize)dgvPrices).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvPrices;
        private Label lblTitle;
        private System.Windows.Forms.Timer timerRefresh;
    }
}
