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

namespace RemoteForkAndroid.Plugins
{
    [PluginAttribute(Id = "acetorrentplay", Version = "0.3", Author = "ORAMAN", Name = "AceTorrentPlay", Description = "Воспроизведение файлов TORRENT через меда-сервер Ace Stream", ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png")]
  
   public class AceTorrentPlay : IPlugin
    {

        public string Id { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public AceTorrentPlay()
        {
            Id="acetorrentplay";
            Version = "0.3";
            Author = "ORAMAN";
            Name = "AceTorrentPlay";
            Description = "Воспроизведение файлов TORRENT через меда-сервер Ace Stream";
            ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png";
        }

        private string IPAdress;
        private string PortRemoteFork = "8028";
        private string PLUGIN_PATH = "pluginPath";
        private string ProxyServr = "proxy.antizapret.prostovpn.org";
        private int ProxyPort = 3128;
        private bool ProxyEnabler = false; //Вкл/выкл прокси сервер
        private string TrackerServer = "http://nnmclub.to"; //  "http://nnm-club.me" -используемый адрес трекера, nnm-club.me работает через прокси
        private Playlist PlayList;
      
        public Playlist GetInfo(IPluginContext context)
        {
            var playlist = new Playlist();
            List<Item> items = new List<Item>();
            Item Item = new Item();
            Item.Name = "information";
            Item.Link = "2";
            Item.Type = ItemType.FILE;
            Item.Description = "peers:2<br>";
            items.Add(Item);
            playlist.Items = items.ToArray();
            return playlist;
        }

        public Playlist GetList(IPluginContext context)
        {
            IPAdress = context.GetRequestParams()["host"].Split(':')[0];
            PlayList = new Playlist();
            if (context.GetRequestParams()["search"] != null)
            {
                return SearchList(context, context.GetRequestParams()["search"]);
            }

            var path = context.GetRequestParams().Get(PLUGIN_PATH);
            path = ((((path == null)) ? "plugin" : "plugin;" + path));




            switch (path)
            {
                case "plugin":
                    return GetTopList(context);
                case "plugin;torrenttv":
                    return GetTorrentTV(context);
                case "plugin;nnmclub":
                    return GetTopNNMClubList(context);
            }




            string[] PathSpliter = path.Split(';');

            switch (PathSpliter[PathSpliter.Length - 1])
            {
                //Трекер
                case "PAGE":
                    return GetPage(context, PathSpliter[PathSpliter.Length - 2]);
                case "PAGEFILM":
                    return GetTorrentPage(context, PathSpliter[PathSpliter.Length - 2]);

                //Торрент тв
                case "ent":
                    return LastModifiedPlayList("ent", context);
                case "child":
                    return LastModifiedPlayList("child", context);
                case "common":
                    return LastModifiedPlayList("common", context);
                case "discover":
                    return LastModifiedPlayList("discover", context);
                case "HD":
                    return LastModifiedPlayList("HD", context);
                case "film":
                    return LastModifiedPlayList("film", context);
                case "man":
                    return LastModifiedPlayList("man", context);
                case "music":
                    return LastModifiedPlayList("music", context);
                case "news":
                    return LastModifiedPlayList("news", context);
                case "region":
                    return LastModifiedPlayList("region", context);
                case "relig":
                    return LastModifiedPlayList("relig", context);
                case "sport":
                    return LastModifiedPlayList("sport", context);

                //Взрослый контент
                case "porn":
                    return LastModifiedPlayList("porn", context);
                case "all":
                    return LastModifiedPlayList("all", context);
            }

            string PathFiles = ((string)(PathSpliter[PathSpliter.Length - 1])).Replace("|", "\\");
            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();

            switch (System.IO.Path.GetExtension(PathFiles))
            {

                case ".torrent":
                    {
                        TorrentPlayList[] PlayListtoTorrent = GetFileListJSON(PathFiles, IPAdress);
                        string Description = SearchDescriptions(System.IO.Path.GetFileNameWithoutExtension(PathFiles.Split('(', '.', '[', '|')[0]));

                        foreach (TorrentPlayList PlayListItem in PlayListtoTorrent)
                        {
                            Item Item = new Item();
                            Item.Name = PlayListItem.Name;
                            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png";
                            Item.Link = PlayListItem.Link;
                            Item.Type = ItemType.FILE;
                            Item.Description = Description;
                            items.Add(Item);
                        }


                        return PlayListPlugPar(items, context);

                    }
                case ".m3u":
                    {
                        Item Item = new Item();
                        System.Net.WebClient WC = new System.Net.WebClient();
                        WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
                        WC.Encoding = System.Text.Encoding.UTF8;
                        Item.Type = ItemType.DIRECTORY;
                        Item.Description = WC.DownloadString(PathFiles);
                        items.Add(Item);
                        return PlayListPlugPar(items, context);
                    }
            }



            string[] ListFolders = System.IO.Directory.GetDirectories(PathFiles);
            foreach (string Fold in ListFolders)
            {
                Item Item = new Item();
                Item.Name = System.IO.Path.GetFileName(Fold);
                Item.Link = Fold.Replace("\\", "|");
                Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
                Item.Type = ItemType.DIRECTORY;
                items.Add(Item);
            }

            if (AceProxEnabl == true)
            {
                foreach (string File in System.IO.Directory.EnumerateFiles(PathFiles, "*.*", System.IO.SearchOption.TopDirectoryOnly).Where((s) => s.EndsWith(".torrent")))
                {
                    Item Item = new Item();
                    Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png";
                    Item.Name = System.IO.Path.GetFileNameWithoutExtension(File);
                    Item.Link = File.Replace("\\", "|");
                    Item.Description = Item.Name;
                    Item.Type = ItemType.DIRECTORY;
                    items.Add(Item);
                }
            }

            foreach (string File in System.IO.Directory.EnumerateFiles(PathFiles, "*.*", System.IO.SearchOption.TopDirectoryOnly).Where((s) => s.EndsWith(".mkv") || s.EndsWith(".avi") || s.EndsWith(".mp4")))
            {
                Item Item = new Item();
                Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png";
                Item.Name = System.IO.Path.GetFileNameWithoutExtension(File);
                Item.Link = ((string)("http://" + IPAdress + ":" + PortRemoteFork + "/?file:/" + File)).Replace("\\", "/");
                Item.Description = Item.Link;
                Item.Type = ItemType.FILE;
                items.Add(Item);
            }

            foreach (string File in System.IO.Directory.EnumerateFiles(PathFiles, "*.*", System.IO.SearchOption.TopDirectoryOnly).Where((s) => s.EndsWith(".mp3")))
            {
                Item Item = new Item();
                Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597240aimp.png";
                Item.Name = System.IO.Path.GetFileNameWithoutExtension(File);
                Item.Link = ((string)("http://" + IPAdress + ":" + PortRemoteFork + "/?file:/" + File)).Replace("\\", "/");
                Item.Description = Item.Link;
                Item.Type = ItemType.FILE;
                items.Add(Item);
            }

            foreach (string File in System.IO.Directory.EnumerateFiles(PathFiles, "*.*", System.IO.SearchOption.TopDirectoryOnly).Where((s) => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".gif") || s.EndsWith(".bmp")))
            {
                Item Item = new Item();
                Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597278imagefile.png";
                Item.Name = System.IO.Path.GetFileNameWithoutExtension(File);
                Item.Link = ((string)("http://" + IPAdress + ":" + PortRemoteFork + "/?file:/" + File)).Replace("\\", "/");
                Item.Description = Item.Link;
                Item.Type = ItemType.FILE;
                items.Add(Item);
            }

            foreach (string File in System.IO.Directory.EnumerateFiles(PathFiles, "*.*", System.IO.SearchOption.TopDirectoryOnly).Where((s) => s.EndsWith(".m3u")))
            {
                Item Item = new Item();
                Item.ImageLink = "http://s1.iconbird.com/ico/0912/VannillACreamIconSet/w128h1281348320736M3U.png";
                Item.Name = System.IO.Path.GetFileNameWithoutExtension(File);
                Item.Link = ((string)("http://" + IPAdress + ":" + PortRemoteFork + "/?file:/" + File)).Replace("\\", "/");
                Item.Description = Item.Link;
                Item.Type = ItemType.DIRECTORY;
                items.Add(Item);
            }

            return PlayListPlugPar(items, context);

        }

        public Playlist PlayListPlugPar(System.Collections.Generic.List<Item> items, IPluginContext context, string next_page_url = "")
        {

            if (next_page_url != "")
            {
                var pluginParams = new NameValueCollection();
                pluginParams[PLUGIN_PATH] = next_page_url;
                PlayList.NextPageUrl = context.CreatePluginUrl(pluginParams);
            }
            PlayList.Timeout = "60"; //sec

            PlayList.Items = items.ToArray();
            foreach (Item Item in PlayList.Items)
            {
                if (ItemType.DIRECTORY == Item.Type)
                {
                    var pluginParams = new NameValueCollection();
                    pluginParams[PLUGIN_PATH] = Item.Link;
                    Item.Link = context.CreatePluginUrl(pluginParams);
                }
            }
            return PlayList;
        }

        public Playlist GetTopList(IPluginContext context)
        {
            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();

            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;

            Item ItemTop = new Item();
            Item ItemTorrentTV = new Item();
            Item ItemNNMClub = new Item();
            try
            {
                AceProxEnabl = true;
                string AceMadiaGet = null;
                AceMadiaGet = WC.DownloadString("http://" + IPAdress + ":" + PortAce + "/webui/api/service?method=get_version&format=jsonp&callback=mycallback");
                AceMadiaGet = " Ответ от движка Ace Media получен: " + "<div>" + AceMadiaGet + "</div></html>";


                ItemTop.ImageLink = "http://static.acestream.net/sites/acestream/img/ACE-logo.png";
                ItemTop.Name = "        - AceTorrentPlay -        ";
                ItemTop.Link = "";
                ItemTop.Type = ItemType.FILE;
                ItemTop.Description = AceMadiaGet + "<p><p><img src=\"http://static.acestream.net/sites/acestream/img/ACE-logo.png\"></html>";

                ItemTorrentTV.Name = "Torrent TV";
                ItemTorrentTV.Type = ItemType.DIRECTORY;
                ItemTorrentTV.Link = "torrenttv";
                ItemTorrentTV.ImageLink = "http://s1.iconbird.com/ico/1112/Television/w256h25613523820647.png";

                if (System.IO.File.Exists(System.IO.Path.GetTempPath() + "MyTraf.tmp") == false)
                {
                    WC.DownloadFile("http://super-pomoyka.us.to/trash/ttv-list/MyTraf.php", System.IO.Path.GetTempPath() + "MyTraf.tmp");
                }
                ItemTorrentTV.Description = "<img src=\"http://torrent-tv.ru/images/logo.png\"></html>" + WC.DownloadString(System.IO.Path.GetTempPath() + "MyTraf.tmp");

                ItemNNMClub.ImageLink = "http://s1.iconbird.com/ico/0912/MorphoButterfly/w128h1281348669898RhetenorMorpho.png";
                ItemNNMClub.Name = "NoNaMe - Club";
                ItemNNMClub.Link = "nnmclub";
                ItemNNMClub.Type = ItemType.DIRECTORY;
                ItemNNMClub.Description = "<font face=\"Arial\" size=\"5\"><b>Трекер " + ItemNNMClub.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";


                items.Add(ItemTop);
                items.Add(ItemTorrentTV);
                items.Add(ItemNNMClub);


            }
            catch
            {
                AceProxEnabl = false;
                ItemTop.ImageLink = "http://errorfix48.ru/uploads/posts/2014-09/1409846068_400px-warning_icon.png";
                ItemTop.Name = "        - AceTorrentPlay -        ";
                ItemTop.Link = "";
                ItemTop.Type = ItemType.FILE;
                ItemTop.Description = "Ответ от движка Ace Media не получен!";
                items.Add(ItemTop);
            }

            System.IO.DriveInfo[] ListDisk = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo Disk in ListDisk)
            {
                if (Disk.DriveType == System.IO.DriveType.Fixed)
                {
                    Item Item = new Item();
                    Item.Name = Disk.Name + "  " + "(" + Math.Round(Disk.TotalFreeSpace / 1024 / 1024.0 / 1024, 2) + "ГБ свободно из " + Math.Round(Disk.TotalSize / 1024 / 1024.0 / 1024, 2) + "ГБ)";
                    Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597268hddwin.png";
                    Item.Link = Disk.Name.Replace("\\", "|");
                    Item.Type = ItemType.DIRECTORY;
                    Item.Description = Item.Name + "\n" + "\r" + " <p> Метка диска: " + Disk.VolumeLabel + "</html>";
                    items.Add(Item);
                }
            }


            return PlayListPlugPar(items, context);
        }

        #region NNM Club
        private string Cookies = "phpbb2mysql_4_data=a%3A2%3A%7Bs%3A11%3A%22autologinid%22%3Bs%3A32%3A%2296229c9a3405ae99cce1f3bc0cefce2e%22%3Bs%3A6%3A%22userid%22%3Bs%3A8%3A%2213287549%22%3B%7D";

        public Playlist SearchList(IPluginContext context, string search)
        {

            System.Net.WebRequest RequestPost = System.Net.WebRequest.Create(TrackerServer + "/forum/tracker.php");
            if (ProxyEnabler == true)
            {
                RequestPost.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
            }
            RequestPost.Method = "POST";
            RequestPost.ContentType = "text/html; charset=windows-1251";
            RequestPost.Headers.Add("Cookie", Cookies);
            RequestPost.ContentType = "application/x-www-form-urlencoded";
            System.IO.Stream myStream = RequestPost.GetRequestStream();
            string DataStr = "prev_sd=1&prev_a=1&prev_my=0&prev_n=0&prev_shc=0&prev_shf=0&prev_sha=0&prev_shs=0&prev_shr=0&prev_sht=0&f%5B%5D=724&f%5B%5D=725&f%5B%5D=729&f%5B%5D=731&f%5B%5D=733&f%5B%5D=730&f%5B%5D=732&f%5B%5D=230&f%5B%5D=659&f%5B%5D=658&f%5B%5D=231&f%5B%5D=660&f%5B%5D=661&f%5B%5D=890&f%5B%5D=232&f%5B%5D=734&f%5B%5D=742&f%5B%5D=735&f%5B%5D=738&f%5B%5D=967&f%5B%5D=907&f%5B%5D=739&f%5B%5D=1109&f%5B%5D=736&f%5B%5D=737&f%5B%5D=898&f%5B%5D=935&f%5B%5D=871&f%5B%5D=973&f%5B%5D=960&f%5B%5D=1239&f%5B%5D=740&f%5B%5D=741&f%5B%5D=216&f%5B%5D=270&f%5B%5D=218&f%5B%5D=219&f%5B%5D=954&f%5B%5D=888&f%5B%5D=217&f%5B%5D=266&f%5B%5D=318&f%5B%5D=320&f%5B%5D=677&f%5B%5D=1177&f%5B%5D=319&f%5B%5D=678&f%5B%5D=885&f%5B%5D=908&f%5B%5D=909&f%5B%5D=910&f%5B%5D=911&f%5B%5D=912&f%5B%5D=220&f%5B%5D=221&f%5B%5D=222&f%5B%5D=882&f%5B%5D=889&f%5B%5D=224&f%5B%5D=225&f%5B%5D=226&f%5B%5D=227&f%5B%5D=891&f%5B%5D=682&f%5B%5D=694&f%5B%5D=884&f%5B%5D=1211&f%5B%5D=693&f%5B%5D=913&f%5B%5D=228&f%5B%5D=1150&f%5B%5D=254&f%5B%5D=321&f%5B%5D=255&f%5B%5D=906&f%5B%5D=256&f%5B%5D=257&f%5B%5D=258&f%5B%5D=883&f%5B%5D=955&f%5B%5D=905&f%5B%5D=271&f%5B%5D=1210&f%5B%5D=264&f%5B%5D=265&f%5B%5D=272&f%5B%5D=1262&f%5B%5D=1219&f%5B%5D=1221&f%5B%5D=1220&f%5B%5D=768&f%5B%5D=779&f%5B%5D=778&f%5B%5D=788&f%5B%5D=1288&f%5B%5D=787&f%5B%5D=1196&f%5B%5D=1141&f%5B%5D=777&f%5B%5D=786&f%5B%5D=803&f%5B%5D=776&f%5B%5D=785&f%5B%5D=1265&f%5B%5D=1289&f%5B%5D=774&f%5B%5D=775&f%5B%5D=1242&f%5B%5D=1140&f%5B%5D=782&f%5B%5D=773&f%5B%5D=1142&f%5B%5D=784&f%5B%5D=1195&f%5B%5D=772&f%5B%5D=771&f%5B%5D=783&f%5B%5D=1144&f%5B%5D=804&f%5B%5D=1290&f%5B%5D=770&f%5B%5D=922&f%5B%5D=780&f%5B%5D=781&f%5B%5D=769&f%5B%5D=799&f%5B%5D=800&f%5B%5D=791&f%5B%5D=798&f%5B%5D=797&f%5B%5D=790&f%5B%5D=793&f%5B%5D=794&f%5B%5D=789&f%5B%5D=796&f%5B%5D=792&f%5B%5D=795&f%5B%5D=713&f%5B%5D=706&f%5B%5D=577&f%5B%5D=894&f%5B%5D=578&f%5B%5D=580&f%5B%5D=579&f%5B%5D=953&f%5B%5D=581&f%5B%5D=806&f%5B%5D=714&f%5B%5D=761&f%5B%5D=809&f%5B%5D=924&f%5B%5D=812&f%5B%5D=576&f%5B%5D=590&f%5B%5D=591&f%5B%5D=588&f%5B%5D=823&f%5B%5D=589&f%5B%5D=598&f%5B%5D=652&f%5B%5D=596&f%5B%5D=600&f%5B%5D=819&f%5B%5D=599&f%5B%5D=956&f%5B%5D=959&f%5B%5D=597&f%5B%5D=594&f%5B%5D=593&f%5B%5D=595&f%5B%5D=582&f%5B%5D=587&f%5B%5D=583&f%5B%5D=584&f%5B%5D=586&f%5B%5D=585&f%5B%5D=614&f%5B%5D=603&f%5B%5D=1287&f%5B%5D=1282&f%5B%5D=1206&f%5B%5D=1200&f%5B%5D=1194&f%5B%5D=1062&f%5B%5D=974&f%5B%5D=609&f%5B%5D=1263&f%5B%5D=951&f%5B%5D=975&f%5B%5D=608&f%5B%5D=607&f%5B%5D=606&f%5B%5D=750&f%5B%5D=605&f%5B%5D=604&f%5B%5D=950&f%5B%5D=610&f%5B%5D=613&f%5B%5D=612&f%5B%5D=655&f%5B%5D=653&f%5B%5D=654&f%5B%5D=611&f%5B%5D=656&f%5B%5D=615&f%5B%5D=616&f%5B%5D=617&f%5B%5D=619&f%5B%5D=620&f%5B%5D=623&f%5B%5D=622&f%5B%5D=635&f%5B%5D=621&f%5B%5D=632&f%5B%5D=643&f%5B%5D=624&f%5B%5D=627&f%5B%5D=626&f%5B%5D=636&f%5B%5D=625&f%5B%5D=633&f%5B%5D=644&f%5B%5D=628&f%5B%5D=631&f%5B%5D=630&f%5B%5D=637&f%5B%5D=629&f%5B%5D=634&f%5B%5D=642&f%5B%5D=645&f%5B%5D=639&f%5B%5D=640&f%5B%5D=648&f%5B%5D=638&f%5B%5D=646&f%5B%5D=695&o=10&s=2&tm=-1&a=1&sd=1&ta=-1&sns=-1&sds=-1&nm=" + search + "&pn=&submit=Поиск";
            byte[] DataByte = Encoding.GetEncoding("windows-1251").GetBytes(DataStr);
            myStream.Write(DataByte, 0, DataByte.Length);
            myStream.Close();

            System.Net.WebResponse Response = RequestPost.GetResponse();
            System.IO.Stream dataStream = Response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream, Encoding.GetEncoding("windows-1251"));
            string ResponseFromServer = reader.ReadToEnd();


            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<tr class=\"prow).*?(</tr>)");
            System.Text.RegularExpressions.MatchCollection Result = Regex.Matches(ResponseFromServer.Replace("\n", "   "));

            if (Result.Count > 0)
            {

                foreach (System.Text.RegularExpressions.Match Match in Result)
                {
                    Regex = new System.Text.RegularExpressions.Regex("(?<=href=\").*?(?=&amp;)");
                    Item Item = new Item();
                    Item.Link = TrackerServer + "/forum/" + Regex.Matches(Match.Value)[0].Value + ";PAGEFILM";
                    Regex = new System.Text.RegularExpressions.Regex("(?<=\"><b>).*?(?=</b>)");
                    Item.Name = Regex.Matches(Match.Value)[0].Value;
                    Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png";
                    Item.Description = GetDescriptionSearhTorrent(Match.Value);
                    items.Add(Item);
                }
            }
            else
            {
                Item Item = new Item();
                Item.Name = "Ничего не найдено";
                Item.Link = "";

                items.Add(Item);
            }

            return PlayListPlugPar(items, context);
        }

