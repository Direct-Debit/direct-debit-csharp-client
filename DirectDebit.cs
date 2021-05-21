using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DirectDebitCSharpClient
{
    public class DirectDebit
    {
        private readonly string _userCode;
        private readonly string _password;
        private readonly string _url;

        public DirectDebit(string userCode, string password, bool prod=false)
        {
            _userCode = userCode;
            _password = password;

            var subdomain = prod ? "dos" : "dos-dr";
            _url = $"https://{subdomain}.directdebit.co.za:31143/v1.1";
        }

        private HttpClient GenerateHttpClient()
        {
            var client = new HttpClient();
            var token = Convert.ToBase64String(Encoding.GetEncoding("utf-8").GetBytes($"{_userCode}:{_password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            return client;
        }

        // Upload a file to Direct Debit via the API
        public async Task UploadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"{filePath} does not exist");
            }

            var content = new List<byte>();
            await using (var fs = File.OpenRead(filePath))
            {
                var b = new byte[1024];
                while (fs.Read(b, 0, b.Length) > 0)
                {
                    content.AddRange(b);
                }
            }
            
            var hc = GenerateHttpClient();
            var postBody = new MultipartFormDataContent
            {
                {new ByteArrayContent(content.ToArray()), "file_data", Path.GetFileName(filePath)}
            };

            var resp = await hc.PostAsync(_url + "/batch/eft", postBody);
            resp.EnsureSuccessStatusCode();
            var r = await resp.Content.ReadAsStringAsync();
            Console.WriteLine(r);
        }
    }
}