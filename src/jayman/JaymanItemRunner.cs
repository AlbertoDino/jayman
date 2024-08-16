using jayman.so;
using System.Diagnostics;
using System.Text;

namespace jayman
{

   public enum JaymanItemRunnerResult
   {
      Success = 1, Fail = 0
   }

   public class JaymanItemRunner
   {
      private JObj JDoc;

      public JaymanItemRunner(JObj jsitem) => JDoc = jsitem;

      public string Name => JDoc["name"].ToString();

      public JObj Item => JDoc;

   }

   public static class PostmanItemRunnerFunctions
   {
      public static JaymanItemRunnerResult Execute(
          this JaymanItemRunner runner,
          IJaymanJSEngine engine,
          IJaymanHttpClient client,
          IJaymanVariables variables,
          Action<JaymanScriptEventTypes, string> eventAction,
          IJaymanRunnerListener listener)
      {
         List<HttpRequestMessage> requestMessages = new List<HttpRequestMessage>();
         listener.OnProgressChanged(scriptName: runner.Name, currentstatus: "Wait");

         foreach (var jsScriptEvent in runner.Item["event"].ToList.Where(j => j["listen"].ToString() == "prerequest"))
         {
            var jsScriptLines = jsScriptEvent["script"]["exec"];
            var jsScript = String.Join(Environment.NewLine, jsScriptLines?.ToList.Select(js => js.ToString()));

            if (!String.IsNullOrEmpty(jsScript))
            {
               engine.RunAsScoped(jsScript);
               engine.UpdateVariableStorage(variables);
               engine.TriggerNextExecutionEvent(eventAction);
            }
         }

         requestMessages.Add(Jayman.CreateHttpRequest(runner.Item["request"], variables));

         return Jayman.Execute(
            requestMessages,
            client,
            variables,
            (httpClient, httpRequest) =>
            {
               try
               {
                  Stopwatch stopwatch = Stopwatch.StartNew();
                  stopwatch.Start();
                  var httpResponse = httpClient.Send(httpRequest);

                  httpResponse.EnsureSuccessStatusCode();

                  string response = string.Empty;

                  using (Stream stream = httpResponse.Content.ReadAsStream())
                  {
                     StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                     response = reader.ReadToEnd();
                  }

                  stopwatch.Stop();

                  listener.OnProgressChanged(scriptName: runner.Name, currentstatus: ((int)httpResponse.StatusCode).ToString() + "-" + httpResponse.StatusCode.ToString(), time: stopwatch.ElapsedMilliseconds + " ms");

                  engine.AddHttpResponse(response);

                  foreach (var jsScriptEvent in runner.Item["event"].ToList.Where(j => j["listen"].ToString() == "test"))
                  {
                     var jsScriptLines = jsScriptEvent["script"]?["exec"];
                     var jsScript = String.Join(Environment.NewLine, jsScriptLines?.ToList.Select(js => js.ToString()));

                     if (!String.IsNullOrEmpty(jsScript))
                     {
                        engine.RunAsScoped(jsScript);
                        engine.UpdateVariableStorage(variables);
                        engine.TriggerNextExecutionEvent(eventAction);
                     }
                  }

                  return JaymanItemRunnerResult.Success;
               }
               catch (HttpRequestException httpEx)
               {
                  listener.OnProgressChanged(scriptName: runner.Name, fail: true, currentstatus: (httpEx.StatusCode != null ? ((int)httpEx.StatusCode + "-" + httpEx.StatusCode.ToString()) : "Fail"), message: httpEx.Message);
                  return JaymanItemRunnerResult.Fail;
               }
               catch (Exception ex)
               {
                  listener.OnProgressChanged(scriptName: runner.Name, fail: true, currentstatus: "Fail", message: ex.Message);
                  return JaymanItemRunnerResult.Fail;
               }
            });




      }
   }
}
