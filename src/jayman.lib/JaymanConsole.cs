using jayman.lib.utils;
using System;

namespace jayman.lib
{
  public class JaymanConsole : IJaymanConsole
  {

    private int lastCursorTop = 0;
    private int lastCursorX = 0;
    private string lastScript = "";

    public void WaitInput()
    {

      Console.ReadKey();
    }

    public void ShowMenu()
    {
      Console.WriteLine("Usage: jayman [options]");
      Console.WriteLine("");
      Console.WriteLine("Options:");
      Console.WriteLine("-v, --version output version number ");
      Console.WriteLine("-h, --help                       display help menu ");
      Console.WriteLine("");
      Console.WriteLine("--enviroment=<file>              set the enviroment file ");
      Console.WriteLine("--collection=<file>              set the collection file ");
      Console.WriteLine("");
      Console.WriteLine("--injectCollection=<key>:<value> inject a collection variable into the request process");
      Console.WriteLine("--injectEnviroment=<key>:<value> inject a enviroment variable into the request process");
      Console.WriteLine("--injectGlobal=<key>:<value>     inject a global variable into the request process");
      Console.WriteLine("");
      Console.WriteLine("--insecure                       disable server certificate verification");
    }

    public void OnUpdateSummary(JaymanExecutionSummary summary)
    {
      if (summary == null)
        return;

      decimal successRate = (((decimal)summary.TotalSuccedded / (decimal)summary.TotalRequests)) * 100;
      decimal faileRate = (((decimal)summary.TotalFailed / (decimal)summary.TotalRequests)) * 100;

      var text = $"Summary: Total Requests: {summary.TotalRequests} | Successful: {summary.TotalSuccedded} ({successRate}%) | Failed: {summary.TotalFailed} ({faileRate}%) ";

      Console.Write(text);
      Console.WriteLine("");
      lastCursorTop = Console.CursorTop;
    }

    public void OnCollectionStarted(string name)
    {
      lastScript = "";
      Console.BackgroundColor = ConsoleColor.DarkMagenta;
      Console.ForegroundColor = ConsoleColor.White;
      Console.Write($"   {name}   :");
      Console.ResetColor();
      Console.WriteLine("");
      lastCursorTop = Console.CursorTop;
    }

    public void OnCollectionFinished()
    {
      Console.WriteLine("");

      lastCursorTop = Console.CursorTop;
    }

    public void OnRequestStarted(string request, string uri)
    {
      Console.WriteLine($"{request.PadRight(40)} ");
      Console.Write($"-> {uri.PadRight(70)} ");
      lastCursorX = Console.CursorLeft;
      Console.WriteLine("");
      Console.WriteLine("");
      Console.WriteLine("");
      lastCursorTop = Console.CursorTop;

    }

    public void OnRequestFinished(string status, bool fail, string time, string feedback)
    {
      Console.SetCursorPosition(lastCursorX, lastCursorTop - 3);
      Console.Write($"{" ".Repeat(50)}");
      Console.SetCursorPosition(lastCursorX, lastCursorTop - 3);
      Console.WriteLine("");
      Console.Write($"   Status : ");
      if (fail)
      {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
      }
      else
      {
        Console.BackgroundColor = ConsoleColor.Green;
        Console.ForegroundColor = ConsoleColor.White;
      }

      Console.Write($"   {status}   ");

      Console.ResetColor();

      if (!string.IsNullOrEmpty(time))
        Console.Write($" {time}");
      else
        Console.Write("   ");

      if (!string.IsNullOrEmpty(feedback))
      {
        Console.WriteLine("");
        Console.Write($"   Message: {feedback.Cut(80)} ");
      }
      else
        Console.Write(" ");
      Console.WriteLine("");
      Console.WriteLine("");
      lastCursorTop = Console.CursorTop;
    }
  }
}
