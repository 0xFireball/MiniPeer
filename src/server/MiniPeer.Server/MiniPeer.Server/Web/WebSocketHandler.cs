using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using MiniPeer.Server.Configuration;

namespace MiniPeer.Server.Web
{
    public class WebSocketHandler : DependencyObject
    {
        private WebSocket _ws;

        private CancellationToken _cancelTok;

        public WebSocketHandler(WebSocket websocket, CancellationToken token, ISContext serverContext) : base(serverContext)
        {
            _ws = websocket;
            _cancelTok = token;
        }

        public async Task EventLoop()
        {
            while (!_cancelTok.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                // recieve data
                
            }
        }

        public static async Task AcceptWebSocketClients(HttpContext hc, Func<Task> n, ISContext context)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var ct = hc.RequestAborted;
            var ws = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new WebSocketHandler(ws, ct, context);
            await h.EventLoop();
        }

        public static void Map(IApplicationBuilder app, ISContext context)
        {
            app.UseWebSockets();
            app.Use((hc, n) => WebSocketHandler.AcceptWebSocketClients(hc, n, context));
        }
    }
}