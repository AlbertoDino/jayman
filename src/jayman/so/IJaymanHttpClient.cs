using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jayman.so
{
    public interface IJaymanHttpClient
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }

    public class JaymanHttpClient : HttpClient,IJaymanHttpClient
    {

        public JaymanHttpClient() { }
        public JaymanHttpClient(HttpClientHandler handler) : base(handler) { }
        public new HttpResponseMessage Send(HttpRequestMessage request) =>  base.Send(request);
        
    }
}
