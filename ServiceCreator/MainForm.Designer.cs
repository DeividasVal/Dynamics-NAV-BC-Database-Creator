namespace ServiceCreator
{
    partial class MainForm
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
            selectBakButton = new Button();
            bakFilePath = new TextBox();
            label2 = new Label();
            databaseName = new TextBox();
            runButton = new Button();
            statusBox = new RichTextBox();
            NAVRadioButton = new RadioButton();
            BCRadioButton = new RadioButton();
            label3 = new Label();
            label1 = new Label();
            selectServerCombo = new ComboBox();
            label4 = new Label();
            label5 = new Label();
            licensePathBC = new TextBox();
            selectButtonLicenseBC = new Button();
            SuspendLayout();
            // 
            // selectBakButton
            // 
            selectBakButton.Location = new Point(522, 80);
            selectBakButton.Name = "selectBakButton";
            selectBakButton.Size = new Size(101, 23);
            selectBakButton.TabIndex = 2;
            selectBakButton.Text = "Select .bak";
            selectBakButton.UseVisualStyleBackColor = true;
            selectBakButton.Click += button1_Click;
            // 
            // bakFilePath
            // 
            bakFilePath.Location = new Point(12, 81);
            bakFilePath.Name = "bakFilePath";
            bakFilePath.Size = new Size(504, 23);
            bakFilePath.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(90, 15);
            label2.TabIndex = 4;
            label2.Text = "Database Name";
            // 
            // databaseName
            // 
            databaseName.Location = new Point(12, 27);
            databaseName.Name = "databaseName";
            databaseName.Size = new Size(263, 23);
            databaseName.TabIndex = 5;
            // 
            // runButton
            // 
            runButton.Location = new Point(552, 508);
            runButton.Name = "runButton";
            runButton.Size = new Size(75, 23);
            runButton.TabIndex = 7;
            runButton.Text = "Run";
            runButton.UseVisualStyleBackColor = true;
            runButton.Click += runButton_Click;
            // 
            // statusBox
            // 
            statusBox.BackColor = SystemColors.Window;
            statusBox.BorderStyle = BorderStyle.FixedSingle;
            statusBox.Location = new Point(12, 228);
            statusBox.Name = "statusBox";
            statusBox.ReadOnly = true;
            statusBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            statusBox.Size = new Size(611, 274);
            statusBox.TabIndex = 8;
            statusBox.Text = "";
            // 
            // NAVRadioButton
            // 
            NAVRadioButton.AutoSize = true;
            NAVRadioButton.Location = new Point(341, 28);
            NAVRadioButton.Name = "NAVRadioButton";
            NAVRadioButton.Size = new Size(48, 19);
            NAVRadioButton.TabIndex = 9;
            NAVRadioButton.TabStop = true;
            NAVRadioButton.Text = "NAV";
            NAVRadioButton.UseVisualStyleBackColor = true;
            // 
            // BCRadioButton
            // 
            BCRadioButton.AutoSize = true;
            BCRadioButton.Location = new Point(295, 28);
            BCRadioButton.Name = "BCRadioButton";
            BCRadioButton.Size = new Size(40, 19);
            BCRadioButton.TabIndex = 10;
            BCRadioButton.TabStop = true;
            BCRadioButton.Text = "BC";
            BCRadioButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(295, 9);
            label3.Name = "label3";
            label3.Size = new Size(124, 15);
            label3.TabIndex = 11;
            label3.Text = "Select if for NAV or BC";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 63);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 12;
            label1.Text = ".bak File Path";
            // 
            // selectServerCombo
            // 
            selectServerCombo.FormattingEnabled = true;
            selectServerCombo.Items.AddRange(new object[] { "Local (on your PC)", "Plan", "Super-Nova", "Super-Plan", "Super-Simple", "Simple" });
            selectServerCombo.Location = new Point(12, 189);
            selectServerCombo.Name = "selectServerCombo";
            selectServerCombo.Size = new Size(121, 23);
            selectServerCombo.TabIndex = 13;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 171);
            label4.Name = "label4";
            label4.Size = new Size(73, 15);
            label4.TabIndex = 14;
            label4.Text = "Select Server";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 117);
            label5.Name = "label5";
            label5.Size = new Size(125, 15);
            label5.TabIndex = 15;
            label5.Text = "License Path (BC only)";
            // 
            // licensePathBC
            // 
            licensePathBC.Location = new Point(12, 135);
            licensePathBC.Name = "licensePathBC";
            licensePathBC.Size = new Size(504, 23);
            licensePathBC.TabIndex = 16;
            // 
            // selectButtonLicenseBC
            // 
            selectButtonLicenseBC.Location = new Point(522, 134);
            selectButtonLicenseBC.Name = "selectButtonLicenseBC";
            selectButtonLicenseBC.Size = new Size(101, 23);
            selectButtonLicenseBC.TabIndex = 17;
            selectButtonLicenseBC.Text = "Select .bclicense";
            selectButtonLicenseBC.UseVisualStyleBackColor = true;
            selectButtonLicenseBC.Click += selectButtonLicenseBC_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 540);
            Controls.Add(selectButtonLicenseBC);
            Controls.Add(licensePathBC);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(selectServerCombo);
            Controls.Add(label1);
            Controls.Add(label3);
            Controls.Add(BCRadioButton);
            Controls.Add(NAVRadioButton);
            Controls.Add(statusBox);
            Controls.Add(runButton);
            Controls.Add(databaseName);
            Controls.Add(label2);
            Controls.Add(bakFilePath);
            Controls.Add(selectBakButton);
            Name = "MainForm";
            Text = "Service Creator NAV/BC";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button selectBakButton;
        private TextBox bakFilePath;
        private Label label2;
        private TextBox databaseName;
        private Button runButton;
        private RichTextBox statusBox;
        private RadioButton NAVRadioButton;
        private RadioButton BCRadioButton;
        private Label label3;
        private Label label1;
        private ComboBox selectServerCombo;
        private Label label4;
        private Label label5;
        private TextBox licensePathBC;
        private Button selectButtonLicenseBC;
    }
}
