using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections;
using System.Web;

namespace SankakuChanGrab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int testing = 0;
        public static int waitEvent = 0;
        private HtmlDocument htmlDoc;
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
                writeLog("Testing set to ON(added debug messages)");
                button03.Content = "Turn testing OFF";
            }
            else
            {
                testing = 0;
                writeLog("Testing set to OFF (removed debug messages)");
                button03.Content = "Turn testing ON";
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
            //webClient.Headers.Add("Referer", @"https://www.sankakucomplex.com");
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
                string jsonURL = @"https://www.sankakucomplex.com/chanbrowse/rssCache/chan_safe.JSON";
                //using (WebClient webClientBase = webClientOriginal)
                //{ 
                //webClientBase.OpenRead(@"https://www.sankakucomplex.com/");

                WebClient webClientOriginal = null;
                while (webClientOriginal == null)
                {
                    Console.WriteLine("Trying..");
                    // webClientOriginal = CloudflareEvader.CreateBypassedWebClient("https://www.sankakucomplex.com");
                    webClientOriginal = getWebClient();
                }
                byte[] jsonData = webClientOriginal.DownloadData(jsonURL);
                string jsonString = Encoding.UTF8.GetString(jsonData);
                List<Pages> deserializedPages = JsonConvert.DeserializeObject<List<Pages>>(jsonString);
                foreach (Pages page in deserializedPages)
                {
                    string reducedName = "";
                    if (page.title.Length > 100)
                    {
                        reducedName = page.title.Remove(100);
                    }
                    else
                    {
                        reducedName = page.title;
                    }
                    if (testing == 1) writeLog(reducedName);
                    if (testing == 1) writeLog("page.href: " + page.href);
                    string fileURL = "";
                    //using (WebClient webClient = webClientOriginal)
                    //{
                    /*WebClient webClientOriginal2 = null;
                    while (webClientOriginal2 == null)
                    {
                        Console.WriteLine("Trying..");
                        webClientOriginal2 = CloudflareEvader.CreateBypassedWebClient("https:" + page.href);
                    }*/
                    //Thread.Sleep(1000);
                    string pageData = webClientOriginal.DownloadString("https:" + page.href);
                    htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(pageData);

                    string xpathQuery = "//a[@id=\"highres\"][contains(@href,\".sankakucomplex.com/data\")]";
                    foreach (HtmlNode resultNode in htmlDoc.DocumentNode.SelectNodes(xpathQuery))
                    {
                        fileURL = "https:" + resultNode.GetAttributeValue("href", "");
                        fileURL = fileURL.Replace("&amp;","&");
                        if (testing == 1) writeLog("fileURL: " + fileURL);
                        break;
                    }
                    
                    //Thread.Sleep(50);
                    string fileName = "empty.txt";
                    fileName = fileURL.Remove(fileURL.IndexOf("?"), fileURL.Length - fileURL.IndexOf("?"));
                    fileName = fileName.Substring(fileName.Length - 20, 20);
                    fileName = reducedName + " " + fileName;
                    fileName = MakeValidFileName(fileName);
                    fileName = "img/" + fileName;
                    if (!File.Exists(fileName))
                    {
                        WebClient webClientOriginal2 = null;
                        while (webClientOriginal2 == null)
                        {
                            Console.WriteLine("Trying..");
                            // webClientOriginal = CloudflareEvader.CreateBypassedWebClient("https://www.sankakucomplex.com");
                            using (webClientOriginal2 = getWebClient()) {

                                byte[] file = webClientOriginal2.DownloadData(fileURL);
                                File.WriteAllBytes(fileName, file);
                                if (testing == 1) writeLog(fileName + " has been saved to the disk successfully!");
                                fileCount++;

                            }
                        }
                        
                    }

                }
                
                writeLog(fileCount + " Files downloaded");
            }
            catch (Exception ex)
            {
                writeLog("Error: " + ex.Message + " " + ex.StackTrace);
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

        private void writeLog(string log)
        {
            File.AppendAllText("changrab.log", DateTime.Now.ToLongTimeString()+": "+log+"\n");
        } 

    }

    public class Pages
    {
        public string title { get; set; }
        public string href { get; set; }
        public string thumb { get; set; }
    }

    public class CloudflareEvader
    {
        /// <summary>
        /// Tries to return a webclient with the neccessary cookies installed to do requests for a cloudflare protected website.
        /// </summary>
        /// <param name="url">The page which is behind cloudflare's anti-dDoS protection</param>
        /// <returns>A WebClient object or null on failure</returns>
        public static WebClient CreateBypassedWebClient(string url)
        {
            var JSEngine = new Jint.Engine(); //Use this JavaScript engine to compute the result.

            //Download the original page
            var uri = new Uri(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";
            //Try to make the usual request first. If this fails with a 503, the page is behind cloudflare.
            try
            {
                var res = req.GetResponse();
                string html = "";
                using (var reader = new StreamReader(res.GetResponseStream()))
                    html = reader.ReadToEnd();
                return new WebClient();
            }
            catch (WebException ex) //We usually get this because of a 503 service not available.
            {
                string html = "";
                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                    html = reader.ReadToEnd();
                //If we get on the landing page, Cloudflare gives us a User-ID token with the cookie. We need to save that and use it in the next request.
                var cookie_container = new CookieContainer();
                //using a custom function because ex.Response.Cookies returns an empty set ALTHOUGH cookies were sent back.
                var initial_cookies = GetAllCookiesFromHeader(ex.Response.Headers["Set-Cookie"], uri.Host);
                foreach (Cookie init_cookie in initial_cookies)
                    cookie_container.Add(init_cookie);

                /* solve the actual challenge with a bunch of RegEx's. Copy-Pasted from the python scrapper version.*/
                var challenge = Regex.Match(html, "name=\"jschl_vc\" value=\"(\\w+)\"").Groups[1].Value;
                var challenge_pass = Regex.Match(html, "name=\"pass\" value=\"(.+?)\"").Groups[1].Value;

                var builder = Regex.Match(html, @"setTimeout\(function\(\){\s+(var t,r,a,f.+?\r?\n[\s\S]+?a\.value =.+?)\r?\n").Groups[1].Value;
                builder = Regex.Replace(builder, @"a\.value =(.+?) \+ .+?;", "$1");
                builder = Regex.Replace(builder, @"\s{3,}[a-z](?: = |\.).+", "");

                //Format the javascript..
                builder = Regex.Replace(builder, @"[\n\\']", "");

                //Execute it. 
                long solved = long.Parse(JSEngine.Execute(builder).GetCompletionValue().ToObject().ToString());
                solved += uri.Host.Length; //add the length of the domain to it.

                Console.WriteLine("***** SOLVED CHALLENGE ******: " + solved);
                Thread.Sleep(3000); //This sleeping IS requiered or cloudflare will not give you the token!!

                //Retreive the cookies. Prepare the URL for cookie exfiltration.
                string cookie_url = string.Format("{0}://{1}/cdn-cgi/l/chk_jschl", uri.Scheme, uri.Host);
                var uri_builder = new UriBuilder(cookie_url);
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                //Add our answers to the GET query
                query["jschl_vc"] = challenge;
                query["jschl_answer"] = solved.ToString();
                query["pass"] = challenge_pass;
                uri_builder.Query = query.ToString();

                //Create the actual request to get the security clearance cookie
                HttpWebRequest cookie_req = (HttpWebRequest)WebRequest.Create(uri_builder.Uri);
                cookie_req.AllowAutoRedirect = false;
                cookie_req.CookieContainer = cookie_container;
                cookie_req.Referer = url;
                cookie_req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";
                //We assume that this request goes through well, so no try-catch
                var cookie_resp = (HttpWebResponse)cookie_req.GetResponse();
                //The response *should* contain the security clearance cookie!
                if (cookie_resp.Cookies.Count != 0) //first check if the HttpWebResponse has picked up the cookie.
                    foreach (Cookie cookie in cookie_resp.Cookies)
                        cookie_container.Add(cookie);
                else //otherwise, use the custom function again
                {
                    //the cookie we *hopefully* received here is the cloudflare security clearance token.
                    if (cookie_resp.Headers["Set-Cookie"] != null)
                    {
                        var cookies_parsed = GetAllCookiesFromHeader(cookie_resp.Headers["Set-Cookie"], uri.Host);
                        foreach (Cookie cookie in cookies_parsed)
                            cookie_container.Add(cookie);
                    }
                    else
                    {
                        //No security clearence? something went wrong.. return null.
                        //Console.WriteLine("MASSIVE ERROR: COULDN'T GET CLOUDFLARE CLEARANCE!");
                        return null;
                    }
                }
                //Create a custom webclient with the two cookies we already acquired.
                WebClient modedWebClient = new WebClientEx(cookie_container);
                modedWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                //modedWebClient.Headers.Add("User-Agent", "Mozilla / 5.0(compatible; Googlebot / 2.1; +http://www.google.com/bot.html)");
                modedWebClient.Headers.Add("Referer", url);
                modedWebClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                modedWebClient.Headers.Add("Accept-Language", "fr-FR,fr;q=0.8,en-US;q=0.6,en;q=0.4");
                //modedWebClient.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                return modedWebClient;
            }
        }

        /* Credit goes to https://stackoverflow.com/questions/15103513/httpwebresponse-cookies-empty-despite-set-cookie-header-no-redirect 
           (user https://stackoverflow.com/users/541404/cameron-tinker) for these functions 
        */
        public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }
            return cc;
        }

        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                    al.Add(strCookTemp[i]);
                i = i + 1;
            }
            return al;
        }

        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }
                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Path = NameValuePairTemp[1];
                            else
                                cookTemp.Path = "/";
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Domain = NameValuePairTemp[1];
                            else
                                cookTemp.Domain = strHost;
                        }
                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                    cookTemp.Path = "/";
                if (cookTemp.Domain == string.Empty)
                    cookTemp.Domain = strHost;
                cc.Add(cookTemp);
            }
            return cc;
        }
    }

    /*Credit goes to  https://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
 (user https://stackoverflow.com/users/129124/pavel-savara) */
    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }

        public CookieContainer CookieContainer
        {
            get { return container; }
            set { container = value; }
        }

        private CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }

        }

    }

}

