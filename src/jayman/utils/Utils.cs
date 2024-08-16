using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jayman.utils
{
   public static class Utils
   {

      /// <summary>
      /// Safe Execution
      /// </summary>
      public static T SafeX<T> (this Object obj, Func<T> main, Func<T> exception)
      {
         try { return main();  } catch { return exception(); }
      }

      public static void Safe(this Object obj, Action main)
      {
         try { main(); } catch {  }
      }
   }
}
