using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using ProxyService.Model;

namespace ProxyService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        FindProxiesResult FindProxies(int timeout, byte[] proxyUrlFile);
    }

    [DataContract]
    public class FindProxiesResult
    {
        [DataMember(Order = 0)]
        public int ProxyCount { get; set; }
        [DataMember(Order = 1)]
        public IEnumerable<Proxy> Proxies { get; set; }
    }
}
