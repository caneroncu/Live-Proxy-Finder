using System;
using System.Threading;
using System.Windows.Forms;

namespace ProxyService.Util
{
    public class WebCrawler
    {
        public WebCrawler()
        {
            IsCrawlingCompleted = false;
            CrawledText = string.Empty;
        }

        public string CrawledText { get; private set; }
        public bool IsCrawlingCompleted { get; private set; }

        public void Crawl(string url)
        {
            Thread thread = new Thread(() =>
            {
                using (WebBrowserNoSound webBrowser = new WebBrowserNoSound())
                {
                    webBrowser.ScriptErrorsSuppressed = true;
                    webBrowser.AllowNavigation = true;
                    webBrowser.Navigate(new Uri(url));
                    webBrowser.DocumentCompleted += delegate(object sender, WebBrowserDocumentCompletedEventArgs e)
                    {
                        WebBrowserNoSound wb = sender as WebBrowserNoSound;
                        //Don't get page's source code, open the page first, simulate "select all" and "copy" operation
                        //Sometimes proxy pages obfuscate their source code, it is best to "act like a user"
                        wb.Document.ExecCommand("SelectAll", false, null);
                        wb.Document.ExecCommand("Copy", false, null);
                        try
                        {
                            CrawledText = Clipboard.GetText();
                        }
                        //If it can't copy the webpage being crawled, don't raise an error
                        catch (Exception)
                        {}
                        IsCrawlingCompleted = true;
                        Application.ExitThread();
                    };
                    Application.Run();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
