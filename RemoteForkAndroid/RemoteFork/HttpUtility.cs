using ModernHttpClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Encoding = System.Text.Encoding;
//
namespace tv.forkplayer.remotefork.server { 

    public static class HttpUtility
    {

        private static readonly CookieContainer CookieContainer = new CookieContainer();
        private static bool _clearCookies;

        public static void GetByteRequest(HttpListenerResponse output, string link, Dictionary<string, string> header = null, bool wcc = false)
        {
            try
            {
                Console.WriteLine("GetByteRequest");
                HttpWebRequest wc = null;
                if (wcc) wc = (HttpWebRequest)HttpWebRequest.Create(link);


                _clearCookies = false;
                if (header != null)
                {
                    foreach (var h in header)
                    {
                        try
                        {
                            Console.WriteLine(h.Key + " set=" + h.Value);
                            if (h.Key == "Cookie")
                            {
                                _clearCookies = true;
                                CookieContainer.SetCookies(new Uri(link), h.Value.Replace(";", ","));
                            }
                            if (wcc)
                            {
                                if (h.Key == "Range")
                                {
                                    var x = h.Value.Split('=')[1].Split('-');
                                    if (x.Length == 1) wc.AddRange(Convert.ToInt64(x[0]));
                                    else if (x.Length == 2)
                                    {
                                        if (String.IsNullOrEmpty(x[1]))
                                        {
                                            Console.WriteLine(x[0]);
                                            if (Convert.ToInt64(x[0]) > 0) wc.AddRange(Convert.ToInt64(x[0]));
                                        }
                                        else wc.AddRange(Convert.ToInt64(x[0]), Convert.ToInt64(x[1]));
                                    }
                                }
                                else wc.Headers.Add(h.Key, h.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                if (wcc)
                {
                    var resp = (HttpWebResponse)wc.GetResponse();
                    Console.WriteLine(resp.Headers);
                    if (!string.IsNullOrEmpty(resp.Headers["Content-Length"]))
                        output.ContentLength64 = Convert.ToInt64(resp.Headers["Content-Length"]);

                    Console.WriteLine("wc stream");
                    var stream = new System.IO.StreamReader(resp.GetResponseStream());
                    stream.BaseStream.CopyTo(output.OutputStream);
                }
                else
                {
                    Console.WriteLine("HttpClientHandler stream");
                    using (var handler = new HttpClientHandler())
                    {
                        SetHandler(handler);
                        using (var httpClient = new System.Net.Http.HttpClient(handler))
                        {

                            AddHeader(httpClient, header);
                            var response = httpClient.GetAsync(link).Result;
                            var r = response.Content.ReadAsByteArrayAsync().Result;
                            Console.WriteLine("Len " + r.Length);
                            if (r.Length > 0)
                            {
                                output.ContentLength64 = r.Length;
                                output.OutputStream.Write(r, 0, r.Length);

                            }

                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Er getbyte" + ex.ToString());
                //output.OutputStream.Close();
                /*  using (var writer = new System.IO.StreamWriter(output.OutputStream))
                  {
                      writer.Write(ex.ToString());
                  }
  */
            }
        }
        public static string GetRequest(string link, Dictionary<string, string> header = null, bool verbose = false, bool databyte = false, bool autoredirect = true, WebProxy proxy = null)
        {
            try
            {
                _clearCookies = false;
                if (header != null)
                {
                    foreach (var entry in header)
                    {
                        if (entry.Key == "Cookie")
                        {
                            _clearCookies = true;
                            CookieContainer.SetCookies(new Uri(link), entry.Value.Replace(";", ","));
                        }
                    }
                }
                HttpClientHandler handler = null;
                if (link.IndexOf("https://") == 0) handler = new NativeMessageHandler() { AllowAutoRedirect = autoredirect };
                else handler = new HttpClientHandler() { AllowAutoRedirect = autoredirect };               
                    SetHandler(handler);
                    using (var httpClient = new HttpClient(handler))
                    {
                        AddHeader(httpClient, header);
                        var response = httpClient.GetAsync(link).Result;
                        if (_clearCookies)
                        {
                            var cookies = handler.CookieContainer.GetCookies(new Uri(link));
                            foreach (Cookie co in cookies)
                            {
                                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                            }
                        }
                        //return response.ToString();
                        Console.WriteLine("HEADS");

                        if (verbose)
                        {
                            string sh = "";
                            try
                            {
                                var headers = response.Headers.Concat(response.Content.Headers);

                                foreach (var i in headers)
                                {
                                    foreach (var j in i.Value)
                                    {
                                        Console.WriteLine(i.Key + ": " + j);
                                        sh += i.Key + ": " + j + "\n";
                                    }

                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Err get headers: " + e.ToString());
                            }
                            return string.Format("{0}\n{1}", sh, ReadContext(response.Content));
                        }
                        else
                        {
                            return ReadContext(response.Content, databyte);
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("HttpUtility->GetRequest: " + ex.ToString());
                return ex.Message;
            }
        }

        public static string PostRequest(string link, string data,
                                         Dictionary<string, string> header = null, bool verbose = false, bool autoredirect = true, WebProxy proxy = null)
        {
            try
            {
                _clearCookies = false;
                if (header != null)
                {
                    foreach (var entry in header)
                    {
                        if (entry.Key == "Cookie")
                        {
                            _clearCookies = true;
                            CookieContainer.SetCookies(new Uri(link), entry.Value.Replace(";", ","));
                        }
                    }
                }
                HttpClientHandler handler=null;
                if (link.IndexOf("https://") == 0) handler = new NativeMessageHandler() { AllowAutoRedirect = autoredirect };
                else handler = new HttpClientHandler() { AllowAutoRedirect = autoredirect };
               
                    if (proxy != null) handler.Proxy = proxy;
                    SetHandler(handler);
                    using (var httpClient = new HttpClient(handler))
                    {
                        AddHeader(httpClient, header);

                        var response = httpClient.PostAsync(link, new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded")).Result;
                        if (_clearCookies)
                        {
                            var cookies = handler.CookieContainer.GetCookies(new Uri(link));
                            foreach (Cookie co in cookies)
                            {
                                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                            }
                        }
                        if (verbose)
                        {
                            var headers = response.Headers.Concat(response.Content.Headers);
                            string sh = "";
                            foreach (var i in headers)
                            {
                                foreach (var j in i.Value)
                                {
                                    Console.WriteLine(i.Key + ": " + j);
                                    sh += i.Key + ": " + j + "\n";
                                }

                            }
                            return string.Format("{0}\n{1}", sh, ReadContext(response.Content));
                        }
                        else
                        {
                            return ReadContext(response.Content);
                        }
                    }
               
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static void SetHandler(HttpClientHandler handler)
        {
            handler.CookieContainer = CookieContainer;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        private static void AddHeader(HttpClient httpClient, Dictionary<string, string> header)
        {
            if (header != null)
            {
                foreach (var h in header)
                {
                    try
                    {
                        Console.WriteLine(h.Key + "=" + h.Value);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        private static string ReadContext(HttpContent context, bool databyte = false)
        {
            try
            { Console.WriteLine(context.Headers.ContentEncoding);
                if (context.Headers.ContentEncoding.Contains("ggggzip"))
                {
                    Console.WriteLine("DECODE GZIP");
                    var responseStream = context.ReadAsStreamAsync().Result;
                    GZipStream ms;
                    var stream = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress);

                   
                }


                if (databyte)
                {
                    var result = context.ReadAsByteArrayAsync().Result;

                    var encoding = Encoding.GetEncoding("windows-1251");
                    result = Encoding.Convert(encoding, Encoding.Default, result);
                    Console.WriteLine("RET DATABYTE ENC");
                    return Encoding.Default.GetString(result);
                }
            }
            catch (Exception e)
            { }

            try
            {
                return context.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                var result = context.ReadAsByteArrayAsync().Result;
                try
                {
                    var encoding = Encoding.Default;
                    result = Encoding.Convert(encoding, Encoding.Default, result);
                }
                catch
                {
                    try
                    {
                        var encoding = Encoding.GetEncoding(context.Headers.ContentType.CharSet);
                        result = Encoding.Convert(encoding, Encoding.Default, result);
                    }
                    catch
                    {
                        try
                        {
                            var encoding = Encoding.UTF8;
                            result = Encoding.Convert(encoding, Encoding.Default, result);
                        }
                        catch
                        {
                            try
                            {
                                var encoding = Encoding.ASCII;
                                result = Encoding.Convert(encoding, Encoding.Default, result);
                            }
                            catch
                            {
                                try
                                {
                                    var encoding = Encoding.Unicode;
                                    result = Encoding.Convert(encoding, Encoding.Default, result);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                }
                return Encoding.Default.GetString(result);
            }
        }

        public static string QueryParametersToString(System.Collections.Specialized.NameValueCollection query)
        {
            if ((query == null) || (query.Count == 0))
            {
                return string.Empty;
            }

            var sb = new System.Text.StringBuilder();

            for (var i = 0; i < query.Count; i++)
            {
                var key = WebUtility.UrlEncode(query.GetKey(i));
                var keyPrefix = key != null ? key + "=" : string.Empty;
                var values = query.GetValues(i);

                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                if ((values == null) || (values.Length == 0))
                {
                    sb.Append(keyPrefix);
                }
                else if (values.Length == 1)
                {
                    sb.Append(keyPrefix).Append(WebUtility.UrlEncode(values[0]));
                }
                else
                {
                    for (var j = 0; j < values.Length; j++)
                    {
                        if (j > 0)
                        {
                            sb.Append('&');
                        }

                        sb.Append(keyPrefix).Append(WebUtility.UrlEncode(values[j]));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
