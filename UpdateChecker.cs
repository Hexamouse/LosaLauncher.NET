using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Windows;

namespace GameLauncher
{
    public static class UpdateChecker
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<long?> GetRemoteFileSizeAsync(string url)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.Headers.ContentLength;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Check Error", $"Failed to get file size: {ex.Message}");
            }

            return null;
        }
    }
}
