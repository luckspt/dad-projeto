﻿using Parser;

namespace Manager
{
    partial class Main
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
            lsvwClients = new ListView();
            lblClients = new Label();
            lsvwTMs = new ListView();
            lsvwLMs = new ListView();
            lblTMs = new Label();
            lblLMs = new Label();
            fdConfig = new OpenFileDialog();
            lblLMsLabel = new Label();
            lblTMsLabel = new Label();
            lblClientsLabel = new Label();
            ctxLMs = new ContextMenuStrip(components);
            crashToolStripMenuItem = new ToolStripMenuItem();
            delayToolStripMenuItem = new ToolStripMenuItem();
            hookConfigToolStripMenuItem = new ToolStripMenuItem();
            btnKillAll = new Button();
            ctxLMs.SuspendLayout();
            SuspendLayout();
            // 
            // lsvwClients
            // 
            lsvwClients.Location = new Point(12, 42);
            lsvwClients.Name = "lsvwClients";
            lsvwClients.ShowItemToolTips = true;
            lsvwClients.Size = new Size(268, 330);
            lsvwClients.TabIndex = 4;
            lsvwClients.UseCompatibleStateImageBehavior = false;
            // 
            // lblClients
            // 
            lblClients.AutoSize = true;
            lblClients.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
            lblClients.Location = new Point(12, 9);
            lblClients.Name = "lblClients";
            lblClients.Size = new Size(77, 30);
            lblClients.TabIndex = 5;
            lblClients.Text = "Clients";
            // 
            // lsvwTMs
            // 
            lsvwTMs.Location = new Point(286, 42);
            lsvwTMs.Name = "lsvwTMs";
            lsvwTMs.ShowItemToolTips = true;
            lsvwTMs.Size = new Size(268, 330);
            lsvwTMs.TabIndex = 6;
            lsvwTMs.UseCompatibleStateImageBehavior = false;
            // 
            // lsvwLMs
            // 
            lsvwLMs.Location = new Point(560, 42);
            lsvwLMs.Name = "lsvwLMs";
            lsvwLMs.ShowItemToolTips = true;
            lsvwLMs.Size = new Size(268, 330);
            lsvwLMs.TabIndex = 7;
            lsvwLMs.UseCompatibleStateImageBehavior = false;
            lsvwLMs.MouseClick += lsvwLMs_MouseClick;
            // 
            // lblTMs
            // 
            lblTMs.AutoSize = true;
            lblTMs.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
            lblTMs.Location = new Point(286, 9);
            lblTMs.Name = "lblTMs";
            lblTMs.Size = new Size(223, 30);
            lblTMs.TabIndex = 8;
            lblTMs.Text = "Transaction Managers";
            // 
            // lblLMs
            // 
            lblLMs.AutoSize = true;
            lblLMs.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
            lblLMs.Location = new Point(560, 9);
            lblLMs.Name = "lblLMs";
            lblLMs.Size = new Size(169, 30);
            lblLMs.TabIndex = 9;
            lblLMs.Text = "Lease Managers";
            // 
            // fdConfig
            // 
            fdConfig.FileName = "fdConfig";
            fdConfig.FileOk += fdConfig_FileOk;
            // 
            // lblLMsLabel
            // 
            lblLMsLabel.AutoSize = true;
            lblLMsLabel.Location = new Point(560, 375);
            lblLMsLabel.Name = "lblLMsLabel";
            lblLMsLabel.Size = new Size(70, 15);
            lblLMsLabel.TabIndex = 10;
            lblLMsLabel.Text = "lblLMsLabel";
            // 
            // lblTMsLabel
            // 
            lblTMsLabel.AutoSize = true;
            lblTMsLabel.Location = new Point(286, 375);
            lblTMsLabel.Name = "lblTMsLabel";
            lblTMsLabel.Size = new Size(70, 15);
            lblTMsLabel.TabIndex = 11;
            lblTMsLabel.Text = "lblTMsLabel";
            // 
            // lblClientsLabel
            // 
            lblClientsLabel.AutoSize = true;
            lblClientsLabel.Location = new Point(12, 375);
            lblClientsLabel.Name = "lblClientsLabel";
            lblClientsLabel.Size = new Size(84, 15);
            lblClientsLabel.TabIndex = 12;
            lblClientsLabel.Text = "lblClientsLabel";
            // 
            // ctxLMs
            // 
            ctxLMs.Items.AddRange(new ToolStripItem[] { crashToolStripMenuItem, delayToolStripMenuItem, hookConfigToolStripMenuItem });
            ctxLMs.Name = "ctxLMs";
            ctxLMs.Size = new Size(143, 70);
            // 
            // crashToolStripMenuItem
            // 
            crashToolStripMenuItem.Name = "crashToolStripMenuItem";
            crashToolStripMenuItem.Size = new Size(142, 22);
            crashToolStripMenuItem.Text = "Crash";
            crashToolStripMenuItem.Click += crashToolStripMenuItem_Click;
            // 
            // delayToolStripMenuItem
            // 
            delayToolStripMenuItem.Name = "delayToolStripMenuItem";
            delayToolStripMenuItem.Size = new Size(142, 22);
            delayToolStripMenuItem.Text = "Delay";
            // 
            // hookConfigToolStripMenuItem
            // 
            hookConfigToolStripMenuItem.Name = "hookConfigToolStripMenuItem";
            hookConfigToolStripMenuItem.Size = new Size(142, 22);
            hookConfigToolStripMenuItem.Text = "Hook Config";
            // 
            // btnKillAll
            // 
            btnKillAll.Location = new Point(753, 397);
            btnKillAll.Name = "btnKillAll";
            btnKillAll.Size = new Size(75, 23);
            btnKillAll.TabIndex = 13;
            btnKillAll.Text = "Kill All";
            btnKillAll.UseVisualStyleBackColor = true;
            btnKillAll.Click += btnKillAllClick;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(844, 462);
            Controls.Add(btnKillAll);
            Controls.Add(lblClientsLabel);
            Controls.Add(lblTMsLabel);
            Controls.Add(lblLMsLabel);
            Controls.Add(lblLMs);
            Controls.Add(lblTMs);
            Controls.Add(lsvwLMs);
            Controls.Add(lsvwTMs);
            Controls.Add(lblClients);
            Controls.Add(lsvwClients);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Main";
            Text = "Main";
            Load += Entry_Load;
            ctxLMs.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView lsvwClients;
        private Label lblClients;
        private ListView lsvwTMs;
        private ListView lsvwLMs;
        private Label lblTMs;
        private Label lblLMs;
        private OpenFileDialog fdConfig;
        private Label lblLMsLabel;
        private Label lblTMsLabel;
        private Label lblClientsLabel;
        private ContextMenuStrip ctxLMs;
        private ToolStripMenuItem crashToolStripMenuItem;
        private ToolStripMenuItem delayToolStripMenuItem;
        private ToolStripMenuItem hookConfigToolStripMenuItem;
        private Button Kill;
        private Button btnKillAll;
    }
}