        public string GetDescriptionSearhTorrent(string HTML)
        {

            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=\"><b>).*?(?=</b>)");
            var NameFilm = Regex.Matches(HTML)[0].Value;

            Regex = new System.Text.RegularExpressions.Regex("(?<=</u>).*?(?=</td>)");
            string SizeFile = "<p> Размер: <b>" + Regex.Matches(HTML)[0].Value + "</b>";

            string DobavlenFile = "<p> Добавлен: <b>" + Regex.Matches(HTML)[1].Value.Replace("<br>", " ") + "</b>";

            Regex = new System.Text.RegularExpressions.Regex("(?<=class=\"seedmed\">).*?(?=</td>)");
            string Seeders = "<p> Seeders: <b> " + Regex.Matches(HTML)[0].Value + "</b>";

            Regex = new System.Text.RegularExpressions.Regex("(?<=ass=\"leechmed\">).*?(?=</td>)");
            string Leechers = "<p> Leechers: <b> " + Regex.Matches(HTML)[0].Value + "</b>";
            return "<font face=\"Arial\" size=\"5\"><b>" + NameFilm + "</font></b><p><font face=\"Arial Narrow\" size=\"4\">" + SizeFile + DobavlenFile + Seeders + Leechers + "</font></html>";
        }

        public Playlist GetTopNNMClubList(IPluginContext context)
        {

            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
            Item Item = new Item();

            Item.Name = "Поиск";
            Item.Link = "http";
            Item.Type = ItemType.DIRECTORY;
            Item.SearchOn = "search_on";
            Item.ImageLink = "http://s1.iconbird.com/ico/0612/MustHave/w256h2561339195991Search256x256.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Новинки кино";
            Item.Link = TrackerServer + "/forum/portal.php?c=10;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Наше кино";
            Item.Link = TrackerServer + "/forum/portal.php?c=13;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Зарубежное кино";
            Item.Link = TrackerServer + "/forum/portal.php?c=6;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "HD (3D) Кино";
            Item.Link = TrackerServer + "/forum/portal.php?c=11;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Артхаус";
            Item.Link = TrackerServer + "/forum/portal.php?c=17;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Наши сериалы";
            Item.Link = TrackerServer + "/forum/portal.php?c=4;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Зарубежные сериалы";
            Item.Link = TrackerServer + "/forum/portal.php?c=3;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Театр, МузВидео, Разное";
            Item.Link = TrackerServer + "/forum/portal.php?c=21;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Док. TV-бренды";
            Item.Link = TrackerServer + "/forum/portal.php?c=22;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Док. и телепередачи";
            Item.Link = TrackerServer + "/forum/portal.php?c=23;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Спорт и Юмор";
            Item.Link = TrackerServer + "/forum/portal.php?c=24;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            Item = new Item();
            Item.Name = "Аниме и Манга";
            Item.Link = TrackerServer + "/forum/portal.php?c=1;PAGE";
            Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png";
            Item.Description = "<font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"http://assets.nnm-club.ws/forum/images/logos/10let8.png\" />";
            items.Add(Item);

            return PlayListPlugPar(items, context);
        }

        public Playlist GetPage(IPluginContext context, string URL)
        {

            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
            string next_page_url = "";
            try
            {
                System.Net.WebRequest RequestGet = System.Net.WebRequest.Create(URL);
                if (ProxyEnabler == true)
                {
                    RequestGet.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
                }
                RequestGet.Method = "GET";
                RequestGet.ContentType = "text/html; charset=windows-1251";
                RequestGet.Headers.Add("Cookie", Cookies);

                System.Net.WebResponse Response2 = RequestGet.GetResponse();
                System.IO.Stream dataStream = Response2.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream, System.Text.Encoding.GetEncoding(1251));
                string responseFromServer = reader.ReadToEnd();

                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<td class=\"pcatHead\"><img class=\"picon\").*?(\" /></span>)");


                foreach (System.Text.RegularExpressions.Match MAtch in Regex.Matches(responseFromServer.Replace("\n", "   ")))
                {
                    Regex = new System.Text.RegularExpressions.Regex("(?<=title=\").*?(?=\">)");
                    Item Item = new Item();
                    Item.Name = Regex.Matches(MAtch.Value)[1].Value;

                    Regex = new System.Text.RegularExpressions.Regex("(?<=<var class=\"portalImg\" title=\").*?(?=\">)");
                    Item.ImageLink = Regex.Matches(MAtch.Value)[0].Value;

                    Item.ImageLink = "http://" + IPAdress + ":"+PortRemoteFork+"/proxym3u8B" + Base64Encode(Item.ImageLink + "OPT:ContentType--image/jpegOPEND:/") + "/";

                    Regex = new System.Text.RegularExpressions.Regex("(?<=<a class=\"pgenmed\" href=\").*?(?=&)");
                    Item.Link = TrackerServer + "/forum/" + Regex.Matches(MAtch.Value)[0].Value + ";PAGEFILM";

                    Regex = new System.Text.RegularExpressions.Regex("(?<=<a class=\"pgenmed\" href=\").*?(?=&)");
                    Item.Description = FormatDescription(MAtch.Value, Item.ImageLink);


                    items.Add(Item);
                }

                Regex = new System.Text.RegularExpressions.Regex("(?<=&nbsp;&nbsp;<a href=\").*?(?=sid=)");
                System.Text.RegularExpressions.MatchCollection Rzult = Regex.Matches(responseFromServer);

                /*
                Item ItemNext = new Item();
                ItemNext.Name = ">> СЛЕДУЯЩАЯ СТРАНИЦА >>";
                ItemNext.Link = TrackerServer + "/forum/" + Rzult[Rzult.Count - 1].Value.Replace("amp;", "") + ";PAGE";
                ItemNext.Description = ItemNext.Link;
                ItemNext.ImageLink = "http://files.lib.byu.edu/exhibits/the-great-war/arrow-right-big.png";
                ItemNext.Type = ItemType.DIRECTORY;

                items.Add(ItemNext);

                */
                next_page_url = TrackerServer + "/forum/" + Rzult[Rzult.Count - 1].Value.Replace("amp;", "") + ";PAGE";
            }
            catch (Exception ex)
            {
                Item Item = new Item();
                Item.Name = "ERROR";
                Item.Description = ex.Message;
                Item.Link = "plugin";
                items.Add(Item);
            }

            PlayList.IsIptv = "false";
            return PlayListPlugPar(items, context, next_page_url);

        }

        public Playlist GetTorrentPage(IPluginContext context, string URL)
        {
            PlayList.IsIptv = "false";
            System.Net.WebRequest RequestGet = System.Net.WebRequest.Create(URL);
            if (ProxyEnabler == true)
            {
                RequestGet.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
            }
            RequestGet.Method = "GET";
            RequestGet.Headers.Add("Cookie", Cookies);

            System.Net.WebResponse Response = RequestGet.GetResponse();
            System.IO.Stream dataStream = Response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream, Encoding.GetEncoding("windows-1251"));
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            Response.Close();

            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=<span class=\"genmed\"><b><a href=\").*?(?=&amp;)");
            string TorrentPath = TrackerServer + "/forum/" + Regex.Matches(responseFromServer)[0].Value;
            System.Net.WebRequest RequestTorrent = System.Net.WebRequest.Create(TorrentPath);
            if (ProxyEnabler == true)
            {
                RequestTorrent.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
            }
            RequestTorrent.Method = "GET";
            RequestTorrent.Headers.Add("Cookie", Cookies);

            Response = RequestTorrent.GetResponse();
            dataStream = Response.GetResponseStream();
            reader = new System.IO.StreamReader(dataStream, Encoding.GetEncoding("windows-1251"));
            string FileTorrent = reader.ReadToEnd();
            System.IO.File.WriteAllText(System.IO.Path.GetTempPath() + "TorrentTemp.torrent", FileTorrent, Encoding.GetEncoding("windows-1251"));
            reader.Close();
            dataStream.Close();
            Response.Close();


            TorrentPlayList[] PlayListtoTorrent = GetFileListJSON(System.IO.Path.GetTempPath() + "TorrentTemp.torrent", IPAdress);


            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();

            foreach (TorrentPlayList PlayListItem in PlayListtoTorrent)
            {
                Item Item = new Item();
                Item.Name = PlayListItem.Name;
                Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png";
                Item.Link = PlayListItem.Link;
                Item.Type = ItemType.FILE;
                Item.Description = FormatDescriptionFile(responseFromServer);
                items.Add(Item);
            }

            return PlayListPlugPar(items, context);
        }

        public string FormatDescriptionFile(string HTML)
        {

            HTML = HTML.Replace("\n", "   ");
            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<span style=\"text-align:).*?(</span>)");
            string Title = Regex.Matches(HTML)[0].Value;


            string SidsPirs = null;
            try
            {
                Regex = new System.Text.RegularExpressions.Regex("(<table cellspacing=\"0\").*?(</table>)");
                SidsPirs = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                SidsPirs = ex.Message;
            }


            string ImagePath = null;
            try
            {
                Regex = new System.Text.RegularExpressions.Regex("(?<=<var class=\"postImg postImgAligned img-right\" title=\").*?(?=\">)");
                ImagePath = Regex.Matches(HTML)[0].Value;
                ImagePath = "http://" + IPAdress + ":" + PortRemoteFork + "/proxym3u8B" + Base64Encode(ImagePath + "OPT:ContentType--image/jpegOPEND:/") + "/";

            }
            catch (Exception ex)
            {

            }


            string InfoFile = null;
            try
            {
                Regex = new System.Text.RegularExpressions.Regex("(<div class=\"kpi\">).*(?=<div class=\"spoiler-wrap\">)");
                InfoFile = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception e)
            {
                try
                {
                    Regex = new System.Text.RegularExpressions.Regex("(<br /><br /><span style=\"font-weight: bold\">).*?(<br />)");

                    System.Text.RegularExpressions.MatchCollection Match = Regex.Matches(HTML);
                    for (int I = 1; I < Match.Count; ++I)
                    {
                        InfoFile = InfoFile + Match[I].Value;
                    }

                }
                catch (Exception ex)
                {
                    InfoFile = ex.Message;
                }

            }
            string Opisanie = null;
            try
            {
                Regex = new System.Text.RegularExpressions.Regex("(<span style=\"font-weight: bold\">Описание:</span><br />).*?(?=<div)");
                Opisanie = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                Opisanie = ex.Message;
            }




            SidsPirs = replacetags(SidsPirs);
            InfoFile = replacetags(InfoFile);
            Title = replacetags(Title);
            Opisanie = replacetags(Opisanie);

            return "<div id=\"poster\" style=\"float:left;padding:4px;	background-color:#EEEEEE;margin:0px 13px 1px 0px;\">" +
              "<img src=\"" + ImagePath + "\" style=\"width:180px;float:left;\" /></div><span style=\"color:#3090F0\">" + Title + "</span><br>" +
              SidsPirs + "<br>" + Opisanie + "<span style=\"color:#3090F0\">Информация</span><br>" + InfoFile;


        }
        public string replacetags(string s)
        {
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("<[^b].*?>");
            s = rgx.Replace(s, "").Replace("<b>", "");
            return s;
        }
        public string FormatDescription(string HTML, string ImagePath)
        {
            HTML = HTML.Replace("\n", "   ");

            string Title = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=title=\").*?(?=\")");
                Title = Regex.Matches(HTML)[1].Value;
            }
            catch (Exception ex)
            {
                Title = ex.Message;
            }


            string InfoFile = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<img class=\"tit-b pims\").*(?=<span id=)");
                InfoFile = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                InfoFile = ex.Message;
            }


            string InfoFilms = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(</var></a>).*?(<br />)");
                InfoFilms = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                InfoFilms = ex.Message;
            }

            string InfoPro = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<br /><b>).*(</span></td> )");
                InfoPro = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                InfoPro = ex.Message;
            }
            return "<div id=\"poster\" style=\"float:left;padding:4px;	background-color:#EEEEEE;margin:0px 13px 1px 0px;\">" +
               "<img src=\"" + ImagePath + "\" style=\"width:180px;float:left;\" /></div><span style=\"color:#3090F0\">" + Title + "</span><br>" +
               InfoFile + InfoPro + "<br><span style=\"color:#3090F0\">Описание: </span>" + InfoFilms;


        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        #endregion

        #region TorrentTV

        public Playlist GetTorrentTV(IPluginContext context)
        {
            var items = new System.Collections.Generic.List<Item>();
            Item Item = new Item();

            Item.Type = ItemType.DIRECTORY;
            Item.Name = "РАЗВЛЕКАТЕЛЬНЫЕ";
            Item.Link = "ent";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "ДЕТСКИЕ";
            Item.Link = "child";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "ПОЗНАВАТЕЛЬНЫЕ";
            Item.Link = "discover";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "HD";
            Item.Link = "HD";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "ОБЩИЕ";
            Item.Link = "common";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "ФИЛЬМЫ";
            Item.Link = "film";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "МУЖСКИЕ";
            Item.Link = "man";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "МУЗЫКАЛЬНЫЕ";
            Item.Link = "music";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "НОВОСТИ";
            Item.Link = "news";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "РЕГИОНАЛЬНЫЕ";
            Item.Link = "region";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "РЕЛИГИОЗНЫЕ";
            Item.Link = "relig";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            Item = new Item();
            Item.Type = ItemType.DIRECTORY;
            Item.Name = "СПОРТ";
            Item.Link = "sport";
            Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            items.Add(Item);

            //Взрослый контент
            //    Item = new Item();
            //    Item.Type = ItemType.DIRECTORY;
            //    Item.Name = "ЭРОТИКА";
            //    Item.Link = "porn";
            //    Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            //    items.Add(Item);

            //    Item = new Item();
            //    Item.Type = ItemType.DIRECTORY;
            //    Item.Name = "ВСЕ КАНАЛЫ";
            //    Item.Link = "all";
            //    Item.ImageLink = "http://torrent-tv.ru/images/all_channels.png";
            //    items.Add(Item);


            return PlayListPlugPar(items, context);
        }

        public Playlist LastModifiedPlayList(string NamePlayList, IPluginContext context)
        {

            string PathFileUpdateTime = System.IO.Path.GetTempPath() + NamePlayList + ".UpdateTime.tmp";
            string PathFilePlayList = System.IO.Path.GetTempPath() + NamePlayList + ".PlayList.tmp";

            System.Net.WebRequest request = System.Net.WebRequest.Create("http://super-pomoyka.us.to/trash/ttv-list/ttv." + NamePlayList + ".iproxy.m3u?ip=" + IPAdress + ":" + PortAce);
            request.Method = "HEAD";
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)(request.GetResponse());
            var responHeader = response.GetResponseHeader("Last-Modified");
            response.Close();

            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;

            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
            Item Item = new Item();

            if ((System.IO.File.Exists(PathFileUpdateTime) && System.IO.File.Exists(PathFilePlayList)) == false)
            {
                UpdatePlayList(NamePlayList, PathFilePlayList, PathFileUpdateTime, responHeader);
                Item.Type = ItemType.DIRECTORY;
                Item.GetInfo = "http://" + IPAdress + ":" + PortRemoteFork + "/treeview?pluginacetorrentplay%5c.xml&host=" + IPAdress + "%3a" + PortRemoteFork + "&pluginPath=getinfo&ID=" + WC.DownloadString(PathFilePlayList);
                items.Add(Item);
                return PlayListPlugPar(items, context);
            }

            if (responHeader != System.IO.File.ReadAllText(PathFileUpdateTime))
            {
                UpdatePlayList(NamePlayList, PathFilePlayList, PathFileUpdateTime, responHeader);
                Item.Type = ItemType.DIRECTORY;
                Item.GetInfo = "http://" + IPAdress + ":" + PortRemoteFork + "/treeview?pluginacetorrentplay%5c.xml&host=" + IPAdress + "%3a" + PortRemoteFork + "&pluginPath=getinfo&ID=" + WC.DownloadString(PathFilePlayList);
                items.Add(Item);
                return PlayListPlugPar(items, context);
            }

            Item.Type = ItemType.DIRECTORY;
            Item.GetInfo = "http://" + IPAdress + ":" + PortRemoteFork + "/treeview?pluginacetorrentplay%5c.xml&host=" + IPAdress + "%3a"+PortRemoteFork+"&pluginPath=getinfo&ID=" + WC.DownloadString(PathFilePlayList);
            items.Add(Item);
            PlayList.IsIptv = "true";
            return PlayListPlugPar(items, context);

        }

        public void UpdatePlayList(string NamePlayList, string PathFilePlayList, string PathFileUpdateTime, string LastModified)
        {
            System.IO.File.WriteAllText(PathFileUpdateTime, LastModified);
            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;
            WC.Headers.Add("Accept-Encoding", "gzip, deflate");
            byte[] Dat = WC.DownloadData("http://super-pomoyka.us.to/trash/ttv-list/ttv." + NamePlayList + ".iproxy.m3u?ip=" + IPAdress + ":" + PortAce);


            System.IO.FileStream decompressedFileStream = System.IO.File.Create(PathFilePlayList);
            System.IO.Compression.GZipStream decompressionStream = new System.IO.Compression.GZipStream(new System.IO.MemoryStream(Dat), System.IO.Compression.CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedFileStream);
            decompressedFileStream.Close();
            decompressionStream.Close();

            Dat = WC.DownloadData("http://super-pomoyka.us.to/trash/ttv-list/MyTraf.php");
            decompressedFileStream = System.IO.File.Create(System.IO.Path.GetTempPath() + "MyTraf.tmp");
            decompressionStream = new System.IO.Compression.GZipStream(new System.IO.MemoryStream(Dat), System.IO.Compression.CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedFileStream);
            decompressedFileStream.Close();
            decompressionStream.Close();
            WC.Dispose();
        }

        #endregion

        #region AceTorrent
        private string PortAce = "6878";
        private bool AceProxEnabl;

        public struct TorrentPlayList
        {
            public string IDX;
            public string Name;
            public string Link;
            public string Description;
        }

        public string GetM3UisTorrent(string PathTorrent, string ServerAdress)
        {
            //Возвращает неверный плейлист если в торренте только один видео файл
            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;

            return WC.DownloadString("http://" + ServerAdress + ":" + PortAce + "/ace/manifest.m3u8?id=" + GetID(PathTorrent, ServerAdress));
        }

        public string GetID(string PathTorrent, string ServerAdress)
        {
            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;
            byte[] FileTorrent = WC.DownloadData(PathTorrent);

            string FileTorrentString = System.Convert.ToBase64String(FileTorrent);
            FileTorrent = System.Text.Encoding.Default.GetBytes(FileTorrentString);

            System.Net.WebRequest request = System.Net.WebRequest.Create("http://api.torrentstream.net/upload/raw");
            request.Method = "POST";
            request.ContentType = "application/octet-stream\\r\\n";
            request.ContentLength = FileTorrent.Length;
            System.IO.Stream dataStream = request.GetRequestStream();
            dataStream.Write(FileTorrent, 0, FileTorrent.Length);
            dataStream.Close();

            System.Net.WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            string[] responseSplit = responseFromServer.Split('\"');
            return responseSplit[3];
        }


        public TorrentPlayList[] GetFileListJSON(string PathTorrent, string ServerAdress)
        {
            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;

            string[] CodeZnaki = { "\\U0430", "\\U0431", "\\U0432", "\\U0433", "\\U0434", "\\U0435", "\\U0451", "\\U0436", "\\U0437", "\\U0438", "\\U0439", "\\U043A", "\\U043B", "\\U043C", "\\U043D", "\\U043E", "\\U043F", "\\U0440", "\\U0441", "\\U0442", "\\U0443", "\\U0444", "\\U0445", "\\U0446", "\\U0447", "\\U0448", "\\U0449", "\\U044A", "\\U044B", "\\U044C", "\\U044D", "\\U044E", "\\U044F", "\\U0410", "\\U0411", "\\U0412", "\\U0413", "\\U0414", "\\U0415", "\\U0401", "\\U0416", "\\U0417", "\\U0418", "\\U0419", "\\U041A", "\\U041B", "\\U041C", "\\U041D", "\\U041E", "\\U041F", "\\U0420", "\\U0421", "\\U0422", "\\U0423", "\\U0424", "\\U0425", "\\U0426", "\\U0427", "\\U0428", "\\U0429", "\\U042A", "\\U042B", "\\U042C", "\\U042D", "\\U042E", "\\U042F", "\\U00AB", "\\U00BB", "U2116" };
            string[] DecodeZnaki = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я", "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "«", "»", "№" };

            string ContentID = GetID(PathTorrent, ServerAdress);
            string ItogStr = WC.DownloadString("http://" + ServerAdress + ":" + PortAce + "/server/api?method=get_media_files&content_id=" + ContentID);
            for (int I = 0; I <= 68; ++I)
            {
                ItogStr = ItogStr.Replace(CodeZnaki[I].ToLower(), DecodeZnaki[I]);
            }
            WC.Dispose();

            string PlayListJson = ItogStr;
            PlayListJson = PlayListJson.Replace(",", null);
            PlayListJson = PlayListJson.Replace(":", null);
            PlayListJson = PlayListJson.Replace("}", null);
            PlayListJson = PlayListJson.Replace("{", null);
            PlayListJson = PlayListJson.Replace("result", null);
            PlayListJson = PlayListJson.Replace("error", null);
            PlayListJson = PlayListJson.Replace("null", null);
            PlayListJson = PlayListJson.Replace("\"\"", "\"");
            PlayListJson = PlayListJson.Replace("\" \"", "\"");

            string[] ListSplit = PlayListJson.Split('\"');

            TorrentPlayList[] PlayListTorrent = new TorrentPlayList[(ListSplit.Length / 2) - 2 + 1];

            int N = 0;
            for (int I = 1; I <= ListSplit.Length - 2; ++I)
            {
                PlayListTorrent[N].IDX = ListSplit[I];
                PlayListTorrent[N].Name = ListSplit[I + 1];
                PlayListTorrent[N].Link = "http://" + ServerAdress + ":" + PortAce + "/ace/getstream?id=" + ContentID + "&_idx=" + PlayListTorrent[N].IDX;

                ++I;
                ++N;
            }
            return PlayListTorrent;
        }


        public string SearchDescriptions(string Name)
        {
            string HtmlFile = null;

            try
            {
                System.Net.WebClient WC = new System.Net.WebClient();
                WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
                WC.Encoding = System.Text.Encoding.UTF8;
                string Str = WC.DownloadString("http://www.kinomania.ru/search/?q=" + System.IO.Path.GetFileName(Name));

                System.Text.RegularExpressions.Regex Regul = new System.Text.RegularExpressions.Regex("<header><h3>По вашему запросу ничего не найдено</h3></header>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                bool Bool = Regul.IsMatch(Str);


                if (Bool == true)
                {
                    HtmlFile = "<div>Описание не найдено.</div><div>Попробуйте переименовать торрент файл</div></html>";
                }
                else
                {
                    Regul = new System.Text.RegularExpressions.Regex("(?<=fid=\").*(?=\">)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.MatchCollection HTML = Regul.Matches(Str);
                    string FidStr = HTML[0].Value;



                    Str = WC.DownloadString("http://www.kinomania.ru/film/" + FidStr);

                    Regul = new System.Text.RegularExpressions.Regex("(?<=<title>).*(?=</title>)");
                    HTML = Regul.Matches(Str);
                    string TitleStr = "<div>" + HTML[0].Value.Replace("| KINOMANIA.RU", "") + "</div>";

                    Regul = new System.Text.RegularExpressions.Regex("(<img width=\"295\")(\\n|.)*?(/>)");
                    HTML = Regul.Matches(Str);
                    string OblojkaStr = HTML[0].Value.Replace("width=\"295\" height=\"434\"", "width=\"200\" height=\"320\"");

                    Regul = new System.Text.RegularExpressions.Regex("(<div class=\"l-col l-col-2\">)(\\n|.)*?(</div>)");
                    HTML = Regul.Matches(Str);
                    string ObzorStr = HTML[0].Value.Replace("<div class=\"l-col l-col-2\">", "<div class=\"l-col l-col-2\"><font size=\"1\">");

                    Regul = new System.Text.RegularExpressions.Regex("(<div class=\"l-col l-col-3\">)(\\n|.)*?(<section)");
                    HTML = Regul.Matches(Str);
                    string SozdateliStr = HTML[0].Value.Replace("<div class=\"l-col l-col-3\">", "<div class=\"l-col l-col-3\"><font size=\"1\">");

                    Regul = new System.Text.RegularExpressions.Regex("(<h2 class=\"b-switcher\">)(\\n|.)*?(</div>)");
                    HTML = Regul.Matches(Str);
                    string OFilmeStr = "<div " + System.Environment.NewLine + HTML[0].Value.Replace("<h2 class=\"b-switcher\">", "<h2 class=\"b-switcher\"><font size=\"2\">") + "</div></div></div>";



                    HtmlFile = "" + TitleStr + "<table><tbody><tr><td valign=\"top\">" + OblojkaStr + "</td><td valign=\"top\">" + ObzorStr + " <div></td><td valign=\"top\">" + SozdateliStr + "</div>" + "</td></tr></tbody></table>" + OFilmeStr + "</html>";

                }

            }
            catch (Exception ex)
            {
                HtmlFile = ex.Message;
            }
            return HtmlFile;

        }
        #endregion

    }
}