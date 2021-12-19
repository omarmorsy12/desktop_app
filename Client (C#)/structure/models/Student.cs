using app.structure.models.general;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace app.structure.models
{
    public class Student
    {
        public string _id;
        public Name name;
        public string gender;
        public string nationality;
        public string religion;
        public Address address;
        [JsonProperty("date_of_birth")]
        public long dateOfBirth;
        [JsonProperty("registeration_date")]
        public long registerationDate;
        [JsonProperty("graduation_date")]
        public long? graduationDate;
        [JsonProperty("profile_image")]
        public string profileImage;
        [JsonProperty("medical_report")]
        public string medicalReport;
        public List<string> guardiansID;
        public List<string> tags;
    }

}
