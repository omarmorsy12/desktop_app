using System.Collections.Generic;

namespace app.structure.models
{
    public class Session
    {
        public string token { get; }
        public string role { get; }

        public List<string> ownedFeatures { get; }
        public List<string> permissions { get; }

        public List<string> ownedTags { get; }


        public Session(string token, string role, List<string> ownedFeatures, List<string> permissions, List<string> ownedTags)
        {
            this.token = token;
            this.role = role;
            this.ownedFeatures = ownedFeatures;
            this.permissions = permissions == null ? new List<string>() : permissions;
            this.ownedTags = ownedTags == null ? new List<string>() : ownedTags;
        }

        public override string ToString() {
            return token;
        }
    }
}
