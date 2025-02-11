﻿using jayman.lib.utils;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using System.Reflection;

namespace jayman.lib
{
  public enum JSEngineVariableType
  {
    Globals,
    Collection,
    Enviroment
  }

  public interface IJaymanJSEngine
  {
    public object RunJS(string js, bool discard);

    public void UpdateVariableStorage(IJaymanVariables updateVariables);

    public void TriggerNextExecutionEvent(Action<JaymanExecutionEventType, string> callback);

    public void InjectVariable(JSEngineVariableType type, string key, string value);
  }

  public class JaymanJSEngine : IJaymanJSEngine, IDisposable
  {
    public class JSConsole
    {
      public static void log(dynamic e) => Console.WriteLine($"V8Output> {Newtonsoft.Json.JsonConvert.SerializeObject(e)}");
    }

    private V8ScriptEngine engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports);

    private delegate object RequireAction(string module);

    public JaymanJSEngine() => Initialize();

    private void Initialize()
    {
      engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.EnableWebLoading | DocumentAccessFlags.AllowCategoryMismatch;
      engine.UseReflectionBindFallback = false;

      var js = @"

            const pm = {
                response : {
                    jresp : {},
                    json : function () { return this.jresp; }
                },
                execution : {
                    nextScript : '',
                    setNextRequest : function( scriptname ) {
                        this.nextScript = scriptname
                    }
                },
                globals : new Map(),
                collectionVariables : new Map(),
                environment : new Map()
            }

            function getglobalVariables() {
                const kvPair = []
                for (const [ckey, cvalue] of pm.globals.entries()) {
                    kvPair.push( { key: ckey, value : cvalue } )
                }
                return kvPair;
            }  

            function getcollectionVariables() {
                const kvPair = []
                for (const [ckey, cvalue] of pm.collectionVariables.entries()) {
                    kvPair.push( { key: ckey, value : cvalue } )
                }
                return kvPair;
            } 

            function getenviromentVariables() {
                const kvPair = []
                for (const [ckey, cvalue] of pm.environment.entries()) {
                    kvPair.push( { key: ckey, value: cvalue } )
                }
                return kvPair;
            }     

            function getNextScriptName() {
                const name = pm.execution.nextScript;
                pm.execution.nextScript = '';
                return name;
            }";

      RunJS(js, false);

      engine.AddHostType("console", typeof(JSConsole));


      engine.AddHostObject("atob", (RequireAction)((e) =>
      {
        string base64Encoded = e.ToString();

        int mod4 = base64Encoded.Length % 4;
        if (mod4 > 0)
        {
          base64Encoded += new string('=', 4 - mod4);
        }

        byte[] data = System.Convert.FromBase64String(base64Encoded);
        return System.Text.ASCIIEncoding.ASCII.GetString(data);
      }));

      engine.AddHostObject("getfile", (RequireAction)((e) => 
      { 
        string executablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        return File.ReadAllText(Path.Combine(executablePath, "./v8plugins/moment/moment.js"));
      }));

      engine.Execute(@"
         function require(moduleName) {
             var source = getfile(moduleName);
             var module = { exports: {} };
             var exports = module.exports;
             eval(source);
             return module.exports;
         }
         "
         );
    }

    public object RunJS(string js, bool discard) => discard ? engine.Evaluate("discarded", true, js) : engine.Evaluate(js);

    public void Dispose() => engine.Dispose();

    public void UpdateVariableStorage(IJaymanVariables updateVariables)
    {
      ParseVariableResult(new JsonObject(RunJS("getglobalVariables();", false)), updateVariables);
      ParseVariableResult(new JsonObject(RunJS("getcollectionVariables();", false)), updateVariables);
      ParseVariableResult(new JsonObject(RunJS("getenviromentVariables();", false)), updateVariables);
    }


    public class jkv
    {
      public string key { get; set; }
      public string value { get; set; }
    }

    private void ParseVariableResult(JObj result, IJaymanVariables updateVariables) =>
       this.RunIgnoreExceptions(() =>
       {
         Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

         var jvariables = result.ToList;

         for (int i = 0; i < jvariables.Count; i++)
         {
           string k = jvariables[i]["key"].ToString();
           string v = string.Empty;

           string valueType = Utils.RunHandleException(result, () => Convert.ToString(jvariables[i]["value"].Raw.Type), () => string.Empty);

           if (valueType == null)
             valueType = string.Empty;

           switch (valueType.ToLower())
           {
             case "date": v = jvariables[i]["value"].Value<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); break;
             default: v = jvariables[i]["value"].ToString(); break;
           }


           if (keyValuePairs.ContainsKey(k))
             keyValuePairs[k] = v;
           else
             keyValuePairs.Add(k, v);
         }

         updateVariables.Update(keyValuePairs);
       });


    public void TriggerNextExecutionEvent(Action<JaymanExecutionEventType, string> callback)
    {
      var jsScript = @$"getNextScriptName();";
      dynamic jsResponse = RunJS(jsScript, false);

      var scriptName = jsResponse is not Undefined ? Convert.ToString(jsResponse) : null;

      if (!string.IsNullOrEmpty(scriptName) && callback != null)
      {
        callback(JaymanExecutionEventType.NextExecution, scriptName);
      }
    }

    public void InjectVariable(JSEngineVariableType type, string key, string value)
    {
      switch (type)
      {
        case JSEngineVariableType.Globals:
          RunJS($"pm.globals.set('{key}','{value}')", true);
          break;
        case JSEngineVariableType.Collection:
          RunJS($"pm.collectionVariables.set('{key}','{value}')", true);
          break;
        case JSEngineVariableType.Enviroment:
          RunJS($"pm.environment.set('{key}','{value}')", true);
          break;
      }
    }
  }

  public static class IJaymanJSEngineFunctions
  {
    public static IJaymanJSEngine AddHttpResponse(this IJaymanJSEngine engine, string httpResponse)
    {
      engine.RunIgnoreExceptions(() =>
      {
        var js = @$" pm.response.jresp = {httpResponse};";
        engine.RunJS(js, false);
      });
      return engine;
    }

    public static IJaymanJSEngine RunAsScoped(this IJaymanJSEngine engine, string jScript)
    {
      var scopedScript = $"function scope() {{ {jScript} }}; scope(); ";
      engine.RunJS(scopedScript, false);
      return engine;
    }

  }
}
