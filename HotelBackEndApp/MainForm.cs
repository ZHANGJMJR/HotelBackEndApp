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

namespace HotelBackEndApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void exit_btn_Click(object sender, EventArgs e)
        {
            //this.Dispose();
            Application.Exit();
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            //开始执行schedule
            QuartzScheduler.Start(".",Dlt.Dlt.getMysqlConnectStr(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));
            if (QuartzScheduler.IsSchedulerRunning())
            {
                stop_btn.Enabled = true;
                start_btn.Enabled = false;
            }
            else
            {
                stop_btn.Enabled = false;
                start_btn.Enabled = true;
            }
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            // 停止schedule 
            QuartzScheduler.StopScheduler();
            if (QuartzScheduler.IsSchedulerRunning())
            {
                stop_btn.Enabled = true;
                start_btn.Enabled = false;
            }
            else
            {
                stop_btn.Enabled = false;
                start_btn.Enabled = true;
            }
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
                stop_btn.Enabled = true;
                start_btn.Enabled = false;
            }
            else
            {
                stop_btn.Enabled = false;
                start_btn.Enabled = true;
            }
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
