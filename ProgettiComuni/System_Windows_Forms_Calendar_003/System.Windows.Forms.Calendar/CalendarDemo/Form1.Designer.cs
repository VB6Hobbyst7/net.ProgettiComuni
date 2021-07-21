namespace CalendarDemo
{
    partial class DemoForm
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
            this.calendarUserControl1 = new System.Windows.Forms.Calendar.CalendarUserControl();
            this.SuspendLayout();
            // 
            // calendarUserControl1
            // 
            this.calendarUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.calendarUserControl1.Location = new System.Drawing.Point(0, 0);
            this.calendarUserControl1.Name = "calendarUserControl1";
            this.calendarUserControl1.Size = new System.Drawing.Size(1121, 781);
            this.calendarUserControl1.TabIndex = 0;
            // 
            // DemoForm
            // 
            this.ClientSize = new System.Drawing.Size(1121, 781);
            this.Controls.Add(this.calendarUserControl1);
            this.Name = "DemoForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Calendar.CalendarUserControl calendarUserControl1;
    }
}

