using AntdUI;
using Dlt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;

namespace HotelBackEndApp
{
    public partial class MainForm : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem startItem;
        private ToolStripMenuItem stopItem;
        private ToolStripMenuItem executeItem;
        private ToolStripMenuItem exitItem;

        public MainForm()
        {
            InitializeComponent();
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            // 创建托盘菜单
            trayMenu = new ContextMenuStrip();
            startItem= new ToolStripMenuItem("Start", null, start_btn_Click);
            stopItem = new ToolStripMenuItem("Stop", null, stop_btn_Click);
            executeItem = new ToolStripMenuItem("Execute", null, exe_btn_Click);
            exitItem = new ToolStripMenuItem("Exit", null, OnExitClick);


            trayMenu.Items.Add(startItem);
            trayMenu.Items.Add("-", null);
            trayMenu.Items.Add(stopItem);
            trayMenu.Items.Add("-", null);
            trayMenu.Items.Add(executeItem);
            trayMenu.Items.Add("-", null);
            trayMenu.Items.Add(exitItem);

            // 创建托盘图标
            trayIcon = new NotifyIcon
            {
                Icon = new Icon("logo.ico"),// 使用默认应用图标
                ContextMenuStrip = trayMenu,
                Visible = true,
                Text = "Hotel Backend App"
            };

            // 双击托盘图标，恢复窗口
            trayIcon.DoubleClick += (sender, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };
        }


        private void exit_btn_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Application.Exit();
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            //开始执行schedule
            QuartzScheduler.Start(".", Dlt.Dlt.getMysqlConnectStr(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));
            if (QuartzScheduler.IsSchedulerRunning())
            {
                stop_btn.Enabled = true;stopItem.Enabled = true;
                start_btn.Enabled = false;startItem.Enabled = false;
            }
            else
            {
                stop_btn.Enabled = false;stopItem.Enabled = false;
                start_btn.Enabled = true;startItem.Enabled = true;
            }
            //trayMenu.Refresh();
            trayMenu.Invalidate(); // 🔹 强制重绘菜单
            trayMenu.Update();
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            // 停止schedule 
            QuartzScheduler.StopScheduler();
            if (QuartzScheduler.IsSchedulerRunning())
            {
                stop_btn.Enabled = true; stopItem.Enabled = true;
                start_btn.Enabled = false;startItem.Enabled = false;
            }
            else
            {
                stop_btn.Enabled = false; stopItem.Enabled = false;
                start_btn.Enabled = true; startItem.Enabled = true;
            }
            //trayMenu.Refresh();
            trayMenu.Invalidate(); // 🔹 强制重绘菜单
            trayMenu.Update();
        }

        private async void exe_btn_Click(object sender, EventArgs e)
        {

            // 立即执行
            DateTime startDate = datePickerRange1.Value[0];
            DateTime endDate = datePickerRange1.Value[1];
            Dlt.Dlt dlt = new Dlt.Dlt();

            if (dlt.CheckExist(startDate, endDate) > 0)
            {
                if (MessageBox.Show(@$"检测到 {startDate.ToString("yyyy-MM-dd")} 至 {endDate.ToString("yyyy-MM-dd")} 数据已存在，是否覆盖？", "数据重复",
                      MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

            }

            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            toolStripProgressBar1.MarqueeAnimationSpeed = 50;
            this.exe_btn.Enabled = false;
            LogHelper.Info("🚀立即 启动...");
            //Console.WriteLine("🚀 启动...");

            foreach (var date in GetDateRange(startDate, endDate))
            {
                dlt.SyncData(date.ToString("yyyy-MM-dd"));
            }
            static IEnumerable<DateTime> GetDateRange(DateTime start, DateTime end)
            {
                return Enumerable.Range(0, (end - start).Days + 1)
                                 .Select(offset => start.AddDays(offset));
            }

            string csvFilePath = await new BrowserDownloader(".", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd")).DownloadFileAsync();

            dlt.ImportCsvToMySQL(csvFilePath, Dlt.Dlt.getMysqlConnectStr()); // 执行 CSV 导入
            this.exe_btn.Enabled = true;
            LogHelper.Info("🚀立即 结束...");
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
        }
        public DateTime[] initDate()
        {
            return new DateTime[2] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1) };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DateTime[] dt_tmp = initDate();
            datePickerRange1.Value = dt_tmp;
            if (QuartzScheduler.IsSchedulerRunning())
            {
                stop_btn.Enabled = true;stopItem.Enabled = true;
                start_btn.Enabled = false;startItem.Enabled = false;
            }
            else
            {
                stop_btn.Enabled = false;stopItem.Enabled = false;
                start_btn.Enabled = true;startItem.Enabled = true;
            }
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;  // 取消关闭
            this.Hide();       // 隐藏窗口
        }

    
        private void OnExitClick(object sender, EventArgs e)
        {
            trayIcon.Visible = false;  // 退出前隐藏托盘图标
            this.Dispose();
            Application.Exit();
        }
    }
}
