using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Discord.Models;

namespace Discord.Rest
{
    public class CordRestClient
    {
        public CordRestClient(string token, bool bot, string HostURL = "discordapp.com", string ApiVersion = "7", string baseApiUrl = "/api")
        {
            this.Token = (bot ? "" : "Bot ") + token;
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("Authorization", this.Token);
        }

        public string Token { get; private set; }
        public string HostURL { get; set; }
        public string ApiVersion { get; set; }
        public string BaseApiURL { get; set; }
        public string Endpoint { get { return $"https://{HostURL}{BaseApiURL}/v{ApiVersion}"; } }
        public static string DefaultEndpoint = "https://discordapp.com/api/v7";
        public static Encoding Encoding = Encoding.UTF8;
        
        public HttpClient Client { get; private set; }

        //An FYI: We're not really making sure this is a 
        public async Task<bool> SendMessage(string channel_id, string content)
        {
            string base_url = Endpoint + $"/channels/{channel_id}/messages";
            
        }

        public static async Task<string> GetToken(string email, string password)
        {
            using (var client = new HttpClient())
            {
                bool request_success = false; //rly bad method ik don't judge me lol.
                string resp_token = null;

                while (!request_success)
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DefaultEndpoint + "/auth/login");
                    string content = JsonConvert.SerializeObject(new { email = email, password = password });
                    request.Content = new StringContent(content, Encoding, "application/json");

                    HttpResponseMessage response = await client.SendAsync(request);
                    string resp_str = await response.Content.ReadAsStringAsync();
                    if (response.Headers.Contains("Retry-After")) //might be a better way to check tho
                    {

                        // await Task.Delay(Convert.ToInt32(response.Content.Headers.GetValues("Retry-After").FirstOrDefault() ?? "0"));
                        //wait for ratelimit without parsing the body

                        await Task.Delay(JsonConvert.DeserializeObject<RateLimitModel>(resp_str).RetryAfter);

                        //uses the body and waits it out

                    }
                    else
                    {
                        request_success = true; //notify that our request was succesful

                        /*
                        Add a check for invalid username and/or password soon.
                         */
                        var resp_data = JsonConvert.DeserializeObject<LoginModel>(resp_str);

                        resp_token = resp_data.Token; // return our fancy token 
                    }
                }
                
                return resp_token;
            }
        }

        public async Task<string> GetGatewayUrl()
        {
            using(var request = new HttpRequestMessage(HttpMethod.Get, Endpoint + "/gateway"))
            {
                string response = await (await Client.SendAsync(request)).Content.ReadAsStringAsync();

                var model = JsonConvert.DeserializeObject<GatewayUrlModel>(response);

                return model.Url + $"?v={ApiVersion}&encoding=json";
            }
        }

    }
}