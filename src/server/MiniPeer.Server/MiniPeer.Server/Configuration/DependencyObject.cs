namespace MiniPeer.Server.Configuration
{
    public class DependencyObject
    {
        public ISContext ServerContext { get; }

        public DependencyObject(ISContext context)
        {
            ServerContext = context;
        }
    }
}