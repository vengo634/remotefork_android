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
using System.Reflection;

namespace RemoteForkAndroid.Plugins
{
    internal class PluginInstance
    {
        public readonly string Author;
        public readonly string Description;
        public readonly string Id;
        public readonly string ImageLink;
        public readonly string Name;
        public readonly string Version;

        private IPlugin instance;

        public PluginInstance(IPlugin attribute)
        {
           // Key = key;
            Id = attribute.Id;
            Name = attribute.Name;
            Description = attribute.Description;
            ImageLink = attribute.ImageLink;
            Version = attribute.Version;
            Author = attribute.Author;
            instance = attribute;
        }

        public IPlugin Instance => instance;

        public override string ToString()
        {
            return $"{Name} {Version} ({Author})";
        }
    }
}