using jayman.lib.so;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jayman.lib
{
   public static class JaymanContainer
   {      
      public static IJaymanHttpClient JaymanHttpClient { get; set; }
   }
}
