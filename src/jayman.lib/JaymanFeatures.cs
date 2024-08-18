namespace jayman.lib
{
   public enum JaymanFeatureType 
   { 
      SSLInsecure
   }

   public static class JaymanFeatures
   {
      private static Dictionary<JaymanFeatureType,bool> Features = new Dictionary<JaymanFeatureType,bool>();
      public static void Set(JaymanFeatureType type, bool value)
      {
         if (Features.ContainsKey(type))
            Features[type] = value;
         else
            Features.Add(type, value);
      }

      public static bool Get(JaymanFeatureType type)
      {
         if (Features.ContainsKey(type))
            return Features[type];
         else
            return false;
      }

   }
}

