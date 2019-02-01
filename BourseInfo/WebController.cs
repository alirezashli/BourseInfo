﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BourseInfo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    using PortfolioManagement;

    public static class WebController
    {
        private static readonly HttpClient HttpClient;

        private const int NumberOfRetries = 1;

        private const int DelayAfterRetry = 10000; // in milliseconds

        private const int RequestTimeout = 20000;

        static WebController()
        {
            HttpClient = new HttpClient();

            // HttpClient.Timeout = TimeSpan.FromSeconds(10); // 10 sec
        }

        public static async Task<string> GetStringAsync(string uri)
        {
            try
            {
                // Create a New HttpClient object and dispose it when done, so the app doesn't leak resources
                // HttpResponseMessage response = await HttpClient.GetAsync(uri);
                // response.EnsureSuccessStatusCode();
                // string responseBody = await response.Content.ReadAsStringAsync();

                // Above three lines can be replaced with new helper method below
                string responseBody = await HttpClient.GetStringAsync(uri);

                return responseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<List<Stock>> GetStocksAsync(string uri)
        {
            var stockList = new List<Stock>();

            string res = await GetStringAsync(uri);

            if (!String.IsNullOrEmpty(res))
            {
                dynamic json = JObject.Parse(res);

                var items = json.embedded?.issues;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        stockList.Add(new Stock(item));
                    }
                }
            }

            return stockList;
        }

        public static string GetString(string uri)
        {
            string result = string.Empty;

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = RequestTimeout;

                try
                {
                    WebResponse response = request.GetResponse();
                    Stream data = response.GetResponseStream();

                    using (StreamReader sr = new StreamReader(data))
                    {
                        result = sr.ReadToEnd();
                    }

                    break; // success, do not retry!
                }
                catch (WebException ex)
                {
                    Log.Write(ex);

                    // if (i == NumberOfRetries)
                    // {
                    // throw;
                    // }
                    if (NumberOfRetries > 1)
                        Thread.Sleep(DelayAfterRetry);
                }
            }

            return result;
        }
    }
}
