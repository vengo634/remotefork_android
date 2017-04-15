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
using System.Collections.Specialized;
using System.Net;
using tv.forkplayer.remotefork.server;

namespace RemoteForkAndroid.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute
    {
        public string Id;
        public string Name;
        public string ImageLink;
        public string Description;
        public string Version;
        public string Author;
    }
    public class Playlist
    {
        public static readonly Playlist EmptyPlaylist = new Playlist
        {
            Items = new Item[0]
        };

        public string GetInfo { get; set; }
        public string NextPageUrl { get; set; }
        public string IsIptv { get; set; }
        public string Timeout { get; set; }
        public Item[] Items { get; set; }
    }
    public class Item
    {
        public string Name;
        public string Link;
        public string ImageLink;
        public string Description;
        public string GetInfo;
        public string SearchOn;
        public ItemType Type = ItemType.DIRECTORY;

        public Item()
        {
        }

        public Item(Item item)
        {

            Name = item.Name;
            Link = item.Link;
            GetInfo = item.GetInfo;
            SearchOn = item.SearchOn;
            ImageLink = item.ImageLink;
            Description = item.Description;
            Type = item.Type;
        }
    }

    public enum ItemType
    {
        DIRECTORY = 0,
        FILE = 1,
        SEARCH = 2
    }
    public interface IPlugin
    {
        string Id { get; set; }
        string Version { get; set; }
        string Author { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string ImageLink { get; set; }
        Playlist GetList(IPluginContext context);
    }
    public interface ILogger
    {
        void Info(string message);

        void Info(string format, params object[] args);

        void Error(string message);

        void Error(string format, params object[] args);

        void Debug(string message);

        void Debug(string format, params object[] args);

    }
    public interface IHTTPClient
    {
        string GetRequest(string link, Dictionary<string, string> header = null);

        string PostRequest(string link, string data, Dictionary<string, string> header = null);
    }
    public interface IPluginContext
    {
        NameValueCollection GetRequestParams();

        string CreatePluginUrl(NameValueCollection parameters);

        IHTTPClient GetHttpClient();

        ILogger GetLogger();
    }

    internal class PluginContext : IPluginContext
    {
        private readonly IHTTPClient _httpClient = new HttpClient();

        private readonly ILogger _logger;

        private readonly string _pluginName;
        private readonly HttpListenerRequest _request;
        private readonly NameValueCollection _requestParams;

        public PluginContext(string pluginName, HttpListenerRequest request, NameValueCollection requestParams)
        {
            _pluginName = pluginName;
            _request = request;
            _requestParams = requestParams;
            _logger = new Logger(pluginName);
        }

        public NameValueCollection GetRequestParams()
        {
            return _requestParams;
        }

        public string CreatePluginUrl(NameValueCollection parameters)
        {
            return PluginRequestHandler.CreatePluginUrl(_request, _pluginName, parameters);
        }

        public IHTTPClient GetHttpClient()
        {
            return _httpClient;
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        internal class HttpClient : IHTTPClient
        {
            public string GetRequest(string link, Dictionary<string, string> header = null)
            {
                return HttpUtility.GetRequest(link, header);
            }

            public string PostRequest(string link, string data, Dictionary<string, string> header = null)
            {
                return HttpUtility.PostRequest(link, data, header);
            }
        }

        internal class Logger : ILogger
        {

            public Logger(string pluginName)
            {
            }

            public void Info(string message)
            {
            }

            public void Info(string format, params object[] args)
            {
            }

            public void Error(string message)
            {
            }

            public void Error(string format, params object[] args)
            {
            }

            public void Debug(string message)
            {
            }

            public void Debug(string format, params object[] args)
            {
            }
        }
    }
}