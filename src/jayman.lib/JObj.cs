using jayman.lib.utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jayman.lib
{
   public interface JObj
   {
      public JObj this[string strKey] { get; }

      public JObj this[int index] { get; }

      public List<JObj> ToList { get; }

      public T Value<T>();

      bool Exist { get; }

      public string ToString();
   }


   public class JsonObject : JObj
   {

      private dynamic json;

      public JsonObject() => json = default;

      public JsonObject(dynamic _json) { this.json = _json; }

      public JObj this[string strKey] { get => new JsonObject(json).SafeNavigate(strKey); }

      public JObj this[int index] { get => new JsonObject(json[index]); }

      public T Value<T>() => this.RunWithExecption( 
                                    () => (T)((JValue)json).Value,
                                    () => default(T));

      public bool Exist => json != null;

      public List<JObj> ToList => this.RunWithExecption(
                                          () => ((IList<dynamic>)JsonConvert.DeserializeObject<IList<dynamic>>(JsonConvert.SerializeObject(json)))
                                                          .Select(d => (JObj)new JsonObject(d))
                                                          .ToList(),
                                          () => [new JsonObject(json)]);

      public override string ToString() => Value<string>();

      private JObj SafeNavigate(string strKey) => new JsonObject(this.RunWithExecption(() => json[strKey] , () => null ));

   }
}
