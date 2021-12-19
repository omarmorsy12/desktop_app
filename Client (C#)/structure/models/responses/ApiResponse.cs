namespace app.structure.models.responses
{
    public class ApiResponse<Data, Info> : DefaultResponse
    {
        public Data data;
        public Info information;
    }
}
