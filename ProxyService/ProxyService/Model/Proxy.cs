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
        [DataMember]
        public ProxyType ProxyType { get; set; }
    }
    [DataContract]
    public enum ProxyType
    {
        [EnumMember]
        None,
        [EnumMember]
        Socks4,
        [EnumMember]
        Socks5
    }
}