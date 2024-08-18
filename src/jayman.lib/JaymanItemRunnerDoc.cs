using jayman.lib.so;
using System.Diagnostics;

namespace jayman.lib
{
   public class JaymanItemRunnerDoc
   {
      private JObj JDoc;

      public JaymanItemRunnerDoc(JObj jsitem) => JDoc = jsitem;

      public string Name => JDoc["name"].ToString();

      public JObj Item => JDoc;

   }

   public static class JaymanItemRunnerDocFunctions
   {

      public static JaymanItemRunnerDoc RunJS(this JaymanItemRunnerDoc runner, string scriptName, IJaymanJSEngine engine, IJaymanVariables variables, Action<JaymanExecutionEventType, string> evtCallback)
      {
         foreach (var jsScriptEvent in runner.Item["event"].ToList.Where(j => j["listen"].ToString() == scriptName))
         {
            var jsScriptLines = jsScriptEvent["script"]["exec"];
            var jsScript = String.Join(Environment.NewLine, jsScriptLines?.ToList.Select(js => js.ToString()));

            if (!String.IsNullOrEmpty(jsScript))
            {
               engine.RunAsScoped(jsScript);
               engine.UpdateVariableStorage(variables);
               engine.TriggerNextExecutionEvent(evtCallback);
            }
         }
         return runner;
      }

      public static Func<JaymanExecuteResult> BuildAction(
          this JaymanItemRunnerDoc runner,
          IJaymanJSEngine engine,
          IJaymanHttpClient client,
          IJaymanVariables variables,
          Action<JaymanExecutionEventType, string> evtCallback,
          IJaymanRunnerListener console)
      {
         return () =>
         {
            console.OnProgressChanged(scriptName: runner.Name, currentstatus: "Wait");

            runner.RunJS(scriptName: "prerequest", engine, variables, evtCallback);

            var httpRequest = JaymanHttpRequestBuilder
               .Create(runner.Item["request"], variables)
               .AddAuthorization(variables)
               .AddHeader(variables)
               .AddBodyUrlEnconded(variables)
               .AddBodyRaw(variables)
               .Build();

            try
            {
               Stopwatch stopwatch = Stopwatch.StartNew();
               stopwatch.Start();

               var httpResponse = client.Fires(httpRequest);

               httpResponse.EnsureSuccessStatusCode();

               string response = httpResponse.GetResponse();

               stopwatch.Stop();

               console.OnProgressChanged(scriptName: runner.Name, currentstatus: ((int)httpResponse.StatusCode).ToString() + "-" + httpResponse.StatusCode.ToString(), time: stopwatch.ElapsedMilliseconds + " ms");

               engine.AddHttpResponse(response);

               runner.RunJS(scriptName: "prerequest", engine, variables, evtCallback);
            }
            catch (HttpRequestException httpEx)
            {
               console.OnProgressChanged(scriptName: runner.Name, fail: true, currentstatus: (httpEx.StatusCode != null ? ((int)httpEx.StatusCode + "-" + httpEx.StatusCode.ToString()) : "Fail"), message: httpEx.Message);
               return JaymanExecuteResult.Fail;
            }
            catch (Exception ex)
            {
               console.OnProgressChanged(scriptName: runner.Name, fail: true, currentstatus: "Fail", message: ex.Message);
               return JaymanExecuteResult.Fail;
            }
            return JaymanExecuteResult.Success;
         };
      }


   }
}
