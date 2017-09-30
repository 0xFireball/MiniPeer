using System.Collections.Concurrent;
using MiniPeer.Server.Web;

namespace MiniPeer.Server.Configuration
{
    public interface ISContext
    {
        ConcurrentDictionary<string, PeerClient> Clients { get; }
    }

    public class ServerContext : ISContext
    {
        public ConcurrentDictionary<string, PeerClient> Clients { get; }
    }
}