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
        public WebSocket Connection { get; }

        public string Id { get; }

        private CancellationToken _cancelTok;

        public WebSocketHandler(WebSocket websocket, CancellationToken token, string id, ISContext serverContext) : base(serverContext)
        {
            Connection = websocket;
            Id = id;
            _cancelTok = token;
        }

        public async Task EventLoop()
        {
            while (!_cancelTok.IsCancellationRequested && Connection.State == WebSocketState.Open)
            {
                // recieve data

            }
        }

        public static async Task AcceptWebSocketClients(HttpContext hc, Func<Task> n, ISContext context)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var ct = hc.RequestAborted;
            var socketId = Guid.NewGuid().ToString("N");
            var ws = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new WebSocketHandler(ws, ct, socketId, context);
            // add to client list
            context.Clients.TryAdd(socketId, new PeerClient(h));
            await h.EventLoop();
        }

        public static void Map(IApplicationBuilder app, ISContext context)
        {
            app.UseWebSockets();
            app.Use((hc, n) => WebSocketHandler.AcceptWebSocketClients(hc, n, context));
        }
    }
}