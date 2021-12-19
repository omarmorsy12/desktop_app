using app.structure.models.general;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace app.structure.models
{
    public class TagGroup
    {
        public ItemTranslation name;
        [JsonProperty("for")]
        public List<string> forRoles;
        public bool isPrimary;
    }
}
