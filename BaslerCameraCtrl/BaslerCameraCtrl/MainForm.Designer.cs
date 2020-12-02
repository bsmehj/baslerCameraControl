namespace BaslerCameraCtrl
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.panel_ctrl = new System.Windows.Forms.Panel();
            this.button_pause_grab = new System.Windows.Forms.Button();
            this.button_continue_grab = new System.Windows.Forms.Button();
            this.button_single_grab = new System.Windows.Forms.Button();
            this.button_close_camera = new System.Windows.Forms.Button();
            this.button_open_camera = new System.Windows.Forms.Button();
            this.pictureBox_basler = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel_main.SuspendLayout();
            this.panel_ctrl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_basler)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.AutoSize = true;
            this.tableLayoutPanel_main.ColumnCount = 2;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.91304F));
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.08696F));
            this.tableLayoutPanel_main.Controls.Add(this.panel_ctrl, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.pictureBox_basler, 0, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 1;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(818, 504);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // panel_ctrl
            // 
            this.panel_ctrl.Controls.Add(this.button_pause_grab);
            this.panel_ctrl.Controls.Add(this.button_continue_grab);
            this.panel_ctrl.Controls.Add(this.button_single_grab);
            this.panel_ctrl.Controls.Add(this.button_close_camera);
            this.panel_ctrl.Controls.Add(this.button_open_camera);
            this.panel_ctrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_ctrl.Location = new System.Drawing.Point(605, 1);
            this.panel_ctrl.Margin = new System.Windows.Forms.Padding(1);
            this.panel_ctrl.Name = "panel_ctrl";
            this.panel_ctrl.Size = new System.Drawing.Size(212, 502);
            this.panel_ctrl.TabIndex = 0;
            // 
            // button_pause_grab
            // 
            this.button_pause_grab.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_pause_grab.Location = new System.Drawing.Point(33, 282);
            this.button_pause_grab.Name = "button_pause_grab";
            this.button_pause_grab.Size = new System.Drawing.Size(99, 35);
            this.button_pause_grab.TabIndex = 0;
            this.button_pause_grab.Text = "暂停拍摄";
            this.button_pause_grab.UseVisualStyleBackColor = true;
            this.button_pause_grab.Click += new System.EventHandler(this.button_pause_grab_Click);
            // 
            // button_continue_grab
            // 
            this.button_continue_grab.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_continue_grab.Location = new System.Drawing.Point(33, 222);
            this.button_continue_grab.Name = "button_continue_grab";
            this.button_continue_grab.Size = new System.Drawing.Size(99, 35);
            this.button_continue_grab.TabIndex = 0;
            this.button_continue_grab.Text = "连续拍摄";
            this.button_continue_grab.UseVisualStyleBackColor = true;
            this.button_continue_grab.Click += new System.EventHandler(this.button_continue_grab_Click);
            // 
            // button_single_grab
            // 
            this.button_single_grab.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_single_grab.Location = new System.Drawing.Point(33, 163);
            this.button_single_grab.Name = "button_single_grab";
            this.button_single_grab.Size = new System.Drawing.Size(99, 35);
            this.button_single_grab.TabIndex = 0;
            this.button_single_grab.Text = "单张拍摄";
            this.button_single_grab.UseVisualStyleBackColor = true;
            this.button_single_grab.Click += new System.EventHandler(this.button_single_grab_Click);
            // 
            // button_close_camera
            // 
            this.button_close_camera.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_close_camera.Location = new System.Drawing.Point(33, 103);
            this.button_close_camera.Name = "button_close_camera";
            this.button_close_camera.Size = new System.Drawing.Size(99, 35);
            this.button_close_camera.TabIndex = 0;
            this.button_close_camera.Text = "关闭相机";
            this.button_close_camera.UseVisualStyleBackColor = true;
            this.button_close_camera.Click += new System.EventHandler(this.button_close_camera_Click);
            // 
            // button_open_camera
            // 
            this.button_open_camera.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_open_camera.Location = new System.Drawing.Point(33, 36);
            this.button_open_camera.Name = "button_open_camera";
            this.button_open_camera.Size = new System.Drawing.Size(99, 35);
            this.button_open_camera.TabIndex = 0;
            this.button_open_camera.Text = "打开相机";
            this.button_open_camera.UseVisualStyleBackColor = true;
            this.button_open_camera.Click += new System.EventHandler(this.button_open_camera_Click);
            // 
            // pictureBox_basler
            // 
            this.pictureBox_basler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_basler.Location = new System.Drawing.Point(1, 1);
            this.pictureBox_basler.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox_basler.Name = "pictureBox_basler";
            this.pictureBox_basler.Size = new System.Drawing.Size(602, 502);
            this.pictureBox_basler.TabIndex = 1;
            this.pictureBox_basler.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 504);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Basler工业相机";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.panel_ctrl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_basler)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Panel panel_ctrl;
        private System.Windows.Forms.PictureBox pictureBox_basler;
        private System.Windows.Forms.Button button_open_camera;
        private System.Windows.Forms.Button button_close_camera;
        private System.Windows.Forms.Button button_pause_grab;
        private System.Windows.Forms.Button button_continue_grab;
        private System.Windows.Forms.Button button_single_grab;
    }
}

