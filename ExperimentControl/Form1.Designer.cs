using System.Drawing;

namespace ExperimentControl
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.laserTxtBox = new System.Windows.Forms.TextBox();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.lampTxtBox = new System.Windows.Forms.TextBox();
            this.redLampTxtBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(12, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(12, 59);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 1;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // laserTxtBox
            // 
            this.laserTxtBox.BackColor = System.Drawing.Color.Red;
            this.laserTxtBox.Location = new System.Drawing.Point(12, 193);
            this.laserTxtBox.Name = "laserTxtBox";
            this.laserTxtBox.Size = new System.Drawing.Size(194, 20);
            this.laserTxtBox.TabIndex = 2;
            this.laserTxtBox.Text = "Laser Shutter: CLOSE";
            // 
            // refreshTimer
            // 
            this.refreshTimer.Tick += new System.EventHandler(this.Refreshtimer_Tick);
            // 
            // lampTxtBox
            // 
            this.lampTxtBox.BackColor = System.Drawing.Color.Gray;
            this.lampTxtBox.Location = new System.Drawing.Point(12, 167);
            this.lampTxtBox.Name = "lampTxtBox";
            this.lampTxtBox.Size = new System.Drawing.Size(194, 20);
            this.lampTxtBox.TabIndex = 3;
            this.lampTxtBox.Text = "Main lamp: NIGHT";
            // 
            // redLampTxtBox
            // 
            this.redLampTxtBox.BackColor = System.Drawing.Color.Red;
            this.redLampTxtBox.Location = new System.Drawing.Point(12, 219);
            this.redLampTxtBox.Name = "redLampTxtBox";
            this.redLampTxtBox.Size = new System.Drawing.Size(194, 20);
            this.redLampTxtBox.TabIndex = 4;
            this.redLampTxtBox.Text = "Red lamp: OFF";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(521, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(8, 8);
            this.button1.TabIndex = 5;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.redLampTxtBox);
            this.Controls.Add(this.lampTxtBox);
            this.Controls.Add(this.laserTxtBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.TextBox laserTxtBox;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.TextBox lampTxtBox;
        private System.Windows.Forms.TextBox redLampTxtBox;
        private System.Windows.Forms.Button button1;
    }
}

