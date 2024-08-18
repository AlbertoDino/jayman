namespace jayman.lib.so
{
   public interface IJaymanHttpRequest
   {
      public JObj jDescriptor { get; }
      public HttpRequestMessage Request { get; }

   }

   public class JaymanHttpRequest : IJaymanHttpRequest
   {
      public JObj jDescriptor { get; set; }
      public HttpRequestMessage Request { get; set; }
   }

   public static class JaymanHttpRequestBuilder
   {

      public static IJaymanHttpRequest Create(JObj jDescriptor, IJaymanVariables vars)
      {
         var method = jDescriptor["method"]?.ToString();
         var url = jDescriptor["url"]["raw"].Exist ? jDescriptor["url"]["raw"].ToString() : jDescriptor["url"].ToString();
         url = url.ReplaceWithJaymanVariable(vars);
         return new JaymanHttpRequest() { Request = new HttpRequestMessage(method.ParseToHttpMethod(), url), jDescriptor = jDescriptor };
      }

      public static HttpRequestMessage Build(this IJaymanHttpRequest request) => request.Request;

      public static HttpMethod ParseToHttpMethod(this string input) => HttpMethod.Parse(input.ToUpper());

      public static IJaymanHttpRequest AddHeader(this IJaymanHttpRequest request, IJaymanVariables vars)
      {
         foreach (var header in request.jDescriptor["header"].ToList)
         {
            if (header["key"].ToString().ToLower() == "content-type")
               continue;

            request.Request.Headers.Add(
                header["key"].ToString(),
                header["value"].ToString().ReplaceWithJaymanVariable(vars)
                );
         }
         return request;
      }

      public static IJaymanHttpRequest AddAuthorization(this IJaymanHttpRequest request, IJaymanVariables vars)
      {
         var jAuth = request.jDescriptor["auth"];

         var type = jAuth["type"]?.ToString();

         if (type == "bearer")
         {
            foreach (var bearerItem in jAuth["bearer"].ToList)
            {
               if (bearerItem["value"].Exist)
               {
                  request.Request.Headers.Add("Authorization", "Bearer " + bearerItem["value"].ToString().ReplaceWithJaymanVariable(vars));
               }
               if (bearerItem["token"].Exist)
               {
                  request.Request.Headers.Add("Authorization", "Bearer " + bearerItem["token"].ToString().ReplaceWithJaymanVariable(vars));
               }

            }
         }
         return request;
      }

      public static IJaymanHttpRequest AddBodyUrlEnconded(this IJaymanHttpRequest request, IJaymanVariables vars)
      {
         if (request.jDescriptor["body"].Exist && request.jDescriptor["body"]["mode"].ToString() == "urlencoded")
         {
            var collection = new List<KeyValuePair<string, string>>();

            foreach (var kvPair in request.jDescriptor["body"]["urlencoded"].ToList)
               collection.Add(new(kvPair["key"].ToString(), kvPair["value"].ToString().ReplaceWithJaymanVariable(vars)));

            request.Request.Content = new FormUrlEncodedContent(collection);
         }
         return request;
      }

      public static IJaymanHttpRequest AddBodyRaw(this IJaymanHttpRequest request, IJaymanVariables vars)
      {
         if (request.jDescriptor["body"].Exist && request.jDescriptor["body"]["mode"].ToString() == "raw")
         {

            var content_type = request.jDescriptor["header"].ToList.FirstOrDefault(h => h["key"].ToString().ToLower() == "content-type");

            var bodyRaw = request.jDescriptor["body"]["raw"].ToString().ReplaceWithJaymanVariable(vars);

            if (content_type == null)
            {
               if (request.jDescriptor["body"]["options"]?["raw"]?["language"].ToString() == "json")
               {
                  request.Request.Content = new StringContent(bodyRaw, null, "application/json");
               }
               else
               {
                  request.Request.Content = new StringContent(bodyRaw);
               }
            }
            else
               request.Request.Content = new StringContent(bodyRaw, null, content_type["value"].ToString());
         }
         return request;
      }
   }
}
