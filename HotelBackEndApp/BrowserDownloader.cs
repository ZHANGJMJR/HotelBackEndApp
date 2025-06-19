using AntdUI;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace HotelBackEndApp
{
    public class BrowserDownloader
    {
        private string _downloadPath;
        private IBrowser? _browser;
        private IPlaywright? _playwright;
        private IBrowserContext? _context;
        private string _url;
        private string _detailsurl;
        private string _orderlisturl;
        private string _filedownloadurl;
        private string _user;
        private string _password;
        private string _startdate;
        private string _enddate;

        public BrowserDownloader(string downloadPath,
            string startdate, string enddate,
            string url = "https://sp.fuioupay.com/login",
            string detailsurl = "https://sp.fuioupay.com/count/commodity/sales/details/details",
            string orderlisturl = "https://sp.fuioupay.com/business/order/list",
            string filedownloadurl = "https://sp.fuioupay.com/download/doc",
            string user = "MX4351344",
            string password = "719720")
        {
            _downloadPath = downloadPath;
            _startdate = startdate;
            _enddate = enddate;
            _url = url;
            _detailsurl = detailsurl;
            _orderlisturl = orderlisturl;
            _filedownloadurl = filedownloadurl;
            _user = user;
            _password = password;
            Directory.CreateDirectory(_downloadPath); // 确保目录存在
        }

        public async Task<Dictionary<string, string>?> DownloadFileAsync()
        {
            // 创建 Playwright 实例
            _playwright ??= await Playwright.CreateAsync();

            _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,  // 让浏览器显示出来
                SlowMo = 2300, // 放慢操作，避免页面加载过快
                
                Args = new[] { "--start-maximized" ,"--remote-allow-origins=*",
        "--disable-gpu",
        "--no-first-run", "--no-sandbox"}, // 最大化窗口
            });

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = ViewportSize.NoViewport, // 让窗口使用操作系统默认大小（接近最大化）
                JavaScriptEnabled = true, // 确保 JS 是启用的
                Locale = "zh-CN",
                ExtraHTTPHeaders = new Dictionary<string, string>
                {
                    ["Accept-Language"] = "zh-CN,cn;q=0.9"
                },
                UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1",
                StorageStatePath ="./cookie.json",
                RecordVideoDir = Path.Combine(Directory.GetCurrentDirectory(), "videos"), // 指定到更快的 SSD 目录
                RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 } // 提高视频分辨率
            });

            var page = await _context.NewPageAsync();
            var returnMapData_filepath = new Dictionary<string, string>();
            int intervalday = 1;
            if (DateTime.TryParseExact(_startdate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date1) &&
            DateTime.TryParseExact(_enddate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date2))
            {
                TimeSpan timeSpan = date2 - date1;
                intervalday = timeSpan.Days; 
            } 
        

            try
            {
                // 让窗口最大化
                await page.EvaluateAsync("window.moveTo(0, 0); window.resizeTo(screen.width, screen.height);");

                await page.GotoAsync(_url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await page.WaitForTimeoutAsync(2000);

                // 登录
                await page.GetByPlaceholder("请输入登录帐号").FillAsync(_user);
                await page.WaitForTimeoutAsync(2000);
                await page.GetByPlaceholder("请输入登录密码").FillAsync(_password);
                await page.WaitForTimeoutAsync(2100);
                await page.ClickAsync("text=登 录");

                //************************************************************************
                await page.WaitForTimeoutAsync(3200);
                // 到商品销售明细   ----- start 
                await page.GotoAsync(_detailsurl);
                await page.WaitForSelectorAsync("input[placeholder='开始日期']");
                // 解除 readonly 限制
                await page.EvaluateAsync("document.querySelectorAll('input[placeholder=\"开始日期\"], input[placeholder=\"结束日期\"]').forEach(el => el.removeAttribute('readonly'))");

                // 选择日期
                await page.GetByPlaceholder("开始日期").Nth(0).FillAsync(_startdate + " 00:00:00");
                await page.GetByPlaceholder("结束日期").Nth(0).FillAsync(_enddate + " 23:59:59");

                await page.WaitForTimeoutAsync(1000);
                //await page.GetByRole(AriaRole.Button, new() { Name = "查询" }).ClickAsync();
                //await page.WaitForTimeoutAsync(1000);
                await page.GetByRole(AriaRole.Button, new() { Name = "导出excel" }).ClickAsync();

                var successLocator = page.Locator("xpath=//div[text()='导出任务记录成功!']");
                await successLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

                // 导航到下载页面
                await page.WaitForTimeoutAsync( 1500 * intervalday);
                await page.GotoAsync(_filedownloadurl);
                await page.WaitForTimeoutAsync(1000);

                // 等待下载
                var waitForDownloadTask = page.WaitForDownloadAsync();
                await page.Locator(".el-table__fixed-body-wrapper > .el-table__body > tbody > tr > .el-table_1_column_10 > .cell > div > button").First.ClickAsync();
                var download = await waitForDownloadTask;

                // 保存下载的文件
                string filePath = Path.Combine(_downloadPath, download.SuggestedFilename);
                await download.SaveAsAsync(filePath);
                // 到商品销售明细   ----- end 
                returnMapData_filepath.Add("details", filePath);

                //************************************************************************

                await page.WaitForTimeoutAsync(3200);
                // 到订单列表   ----- start 
                await page.GotoAsync(_orderlisturl);
                await page.WaitForSelectorAsync("input[placeholder='开始日期']");
                // 解除 readonly 限制
                await page.EvaluateAsync("document.querySelectorAll('input[placeholder=\"开始日期\"], input[placeholder=\"结束日期\"]').forEach(el => el.removeAttribute('readonly'))");

                // 选择日期
                await page.GetByPlaceholder("开始日期").Nth(0).FillAsync(_startdate + " 00:00:00");
                await page.GetByPlaceholder("结束日期").Nth(0).FillAsync(_enddate + " 23:59:59");

                await page.WaitForTimeoutAsync(1000);
                //await page.GetByRole(AriaRole.Button, new() { Name = "查询" }).ClickAsync();
                //await page.WaitForTimeoutAsync(1000);

                await page.GetByRole(AriaRole.Button, new() { Name = "导出excel" }).ClickAsync();
                await page.WaitForTimeoutAsync(1000);
                await page.GetByRole(AriaRole.Button, new() { Name = "确 定" }).ClickAsync();
                var successLocator2 = page.Locator("xpath=//div[text()='导出任务记录成功!']");
                await successLocator2.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

                // 导航到下载页面
                await page.WaitForTimeoutAsync(1500 * intervalday);
                await page.GotoAsync(_filedownloadurl);
                await page.WaitForTimeoutAsync(1000);

                // 等待下载
                var waitForDownloadTask2 = page.WaitForDownloadAsync();
                await page.Locator(".el-table__fixed-body-wrapper > .el-table__body > tbody > tr > .el-table_1_column_10 > .cell > div > button").First.ClickAsync();
                var download2 = await waitForDownloadTask2;

                // 保存下载的文件
                string orderlistfilePath = Path.Combine(_downloadPath, download2.SuggestedFilename);
                await download2.SaveAsAsync(orderlistfilePath);
                await page.WaitForTimeoutAsync(3000);

                returnMapData_filepath.Add("orderlistzip", orderlistfilePath);
                // 到订单列表   ----- end 
                string getzipfile;
                if (IsZipFileEnhanced(orderlistfilePath))
                {
                    getzipfile = unzipFile(orderlistfilePath);
                    returnMapData_filepath.Add("orderlistunzip", getzipfile??"");
                }
                



                //return returnMapData_filepath;
                //return filePath;
            }
            catch (Exception ex) { 
                Console.WriteLine(ex);
                //return returnMapData_filepath;
            }
            finally
            {
                await _browser.CloseAsync();
                //return returnMapData_filepath;
            }
            return returnMapData_filepath;

        }
        public   bool IsZipByExtension(string filePath)
        {
            return string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase);
        }
       public bool IsZipFile(string filePath)
        {
            const int signatureLength = 4;
            byte[] signature = new byte[signatureLength];

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                if (fileStream.Length < signatureLength)
                    return false;

                fileStream.Read(signature, 0, signatureLength);
            }

            return signature[0] == 0x50 &&
                   signature[1] == 0x4B &&
                   signature[2] == 0x03 &&
                   signature[3] == 0x04;
        }
        public bool IsZipFileEnhanced(string filePath)
        {
            if (!IsZipByExtension(filePath))
                return false;

            return IsZipFile(filePath);
        }

        //public async Task<string?> DownloadFileAsync()
        //{
        //    // 创建 Playwright 实例
        //    _playwright ??= await Playwright.CreateAsync();

        //    _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //    {
        //        Headless = false,  // 让浏览器显示出来
        //        SlowMo = 2300, // 放慢操作，避免页面加载过快

        //        Args = new[] { "--start-maximized" ,"--remote-allow-origins=*",
        //"--disable-gpu",
        //"--no-first-run", "--no-sandbox"}, // 最大化窗口
        //    });

        //    _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        //    {
        //        ViewportSize = ViewportSize.NoViewport, // 让窗口使用操作系统默认大小（接近最大化）
        //        JavaScriptEnabled = true, // 确保 JS 是启用的
        //        Locale = "zh-CN",
        //        ExtraHTTPHeaders = new Dictionary<string, string>
        //        {
        //            ["Accept-Language"] = "zh-CN,cn;q=0.9"
        //        },
        //        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1",
        //        StorageStatePath = "./cookie.json",
        //        RecordVideoDir = Path.Combine(Directory.GetCurrentDirectory(), "videos"), // 指定到更快的 SSD 目录
        //        RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 } // 提高视频分辨率
        //    });

        //    var page = await _context.NewPageAsync();
        //    try
        //    {
        //        // 让窗口最大化
        //        await page.EvaluateAsync("window.moveTo(0, 0); window.resizeTo(screen.width, screen.height);");

        //        await page.GotoAsync(_url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        //        await page.WaitForTimeoutAsync(2000);

        //        // 登录
        //        await page.GetByPlaceholder("请输入登录帐号").FillAsync(_user);
        //        await page.WaitForTimeoutAsync(2000);
        //        await page.GetByPlaceholder("请输入登录密码").FillAsync(_password);
        //        await page.WaitForTimeoutAsync(2100);
        //        await page.ClickAsync("text=登 录");

        //        await page.WaitForTimeoutAsync(3200);
        //        // 到商品销售明细   ----- start 
        //        await page.GotoAsync(_detailsurl);
        //        await page.WaitForSelectorAsync("input[placeholder='开始日期']");
        //        // 解除 readonly 限制
        //        await page.EvaluateAsync("document.querySelectorAll('input[placeholder=\"开始日期\"], input[placeholder=\"结束日期\"]').forEach(el => el.removeAttribute('readonly'))");

        //        // 选择日期
        //        await page.GetByPlaceholder("开始日期").FillAsync(_startdate);
        //        await page.GetByPlaceholder("结束日期").FillAsync(_enddate);

        //        await page.WaitForTimeoutAsync(1000);
        //        await page.GetByRole(AriaRole.Button, new() { Name = "导出excel" }).ClickAsync();

        //        var successLocator = page.Locator("xpath=//div[text()='导出任务记录成功!']");
        //        await successLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        //        // 导航到下载页面
        //        await page.GotoAsync(_filedownloadurl);
        //        await page.WaitForTimeoutAsync(1000);

        //        // 等待下载
        //        var waitForDownloadTask = page.WaitForDownloadAsync();
        //        await page.Locator(".el-table__fixed-body-wrapper > .el-table__body > tbody > tr > .el-table_1_column_10 > .cell > div > button").First.ClickAsync();
        //        var download = await waitForDownloadTask;

        //        // 保存下载的文件
        //        string filePath = Path.Combine(_downloadPath, download.SuggestedFilename);
        //        await download.SaveAsAsync(filePath);
        //        // 到商品销售明细   ----- end 


        //        return filePath;
        //    }
        //    finally
        //    {
        //        await _browser.CloseAsync();
        //    }
        //}


        public async Task CloseBrowserAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
            _playwright?.Dispose();
        }

        public string unzipFile(string zipfile)
        {
            string filename="";
                try
                {
                // ZIP文件名
                //string zipFileName = "订单列表2025-06-15+00_00_10-2025-06-15+23_59_59+faff8e0a90134844bfb18c90d9fc58481031206.zip";
                string zipFileName = zipfile;
                // 获取当前目录
                string currentDirectory = Directory.GetCurrentDirectory();

                // 创建以ZIP文件名(不含扩展名)命名的目录
                string extractDirectory = Path.Combine(currentDirectory, Path.GetFileNameWithoutExtension(zipFileName));
                Directory.CreateDirectory(extractDirectory);

                // 解压ZIP文件到指定目录
                Console.WriteLine($"开始解压文件: {zipFileName} 到目录: {extractDirectory}");
                using (ZipArchive archive = ZipFile.OpenRead(Path.Combine(currentDirectory, zipFileName)))
                {
                    archive.ExtractToDirectory(extractDirectory, true);
                    LogHelper.Info($"✅ 解压完成：{DateTime.Now}");
                    //Console.WriteLine("解压完成");
                }

                // 搜索文件名开头为"订单详情总表"的文件
                //Console.WriteLine("\n开始搜索符合条件的文件...");
                LogHelper.Info($"✅ 开始搜索符合条件的文件：{DateTime.Now}");
                string[] matchingFiles = Directory.GetFiles(extractDirectory)
                    .Where(file => Path.GetFileName(file).StartsWith("A订单详情总表", StringComparison.Ordinal))
                    .ToArray();

                // 输出搜索结果
                if (matchingFiles.Length > 0)
                {
                    //Console.WriteLine($"找到 {matchingFiles.Length} 个符合条件的文件:");
                    LogHelper.Info($"✅找到 {matchingFiles.Length} 个符合条件的文件:");
                    foreach (string file in matchingFiles)
                    {
                        filename = file;
                        Console.WriteLine(Path.GetFileName(file));
                    }
                    filename = Path.GetRelativePath(currentDirectory, filename);
                }
                else
                {
                    LogHelper.Info($"未找到符合条件的文件");
                    //Console.WriteLine("未找到符合条件的文件");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Info($"发生错误: {ex.Message}");
                //Console.WriteLine($"发生错误: {ex.Message}");
            }
            
            return filename;
        }


    }
}
