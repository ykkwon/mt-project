using System;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;


namespace AcousticFingerprintingClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() => {
                RunAsync().Wait();
            })).Start();
        }

        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58293/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("Fingerprints/Getproduct/2");
                if (response.IsSuccessStatusCode)
                {
                    Fingerprint value = await response.Content.ReadAsAsync<Fingerprint>();
                    Console.WriteLine(value.Hash);
                }
                else
                {
                    Console.WriteLine(response.StatusCode);
                }
            }
        }
    }
}