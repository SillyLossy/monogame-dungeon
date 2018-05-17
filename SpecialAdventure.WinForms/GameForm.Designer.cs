using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Win32;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.World.Tiles;
using Point = SpecialAdventure.Core.Common.Point;
using Rectangle = System.Drawing.Rectangle;

namespace SpecialAdventure.WinForms
{
    partial class GameForm
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
            this.gamePanel = new SpecialAdventure.WinForms.ViewportPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.inventoryButton = new System.Windows.Forms.Button();
            this.characterButton = new System.Windows.Forms.Button();
            this.gameLogList = new System.Windows.Forms.ListView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gamePanel
            // 
            this.gamePanel.AutoSize = true;
            this.gamePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.gamePanel, 2);
            this.gamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamePanel.Location = new System.Drawing.Point(0, 0);
            this.gamePanel.Margin = new System.Windows.Forms.Padding(0);
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(707, 333);
            this.gamePanel.Sprites = null;
            this.gamePanel.State = null;
            this.gamePanel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.gamePanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.gameLogList, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(707, 436);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.inventoryButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.characterButton, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(620, 336);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(84, 97);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // inventoryButton
            // 
            this.inventoryButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.inventoryButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventoryButton.Location = new System.Drawing.Point(3, 3);
            this.inventoryButton.Name = "inventoryButton";
            this.inventoryButton.Size = new System.Drawing.Size(78, 42);
            this.inventoryButton.TabIndex = 0;
            this.inventoryButton.Text = "Inventory";
            this.inventoryButton.UseVisualStyleBackColor = false;
            this.inventoryButton.Click += new System.EventHandler(this.InventoryButton_Click);
            // 
            // characterButton
            // 
            this.characterButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.characterButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.characterButton.Location = new System.Drawing.Point(3, 51);
            this.characterButton.Name = "characterButton";
            this.characterButton.Size = new System.Drawing.Size(78, 43);
            this.characterButton.TabIndex = 1;
            this.characterButton.Text = "Character";
            this.characterButton.UseVisualStyleBackColor = false;
            this.characterButton.Click += new System.EventHandler(this.CharacterButton_Click);
            // 
            // gameLogList
            // 
            this.gameLogList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gameLogList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameLogList.Location = new System.Drawing.Point(3, 336);
            this.gameLogList.MultiSelect = false;
            this.gameLogList.Name = "gameLogList";
            this.gameLogList.Size = new System.Drawing.Size(611, 97);
            this.gameLogList.TabIndex = 2;
            this.gameLogList.UseCompatibleStateImageBehavior = false;
            this.gameLogList.View = System.Windows.Forms.View.List;
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(707, 436);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GameForm";
            this.Text = "GameForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ViewportPanel gamePanel;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button inventoryButton;
        private Button characterButton;
        private ListView gameLogList;
    }
}