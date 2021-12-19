namespace app.structure.models.requests
{
    public class SessionRequestBody
    {
        public string token;

        public SessionRequestBody()
        {
            token = App.session.token;
        }
    }
}
