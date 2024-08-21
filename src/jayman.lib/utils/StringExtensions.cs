using System.Text;

namespace jayman.lib.utils
{
   public static class JaymanConsoleStringExtensions
   {
      public static string Repeat(this string e, int repeat)
      {
         StringBuilder builder = new StringBuilder();
         for (int i = 0; i < repeat; i++)
         {
            builder.Append(e);
         }
         return builder.ToString();
      }

      public static string Cut(this string e, int maxChars)
      {
         if (e.Length <= maxChars)
            return e;
         return e.Substring(0, maxChars - 3) + "...";
      }


      public static string[] SplitByFirst(this string e, char value)
      {
         int index = e.IndexOf(value);
         return new string[] { e.Substring(0, index), e.Substring(index + 1) };
      }

   }
}
