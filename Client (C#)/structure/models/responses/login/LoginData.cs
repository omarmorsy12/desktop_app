using app.structure.models.general;
using System.Collections.Generic;

namespace app.structure.models.responses.login
{
    public class LoginData
    {
        public LoginDataAccount account;
        public LoginDataSession session;
        public List<string> multipleRoles;
    }

    public class LoginDataAccount
    {
        public string id;
        public string username;
        public LoginDataAccountInformation information;
        public LoginDataAccountSettings settings;
    }

    public class LoginDataSession
    {
        public string token;
        public string role;
        public bool require_role_index;
        public List<string> ownedFeatures;
        public List<string> ownedTags;
        public List<string> permissions;
    }

    public class LoginDataAccountInformation
    {
        public Name name;
        public string email;
        public string phone_number;
    }

    public class LoginDataAccountSettings
    {
        public string profile_image;
    }
}
