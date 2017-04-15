using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Java.Lang;
using RemoteForkAndroid;
using tv.forkplayer.remotefork.server;
using Exception = System.Exception;
using Uri = Android.Net.Uri;
using Unosquare.Labs.EmbedIO;
using System.Threading;
using System.Threading.Tasks;
using RemoteForkAndroid.RemoteFork;

namespace tv.forkplayer.remotefork {
    [Activity(Label = "RemoteFork "+SettingManager.AppVersion+" c плагинами для Ace Stream", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {
        private static string logs;
        public static HashSet<string> Devices = new HashSet<string>();
        public string url_new = "";
        Button bStartServer, bStopServer, bLoadPlaylist, bNewVersion;
        EditText etLogs;
        TextView tvStatus;
        Spinner sIps;


        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            bStartServer = FindViewById<Button>(Resource.Id.bStartServer);
            bNewVersion = FindViewById<Button>(Resource.Id.bNewVersion);
            bNewVersion.Enabled = false;
            bStopServer = FindViewById<Button>(Resource.Id.bStopServer);
            bLoadPlaylist = FindViewById<Button>(Resource.Id.bLoadPlaylist);
            etLogs = FindViewById<EditText>(Resource.Id.etLogs);
            tvStatus = FindViewById<TextView>(Resource.Id.tvStatusServer);
            sIps = FindViewById<Spinner>(Resource.Id.sIps);

            bStartServer.Click += StartServerOnClick;
            bStopServer.Click += StopServerOnClick;
            bLoadPlaylist.Click += LoadPlaylistOnClick;
            bNewVersion.Click += NewVersion;

            var ips = Tools.GetIPAddresses();
            if (ips.Length > 0) {
                var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);

                foreach (var ipAddress in ips) {
                    adapter.Add(ipAddress.ToString());
                }
                sIps.Adapter = adapter;

                string ip = SettingManager.GetValue(SettingManager.LastIp);
                if (string.IsNullOrEmpty(ip)) {
                    ip = ips[0].ToString();
                }
                sIps.SetSelection(adapter.GetPosition(ip));

               // if (httpServer == null) {
                    bStartServer.PerformClick();
               /* } else {
                    etLogs.Text = logs;
                    if (httpServer != null && !httpServer.IsWork) {
                        tvStatus.Text = GetString(Resource.String.StartStatus);
                        bStartServer.Enabled = false;
                        bStopServer.Enabled = true;
                    }
                }*/
            }
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((requestCode == Resource.Id.bLoadPlaylist) && (resultCode == Result.Ok) && (data != null)) {
                Uri uri = data.Data;
                var text = File.ReadAllText(uri.Path);

                if (text.Length < 102401 &&
                    (text.Contains("EXTM3U") || text.Contains("<title>") || text.Contains("http://"))) {
                    string fileName = Path.GetFileName(uri.Path);
                    foreach (var device in MainActivity.Devices) {
                    string url = "http://forkplayer.tv/remote/index.php?do=uploadfile&fname=" + fileName + "&initial=" + device;

                        var data1 = "text=" + System.Net.WebUtility.UrlEncode(text);
                        string text2 = HttpUtility.PostRequest(url, data1);
                    }

                    ShowAlert(GetString(Resource.String.LoadedPlaylist));
                } else {
                    ShowAlert(GetString(Resource.String.PlaylistBadFormat));
                }
            }
        }

        private HttpServer _httpServer;
        private System.Diagnostics.Process thvpid;

        private void StartServerOnClick(object sender, EventArgs e) {
            try {
                WriteLine(GetString(Resource.String.StartingStatus));
                tvStatus.Text = GetString(Resource.String.StartingStatus);

                IPAddress ip;
                if (IPAddress.TryParse(sIps.SelectedItem.ToString(), out ip)) {
                    _httpServer = new HttpServer(ip, 8028);

                    
                    var s=HttpUtility.GetRequest(
                            string.Format(
                                "http://getlist2.obovse.ru/remote/index.php?v={0}&appl=android&do=list&localip={1}:{2}",
                                SettingManager.AppVersion,
                                ip, 8028));
                    if (s.IndexOf("new_version") >= 0)
                    {
                        bNewVersion.Enabled = true;
                        var x=s.Split('|');
                        bNewVersion.Text = x[1];
                        WriteLine(x[1]);
                        url_new = x[2];
                        WriteLine(x[2]);
                    }
                    else  bNewVersion.Text ="Нет новых версий";

                    SettingManager.SetValue(SettingManager.LastIp, ip.ToString());

                    WriteLine(GetString(Resource.String.StartStatus));
                    tvStatus.Text = GetString(Resource.String.StartStatus);
                    bStartServer.Enabled = false;
                    bStopServer.Enabled = true;
                }
            } catch (Exception ex) {
                Console.Out.WriteLine("Exception: " + ex.Message);
                WriteLine(GetString(Resource.String.ErrorStart));
            }
        }
       

            private void StopServerOnClick(object sender, EventArgs e) {
            try {
                WriteLine(GetString(Resource.String.StopingStatus));
                tvStatus.Text = GetString(Resource.String.StopingStatus);
                _httpServer?.Dispose();
                _httpServer = null;
                WriteLine(GetString(Resource.String.StopStatus));
                tvStatus.Text = GetString(Resource.String.StopStatus);
                bStartServer.Enabled = true;
                bStopServer.Enabled = false;
            } catch (Exception ex) {
                Console.Out.WriteLine("Exception: " + ex.Message);
                WriteLine(GetString(Resource.String.ErrorStop));
            }
        }

        private void LoadPlaylistOnClick(object sender, EventArgs eventArgs) {
            if (MainActivity.Devices.Count > 0) {
                Intent = new Intent();
                Intent.SetType("*/m3u");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Playlist"), Resource.Id.bLoadPlaylist);
            } else {
                ShowAlert(GetString(Resource.String.DevicesNotFound));
            }
        }
        private void NewVersion(object sender, EventArgs eventArgs) {
            if (url_new!="") {
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url_new));
                StartActivity(intent);
            } else {
                ShowAlert("Нету новой версии!");
            }
        }
        public void ShowAlert(string str) {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(str);
            alert.SetPositiveButton("OK", (senderAlert, args) => {
            });

            RunOnUiThread(() => {
                alert.Show();
            });
        }

        public void WriteLine(string text, bool save = true) {
            etLogs.Text = text + "\r\n" + etLogs.Text;
            if (save) {
                logs = etLogs.Text;
            }
        }
    }

    internal class HttpServer : IDisposable
    {

        private WebServer _webServer;

        private CancellationTokenSource _cts;

        private Task _task;

        public HttpServer(IPAddress ip, int port)
        {
            //Log.Info(m => m("Server start"));

            _cts = new CancellationTokenSource();

            _webServer = WebServer.Create(
                new UriBuilder
                {
                    Scheme = "http",
                    Host = ip.ToString(),
                    Port = port,
                    Path = "/"
                }.ToString()
            );

            _webServer.RegisterModule(new RequestDispatcher());

            _webServer?.RunAsync(_cts.Token);
        }

        ~HttpServer()
        {
            Dispose(false);
        }

        private void Stop()
        {
            if (!(_cts?.IsCancellationRequested ?? true))
            {
                _cts?.Cancel();

                try
                {
                    _task?.Wait();
                }
                catch (AggregateException)
                {
                    //Log.Info(m => m("Server stop"));
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Stop();

            if (_webServer != null)
            {
                _webServer.Dispose();
                _webServer = null;
            }

            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }

            if (_task != null)
            {
                _task?.Dispose();
                _task = null;
            }
        }
    }
}
