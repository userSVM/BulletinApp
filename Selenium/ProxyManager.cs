﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Selenium
{
    class ProxyData
    {
        public string Address { get; set; }
        public DateTime Date { get; set; }
    }
    static class ProxyManager
    {
        static readonly string query = @"http://api.best-proxies.ru/proxylist.txt?key=faa165ff460844283e7e5fa2875ee106&type=http&ports=80,8080&pex=1&level=3&limit=0";

        static Queue<ProxyData> proxies = new Queue<ProxyData>();

        static ProxyData currentProxy { get; set; }

        public static ProxyData UseProxy()
        {
            var limitTryProxyLoading = 3;
            var countProxyLoading = 0;
            var hasValidProxy = false;
            if (currentProxy != null)
            {
                hasValidProxy = CheckProxy(currentProxy);
            }

            if (currentProxy == null || !hasValidProxy)
            {
                while (!hasValidProxy && countProxyLoading < limitTryProxyLoading)
                {
                    if (proxies.Count > 0)
                    {
                        var nextProxy = proxies.Dequeue();
                        hasValidProxy = CheckProxy(nextProxy);
                        if (hasValidProxy)
                        {
                            currentProxy = nextProxy;
                        }
                    }
                    else
                    {
                        UpdateProxyList();
                        countProxyLoading++;
                    }
                }
            }
            if (hasValidProxy)
            {
                return currentProxy;
            }

            return null;

        }
        static bool CheckProxy(ProxyData proxy)
        {
            var result = false;
            HttpWebResponse response = null;
            WebRequest.DefaultWebProxy = new WebProxy
            {
                Address = new Uri("http://" + proxy.Address)
            };
            var request = WebRequest.Create("http://avito.ru");
            request.Timeout = 5000;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (response != null) response.Close();
            }
            return result;
        }

        static void UpdateProxyList()
        {
            var queryResult = LoadProxies();
            var rawProxies = queryResult.Split(new[] { "\r\n" }, StringSplitOptions.None);
            proxies = new Queue<ProxyData>(rawProxies.Select(q => new ProxyData { Address = q }));
        }

        static string LoadProxies()
        {
            var result = string.Empty;
            var request = WebRequest.Create(query);
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        result += line + Environment.NewLine;
                    }
                }
            }
            response.Close();
            return result;
        }


    }
}