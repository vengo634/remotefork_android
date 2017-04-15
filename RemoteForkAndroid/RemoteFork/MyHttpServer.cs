using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace tv.forkplayer.remotefork.server {
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

            Handle(context.Request, context.Response);
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
        public virtual void Handle(HttpListenerRequest request, HttpListenerResponse response) {
            SetDefaultReponseHeaders(response);
            Handle(request, response);
        }

        protected virtual void SetDefaultReponseHeaders(HttpListenerResponse response)
        {
            response.SendChunked = false;
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentType = Unosquare.Labs.EmbedIO.Constants.DefaultMimeTypes[".html"];
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
            return new UriBuilder
            {
                Scheme = request.Url.Scheme,
                Host = request.Url.Host,
                Port = request.Url.Port,
                Path = WebUtility.UrlEncode(path),
                Query = query.ToString()
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
            response.StatusCode = (int)status;

            WriteResponse(response, responseText);
        }
    }
    internal class TestRequestHandler : BaseRequestHandler
    {
        internal static readonly string UrlPath = "/test";

        public override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            var rawUrl = WebUtility.UrlDecode(request.RawUrl);

            if (rawUrl != null && rawUrl.IndexOf('|') > 0)
            {
                var device = rawUrl.Replace("/test?", "");
                if (!MainActivity.Devices.Contains(device))
                {
                    MainActivity.Devices.Add(device);
                }

            }

            WriteResponse(response, $"<html><h1>ForkPlayer DLNA Work!</h1><br><b>Android RemoteFork Server. v. 1.36.0a</b></html>");
        }
    }
    internal class ProxyM3u8 : BaseRequestHandler
    {
        internal static readonly string UrlPath = "/proxym3u8";

        public override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            var result = string.Empty;
            var url = System.Net.WebUtility.UrlDecode(request.RawUrl)?.Substring(UrlPath.Length);
            if (url.Substring(0, 1) == "B")
            {
                if (url.IndexOf("endbase64") > 0)
                {
                    url = Encoding.UTF8.GetString(Convert.FromBase64String(url.Substring(1, url.IndexOf("endbase64") - 1))) + url.Substring(url.IndexOf("endbase64") + 9);
                }
                else url = Encoding.UTF8.GetString(Convert.FromBase64String(url.Substring(1, url.Length - 2)));
            }
            var ts = "";
            bool usertype = false;
            Dictionary<string, string> header = new Dictionary<string, string>();
            Console.WriteLine("Proxy url: " + url);
            if (url.IndexOf("OPT:") > 0)
            {
                if (url.IndexOf("OPEND:/") == url.Length - 7)
                {
                    url = url.Replace("OPEND:/", "");
                    Console.WriteLine("Req root m3u8 " + url);
                }
                else
                {
                    ts = url.Substring(url.IndexOf("OPEND:/") + 7);
                    Console.WriteLine("Req m3u8 ts " + ts);
                }
                if (url.IndexOf("OPEND:/") > 0) url = url.Substring(0, url.IndexOf("OPEND:/"));
                var Headers = url.Substring(url.IndexOf("OPT:") + 4).Replace("--", "|").Split('|');
                url = url.Substring(0, url.IndexOf("OPT:"));
                for (var i = 0; i < Headers.Length; i++)
                {
                    if (Headers[i] == "ContentType")
                    {
                        if (!string.IsNullOrEmpty(request.Headers.Get("Range")))
                        {
                            header["Range"] = request.Headers.Get("Range");
                        }
                        response.Headers.Add(Unosquare.Labs.EmbedIO.Constants.HeaderAcceptRanges, "bytes");
                        Console.WriteLine("reproxy with ContentType");
                        usertype = true;
                        response.ContentType = Headers[++i];
                        continue;
                    }
                    header[Headers[i]] = Headers[++i];
                    Console.WriteLine(Headers[i - 1] + "=" + Headers[i]);
                }
            }
            if (!usertype)
            {
                if (ts != "")
                {
                    url = url.Substring(0, url.LastIndexOf("/") + 1) + ts;
                    Console.WriteLine("Full ts url " + url);
                    response.ContentType = "video/mp2t";

                }
                else response.ContentType = "application/vnd.apple.mpegurl";
            }
            response.Headers.Remove("Tranfer-Encoding");
            response.Headers.Remove("Keep-Alive");
            // response.AddHeader("Accept-Ranges", "bytes");
            Console.WriteLine("Real url:" + url);
            HttpUtility.GetByteRequest(response, url, header, usertype);
        }
    }
    internal class ParseLinkRequestHandler : BaseRequestHandler
    {

        internal static readonly string UrlPath = "/parserlink";

        public override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            var result = string.Empty;

            var requestStrings = WebUtility.UrlDecode(request.RawUrl)?.Substring(UrlPath.Length + 1).Split('|');
            if (request.HttpMethod == "POST")
            {
                StreamReader getPostParam = new StreamReader(request.InputStream, true);
                var postData = getPostParam.ReadToEnd();
                Console.WriteLine("POST "+ postData);
                requestStrings = WebUtility.UrlDecode(postData)?.Substring(2).Split('|');
            }
            if (requestStrings != null)
            {
                var curlResponse = requestStrings[0].StartsWith("curl")
                    ? CurlRequest(requestStrings[0])
                    : HttpUtility.GetRequest(requestStrings[0]);

                if (requestStrings.Length == 1)
                {
                    result = curlResponse;
                }
                else
                {
                    if (!requestStrings[1].Contains(".*?"))
                    {
                        if (string.IsNullOrEmpty(requestStrings[1]) && string.IsNullOrEmpty(requestStrings[2]))
                        {
                            result = curlResponse;
                        }
                        else
                        {
                            var num1 = curlResponse.IndexOf(requestStrings[1], StringComparison.Ordinal);
                            if (num1 == -1)
                            {
                                result = string.Empty;
                            }
                            else
                            {
                                num1 += requestStrings[1].Length;
                                var num2 = curlResponse.IndexOf(requestStrings[2], num1, StringComparison.Ordinal);
                                result = num2 == -1 ? string.Empty : curlResponse.Substring(num1, num2 - num1);
                            }
                        }
                    }
                    else
                    {
                        var pattern = requestStrings[1] + "(.*?)" + requestStrings[2];
                        var regex = new Regex(pattern, RegexOptions.Multiline);
                        var match = regex.Match(curlResponse);
                        if (match.Success) result = match.Groups[1].Captures[0].ToString();
                    }
                }
            }

            WriteResponse(response, result);
        }

        public string CurlRequest(string text)
        {
            string result;
            var verbose = text.IndexOf(" -i", StringComparison.Ordinal) > 0;
            var autoredirect = text.IndexOf(" -L", StringComparison.Ordinal) > 0;

            var url = Regex.Match(text, "(?:\")(.*?)(?=\")").Groups[1].Value;
            var matches = Regex.Matches(text, "(?:-H\\s\")(.*?)(?=\")");
            var header = (
                from Match match in matches
                select match.Groups
                into groups
                where groups.Count > 1
                select groups[1].Value
                into value
                where value.Contains(": ")
                select value).ToDictionary(value => value.Remove(value.IndexOf(": ", StringComparison.Ordinal)),
                    value => value.Substring(value.IndexOf(": ", StringComparison.Ordinal) + 2));
            if (text.Contains("--data"))
            {
                var dataString = Regex.Match(text, "(?:--data\\s\")(.*?)(?=\")").Groups[1].Value;
                result = HttpUtility.PostRequest(url, dataString, header, verbose,autoredirect);
            }
            else
            {
                result = HttpUtility.GetRequest(url, header, verbose,false,autoredirect);
            }

            return result;
        }
    }
}
