using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


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
                    Console.WriteLine("{0}\t${1}\t{2}", value.Id, value.Title, value.Hash);
                }
                else
                {
                    Console.WriteLine(response.StatusCode);
                }
            }
        }
    }
}