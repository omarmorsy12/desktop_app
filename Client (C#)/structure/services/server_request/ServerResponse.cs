using app.structure.models.responses;
using System.Threading;
using System.Threading.Tasks;

namespace app.structure.services.server_request
{
    public class ServerResponse<Response> where Response : DefaultResponse
    {
        private CancellationTokenSource cancellation;
        public Task<Response> result;

        public bool isCanceled
        {
            get {
                return cancellation.IsCancellationRequested || result.IsCanceled;
            }
        }

        public ServerResponse(Task<Response> result, CancellationTokenSource cancellation)
        {
            this.result = result;
            this.cancellation = cancellation;
        }

        public void cancel()
        {
            cancellation.Cancel();
        }

    }
}
