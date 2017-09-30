namespace MiniPeer.Server.Web
{
    public class PeerClient
    {
        public WebSocketHandler Handler { get; }

        public PeerClient(WebSocketHandler handler)
        {
            Handler = handler;
        }
    }
}