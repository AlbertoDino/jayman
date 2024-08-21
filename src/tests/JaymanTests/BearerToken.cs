using jayman.lib;

namespace jayman.Tests
{
   public class BearingTest
   {
      private JaymanCollectionDocument document;

      [SetUp]
      public void Setup()
      {
         document.LoadFromFile("data\\");
      }

      [Test]
      public void Test1()
      {
         Assert.Pass();
      }
   }
}