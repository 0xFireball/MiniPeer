namespace MiniPeer.Server.Web
{
    public class PeerClient
    {
        public WebSocketHandler Handler { get; }

        public string Id { get; }

        public PeerClient(WebSocketHandler handler, string id)
        {
            Handler = handler;
            Id = id;
        }
    }
}