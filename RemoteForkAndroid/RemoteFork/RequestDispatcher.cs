using RemoteForkAndroid.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using tv.forkplayer.remotefork.server;
using Unosquare.Labs.EmbedIO;
using Encoding = System.Text.Encoding;

namespace RemoteForkAndroid.RemoteFork
{

        internal class RequestDispatcher : WebModuleBase
        {
            public RequestDispatcher()
            {
                AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (server, context) => Handle(context));
            }

            public override string Name => "RequestDispatcher";

            private bool Handle(HttpListenerContext context)
            {

                context.Response.StatusCode = 200;
                context.Response.Headers.Add(Constants.HeaderAccessControlAllowOrigin);
                context.Response.Headers.Add("Server", $"RemoteFork/1.3a");

            string httpUrl = context.Request.RawUrl;
                Console.WriteLine("URL:" + httpUrl);
            if (httpUrl.IndexOf("/treeview") == 0)
            {
                if (context.Request.QueryString.GetValues(null)?.FirstOrDefault(s => PluginRequestHandler.PluginParamRegex.IsMatch(s ?? string.Empty)) != null)
                {
                    var Handler = new PluginRequestHandler();
                    Handler.Handle(context);
                }
                else {
                    var Handler = new RootRequestHandler();
                    Handler.Handle(context);
                }

            }
            else if (httpUrl.IndexOf("/proxym3u8") == 0)
            {
                var Handler = new ProxyM3u8();
                Handler.Handle(context, true);

            }
            else
            {
                if (httpUrl.StartsWith("/parserlink"))
                {
                        var Handler = new ParseLinkRequestHandler();
                        Handler.Handle(context, true);
                }
                else
                {
                    if (httpUrl.StartsWith("/test"))
                    {
                        var Handler = new TestRequestHandler();
                        Handler.Handle(context, true);
                    }
                }

            }
            return true;
        }
    }
}