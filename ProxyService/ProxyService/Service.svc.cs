using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ProxyService.Helper;
using ProxyService.Model;

namespace ProxyService
{
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service.svc or Service.svc.cs at the Solution Explorer and start debugging.
    public class Service : IService
    {
        private IEnumerable<Proxy> foundProxies;
        private int timeout;
        private byte[] proxyUrlFile;

         public IEnumerable<Proxy> FindProxies(int timeout, byte[] proxyUrlFile)
         {
             return FindProxiesSTA(timeout, proxyUrlFile);
         }

        /* In order to not receive ActiveX error from WebBrowser operation, 
         * I've made it a single-threaded method */
         [STAThread]
         public IEnumerable<Proxy> FindProxiesSTA(int timeout, byte[] proxyUrlFile)
         {
             this.timeout = timeout;
             this.proxyUrlFile = proxyUrlFile;
             Thread thread = new Thread(findProxiesSTA);
             thread.SetApartmentState(ApartmentState.STA);
             thread.Start();
             thread.Join();
             return foundProxies;
         }

         private void findProxiesSTA()
         {
             try
             {
                 foundProxies = ProxyHelper.FindProxies(timeout, proxyUrlFile);
             }
             catch (Exception)
             {
                 foundProxies = null;
                 throw;
             }
         }
    }
}
