using jayman.lib.so;
using jayman.lib.utils;

namespace jayman.lib
{

  public interface IJayman
  {
    IJaymanConsole Console { get; }
    IJaymanVariables Variables { get; }
    IJaymanJSEngine JSEngine { get; }
    Func<JaymanExecuteResult> Action { get; set; }
  }

  public class Jayman : IJayman
  {
    public IJaymanConsole Console { get; set; }
    public IJaymanVariables Variables { get; set; }
    public IJaymanJSEngine JSEngine { get; set; }
    public Func<JaymanExecuteResult> Action { get; set; }
  }


  public static class JaymanBuilder
  {

    public static IJayman Create<C, V, J>()
       where C : IJaymanConsole, new()
       where V : IJaymanVariables, new()
       where J : IJaymanJSEngine, new() => new Jayman()
       {
         Console = new C(),
         Variables = new V(),
         JSEngine = new J(),
         Action = () =>
           {
             return JaymanExecuteResult.Success;
           }
       };

    public static IJayman ParseArguments(this IJayman jayman, string[] args)
    {
      var options = CommandLineArguments.ParseArguments(args);

      foreach (var arg in options)
      {
        switch (arg.Key)
        {
          case "v":
          case "version":
            {
              System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
              System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
              string version = fvi.FileVersion;
              Console.WriteLine($"jayman v {version}");
            }

            break;
          case "h":
          case "help":
            jayman.Console.ShowMenu();
            break;

          case "enviroment":
            new JaymanEnviromentDoc()
               .LoadFromFile(arg.Value)
               .UpdateVariables(jayman.Variables);
            break;

          case "collection":
            jayman.Action = new JaymanCollectionDocument()
               .LoadFromFile(arg.Value)
               .BuildAction(jayman.JSEngine,
                  jayman.Variables,
                  jayman.Console);
            break;

          case "ig":
          case "injectGlobal":
            jayman.JSEngine.InjectVariable(JSEngineVariableType.Globals, arg.Value.SplitByFirst(':')[0], arg.Value.SplitByFirst(':')[1]);
            break;

          case "ic":
          case "injectCollection":
            jayman.JSEngine.InjectVariable(JSEngineVariableType.Collection, arg.Value.SplitByFirst(':')[0], arg.Value.SplitByFirst(':')[1]);
            break;

          case "ie":
          case "injectEnviroment":
            jayman.JSEngine.InjectVariable(JSEngineVariableType.Enviroment, arg.Value.SplitByFirst(':')[0], arg.Value.SplitByFirst(':')[1]);
            break;

          case "insecure":
            JaymanFeatures.Set(JaymanFeatureType.SSLInsecure, true);
            break;
        }
      }

      return jayman;
    }

    public static IJayman UseDefaultServices(this IJayman jayman)
    {

      if (JaymanFeatures.Get(JaymanFeatureType.SSLInsecure))
      {
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        JaymanContainer.JaymanHttpClient = new JaymanHttpClient(clientHandler);
      }
      else
      {
        JaymanContainer.JaymanHttpClient = new JaymanHttpClient();
      }

      return jayman;
    }

    public static JaymanExecuteResult Run(this IJayman jayman) => jayman.Action();

  }
}