namespace jayman.lib.so
{
   public interface IJaymanHttpClient
   {
      IJaymanHttpResponse Send(HttpRequestMessage request);
   }

   public class JaymanHttpClient : HttpClient, IJaymanHttpClient
   {
      public JaymanHttpClient() { }
      public JaymanHttpClient(HttpClientHandler handler) : base(handler) { }
      public new IJaymanHttpResponse Send(HttpRequestMessage request) => new JaymanHttpResponse(base.Send(request));

   }
}
