namespace jayman
{
   public interface IJaymanVariables
    {
        void Update(Dictionary<string, string> vars);

        bool TryResolveMacro(string key, out string resolved);

        string MustFindValue(string key);

        void ClearVariables();
    }
}
