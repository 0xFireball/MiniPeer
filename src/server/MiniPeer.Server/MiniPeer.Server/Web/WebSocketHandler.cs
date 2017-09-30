using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using MiniPeer.Server.Configuration;
using System.IO;
using System.Text;

namespace MiniPeer.Server.Web
{
    public class WebSocketHandler : DependencyObject
    {
        public const int BUF_SIZE = 8192;

        public WebSocket Connection { get; }

        private CancellationToken _cancelTok;

        public WebSocketHandler(WebSocket websocket, CancellationToken token, ISContext serverContext) : base(serverContext)
        {
            Connection = websocket;
            _cancelTok = token;
        }

        public async Task EventLoop()
        {
            while (!_cancelTok.IsCancellationRequested && Connection.State == WebSocketState.Open)
            {
                // recieve data


            }
        }

        public async Task<byte[]> RecieveDataAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[BUF_SIZE]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    _cancelTok.ThrowIfCancellationRequested();

                    result = await Connection.ReceiveAsync(buffer, _cancelTok);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public Task SendDataAsync(string data)
        {
            var buf = Encoding.UTF8.GetBytes(data);
            return SendDataAsync(buf);
        }

        public Task SendDataAsync(byte[] buf)
        {
            var seg = new ArraySegment<byte>(buf);
            return Connection.SendAsync(seg, WebSocketMessageType.Binary, true, _cancelTok);
        }

        public static async Task AcceptWebSocketClients(HttpContext hc, Func<Task> n, ISContext context)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var ct = hc.RequestAborted;
            var socketId = Guid.NewGuid().ToString("N");
            var ws = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new WebSocketHandler(ws, ct, context);
            // add to client list
            context.Clients.TryAdd(socketId, new PeerClient(h, socketId));
            await h.EventLoop();

            context.Clients.TryRemove(socketId, out var rem);

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            ws.Dispose();
        }

        public static void Map(IApplicationBuilder app, ISContext context)
        {
            app.UseWebSockets();
            app.Use((hc, n) => WebSocketHandler.AcceptWebSocketClients(hc, n, context));
        }
    }
}