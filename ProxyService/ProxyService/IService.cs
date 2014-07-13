using System.Collections.Generic;
using System.ServiceModel;
using ProxyService.Model;

namespace ProxyService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        IEnumerable<Proxy> FindProxies(int timeout, byte[] proxyUrlFile);
    }
}
