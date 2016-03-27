using System;
using Microsoft.Win32;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace iOSApplication_0._4._5
{
    class FingerprintDatabaseManager
    {
        public async void getTitles()
        {
            using (var client = new HttpClient())
            {
                var inputString =
                String.Format("http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetSingleFingerprintByHash?inputHash=" + "{0}",
                "Deadpool");
                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Response: " + responseString);
            }
        }
    }
}
