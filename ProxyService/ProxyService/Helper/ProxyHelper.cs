using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProxyService.Exceptions;
using ProxyService.Model;

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

            if(proxyURLS == null)
                throw new ProxyFileNotFoundException();

            //Add each URL's proxy list to proxies list 
            foreach (string url in proxyURLS)
            {
                proxies.AddRange(getProxiesFromURL(url));
            }
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
            string urlSourceCode = null;

            WebBrowserNoSound webBrowser = new WebBrowserNoSound();
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.AllowNavigation = true;
            webBrowser.Navigate(new Uri(url));

            bool isDocumentCompleted = false;
            webBrowser.DocumentCompleted += delegate(object sender, WebBrowserDocumentCompletedEventArgs e)
                {
                    WebBrowserNoSound wb = sender as WebBrowserNoSound;
                    wb.Document.ExecCommand("SelectAll", false, null);
                    wb.Document.ExecCommand("Copy", false, null);
                    urlSourceCode = Clipboard.GetText();
                    isDocumentCompleted = true;
                };

            while(isDocumentCompleted == false)
                Application.DoEvents();

            //IP:PORT pattern (whitespaces between ip and port are ignored)
            //TODO: Can find a better regex
            string pattern =
                @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\s{0,}[\:*\s{0,}]([0-9]{1,5}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])";

            string refinedSourceCode = urlSourceCode.Replace("\n", "");
            refinedSourceCode = refinedSourceCode.Replace("\r", "");
            MatchCollection matchCollection = Regex.Matches(refinedSourceCode, pattern);

            //TODO: Should be multi-threaded
            foreach (Match match in matchCollection)
            {
                try
                {
                    string[] array = match.Value.Split(new char[] { ' ', ':' });
                    Proxy proxy = new Proxy();
                    proxy.IP = array[0];
                    proxy.Port = array[1];
                    //TODO: Check socks type and timeout - if it is live (set ProxyType)
                    proxies.Add(proxy);
                }
                //Ignore parsing errors, just don't add which it couldn't parse
                catch (Exception){}
            }
            return proxies;
        }
    }
}