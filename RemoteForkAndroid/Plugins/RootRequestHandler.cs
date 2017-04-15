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
using tv.forkplayer.remotefork.server;
using System.Collections.Specialized;

namespace RemoteForkAndroid.Plugins
{
    internal class RootRequestHandler : BaseRequestHandler
    {

        internal static readonly string TreePath = "/treeview";

        internal static readonly string RootPath = "/";

        public override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {

            try
            {
                var result = new List<Item>();
            
                /*
                var drives = DriveInfo.GetDrives();

                foreach (var drive in drives)
                {
                    try
                    {
                        if (drive.IsReady)
                        {
                            string mainText = $"{drive.Name} ({Tools.FSize(drive.AvailableFreeSpace)} свободно из {Tools.FSize(drive.TotalSize)})";
                            string subText = $"<br>Метка диска: {drive.VolumeLabel}<br>Тип носителя: {drive.DriveType}";

                            result.Add(new Item
                            {
                                Name = mainText + subText,
                                Link = CreateUrl(
                                               request,
                                               TreePath,
                                               new NameValueCollection { [null] = new Uri(drive.Name).AbsoluteUri+"/" }
                                           ),
                                Type = ItemType.DIRECTORY
                            });

                            Console.WriteLine($"Drive: {mainText}{subText}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
        */
            foreach (var plugin in PluginManager.Instance.GetPlugins())
            {
                result.Add(
                    new Item
                    {
                        Name = plugin.Value.Name,
                        Link = PluginRequestHandler.CreatePluginUrl(request, plugin.Key),
                        ImageLink = plugin.Value.ImageLink,
                        Type = ItemType.DIRECTORY
                    }
                );

            }

           
            WriteResponse(response, ResponseSerializer.ToM3U(result.ToArray())); }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
          
        }
    }
}