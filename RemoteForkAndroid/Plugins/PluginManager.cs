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
using System.IO;
using System.Security.Cryptography;
using static Android.Resource;
using Android.Renderscripts;

namespace RemoteForkAndroid.Plugins
{
    internal class PluginManager
    {

        public static readonly PluginManager Instance = new PluginManager();

        private readonly Dictionary<string, PluginInstance> plugins = new Dictionary<string, PluginInstance>();

        private PluginManager()
        {
            LoadPlugins();
        }

      
        private void LoadPlugins()
        {
            var p = new AceTorrentPlay();
            var plugin = new PluginInstance(p);
            plugins.Add(p.Id, plugin);

            var p2 = new RuTracker();
            var plugin2 = new PluginInstance(p2);
            plugins.Add(p2.Id, plugin2);
        }            
        

        private static string GetChecksum(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        public void ReimportPlugins()
        {
            plugins.Clear();
            LoadPlugins();
        }

        public void RemovePlugin(string name)
        {
            if (plugins.ContainsKey(name))
                plugins.Remove(name);
        }

        public Dictionary<string, PluginInstance> GetPlugins(bool filtering = true)
        {          
            return plugins;
        }

        public PluginInstance GetPlugin(string id)
        {
            if (plugins.ContainsKey(id))
            {
               return plugins[id];
            }
            return null;
        }
    }
}