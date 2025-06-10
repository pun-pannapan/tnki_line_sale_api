using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

namespace tnki_line_sale_api.Utilities
{
    public class ConvertUtil
    {
        public static string obj2string(Object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
        }

        public static string LogRequest(IRestRequest request, RestClient _restClient)
        {
            return obj2string(new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = _restClient.BuildUri(request),
            });
        }

        public static string LogResponse(IRestResponse response)
        {
            return obj2string(new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            });
        }

        private static byte[] GetPassword(string inputString)
        {
            HashAlgorithm algorithm = SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetEncryptPassword(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetPassword(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string GetMyEncryptPassword(string inputString)
        {
            string hash = GetEncryptPassword(inputString + ",W0r@y0t#W%FSEComF0rt๘Biz@9854!");
            // SHA MAX Lenght is 40
            string modHash = hash.Substring(15, 20);
            Console.WriteLine("hash = {0}\nmod = {1}", hash, modHash);

            return modHash;
        }
    }
}
