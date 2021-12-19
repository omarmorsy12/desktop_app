namespace app.structure.models.user
{
    public class UserAccount
    {
        public string id { get; }
        public UserAccountName name { get; }
        public string email { get; }
        public string username { get; }
        public string phoneNumber { get; }

        public UserAccountSettings settings { get; }

        public UserAccount(string id, string username, UserAccountName name, string email, string phoneNumber, UserAccountSettings settings)
        {
            this.id = id;
            this.username = username;
            this.name = name;
            this.email = email;
            this.phoneNumber = phoneNumber;
            this.settings = settings;
        }
    }

    public class UserAccountName
    {
        public string first { get; }
        public string last { get; }

        public UserAccountName(string first, string last)
        {
            this.first = first;
            this.last = last;
        }
    }

    public class UserAccountSettings
    {
        public string profileImage { get; }

        public UserAccountSettings(string profileImage)
        {
            this.profileImage = profileImage;
        }
    }
}
