using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HotelBackEndApp
{
    public class BrowserDownloader
    {
        private  string _downloadPath;
        private  IBrowser? _browser;
        private  IPlaywright? _playwright;

        private string _url = "https://sp.fuioupay.com/login";
        private string _detailsurl = "https://sp.fuioupay.com/count/commodity/sales/details/details";
        private string _filedownloadurl = "https://sp.fuioupay.com/download/doc";
        private string _user = "MX4351344";
        private string _password = "719720";
        private string _startdate = "";
        private string _enddate = "";
        public BrowserDownloader(string downloadPath,
            string startdate, string enddate,
            string url= "https://sp.fuioupay.com/login",
            string detailsurl= "https://sp.fuioupay.com/count/commodity/sales/details/details",
            string filedownloadurl = "https://sp.fuioupay.com/download/doc",
            string user= "MX4351344",
            string password= "719720")
        {
            _downloadPath = downloadPath;
            _startdate = startdate;
            _enddate = enddate;
            Directory.CreateDirectory(_downloadPath); // 确保目录存在
        }

        public async Task<string?> DownloadFileAsync()
        {
            // 创建 Playwright 实例
            _playwright ??= await Playwright.CreateAsync();
            _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Args = new[] { "--start-maximized" }  // 窗口最大化
            });

            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = ViewportSize.NoViewport,//new ViewportSize() { Width = 1820, Height = 1080 },
                                                       //Viewport = new ViewportSize { Width = 1920, Height = 1080 },
                Permissions = new[] { "geolocation" },
                Locale = "zh-CN",
                ExtraHTTPHeaders = new Dictionary<string, string>
                {
                    ["Accept-Language"] = "zh-CN,cn;q=0.9"
                },
                Geolocation = new Geolocation
                {
                    Longitude = 116.4f,
                    Latitude = 39.9024f,
                    Accuracy = 100,
                },
                RecordVideoDir = "videos/",
                RecordVideoSize = new RecordVideoSize() { Width = 1820, Height = 1080 },
            });
            var page = await context.NewPageAsync();
            var browserOptions = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Args = new List<string> { "--start-maximized" },
            };
            try
            {
                await using var browser = await _playwright.Chromium.LaunchAsync(browserOptions);
                var path = await page.Video.PathAsync();
                await page.GotoAsync(_url);
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "EAAP.jpg" });
                await page
                    .GetByPlaceholder("请输入登录帐号")
                    .FillAsync(_user);
                Task.Delay(2000).Wait();
                await page.GetByPlaceholder("请输入登录密码").FillAsync(_password);
                Task.Delay(2000).Wait();
                await page.ClickAsync("text=登 录");
                Task.Delay(1000).Wait();
                await page.GotoAsync(_detailsurl);
                await page.WaitForSelectorAsync("input[placeholder='开始日期']");
                await page.EvaluateAsync(@"() => {
            const input = document.querySelector('input[placeholder=""开始日期""].el-range-input');
            if (input) input.removeAttribute('readonly');
            
        }");
                await page.GetByPlaceholder("开始日期").FillAsync(_startdate);
                await page.EvaluateAsync(@"() => {
           const input = document.querySelector('input[placeholder=""结束日期""].el-range-input');
if (input) input.removeAttribute('readonly');
            
        }"
                );

                await page.GetByPlaceholder("结束日期").FillAsync(_enddate);
                Task.Delay(1000).Wait();
                await page.GetByRole(AriaRole.Button, new() { Name = "导出excel" }).ClickAsync();
                var successLocator = page.Locator("xpath=//div[text()='导出任务记录成功!']");

                // 等待元素出现（最多等待 10 秒）
                await successLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    Timeout = 5000 // 10 秒超时
                });

                // await page.ScreenshotAsync(new PageScreenshotOptions { Path = "EAAP2.jpg" });
                await page.GotoAsync(_filedownloadurl);

                Task.Delay(1000).Wait();
                //var buttons = await page.QuerySelectorAll(".el-table__fixed-body-wrapper > .el-table__body > tbody > tr > .el-table_1_column_10 > .cell > div > button");
                //await buttons[0].ClickAsync(); //  

                // Start the task of waiting for the download before clicking
                var waitForDownloadTask = page.WaitForDownloadAsync();
                await page.Locator(".el-table__fixed-body-wrapper > .el-table__body > tbody > tr > .el-table_1_column_10 > .cell > div > button").First.ClickAsync();
                var download = await waitForDownloadTask;
                // Wait for the download process to complete and save the downloaded file somewhere

                string tmpfile;
                tmpfile = download.SuggestedFilename;
                await download.SaveAsAsync("./" + download.SuggestedFilename);
                string filePath = Path.Combine(_downloadPath, download.SuggestedFilename);
                return filePath;
                // await page.ScreenshotAsync(new PageScreenshotOptions { Path = "EAAP3.jpg" });
            }
            finally
            {
                await _browser.CloseAsync(); 
            }
        }

        public async Task CloseBrowserAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
            _playwright?.Dispose();
        }
    }

}
