using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Net;
using mshtml;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;

namespace WpfApplicationTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int testing = 0;
        private int logic = 0;
        public static int waitEvent = 0;
        private HtmlDocument htmlDoc;
        private string reducedName;
        //private Object mutex = new Object();
        //ManualResetEvent oSignalEvent = new ManualResetEvent(true);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_03(object sender, RoutedEventArgs e)
        {
            if (testing == 0)
            {
                testing = 1;
                MessageBox.Show("Testing set to 1 with debug messages");
                button03.Content = "Set testing 0";
            }
            else
            {
                testing = 0;
                MessageBox.Show("Testing set to 0 without debug messages");
                button03.Content = "Set testing 1";
            }
        }

        /*
        private void navigateWithAgent(string url)
        {
            webBrowser01.Navigate(url, "_self", null, @"User-Agent: Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
        }

        private void hideErrorSilenceActiveX()
        {
            dynamic activeX = this.webBrowser01.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, this.webBrowser01, new object[] { });

            activeX.Silent = true;
        }*/

        private WebClient getWebClient()
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("User-Agent", @"Mozilla/5.0 (Windows NT 6.3; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0");
            //webClient.Headers.Add("Referer", @"http://www.sankakucomplex.com");
            webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            webClient.Headers.Add("Accept-Language", "en-us,en;q=0.5");
            //webClient.Headers.Add("Accept-Encoding", "gzip,deflate");
            webClient.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            return webClient;
        }

        private void button05_Click(object sender, RoutedEventArgs e) //image tag search and download using webclient
        {
            try
            {
                int fileCount = 0;
                createBaseDirectory();
                string jsonURL = @"http://www.sankakucomplex.com/chanbrowse/rssCache/chan_safe.JSON";
                using (WebClient webClientBase = getWebClient())
                {
                    byte[] jsonData = webClientBase.DownloadData(jsonURL);
                    string jsonString = Encoding.UTF8.GetString(jsonData);
                    List<Pages> deserializedPages = JsonConvert.DeserializeObject<List<Pages>>(jsonString);
                    foreach (Pages page in deserializedPages)
                    {
                        string reducedName = "";
                        if (page.title.Length > 100) reducedName = page.title.Remove(100);
                        if (testing == 1) MessageBox.Show(reducedName);
                        if (testing == 1) MessageBox.Show("page.href: " + page.href);
                        //Thread.Sleep(50);
                        string fileURL = "";
                        using (WebClient webClient = getWebClient())
                        {
                            string pageData = webClient.DownloadString("https:" + page.href);
                            htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(pageData);
                            
                            string xpathQuery = "//a[@id=\"highres\"][contains(@href,\".sankakucomplex.com/data\")]";
                            foreach (HtmlNode resultNode in htmlDoc.DocumentNode.SelectNodes(xpathQuery))
                            {
                                fileURL = "http:" + resultNode.GetAttributeValue("href", "");
                                if (testing == 1) MessageBox.Show("fileURL: " + fileURL);
                                break;
                            }
                        }
                        //Thread.Sleep(50);
                        string fileName = "empty.txt";
                        fileName = fileURL.Remove(fileURL.IndexOf("?"), fileURL.Length - fileURL.IndexOf("?"));
                        fileName = fileName.Substring(fileName.Length - 20, 20);
                        fileName = reducedName + " " + fileName;
                        fileName = MakeValidFileName(fileName);
                        fileName = "img/"+fileName;
                        if (!File.Exists(fileName))
                        {
                            using (WebClient webClient = getWebClient())
                            {
                                byte[] file = webClient.DownloadData(fileURL);
                                File.WriteAllBytes(fileName, file);
                                if (testing == 1) MessageBox.Show(fileName + " has been saved to the disk successfully!");
                                fileCount++;
                            }
                        }

                    }
                }
                MessageBox.Show(fileCount + " Files downloaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + " " + ex.StackTrace);
                return;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void createBaseDirectory()
        {
            if (!Directory.Exists("img")) Directory.CreateDirectory("img");
            if (!Directory.Exists("preview")) Directory.CreateDirectory("preview");
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

    }

    public class Pages
    {
        public string title { get; set; }
        public string href { get; set; }
        public string thumb { get; set; }
    }

}
