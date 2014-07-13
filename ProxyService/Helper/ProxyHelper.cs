using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProxyService.Exceptions;
using ProxyService.Model;
using ProxyService.Util;
using Starksoft.Net.Proxy;

namespace ProxyService.Helper
{
    public class ProxyHelper
    {
        public static IEnumerable<Proxy> FindProxies(int timeout, byte[] urlFile)
        {
            List<Proxy> proxies = new List<Proxy>();
            //Full patch for proxy websites file
            string defaultProxyFilePath = string.Format(@"{0}{1}", AppDomain.CurrentDomain.BaseDirectory,
                                                        ConfigurationManager.AppSettings["ProxyAddressFile"]);
            IEnumerable<string> proxyURLS = null;
            //Fetch proxy websites from default file
            //TODO: Should be multi-threaded
            if (urlFile == null)
                proxyURLS = getProxyURLs(defaultProxyFilePath);
            else
                proxyURLS = getProxyURLs(urlFile);

            if (proxyURLS == null)
                throw new ProxyFileNotFoundException();

            //Add each URL's proxy list to proxies list 
            //TODO: Test multithread success results
            List<Task> tasks = new List<Task>();
            foreach (string url in proxyURLS)
            {
                Task task = new TaskFactory().StartNew(delegate()
                {
                    proxies.AddRange(getProxiesFromURL(url));
                });
                tasks.Add(task);
            }
            if (timeout > 0)
                Task.WaitAll(tasks.ToArray(), timeout);
            else
                Task.WaitAll(tasks.ToArray());
            return proxies;
        }

        private static IEnumerable<string> getProxyURLs(string urlFilePath)
        {
            using (StreamReader stream = new StreamReader(urlFilePath))
            {
                string line = string.Empty;
                while ((line = stream.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static IEnumerable<string> getProxyURLs(byte[] urlFile)
        {
            using (StreamReader stream = new StreamReader(new MemoryStream(urlFile)))
            {
                string line = string.Empty;
                while ((line = stream.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static IEnumerable<Proxy> getProxiesFromURL(string url)
        {
            List<Proxy> proxies = new List<Proxy>();
            string urlText = null;

            WebCrawler crawler = new WebCrawler();
            crawler.Crawl(url);
            if (crawler.IsCrawlingCompleted)
                urlText += crawler.CrawledText;

            //IP:PORT pattern (whitespaces between ip and port are ignored)
            //TODO: Can find a better regex
            string pattern =
                @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\s{0,}[\:*\s{0,}]([0-9]{1,5}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])";

            string refinedSourceCode = urlText.Replace("\n", "");
            refinedSourceCode = refinedSourceCode.Replace("\r", "");
            MatchCollection matchCollection = Regex.Matches(refinedSourceCode, pattern);

            //TODO: Should be multi-threaded
            foreach (Match match in matchCollection)
            {
                try
                {
                    //Split IP and PORT info
                    string[] array = match.Value.Split(new[] { ' ', ':' });
                    Proxy proxy = new Proxy();
                    proxy.IP = array[0];
                    proxy.Port = array[1];
                    //TODO: Check socks type and timeout - if it is live (set ProxyType)
                    proxies.Add(proxy);
                }
                //Ignore parsing errors, just don't add which it couldn't parse
                catch (Exception) { }
            }
            return proxies;
        }


        public static ProxyWithType CheckProxy(string IP, int port, int timeout)
        {
            string connectionTestIPAddress = ConfigurationManager.AppSettings["ConnectionTestIP"];
            string connectionTestWebAddress = ConfigurationManager.AppSettings["ConnectionTestWebAddress"];
            string connectionTestPort = ConfigurationManager.AppSettings["ConnectionTestPort"];

            ProxyWithType proxyWithType = new ProxyWithType();
            proxyWithType.IP = IP;
            proxyWithType.Port = port.ToString();
            proxyWithType.ProxyType = Model.ProxyType.Dead;

            TaskFactory taskFactory = new TaskFactory();
            List<Task> tasks = new List<Task>();

            tasks.Add(taskFactory.StartNew(delegate
            {
                try
                {
                    Socks4ProxyClient proxyClient4 = new Socks4ProxyClient(IP, port);
                    proxyClient4.CreateConnection(connectionTestIPAddress, int.Parse(connectionTestPort));
                    proxyWithType.ProxyType = Model.ProxyType.Socks4;
                }
                catch (Exception)
                {
                    //If proxy timeouts don't do anything
                }
            }));
            tasks.Add(taskFactory.StartNew(delegate
            {
                try
                {
                    Socks5ProxyClient proxyClient5 = new Socks5ProxyClient(IP, port);
                    proxyClient5.CreateConnection(connectionTestIPAddress, int.Parse(connectionTestPort));
                    proxyWithType.ProxyType = Model.ProxyType.Socks5;
                }
                catch (Exception)
                {
                    //If proxy timeouts don't do anything
                }
            }));
            tasks.Add(taskFactory.StartNew(delegate
            {
                try
                {
                    WebClient wc = new WebClient();
                    wc.Proxy = new WebProxy(IP, port);
                    wc.DownloadString(connectionTestWebAddress);
                    proxyWithType.ProxyType = Model.ProxyType.HTTP;
                }
                catch (Exception)
                {
                    //If proxy timeouts don't do anything
                }
            }));
            if (timeout > 0)
                Task.WaitAll(tasks.ToArray(), timeout);
            else
                Task.WaitAll(tasks.ToArray());

            return proxyWithType;
        }
    }
}