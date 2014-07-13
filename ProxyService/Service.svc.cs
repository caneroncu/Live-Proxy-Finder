using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using ProxyService.Helper;
using ProxyService.Model;
using Starksoft.Net.Proxy;
using ProxyType = ProxyService.Model.ProxyType;

namespace ProxyService
{
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service.svc or Service.svc.cs at the Solution Explorer and start debugging.
    public class Service : IService
    {
        public FindProxiesResult FindProxies(int timeout, byte[] proxyUrlFile)
        {
            List<Proxy> proxies = (List<Proxy>)ProxyHelper.FindProxies(timeout, proxyUrlFile);
            return new FindProxiesResult()
                {
                    Proxies = proxies,
                    ProxyCount = proxies.Count
                };
        }

        public ProxyWithType CheckProxy(string IP, int port, int timeout)
        {
            return ProxyHelper.CheckProxy(IP, port, timeout);
        }

    }
}
