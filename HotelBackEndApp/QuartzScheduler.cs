using HotelBackEndApp;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


[DisallowConcurrentExecution]
public class SyncJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        LogHelper.Info($"🔹 scheduler任务开始执行：{DateTime.Now}");

        // 1️⃣ 读取外部传入的参数
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        string csvFilePath = dataMap.GetString("CsvFilePath");
        string mysqlConnectionString = dataMap.GetString("MySQLConnectionString");
        DateTime startDate = dataMap.GetDateTime("startDate");
        DateTime endDate = dataMap.GetDateTime("endDate");
        startDate = DateTime.Now.AddDays(-1);
        endDate = DateTime.Now.AddDays(-1);
        // 2️⃣ 调用数据同步方法 
        Dlt.Dlt dlt = new Dlt.Dlt(); 
        //dlt.SyncData();  // 执行 Oracle 到 MySQL 的同步
        static IEnumerable<DateTime> GetDateRange(DateTime start, DateTime end)
        {
            return Enumerable.Range(0, (end - start).Days + 1)
                             .Select(offset => start.AddDays(offset));
        }
        /// 实现多日导入数据
        foreach (var date in GetDateRange(startDate, endDate))
        {
            // Console.WriteLine(date.ToString("yyyy-MM-dd"));
            dlt.SyncData(date.ToString("yyyy-MM-dd"));
        }
        // 需要添加，自动下载数据
        csvFilePath = await new BrowserDownloader(".", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd")).DownloadFileAsync();

        dlt.ImportCsvToMySQL(csvFilePath, mysqlConnectionString); // 执行 CSV 导入

        LogHelper.Info($"✅ scheduler任务执行完成：{DateTime.Now}");
        //return await Task.CompletedTask;
    }
}

public class QuartzScheduler
{
    private static IScheduler? _scheduler;  // 全局持久化 Scheduler 实例

    public static async Task Start(string csvFilePath,
         string mysqlConnectionString, DateTime startDate, DateTime endDate,
         string argcronExpression = "0 0 1 * * ? *")
    {
        if ( _scheduler != null &&
             !_scheduler.IsShutdown)
        {
            LogHelper.Info("⚠ Scheduler 已经在运行中 ,不得重复运行！");
            return;
        }
        // 1️⃣ 创建 Quartz 调度器工厂
        
        StdSchedulerFactory factory = new StdSchedulerFactory();
          _scheduler = await factory.GetScheduler();
        LogHelper.Info("⏳ scheduler 定时任务准备启动...");
        // 2️⃣ 启动调度器
        await _scheduler.Start();

        // 3️⃣ 定义 Job，并传递外部参数
        IJobDetail job = JobBuilder.Create<SyncJob>()
            .WithIdentity("SyncJob", "Group1")
            .UsingJobData("CsvFilePath", csvFilePath)  // 传递 CSV 文件路径
            .UsingJobData("MySQLConnectionString", mysqlConnectionString)
            .UsingJobData("startDate", startDate.ToShortDateString())
            .UsingJobData("endDate", endDate.ToShortDateString())  // 传递 MySQL 连接字符串
            .Build();
        CrontabService crontabService = new CrontabService(mysqlConnectionString);
        string cronExpression = crontabService.GetCronExpression(1) ?? argcronExpression; // 默认30分钟执行

        // 4️⃣ 创建 Cron 触发器（每天凌晨 1:00 执行）
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("SyncTrigger", "Group1")
            .WithCronSchedule(cronExpression)  // CRON 表达式：每天凌晨 1:00 执行
            .Build();

        // 5️⃣ 将任务和触发器加入调度器
        await _scheduler.ScheduleJob(job, trigger);
        LogHelper.Info("⏳ scheduler 定时任务已启动...");
    }
    public static async Task StopScheduler()
    {
        if ( _scheduler == null || _scheduler.IsShutdown)
        {
            LogHelper.Info("⚠ Scheduler 未启动，无法停止！");
            return;
        }

        await _scheduler.Shutdown();
        LogHelper.Info("❌ Scheduler 已停止！");
    }

    public static bool IsSchedulerRunning()
    {
        return _scheduler != null && !_scheduler.IsShutdown;
    }
}
