namespace jayman.lib
{
  public class JaymanExecutionSummary
  {
    public string Name { get; set; }
    public int TotalRequests { get; set; }
    public int TotalFailed { get; set; }
    public int TotalSuccedded { get; set; }

  }

  public interface IJaymanConsole
  {
    void ShowMenu();
    void OnCollectionStarted(string name);
    void OnCollectionFinished();
    void OnRequestStarted(string request, string uri);
    void OnRequestFinished(string status, bool fail, string time, string feedback);
    void OnUpdateSummary(JaymanExecutionSummary summary);
  }
}
