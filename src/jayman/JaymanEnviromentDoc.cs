using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jayman
{


   public class JaymanEnviromentDoc
   {

      private JObj jDoc;

      public void Load(JsonObject content) => jDoc = content;

      public string Name => (jDoc["name"]?.ToString() ?? string.Empty);

      public IList<JObj> Values => jDoc["values"].ToList;

   }

   public static class JaymanEnviromentDocFunctions
   {

      public static JaymanEnviromentDoc LoadFromFile(this JaymanEnviromentDoc env, string file)
      {
         dynamic doc = JsonConvert.DeserializeObject(File.ReadAllText(file));
         env.Load(new JsonObject(doc));
         return env;
      }

      public static JaymanEnviromentDoc UpdateVariableStorage(this JaymanEnviromentDoc env, IJaymanVariables updateVariables)
      {
         Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

         foreach (var value in env.Values)
         {
            keyValuePairs.Add(value["key"].ToString(), value["value"].ToString());
         }

         updateVariables.Update(keyValuePairs);
         return env;
      }

   }
}
