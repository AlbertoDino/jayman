using Newtonsoft.Json;

namespace jayman.lib
{
  public class JaymanCollectionDocument
  {
    private JObj jDocument;
    public void Load(JObj content) => jDocument = content;
    public string Name => (jDocument["info"]["name"].ToString() ?? string.Empty);
    public IList<JObj> Items => jDocument["item"].ToList;
  }

  public static class JaymanCollectionDocumentFunctions
  {
    public static JaymanCollectionDocument LoadFromFile(this JaymanCollectionDocument collection, string file)
    {
      var doc = JsonConvert.DeserializeObject(File.ReadAllText(file));
      collection.Load(new JsonObject(doc));
      return collection;
    }

    public static Func<JaymanExecuteResult> BuildAction(this JaymanCollectionDocument collection, IJaymanJSEngine engine, IJaymanVariables variables, IJaymanConsole listener)
    {
      return () =>
      {
        listener.OnCollectionStarted(collection.Name);

        int fails = 0;
        int succeeded = 0;

        var items = collection.Items;
        int count = items.Count();

        var client = JaymanContainer.JaymanHttpClient;

        for (int index = 0; index < count; index++)
        {
          var runner = new JaymanItemRunnerDoc(items[index]);
          var action = runner.BuildAction(engine, client, variables,
              (type, name) =>
              {
                switch (type)
                {
                  case JaymanExecutionEventType.NextExecution:
                    items.Clear();
                    count = 1;
                    index = -1;
                    items.Add(collection.Items.FirstOrDefault(r => r["name"].ToString() == name));
                    break;
                  default:
                    break;
                }
              },
              listener);

          _ = action() == JaymanExecuteResult.Success ? succeeded++ : fails++;
        }

        listener.OnUpdateSummary(new JaymanExecutionSummary()
        {
          Name = collection.Name,
          TotalFailed = fails,
          TotalSuccedded = succeeded,
          TotalRequests = collection.Items.Count()
        });
        listener.OnCollectionFinished();

        return JaymanExecuteResult.Success;
      };
    }
  }
}
