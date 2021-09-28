namespace NO.RebarUtils
{
    partial class FormRenumber
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboPartition = new System.Windows.Forms.ComboBox();
            this.comboRebarNumber = new System.Windows.Forms.ComboBox();
            this.textNewNumber = new System.Windows.Forms.TextBox();
            this.buttonChange = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Partition";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Rebar Number";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "New Number";
            // 
            // comboPartition
            // 
            this.comboPartition.FormattingEnabled = true;
            this.comboPartition.Location = new System.Drawing.Point(120, 22);
            this.comboPartition.Name = "comboPartition";
            this.comboPartition.Size = new System.Drawing.Size(121, 21);
            this.comboPartition.TabIndex = 3;
            this.comboPartition.SelectedIndexChanged += new System.EventHandler(this.comboPartitions_SelectedIndexChanged);
            // 
            // comboRebarNumber
            // 
            this.comboRebarNumber.FormattingEnabled = true;
            this.comboRebarNumber.Location = new System.Drawing.Point(120, 61);
            this.comboRebarNumber.Name = "comboRebarNumber";
            this.comboRebarNumber.Size = new System.Drawing.Size(121, 21);
            this.comboRebarNumber.TabIndex = 4;
            // 
            // textNewNumber
            // 
            this.textNewNumber.Location = new System.Drawing.Point(120, 102);
            this.textNewNumber.Name = "textNewNumber";
            this.textNewNumber.Size = new System.Drawing.Size(121, 20);
            this.textNewNumber.TabIndex = 5;
            // 
            // buttonChange
            // 
            this.buttonChange.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonChange.Location = new System.Drawing.Point(84, 156);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(75, 23);
            this.buttonChange.TabIndex = 6;
            this.buttonChange.Text = "Change";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Clicked);
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(166, 156);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 7;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Clicked);
            // 
            // FormRenumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 219);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.textNewNumber);
            this.Controls.Add(this.comboRebarNumber);
            this.Controls.Add(this.comboPartition);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormRenumber";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change Rebar Number";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboPartition;
        private System.Windows.Forms.ComboBox comboRebarNumber;
        private System.Windows.Forms.TextBox textNewNumber;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Button buttonClose;
    }
}