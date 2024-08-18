namespace jayman.lib.utils
{
   public static class Utils
   {
      public static T RunWithException<T> (this Object obj, Func<T> run, Func<T> exception)
      {
         try { return run();  } catch { return exception(); }
      }

      public static void RunIgnoreExceptions(this Object obj, Action main)
      {
         try { main(); } catch {  }
      }
   }
}
