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
using Newtonsoft.Json.Linq;

namespace MiniPeer.Server.Web
{
    public class WebSocketHandler : DependencyObject
    {
        public const int BUF_SIZE = 8192;

        public WebSocket Connection { get; }

        private CancellationToken _cancelTok;

        private string _id;

        public WebSocketHandler(WebSocket websocket, CancellationToken token, string id, ISContext serverContext) : base(serverContext)
        {
            Connection = websocket;
            _cancelTok = token;
            _id = id;
        }

        public async Task EventLoop()
        {
            while (!_cancelTok.IsCancellationRequested && Connection.State == WebSocketState.Open)
            {
                // recieve data
                var data = await ReceiveDataAsync();
                var dataStr = Encoding.UTF8.GetString(data);
                if (string.IsNullOrEmpty(dataStr))
                {
                    break;
                }
                var dataBundle = JObject.Parse(dataStr);
                var targetId = (string)dataBundle["target"];
                // find a target with matching id
                if (!ServerContext.Clients.ContainsKey(targetId))
                {
                    await SendDataAsync(new JObject(
                        new JProperty("success", false),
                        new JProperty("error", "notfound")
                    ).ToString());
                }
                else
                {
                    // send data
                    var targetPeer = ServerContext.Clients[targetId];
                    await targetPeer.Handler.SendDataAsync(new JObject(
                        new JProperty("type", "data"),
                        new JProperty("data", (string)dataBundle["data"]),
                        new JProperty("source", _id)
                    ).ToString());

                    // send result
                    await SendDataAsync(new JObject(
                        new JProperty("success", true)
                    ).ToString());
                }
            }
        }

        public async Task<byte[]> ReceiveDataAsync()
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
            return Connection.SendAsync(seg, WebSocketMessageType.Text, true, _cancelTok);
        }

        public static async Task AcceptWebSocketClients(HttpContext hc, Func<Task> n, ISContext context)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var ct = hc.RequestAborted;
            var peerId = Guid.NewGuid().ToString("N");
            var ws = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new WebSocketHandler(ws, ct, peerId, context);
            // add to client list
            context.Clients.TryAdd(peerId, new PeerClient(h, peerId));
            // send peer id to client
            await h.SendDataAsync(new JObject(
                new JProperty("id", peerId)
            ).ToString());
            await h.EventLoop();

            context.Clients.TryRemove(peerId, out var rem);

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