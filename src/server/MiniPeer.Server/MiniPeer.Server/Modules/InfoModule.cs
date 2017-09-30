using Nancy;

namespace MiniPeer.Server.Modules
{
    public class InfoModule : NancyModule
    {
        public InfoModule()
        {
            Get("/info", _ => "MiniPeer");
        }
    }
}