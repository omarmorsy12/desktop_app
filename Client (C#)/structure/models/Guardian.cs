using Newtonsoft.Json;

namespace app.structure.models
{
    public class GuardianIdentification
    {
        public string id;
        public string type;
    }

    public class Guardian
    {
        public string type;
        public string fullname;
        [JsonProperty("phone_number")]
        public string phoneNumber;
        public GuardianIdentification identification;
        public string email;
        [JsonProperty("profile_image")]
        public string profileImage;
    }
}
