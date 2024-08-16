using jayman.so;
using System.Text.RegularExpressions;

namespace jayman
{

   public delegate JaymanItemRunnerResult PostmanRequestExecutionAction(IJaymanHttpClient client, HttpRequestMessage request);

    public static class Jayman
    {
        public static HttpRequestMessage CreateHttpRequest(JObj jRequest, IJaymanVariables vars)
        {

            var method = jRequest["method"]?.ToString();

            var url = jRequest["url"]["raw"].Exist ? jRequest["url"]["raw"].ToString() : jRequest["url"].ToString();

            url = url.ReplaceWithPostmanVariables(vars);

            var request = new HttpRequestMessage(method.ParseToHttpMethod(), url);

            request.AddPostmanAuthorization(jRequest, vars);
            request.AddPostmanHeader(jRequest, vars);
            request.AddPostmanBodyUrlEnconded(jRequest, vars);
            request.AddPostmanBodyRaw(jRequest, vars);

            return request;
        }

        public static JaymanItemRunnerResult Execute(List<HttpRequestMessage> httpRequests, IJaymanHttpClient client, IJaymanVariables vars, PostmanRequestExecutionAction callback)
        {
            JaymanItemRunnerResult result = JaymanItemRunnerResult.Success;

            foreach (var httpRequest in httpRequests)
            {
                result &= callback(client, httpRequest);
            }

            return result;
        }

        public static string ReplaceWithPostmanVariables(this string input, IJaymanVariables vars)
        {
            var regex = new Regex("{{(.*?)}}");

            foreach (Match match in regex.Matches(input))
            {
                var key = match.Groups[1].Value;
                var variable = string.Empty;
                if (vars.TryResolveMacro(key, out variable))
                {
                    input = input.Replace(match.Value, variable);
                }
                else
                {
                    variable = vars.MustFindValue(key);
                    input = Regex.Replace(input, match.Value, variable);
                }
            }

            return input;
        }

        public static HttpMethod ParseToHttpMethod(this string input) => HttpMethod.Parse(input.ToUpper());

        public static void AddPostmanHeader(this HttpRequestMessage request, JObj jrequest, IJaymanVariables vars)
        {
            foreach (var header in jrequest["header"].ToList)
            {
                if (header["key"].ToString().ToLower() == "content-type")
                    continue;

                request.Headers.Add(
                    header["key"].ToString(),
                    header["value"].ToString().ReplaceWithPostmanVariables(vars)
                    );
            }
        }

        public static void AddPostmanAuthorization(this HttpRequestMessage request, JObj jrequest, IJaymanVariables vars)
        {
            var jAuth = jrequest["auth"];
            
            var type = jAuth["type"]?.ToString();

            if (type == "bearer")
            {
                foreach (var bearerItem in jAuth["bearer"].ToList)
                {
                    if (bearerItem["value"].Exist)
                    {
                        request.Headers.Add("Authorization", "Bearer " + bearerItem["value"].ToString().ReplaceWithPostmanVariables(vars));
                    }
                    if (bearerItem["token"].Exist)
                    {
                        request.Headers.Add("Authorization", "Bearer " + bearerItem["token"].ToString().ReplaceWithPostmanVariables(vars));
                    }

                }
            }
        }

        public static void AddPostmanBodyUrlEnconded(this HttpRequestMessage request, JObj jrequest, IJaymanVariables vars)
        {
            if (!jrequest["body"].Exist || jrequest["body"]["mode"].ToString() != "urlencoded")
                return;

            var collection = new List<KeyValuePair<string, string>>();

            foreach (var kvPair in jrequest["body"]["urlencoded"].ToList)
                collection.Add(new(kvPair["key"].ToString(), kvPair["value"].ToString().ReplaceWithPostmanVariables(vars)));
            
            request.Content = new FormUrlEncodedContent(collection);
        }

        public static void AddPostmanBodyRaw(this HttpRequestMessage request, JObj jrequest, IJaymanVariables vars)
        {
            if (!jrequest["body"].Exist || jrequest["body"]["mode"].ToString() != "raw")
                return;

            var content_type = jrequest["header"].ToList.FirstOrDefault(h => h["key"].ToString().ToLower() == "content-type");
            
            var bodyRaw = jrequest["body"]["raw"].ToString().ReplaceWithPostmanVariables(vars);

            if (content_type == null)
            {                
                if (jrequest["body"]["options"]?["raw"]?["language"].ToString() == "json")
                {
                    request.Content = new StringContent(bodyRaw, null, "application/json");
                }
                else
                {
                    request.Content = new StringContent(bodyRaw);
                }
            }
            else
                request.Content = new StringContent(bodyRaw, null, content_type["value"].ToString());

        }

    }
}
