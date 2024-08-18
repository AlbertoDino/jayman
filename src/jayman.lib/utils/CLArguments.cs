using System.Text.RegularExpressions;

namespace jayman.lib.utils
{
   public static class CLArguments
   {
      public static Dictionary<string, string> ParseArguments(string[] args)
      {
         var options = new Dictionary<string, string>();
         var flagPattern = new Regex(@"^-([a-zA-Z])$");
         var longOptionPattern = new Regex(@"^--([a-zA-Z0-9-]+)(?:=(.*))?$");
         var shortOptionsGroupedPattern = new Regex(@"^-([a-zA-Z]+)$");
         var positionalPattern = new Regex(@"^[^-].*");

         for (int i = 0; i < args.Length; i++)
         {
            var arg = args[i];

            if (flagPattern.IsMatch(arg))
            {
               var match = flagPattern.Match(arg);
               options[match.Groups[1].Value] = "true";
            }
            else if (longOptionPattern.IsMatch(arg))
            {
               var match = longOptionPattern.Match(arg);
               var key = match.Groups[1].Value;
               var value = match.Groups[2].Success ? match.Groups[2].Value : "true";
               options[key] = value;
            }
            else if (shortOptionsGroupedPattern.IsMatch(arg))
            {
               var match = shortOptionsGroupedPattern.Match(arg);
               foreach (char c in match.Groups[1].Value)
               {
                  options[c.ToString()] = "true";
               }
            }
            else if (positionalPattern.IsMatch(arg))
            {
               options[$"positional_{i}"] = arg;
            }
            else if (arg.StartsWith("--") && arg.Contains("="))
            {
               // Handle key-value pairs like --output=output.txt
               var split = arg.Substring(2).Split('=');
               options[split[0]] = split.Length > 1 ? split[1] : "true";
            }
            else if (arg.StartsWith("--"))
            {
               // Handle long option without value
               options[arg.Substring(2)] = "true";
            }
            else if (arg.StartsWith("-"))
            {
               // Handle short option without value
               options[arg.Substring(1)] = "true";
            }
            else
            {
               // Positional arguments
               options[$"positional_{i}"] = arg;
            }
         }

         return options;
      }
   }
}
