namespace jayman.lib
{
   public class JaymanExecutionSummary
   {
      public string Name { get; set; }
      public int TotalRequests { get; set; }
      public int TotalFailed { get; set; }
      public int TotalSuccedded { get; set; }

   }

   public interface IJaymanRunnerListener
   {
      void CollectionStarted(string name);
      void CollectionFinished();
      void OnProgressChanged(string scriptName, string currentstatus, bool fail = false, string message = "", string time = "");
      void OnUpdateSummary(JaymanExecutionSummary summary);
   }
}
