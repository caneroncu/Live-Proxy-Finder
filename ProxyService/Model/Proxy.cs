using System.Runtime.Serialization;

namespace ProxyService.Model
{
    [DataContract]
    public class Proxy
    {
        [DataMember]
        public string IP { get; set; }
        [DataMember]
        public string Port { get; set; }
    }
    [DataContract]
    public class ProxyWithType : Proxy
    {
        [DataMember]
        public ProxyType ProxyType { get; set; }
    }

    [DataContract]
    public enum ProxyType
    {
        [EnumMember]
        Dead,
        [EnumMember]
        Socks4,
        [EnumMember]
        Socks5,
        [EnumMember]
        HTTP
    }
}