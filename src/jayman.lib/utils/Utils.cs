namespace jayman.lib.utils
{
   public static class Utils
   {

      /// <summary>
      /// Safe Execution
      /// </summary>
      public static T RunWithExecption<T> (this Object obj, Func<T> run, Func<T> exception)
      {
         try { return run();  } catch { return exception(); }
      }

      public static void RunIgnoreExceptions(this Object obj, Action main)
      {
         try { main(); } catch {  }
      }
   }
}
