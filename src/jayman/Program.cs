using jayman.lib;

public static class Program
{
   public static int Main(string[] args)
   {
      return (int)JaymanBuilder
         .Create<JaymanConsole, JaymanVariablesSession, JaymanJSEngine> ()
         .ParseArguments(args)
         .UseDefaultServices()
         .Run();
   }
}
