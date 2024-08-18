﻿using jayman.lib.so;
using jayman.lib.utils;

namespace jayman.lib
{

   public interface IJayman
   {
      IJaymanRunnerListener Console { get; }
      IJaymanVariables Variables { get; }
      IJaymanJSEngine JSEngine { get; }
      Func<JaymanExecuteResult> Action { get; set; }
   }

   public class Jayman : IJayman
   {
      public IJaymanRunnerListener Console { get; set; }
      public IJaymanVariables Variables { get; set; }
      public IJaymanJSEngine JSEngine { get; set; }
      public Func<JaymanExecuteResult> Action { get; set; }
   }


   public static class JaymanBuilder
   {

      public static IJayman Create<C, V, J>()
         where C : IJaymanRunnerListener, new()
         where V : IJaymanVariables, new()
         where J : IJaymanJSEngine, new() => new Jayman()
         {
            Console = new C(),
            Variables = new V(),
            JSEngine = new J(),
            Action = () =>
            {
               Console.WriteLine("not enough arguments!");
               return JaymanExecuteResult.Success;
            }
         };

      public static IJayman ParseArguments(this IJayman jayman, string[] args)
      {
         var options = CLArguments.ParseArguments(args);

         foreach (var arg in options)
         {
            switch (arg.Key)
            {
               case "h":
               case "help":

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
                  jayman.JSEngine.InjectVariable(JSEngineVariableType.Globals, arg.Value.Split(':')[0], arg.Value.Split(':')[1]);
                  break;

               case "ic":
               case "injectCollection":
                  jayman.JSEngine.InjectVariable(JSEngineVariableType.Collection, arg.Value.Split(':')[0], arg.Value.Split(':')[1]);
                  break;

               case "ie":
               case "injectEnviroment":
                  jayman.JSEngine.InjectVariable(JSEngineVariableType.Enviroment, arg.Value.Split(':')[0], arg.Value.Split(':')[1]);
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
