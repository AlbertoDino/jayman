using System.Net;
using System.Text;

namespace jayman.lib.so
{
   public interface IJaymanHttpResponse
   {
      void EnsureSuccessStatusCode();

      Stream ReadAsStream();

      HttpStatusCode StatusCode { get; }

      string GetResponse();
   }

   public class JaymanHttpResponse(HttpResponseMessage _message) : IJaymanHttpResponse
   {
      private HttpResponseMessage Message = _message;

      public HttpStatusCode StatusCode => Message.StatusCode;

      public void EnsureSuccessStatusCode() => Message.EnsureSuccessStatusCode();

      public Stream ReadAsStream() => Message.Content.ReadAsStream();

      public string GetResponse()
      {
         using (Stream stream = ReadAsStream())
         {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
         }
      }
   }
}
