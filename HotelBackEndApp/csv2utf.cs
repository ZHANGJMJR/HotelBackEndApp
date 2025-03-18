using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CsvConverter
{ // 调用方法转换编码，并删除最后一行
    public static void ConvertCsvEncoding(string inputFilePath, string outputFilePath, bool removeLastLine = false)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding gb2312 = Encoding.GetEncoding("GB2312");
        Encoding utf8 = Encoding.UTF8;

        List<string> lines = new List<string>();

        // 读取GB2312编码的CSV文件
        using (StreamReader reader = new StreamReader(inputFilePath, gb2312))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }

        // 如果需要，移除最后一行
        if (removeLastLine && lines.Count > 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        // 将UTF-8编码的CSV内容写入新文件
        using (StreamWriter writer = new StreamWriter(outputFilePath, false, utf8))
        {
            foreach (string line in lines)
            {
                writer.WriteLine(line);
            }
        }

        //Console.WriteLine("CSV文件编码转换完成。");
        if (removeLastLine)
        {
           // Console.WriteLine("同时删除了转换后的CSV文件的最后一行。");
        }
    }

}