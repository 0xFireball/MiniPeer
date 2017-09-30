using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniPeer.Server.Configuration;
using MiniPeer.Server.Web;
using Nancy;
using Nancy.Owin;
using Newtonsoft.Json;

namespace MiniPeer.Server
{
    public class Startup
    {
        public const string ConfigFileName = "minipeer.json";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            MiniPeerConfiguration config = null;
            // load config file
            if (File.Exists(ConfigFileName))
            {
                config = JsonConvert.DeserializeObject<MiniPeerConfiguration>(File.ReadAllText(ConfigFileName));
            }
            else
            {
                File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(new MiniPeerConfiguration()));
            }

            var serverContext = new ServerContext(config);
            app.Map("/ws", a => WebSocketHandler.Map(a, serverContext));

            app.UseOwin(x => x.UseNancy(options => options.PassThroughWhenStatusCodesAre(
                HttpStatusCode.NotFound,
                HttpStatusCode.InternalServerError
            )));
        }
    }
}
