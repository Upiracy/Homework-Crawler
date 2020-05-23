using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleCrawler
{
    class SimpleCrawler
    {
        private Hashtable urls = new Hashtable();
        private int count = 1;
        private int max = 10;
        private string regex = @"(href|HREF)[]*=[]*[""'][^""'#>]+[""']";
        private string http = "";
        private string baseAddr = "";
        private bool local = true;
        private int success = 0;
        private int id = 0;
        //(href|HREF)[]*=[]*[""'][^""'#>]+.*html[""']
        //(href|HREF)[]*=[]*[""'][^""'#>]+.*www\.cnblogs\.com.*[""']
        /*
https://indienova.com/channel/news
.*(news/guide|news/page).*
50
        */

        static void Main(string[] args)
        {
            SimpleCrawler myCrawler = new SimpleCrawler();
            Console.Write("Start Url:\t");
            string startUrl;
            startUrl = Console.ReadLine();
            if (startUrl == "") { startUrl = "http://www.cnblogs.com/dstang2000/"; Console.WriteLine("  Default Url = http://www.cnblogs.com/dstang2000/"); }
            myCrawler.baseAddr = new Regex("://.*?/").Match(startUrl).Value.Trim(':', '/');
            myCrawler.http = startUrl.StartsWith("https://") ? "https://" : "http://" ;
            Console.Write("Regex:\t\t");
            myCrawler.regex = Console.ReadLine();
            if (myCrawler.regex == "") { myCrawler.regex = @".*";  Console.WriteLine(@"  Default Regex = (href|HREF)[]*=[]*[""'][^""'#>]+.*[""']"); }
            if (args.Length >= 1) startUrl = args[0];
            myCrawler.urls.Add(startUrl, false);//加入初始页面
            Console.Write("Craw Limits:\t");
            string m = Console.ReadLine();
            if (m == "") { myCrawler.max = 10; Console.WriteLine("  Default Limits = 10"); }
            else myCrawler.max = int.Parse(m);
            new Thread(myCrawler.Crawl).Start(); 
            new Thread(myCrawler.Crawl).Start();
        }

        private void Crawl()
        {
            int m_id = id;
            int tolerant = 0;
            id++;
            Console.WriteLine("Start Crawing.... ");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("------------------------------------------------------------------");
            while (true)
            {
                string current = null;
                foreach (string url in urls.Keys)
                {
                    if ((bool)urls[url]) continue;
                    current = url;
                    break;
                }

                if (current == null)
                {
                    tolerant++;
                    if (tolerant >= 10) break;
                }
                if (count >= max) break;
                Console.Write("Routine"+ m_id +"-ID_" + count + ".Crawing Page: " + current + "...");
                urls[current] = true;
                string html = DownLoad(current); // 下载
                count++;
                Parse(html);//解析,并加入新的链接
                Console.WriteLine("Routine" + m_id + " Crawing Finished.");
                Console.WriteLine();
            }
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("Program Finished, Tried:{0}, Succeeded:{1}, Success Rate:{2}%", count - 1, success, (float)success / (count - 1) * 100);
        }

        public string DownLoad(string url)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                string html = webClient.DownloadString(url);
                string fileName = count.ToString();
                File.WriteAllText(fileName, html, Encoding.UTF8);
                success++;
                return html;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine("      ------------------------------------------------------------------");
                Console.WriteLine("      Message:");
                Console.WriteLine("      " + ex.Message);
                Console.WriteLine("      ------------------------------------------------------------------");
                return "";
            }
        }

        private void Parse(string html)
        {
            MatchCollection matches = new Regex(@"(href|HREF)[]*=[]*[""'][^""'#>]+" + regex + @"[""']").Matches(html);
            foreach (Match match in matches)
            {
                string strRef = match.Value.Substring(match.Value.IndexOf('=') + 1).Trim('"', '\"', '#', '>');
                if (!strRef.StartsWith("http://") && !strRef.StartsWith("https://"))
                {
                    if(!strRef.Contains(baseAddr))
                    {
                        if(strRef.StartsWith("/"))
                        {
                            strRef = http + baseAddr + strRef;
                        }
                        else
                        {
                            strRef = http + baseAddr + "/" + strRef;
                        }
                    }
                    else
                    {
                        strRef = http + strRef;
                    }
                }
                if (!strRef.StartsWith(http + baseAddr)) continue;
                if (strRef.Length == 0) continue;
                if (urls[strRef] == null) urls[strRef] = false;
            }
        }
    }
}
