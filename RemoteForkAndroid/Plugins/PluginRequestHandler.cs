using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Unosquare.Labs.EmbedIO;
using tv.forkplayer.remotefork.server;
using Android.Net;
using System.Collections.Specialized;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace RemoteForkAndroid.Plugins
{
    internal class ResponseSerializer
    {
        public static string ToM3U(Item[] items)
        {
            var sb = new StringBuilder("#EXTM3U");

            foreach (var item in items)
            {
                sb.AppendFormat(
                    @"{0}#EXTINF:0,{1}{0}{2}",
                    System.Environment.NewLine,
                    item.Name,
                    item.Link
                );
            }

            return sb.ToString();
        }

        public static string ToJson(Playlist pluginResponse)
        {
            var json = new JObject();

            if (pluginResponse.Items != null && pluginResponse.Items.Any())
            {
                var channels = new JArray();

                foreach (var item in pluginResponse.Items)
                {
                    var channel = new JObject
                    {
                        ["title"] = item.Name ?? string.Empty,
                        ["description"] = item.Description ?? string.Empty,
                        ["logo_30x30"] = item.ImageLink ?? string.Empty
                    };

                    switch (item.Type)
                    {
                        case ItemType.DIRECTORY:
                            channel["playlist_url"] = item.Link ?? string.Empty;
                            break;
                        case ItemType.FILE:
                            channel["stream_url"] = item.Link ?? string.Empty;
                            break;
                        case ItemType.SEARCH:
                            channel["playlist_url"] = item.Link ?? string.Empty;
                            channel["search_on"] = item.Description ?? string.Empty;
                            break;
                    }

                    channels.Add(channel);
                }

                json["next_page_url"] = pluginResponse.NextPageUrl ?? string.Empty;
                if (pluginResponse.Timeout != null) json["timeout"] = pluginResponse.Timeout ?? string.Empty;
                if (pluginResponse.IsIptv != null) json["is_iptv"] = pluginResponse.IsIptv ?? string.Empty;
                json["channels"] = channels;
            }

            return json.ToString();
        }

        public static string ToXml(Playlist pluginResponse)
        {
            var items = ItemsToXml(pluginResponse.Items);
            if (pluginResponse.Timeout != null) items.AddFirst(new XElement("timeout", new XCData(pluginResponse.Timeout ?? string.Empty)));
            if (pluginResponse.IsIptv != null) items.AddFirst(new XElement("is_iptv", new XCData(pluginResponse.IsIptv ?? string.Empty)));
            items.AddFirst(new XElement("next_page_url", new XCData(pluginResponse.NextPageUrl ?? string.Empty)));
            if (pluginResponse.GetInfo != null) items.AddFirst(new XElement("get_info", new XCData(pluginResponse.GetInfo ?? string.Empty)));

            return XmlToString(
                new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    items
                )
            );
        }

        public static string ToXml(Item[] items)
        {
            return XmlToString(
                new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    ItemsToXml(items)
                )
            );
        }

        private static string XmlToString(XDocument doc)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                doc.WriteTo(writer);
            }

            return builder.ToString();
        }

        private static XElement ItemsToXml(Item[] items)
        {
            var xmlItems = new XElement("items");

            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    var channel = new XElement("channel");

                    channel.Add(new XElement("title", new XCData(item.Name ?? string.Empty)));
                    channel.Add(new XElement("description", new XCData(item.Description ?? string.Empty)));
                    channel.Add(new XElement("logo_30x30", new XCData(item.ImageLink ?? string.Empty)));
                    if (item.GetInfo != null) channel.Add(new XElement("get_info", new XCData(item.GetInfo ?? string.Empty)));
                    if (item.SearchOn != null) channel.Add(new XElement("search_on", new XCData(item.SearchOn ?? string.Empty)));

                    switch (item.Type)
                    {
                        case ItemType.DIRECTORY:
                            channel.Add(new XElement("playlist_url", new XCData(item.Link ?? string.Empty)));
                            break;
                        case ItemType.FILE:
                            channel.Add(new XElement("stream_url", new XCData(item.Link ?? string.Empty)));
                            break;
                        case ItemType.SEARCH:
                            channel.Add(new XElement("playlist_url", new XCData(item.Link ?? string.Empty)));
                            channel.Add(new XElement("search_on", new XCData(item.Description ?? string.Empty)));
                            break;
                    }

                    xmlItems.Add(channel);
                }
            }

            return xmlItems;
        }
    }
    internal interface IRequestHandler
    {
        void Handle(HttpListenerContext context);
    }
    internal abstract class BaseRequestHandler : IRequestHandler
    {

        public virtual void Handle(HttpListenerContext context)
        {
            Console.WriteLine("SET DEFAULT HEADERS");
            SetDefaultReponseHeaders(context.Response);
            try
            {
                Handle(context.Request, context.Response);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public virtual void Handle(HttpListenerContext context, bool datatype)
        {
            if (!datatype)
            {
                SetDefaultReponseHeaders(context.Response);
            }
            else Console.WriteLine("NO DEFAULT HEADERS");
            Handle(context.Request, context.Response);
        }
        public virtual void Handle(HttpListenerRequest request, HttpListenerResponse response) { }

        protected virtual void SetDefaultReponseHeaders(HttpListenerResponse response)
        {
            response.SendChunked = false;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = Constants.DefaultMimeTypes[".html"];
        }

        protected static string GetHostUrl(HttpListenerRequest request)
        {
            return new UriBuilder
            {
                Scheme = request.Url.Scheme,
                Host = request.Url.Host,
                Port = request.Url.Port,
                Path = "/",
            }.ToString();
        }

        internal static string CreateUrl(HttpListenerRequest request, string path, NameValueCollection query = null)
        {
            Console.WriteLine("path="+path);
            return new UriBuilder
            {
                Scheme = request.Url.Scheme,
                Host = request.Url.Host,
                Port = request.Url.Port,
                Path = WebUtility.UrlDecode(path),
                Query = HttpUtility.QueryParametersToString(query)
            }.ToString();
        }

        internal static void WriteResponse(HttpListenerResponse response, string responseText)
        {

            using (var writer = new StreamWriter(response.OutputStream))
            {
                writer.Write(responseText);
            }
        }

        internal static void WriteResponse(HttpListenerResponse response, HttpStatusCode status, string responseText)
        {
            response.StatusCode = Convert.ToInt32(status);

            WriteResponse(response, responseText);
        }
    }

    internal class PluginRequestHandler : BaseRequestHandler
    {
       
        internal static readonly string ParamPluginKey = "plugin";

        internal static readonly Regex PluginParamRegex = new Regex($@"{ParamPluginKey}(\w+)[\\]?", RegexOptions.Compiled);

        public override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            var pluginKey = ParsePluginKey(request);

            if (!string.IsNullOrEmpty(pluginKey))
            {
                var plugin = PluginManager.Instance.GetPlugin(pluginKey);

                if (plugin != null)
                {
                    Console.WriteLine("Execute: "+plugin.Name);

                    var pluginResponse = plugin.Instance.GetList(new PluginContext(pluginKey, request, new NameValueCollection(request.QueryString)));

                    if (pluginResponse != null)
                    {
                        WriteResponse(response, ResponseSerializer.ToXml(pluginResponse));
                    }
                    else
                    {

                        WriteResponse(response, HttpStatusCode.NotFound, $"Plugin Playlist is null. Plugin: {pluginKey}");
                    }
                }
                else
                {
                    Console.WriteLine("Plugin Not Found. Plugin:" +pluginKey);

                    WriteResponse(response, HttpStatusCode.NotFound, $"Plugin Not Found. Plugin: {pluginKey}");
                }
            }
            else
            {
                Console.WriteLine("Plugin is not defined in request. Plugin: "+pluginKey);

                WriteResponse(response, HttpStatusCode.NotFound, $"Plugin is not defined in request. Plugin: {pluginKey}");
            }
        }

        private static string ParsePluginKey(HttpListenerRequest request)
        {
            var pluginParam = request.QueryString.GetValues(null)?.FirstOrDefault(s => PluginParamRegex.IsMatch(s ?? string.Empty));

            var pluginParamMatch = PluginParamRegex.Match(pluginParam ?? string.Empty);

            return pluginParamMatch.Success ? pluginParamMatch.Groups[1].Value : string.Empty;
        }

        internal static string CreatePluginUrl(HttpListenerRequest request, string pluginName, NameValueCollection parameters = null)
        {
            var query = new NameValueCollection
            {
                [null] = string.Concat(ParamPluginKey, pluginName, Path.DirectorySeparatorChar, ".xml")
            };
            string url = request.Url.OriginalString.Substring(7);
            query.Set("host", url.Substring(0, url.IndexOf("/")));
            if (parameters != null)
            {
                query.Add(parameters);
            }

            return CreateUrl(request, RootRequestHandler.TreePath, query);
        }
    }
}