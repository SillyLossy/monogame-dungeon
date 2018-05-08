using Newtonsoft.Json;

namespace SpecialAdventure.Core.Common
{
    public class SaveManager
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
}
