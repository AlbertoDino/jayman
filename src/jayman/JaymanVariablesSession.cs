using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jayman
{
    public class JaymanVariablesSession : IJaymanVariables
    {
        public Dictionary<string, string> Variables = new Dictionary<string, string>();
        public Dictionary<string, Func<string,string>> Macros = new Dictionary<string, Func<string, string>>();

        public JaymanVariablesSession()
        {
            Macros.Add("$isoTimestamp",(e) => { return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); });
        }

        public void Update(Dictionary<string, string> vars)
        {
            foreach (var variable in vars)
            {
                if (Variables.ContainsKey(variable.Key))
                {
                    Variables[variable.Key] = variable.Value;
                }
                else
                { 
                    Variables.Add(variable.Key, variable.Value);
                }
            }
        }

        public void ClearVariables() => Variables.Clear();

        public string MustFindValue(string key)
        {
            if (!Variables.ContainsKey(key))
            {
                throw new ArgumentException($"Key [{key}] not found as Variable");
            }
            return Variables[key];
        }

        public bool TryResolveMacro(string key,out string resolved)
        {
            bool macro = Macros.ContainsKey(key);
            if (macro)
                resolved = Macros[key](key);
            else resolved = key;
            return macro;
        }
    }
}
