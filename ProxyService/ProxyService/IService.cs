using System.Collections.Generic;
using System.ServiceModel;
using ProxyService.Model;

namespace ProxyService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        IEnumerable<Proxy> FindProxies(int timeout, byte[] proxyUrlFile);
    }
}
