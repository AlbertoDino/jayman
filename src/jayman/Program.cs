using jayman.lib;

public static class Program
{
   public static int Main(string[] args)
   {
      return (int)JaymanBuilder.Create().ParseArguments(args).Run();
   }
}
