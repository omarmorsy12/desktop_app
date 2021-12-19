using app.windows.login.components;

namespace app.structure.models.requests
{
    class LoginRequestBody
    {
        public string id;
        public string password;
        public int? role_index;
        public bool session_only;

        public LoginRequestBody(InputComponent id, InputComponent password)
        {
            this.id = id.getText();
            this.password = password.getText();
        }

        public LoginRequestBody(InputComponent id, InputComponent password, int role_index, bool session_only = true)
        {
            this.id = id.getText();
            this.password = password.getText();
            this.role_index = role_index;
            this.session_only = session_only;
        }
    }
}
