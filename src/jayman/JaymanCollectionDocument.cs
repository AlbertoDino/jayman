using Newtonsoft.Json;
using jayman.so;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jayman
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

        public static void Run(this JaymanCollectionDocument collection, IJaymanJSEngine engine, JaymanVariablesSession variables, IJaymanRunnerListener listener)
        {
            listener.CollectionStarted(collection.Name);
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            int fails = 0;
            int succeeded = 0;

            var items = collection.Items;
            int count = items.Count();

            using (JaymanHttpClient client = new JaymanHttpClient(clientHandler))
            {
                for (int index = 0; index< count; index++) 
                {
                    var runner = new JaymanItemRunner(items[index]);

                    var result = runner.Execute(engine, client, variables, 
                        (type,name ) => {
                            switch (type)
                            {
                                case JaymanScriptEventTypes.NextExecution:
                                    items.Clear();
                                    count = 1;
                                    index = 0;
                                    items.Add(collection.Items.FirstOrDefault( r => r["name"].ToString() == name));
                                    break;
                                default:
                                    break;
                            }
                        }, 
                        listener);

                    _ = result == JaymanItemRunnerResult.Success ? succeeded++ : fails++;

                    
                }

            }


            listener.OnUpdateSummary(new JaymanExecutionSummary()
            {
                Name = collection.Name,
                TotalFailed = fails,
                TotalSuccedded = succeeded,
                TotalRequests = collection.Items.Count()
            });
            listener.CollectionFinished();
        }
    }
}
