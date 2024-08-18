namespace jayman.lib.so
{
   public interface IJaymanHttpClient : IDisposable
   {
      IJaymanHttpResponse Fires(HttpRequestMessage request);
   }

   public class JaymanHttpClient : HttpClient, IJaymanHttpClient
   {
      public JaymanHttpClient() { }
      public JaymanHttpClient(HttpClientHandler handler) : base(handler) { }
      public IJaymanHttpResponse Fires(HttpRequestMessage request) => new JaymanHttpResponse(base.Send(request));

   }
}
