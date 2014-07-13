using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using ProxyService.Helper;
using ProxyService.Model;

namespace ProxyService
{
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service.svc or Service.svc.cs at the Solution Explorer and start debugging.
    public class Service : IService
    {
        public FindProxiesResult FindProxies(int timeout, byte[] proxyUrlFile)
        {
            IEnumerable<Proxy> proxies = ProxyHelper.FindProxies(timeout, proxyUrlFile);
            return new FindProxiesResult()
                {
                    Proxies = proxies,
                    ProxyCount = ((List<Proxy>)proxies).Count
                };
        }
    }
}
