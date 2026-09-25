using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CustomAuthentication.Services
{
    public class BkashPaymentService
    {
        private readonly string baseUrl =
            ConfigurationManager.AppSettings["BkashBaseUrl"];

        private readonly string appKey =
            ConfigurationManager.AppSettings["BkashAppKey"];

        private readonly string appSecret =
            ConfigurationManager.AppSettings["BkashAppSecret"];

        private readonly string username =
            ConfigurationManager.AppSettings["BkashUsername"];

        private readonly string password =
            ConfigurationManager.AppSettings["BkashPassword"];


        // =====================================================
        // 1. Get Token
        // =====================================================

        public async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                var url =
                    baseUrl + "/tokenized/checkout/token/grant";

                client.DefaultRequestHeaders.Accept
                    .Add(
                        new MediaTypeWithQualityHeaderValue(
                            "application/json"
                        )
                    );

                client.DefaultRequestHeaders.Add(
                    "username",
                    username
                );

                client.DefaultRequestHeaders.Add(
                    "password",
                    password
                );


                var body = new
                {
                    app_key = appKey,
                    app_secret = appSecret
                };


                var json =
                    JsonConvert.SerializeObject(body);


                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );


                var response =
                    await client.PostAsync(
                        url,
                        content
                    );


                var result =
                    await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        "bKash Token Error: " + result
                    );
                }


                dynamic data =
                    JsonConvert.DeserializeObject(result);


                return data.id_token;
            }
        }


        // =====================================================
        // 2. Create Payment
        // =====================================================

        public async Task<dynamic> CreatePayment(
            decimal amount,
            string invoiceNumber,
            string callbackUrl)
        {
            var token =
                await GetToken();


            using (var client = new HttpClient())
            {
                var url =
                    baseUrl + "/tokenized/checkout/create";


                client.DefaultRequestHeaders.Accept
                    .Add(
                        new MediaTypeWithQualityHeaderValue(
                            "application/json"
                        )
                    );


                client.DefaultRequestHeaders.Add(
                    "Authorization",
                    token
                );


                client.DefaultRequestHeaders.Add(
                    "X-APP-Key",
                    appKey
                );


                var body = new
                {
                    mode = "0011",

                    payerReference = "",

                    callbackURL = callbackUrl,

                    amount = amount.ToString("0.00"),

                    currency = "BDT",

                    intent = "sale",

                    merchantInvoiceNumber = invoiceNumber
                };


                var json =
                    JsonConvert.SerializeObject(body);


                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );


                var response =
                    await client.PostAsync(
                        url,
                        content
                    );


                var result =
                    await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        "bKash Create Payment Error: "
                        + result
                    );
                }


                return JsonConvert.DeserializeObject(result);
            }
        }


        // =====================================================
        // 3. Execute Payment
        // =====================================================

        public async Task<dynamic> ExecutePayment(
            string paymentId)
        {
            var token =
                await GetToken();


            using (var client = new HttpClient())
            {
                var url =
                    baseUrl + "/tokenized/checkout/execute";


                client.DefaultRequestHeaders.Accept
                    .Add(
                        new MediaTypeWithQualityHeaderValue(
                            "application/json"
                        )
                    );


                client.DefaultRequestHeaders.Add(
                    "Authorization",
                    token
                );


                client.DefaultRequestHeaders.Add(
                    "X-APP-Key",
                    appKey
                );


                var body = new
                {
                    paymentID = paymentId
                };


                var json =
                    JsonConvert.SerializeObject(body);


                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );


                var response =
                    await client.PostAsync(
                        url,
                        content
                    );


                var result =
                    await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        "bKash Execute Payment Error: "
                        + result
                    );
                }


                return JsonConvert.DeserializeObject(result);
            }
        }


        // =====================================================
        // 4. Query Payment
        // =====================================================

        public async Task<dynamic> QueryPayment(
            string paymentId)
        {
            var token =
                await GetToken();


            using (var client = new HttpClient())
            {
                var url =
                    baseUrl + "/tokenized/checkout/general/searchTransaction";


                client.DefaultRequestHeaders.Accept
                    .Add(
                        new MediaTypeWithQualityHeaderValue(
                            "application/json"
                        )
                    );


                client.DefaultRequestHeaders.Add(
                    "Authorization",
                    token
                );


                client.DefaultRequestHeaders.Add(
                    "X-APP-Key",
                    appKey
                );


                var body = new
                {
                    paymentID = paymentId
                };


                var json =
                    JsonConvert.SerializeObject(body);


                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );


                var response =
                    await client.PostAsync(
                        url,
                        content
                    );


                var result =
                    await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        "bKash Query Payment Error: "
                        + result
                    );
                }


                return JsonConvert.DeserializeObject(result);
            }
        }
    }
}