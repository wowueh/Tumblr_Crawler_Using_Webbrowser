using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;
using HtmlAgilityPack;
using System.Threading;

namespace TumblrCrawler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonGet_Click(object sender, EventArgs e)
        {
            webBrowser1.Visible = false;
            richTextBox1.Visible = true;
            TumblrSite();
            List<string> listOfFiles = GetAllLinks(1, PageNums);         
            TotalThreads = listOfFiles.Count();
            int i = 1;
            timer1.Enabled = true;
            foreach (string item in listOfFiles)
            {
                Task.Factory.StartNew(() => DownloadMultiThreadAsync(item));
                Thread.Sleep(2000);
                richTextBox1.AppendText("Start thread " + i);
                i++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate("https://www.tumblr.com/login");
        }


        public string HomePage { get; set; }
        public int PageNums { get; set; }
        public int ImgNumsPerPage { get; set; }
        public int SizeLimit { get; set; }
        public int CompleteThreadNums { get; set; }
        public int CompleteImgNums { get; set; }
        public int TotalThreads { get; set; }
        public int TotalImgs { get; set; }
        public void TumblrSite()
        {
            CompleteThreadNums = 0;
            CompleteImgNums = 0;
            SizeLimit = 0;
            //checkTumbUrl:
            //richTextBox1.AppendText("Please paste valid Url of Tumblrsite:");
            string homePage = textBox1.Text;
            string checkPattern = @"^http.+\.tumblr\.com/$";
            Regex checkRegex = new Regex(checkPattern);
            HomePage = homePage;
            PageNums = CountNumOfPages();
            richTextBox1.AppendText("This site have " + PageNums + " page(s)!\r\n");
            ImgNumsPerPage = CountImgNumsPerPage();
            richTextBox1.AppendText("Each page have " + ImgNumsPerPage + " image(s)!\r\n");
            //richTextBox1.AppendText("Please set limit of image size (KBs):");
            SizeLimit = 50 * 1024;
            richTextBox1.AppendText("Size limit: " + SizeLimit);
        }
        public async void DownloadImagesFromAllPagesAsync()
        {
            int sequenceNo = 1;
            long totalSize = 0;
            //Create download folder:
            string siteNamePattern = @"[^\/]+\..+\..{3}";
            Regex siteNameRegex = new Regex(siteNamePattern);
            Match siteNameMatch = siteNameRegex.Match(HomePage);
            string siteName = siteNameMatch.Value;
            string downloadFolder = "d:/ImageCrawler/" + siteName + " (page All)";
            System.IO.Directory.CreateDirectory(downloadFolder);
            //Download each specific page of site:
            for (int pageNo = 1; pageNo <= PageNums; pageNo++)
            {
                richTextBox1.AppendText("-------------- Starting download Page " + pageNo + "--------------");
                string childrenPageUrl = HomePage + "page/" + pageNo;
                string html = RequestHtml(childrenPageUrl);
                List<string> linksOfChildrenPage = GetLink(html);
                //Download each specific link of page:
                foreach (var item in linksOfChildrenPage)
                {
                    //Tim extension cua file anh
                    string fileTypePattern = "...$";
                    Regex fileTypeRegex = new Regex(fileTypePattern);
                    Match fileTypeMatch = fileTypeRegex.Match(item);
                    string fileType = fileTypeMatch.Value;
                    //Request image using it's URL
                    var client2 = new HttpClient();
                    var response = new HttpResponseMessage();
                    response = await client2.GetAsync(item);
                    var responseContent = await response.Content.ReadAsByteArrayAsync();
                    var fileSize = response.Content.Headers.ContentLength;
                    //Save to disk
                    if ((fileType == "jpg" || fileType == "png" || fileType == "gif") && fileSize >= SizeLimit)
                    {
                        var file = new FileStream(downloadFolder + "/" + sequenceNo + "." + fileType, FileMode.OpenOrCreate);
                        var bw = new BinaryWriter(file);
                        bw.Write(responseContent);
                        bw.Flush();
                        bw.Close();
                        richTextBox1.AppendText(sequenceNo + "." + fileType + " Download completed..." + fileSize / 1024 + " KB(s)");
                        sequenceNo++;
                        totalSize += Convert.ToInt64(fileSize);
                    }
                }
            }
            //richTextBox1.AppendText("Download Complete. Total {0} image(s). Saved Location: {1}. Total size: {2} KB(s)", sequenceNo - 1, downloadFolder, totalSize / 1024);
        }
        public async void DownloadImagesFromRangedPagesAsync()
        {
            richTextBox1.AppendText("Start page's number: ");
            int startPage = Convert.ToInt32(Console.ReadLine());
            richTextBox1.AppendText("End page's number: ");
            int endPage = Convert.ToInt32(Console.ReadLine());
            int sequenceNo = 1;
            long totalSize = 0;
            //Create download folder:
            string siteNamePattern = @"[^\/]+\..+\..{3}";
            Regex siteNameRegex = new Regex(siteNamePattern);
            Match siteNameMatch = siteNameRegex.Match(HomePage);
            string siteName = siteNameMatch.Value;
            string downloadFolder = "d:/ImageCrawler/" + siteName + "/(page " + startPage + "_" + endPage + ")";
            System.IO.Directory.CreateDirectory(downloadFolder);
            //Download each specific page of site:
            for (int pageNo = startPage; pageNo <= endPage; pageNo++)
            {
                //richTextBox1.AppendText("-------------- Starting download Page {0} --------------", pageNo);
                string childrenPageUrl = HomePage + "page/" + pageNo;
                string html = RequestHtml(childrenPageUrl);
                List<string> linksOfChildrenPage = GetLink(html);
                //Download each specific link of page:
                foreach (var item in linksOfChildrenPage)
                {
                    //Tim extension cua file anh
                    string fileTypePattern = "...$";
                    Regex fileTypeRegex = new Regex(fileTypePattern);
                    Match fileTypeMatch = fileTypeRegex.Match(item);
                    string fileType = fileTypeMatch.Value;
                    //Request image using it's URL
                    var client2 = new HttpClient();
                    var response = new HttpResponseMessage();
                    response = await client2.GetAsync(item);
                    var responseContent = await response.Content.ReadAsByteArrayAsync();
                    var fileSize = response.Content.Headers.ContentLength;
                    //Save to disk
                    if ((fileType == "jpg" || fileType == "png" || fileType == "gif") && fileSize >= SizeLimit)
                    {
                        var file = new FileStream(downloadFolder + "/" + sequenceNo + "." + fileType, FileMode.OpenOrCreate);
                        var bw = new BinaryWriter(file);
                        bw.Write(responseContent);
                        bw.Flush();
                        bw.Close();
                        //richTextBox1.AppendText("{0}.{1} Download completed...{2} KB(s)", sequenceNo, fileType, fileSize / 1024);
                        sequenceNo++;
                        totalSize += Convert.ToInt64(fileSize);
                    }
                }
            }
            //richTextBox1.AppendText("Download Complete. Total {0} image(s). Saved Location: {1}. Total size: {2} KB(s)", sequenceNo - 1, downloadFolder, totalSize / 1024);
        }
        //This method use for Multithreading purpose:
        public async void DownloadMultiThreadAsync(string listUrlOfImgs)
        {
            int sequenceNo = 1;
            long totalSize = 0;
            //extract folder name
            string threadNamePattern = @"\/Thread.+\.";
            Regex threadNameRegex = new Regex(threadNamePattern);
            Match threadNameMatch = threadNameRegex.Match(listUrlOfImgs);
            string threadName = threadNameMatch.Value;
            //Create download folder:
            string siteNamePattern = @"[^\/]+\..+\..{3}";
            Regex siteNameRegex = new Regex(siteNamePattern);
            Match siteNameMatch = siteNameRegex.Match(HomePage);
            string siteName = siteNameMatch.Value;
            string downloadFolder = "d:/ImageCrawler/" + siteName + "/image " + threadName;
            System.IO.Directory.CreateDirectory(downloadFolder);
            //Download each specific link:
            // Read the file and display it line by line.
            string line;
            using (System.IO.StreamReader linksFile = new System.IO.StreamReader(listUrlOfImgs))
            {
                while ((line = linksFile.ReadLine()) != null)
                {
                    //Tim extension cua file anh
                    string fileTypePattern = "...$";
                    Regex fileTypeRegex = new Regex(fileTypePattern);
                    Match fileTypeMatch = fileTypeRegex.Match(line);
                    string fileType = fileTypeMatch.Value;
                    //Request image using it's URL
                    var client2 = new HttpClient();
                    var response = new HttpResponseMessage();
                    response = await client2.GetAsync(line);
                    var responseContent = await response.Content.ReadAsByteArrayAsync();
                    var fileSize = response.Content.Headers.ContentLength;
                    //Save to disk
                    if ((fileType == "jpg" || fileType == "png" || fileType == "gif") && fileSize >= SizeLimit)
                    {
                        var file = new FileStream(downloadFolder + "/" + sequenceNo + "." + fileType, FileMode.OpenOrCreate);
                        var bw = new BinaryWriter(file);
                        bw.Write(responseContent);
                        bw.Flush();
                        bw.Close();
                        sequenceNo++;
                        totalSize += Convert.ToInt64(fileSize);
                    }
                    CompleteImgNums++;
                } 
            }
            CompleteThreadNums++;
        }
        public List<string> GetAllLinks(int startPage, int endPage)
        {
            TotalImgs = 0;
            int sequenceNo = 1;
            long totalSize = 0;
            List<string> listOfFiles = new List<string>();
            //Create Mother folder:
            string siteNamePattern = @"[^\/]+\..+\..{3}";
            Regex siteNameRegex = new Regex(siteNamePattern);
            Match siteNameMatch = siteNameRegex.Match(HomePage);
            string siteName = siteNameMatch.Value;
            string downloadFolder = "d:/ImageCrawler/" + siteName;
            System.IO.Directory.CreateDirectory(downloadFolder);
            //Dividing threads
            List<int[]> listThread = SliceDownloadList(startPage, endPage);
            int threadSeq = 0;
            foreach (int[] item in listThread)
            {
                threadSeq++;
                //Create each linksFile present for each Thread:
                string fileName = downloadFolder + "/Thread " + threadSeq + ".txt";
                using (File.Create(fileName)) { };
                //Get links from each specific page of site:
                for (int pageNo = item[0]; pageNo <= item[1]; pageNo++)
                {
                    labelStatus.Text = "Get links of page " + pageNo + "/" + endPage;
                    string childrenPageUrl = HomePage + "page/" + pageNo;
                    string html = RequestHtml(childrenPageUrl);
                    List<string> linksOfChildrenPage = GetLink(html);
                    foreach (string item2 in linksOfChildrenPage)
                    {
                        using (StreamWriter sw = File.CreateText(fileName))
                        {
                            sw.WriteLine(item2);
                        }
                        TotalImgs++;
                    }
                }
                listOfFiles.Add(fileName);
            }           
            return listOfFiles;
        }
        private List<string> GetLink(string html)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            List<HtmlNode> imageNodes = null;
            imageNodes = doc.DocumentNode.SelectNodes("//img[@src]").ToList();
            //int nodeNum = imageNodes.Count;
            //richTextBox1.AppendText("Trang web co {0} hinh anh.", nodeNum);
            List<string> imageUrls = new List<string>();
            foreach (HtmlNode node in imageNodes)
            {
                string src = node.Attributes["src"].Value;
                string httppattern = "^http.+";
                Regex httpregex = new Regex(httppattern);
                string fileTypePattern = "...$";
                Regex fileTypeRegex = new Regex(fileTypePattern);
                if (httpregex.IsMatch(src)&&fileTypeRegex.IsMatch(src))
                {
                    imageUrls.Add(src);
                    //richTextBox1.AppendText(src);
                }
            }
            //richTextBox1.AppendText("Co {0} link anh http.", imageUrls.Count());
            return imageUrls;
        }
        private string RequestHtml(string pageAddress)
        {
            string html;
            webBrowser1.Navigate(pageAddress);
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
                Thread.Sleep(500);
            }
            html = webBrowser1.DocumentText;
            return html;
        }
        private int CountNumOfPages()
        {
            richTextBox1.AppendText("Counting number of pages ...\r\n");
            int startNum = 1;
            int endNum = 10000;
            int numOfPages = 1;
            int lastEndNum = endNum;
            while (endNum > startNum)
            {
                //richTextBox1.AppendText("from {0} to {1}",startNum,endNum);
                labelStatus.Text = startNum + " - " + endNum;
                string childrenPageUrl = HomePage + "page/" + endNum;
                string html = RequestHtml(childrenPageUrl);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                if (doc.DocumentNode.SelectNodes("//@class[@class='post photo']") == null && doc.DocumentNode.SelectNodes("//@class[@class='photoset']") == null && doc.DocumentNode.SelectNodes("//@class[@class='photo_wrap']") == null && doc.DocumentNode.SelectNodes("//@id[@id='photo']") == null && doc.DocumentNode.SelectNodes("//@class[@class='post-photo']") == null)
                {
                    if (endNum - startNum > 1)
                    {
                        int temp = (startNum + endNum) / 2;
                        lastEndNum = endNum;
                        endNum = temp;
                    }
                    else
                    {
                        endNum = startNum;
                    }
                }
                else
                {
                    if (endNum - startNum > 1)
                    {
                        startNum = endNum;
                        endNum = lastEndNum;
                    }
                    else
                    {
                        startNum = endNum;
                    }
                }
            }
            numOfPages = startNum;
            return numOfPages;
        }
        private int CountImgNumsPerPage()
        {
            richTextBox1.AppendText("Counting Number of Images per Page ...\r\n");
            string firstPageUrl = HomePage + "page/2";
            string html = RequestHtml(firstPageUrl);
            //richTextBox1.AppendText(html);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            List<HtmlNode> imageNodes = null;
            imageNodes = doc.DocumentNode.SelectNodes("//img[@src]").ToList();
            int nodeNum = 0;
            foreach (var item in imageNodes)
            {
                string src = item.Attributes["src"].Value;
                string imgCheckPattern = @"(.+jpg|.+png|.+gif)";
                Regex imgCheckRegex = new Regex(imgCheckPattern);
                if (imgCheckRegex.IsMatch(src))
                {
                    nodeNum++;
                }
            }
            return nodeNum;
        }
        public List<int[]> SliceDownloadList(int startPage, int endPage)
        {
            int threadNums = 10;
            List<int[]> rangesSet = new List<int[]>();
            int range = endPage - startPage + 1;
            if (range <= threadNums)
            {
                for (int i = 1; i <= range; i++)
                {
                    int[] j = new int[2] { i, i };
                    rangesSet.Add(j);
                }
            }
            else
            {
                int step = range / threadNums;
                int check = range % threadNums;
                if (check == 0)
                {
                    int temp = startPage;
                    for (int i = 1; i <= threadNums; i++)
                    {
                        int[] j = new int[2] { temp, temp + step - 1 };
                        rangesSet.Add(j);
                        temp += step;
                    }
                }
                else
                {
                    int temp = startPage;
                    for (int i = 1; i <= threadNums; i++)
                    {
                        if (temp + step - 1 <= endPage)
                        {
                            int[] j = new int[2] { temp, temp + step - 1 };
                            rangesSet.Add(j);
                            temp += step;
                        }
                        else
                        {
                            int[] j = new int[2] { temp, temp + step - 2 };
                            rangesSet.Add(j);
                            temp += (step - 1);
                        }
                    }
                }
            }
            return rangesSet;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelStatus.Text = CompleteImgNums + "/" + TotalImgs + " Images completed.";
            label2.Text = CompleteThreadNums + "/" + TotalThreads + " Threads completed!";
        }
    }

}
