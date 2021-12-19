using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Text;
using app.structure.models.responses;
using System.Threading;
using app.structure.services.server_request;
using app.structure.models.error;

namespace app.structure.services
{
    public class ServerRequestService : Services
    {
        public delegate void RequestSucceededAction<ResponseType>(ResponseType response) where ResponseType : DefaultResponse;
        public delegate void RequestFailedAction(ErrorState err);

        private static string PORT = "3000";
        private static string IP_ADDRESS = "localhost";

        public static string BASE_URL {
            get {
                return "http://" + IP_ADDRESS + ":" + PORT + "/";
            }
        }

        private static HttpClient client = new HttpClient();

        public ServerRequestService()
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        }

        public ServerRequestService(TimeSpan timeout)
        {
            client.Timeout = timeout;
        }

        private async Task<ResponseType> getJsonResult<ResponseType>(Task<HttpResponseMessage> result, RequestSucceededAction<ResponseType> onSuccess = null, RequestFailedAction onFail = null) where ResponseType : DefaultResponse
        {
            ResponseType response = null;

            try
            {
                HttpResponseMessage httpResponse = await result;
                string json = httpResponse.IsSuccessStatusCode ? await httpResponse.Content.ReadAsStringAsync() : "";
                response = JsonConvert.DeserializeObject<ResponseType>(json);

                if (response.err != null)
                {
                    onFail?.Invoke(response.err);
                }
                else
                {
                    onSuccess?.Invoke(response);
                }
            } catch (Exception e)
            {
                if (e is HttpRequestException || e is TaskCanceledException)
                {
                    ErrorState err = new ErrorState();
                    err.code = ErrorCodes.REQUEST_FAILED;
                    onFail?.Invoke(err);
                }
                else
                {
                    throw;
                }
            }

            return response;
        }

        private ServerResponse<ResponseType> method<ResponseType>(RequestType type, string url, object data = null, RequestSucceededAction < ResponseType> onSuccess = null, RequestFailedAction onFail = null) where ResponseType : DefaultResponse
        {
            HttpResponse response = getHttpResponse(type, url, data);
            Task<ResponseType> result = getJsonResult(response.result, onSuccess, onFail);

            return new ServerResponse<ResponseType>(result, response.cancellation);
        }

        private HttpResponse getHttpResponse(RequestType type, string url, object data = null)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            url = BASE_URL + url;

            Task<HttpResponseMessage> result;
            CancellationTokenSource cancellation = new CancellationTokenSource();

            switch (type)
            {
                case RequestType.GET:
                    result = client.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellation.Token);
                    break;
                case RequestType.DELETE:
                    result = client.DeleteAsync(url, cancellation.Token);
                    break;
                case RequestType.POST:
                    result = client.PostAsync(url, content, cancellation.Token);
                    break;
                case RequestType.PUT:
                    result = client.PutAsync(url, content, cancellation.Token);
                    break;
                default:
                    return null;
            }
            
            return new HttpResponse(result, cancellation);
        }

        public ServerResponse<ResponseType> get<ResponseType>(string url, RequestSucceededAction<ResponseType> onSuccess = null, RequestFailedAction onFail = null) where ResponseType : DefaultResponse
        {
            return method(RequestType.GET, url, null, onSuccess, onFail);
        }

        public ServerResponse<ResponseType> post<ResponseType>(string url, object data = null, RequestSucceededAction<ResponseType> onSuccess = null, RequestFailedAction onFail = null) where ResponseType : DefaultResponse
        {
            return method(RequestType.POST, url, data, onSuccess, onFail);
        }

        public ServerResponse<ResponseType> delete<ResponseType>(string url, RequestSucceededAction<ResponseType> onSuccess = null, RequestFailedAction onFail = null) where ResponseType : DefaultResponse
        {
            return method(RequestType.DELETE, url, null, onSuccess, onFail);
        }

        public ServerResponse<ResponseType> put<ResponseType>(string url, object data, RequestSucceededAction<ResponseType> onSuccess = null, RequestFailedAction onFail = null) where ResponseType : DefaultResponse
        {
            return method(RequestType.PUT, url, data, onSuccess, onFail);
        }

        public static void cancelPendingRequests()
        {
            client.CancelPendingRequests();
        }

    }

    enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }


    class HttpResponse
    {
        public Task<HttpResponseMessage> result;
        public CancellationTokenSource cancellation;

        public HttpResponse(Task<HttpResponseMessage> result, CancellationTokenSource cancellation)
        {
            this.result = result;
            this.cancellation = cancellation;
        }

    }

}
