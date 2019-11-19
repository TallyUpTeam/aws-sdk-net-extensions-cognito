using Amazon.Runtime;
using System.Threading.Tasks;

namespace Amazon.Extensions.CognitoAuthentication.Util
{
    /// <summary>
    /// The Unity code generator (see
    /// https://github.com/TallyUpTeam/aws-sdk-net/blob/master/generator/ServiceClientGeneratorLib/Generators/SourceFiles/ServiceClientUnity.tt)
    /// generates methods called '***Async' but which don't follow the modern
    /// async/await pattern of the other platforms - instead, they return void
    /// and take a callback argument to signal completion of the operation. This
    /// class provides a helper that wraps those service methods and returns a
    /// Task value that can be awaited by an async caller.
    ///
    /// Author: paul@tallyup.com
    /// </summary>
    public class AsyncServiceInvoker
    {
        protected delegate void ServiceMethod<TRequest, TResponse>(TRequest request, AmazonServiceCallback<TRequest, TResponse> callback, AsyncOptions options = null)
            where TRequest : AmazonWebServiceRequest
            where TResponse : AmazonWebServiceResponse;

        /// <summary>
        /// Invokes an Amazon service method (one that uses a callback to signal
        /// completion of an asynchronous operation), returning a Task that can
        /// be used to await the operation in an async calling method.
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="serviceMethod"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        // See https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types#from-apm-to-tap
        // TODO: support a CancellationToken? There doesn't seem to be a way to
        // actually cancel a service request but see
        // https://stackoverflow.com/questions/17819629/how-to-cancel-a-task-from-a-taskcompletionsource
        // and
        // https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Tasks/CancellationTokenTaskSource.cs
        // anyway. Maybe we can just let the async operation complete or timeout
        // and simply discard its result, having already signalled the Task that
        // it's been canceled?
        protected Task<TResponse> InvokeServiceAsync<TRequest, TResponse>(ServiceMethod<TRequest, TResponse> serviceMethod, TRequest request)
            where TRequest : AmazonWebServiceRequest
            where TResponse : AmazonWebServiceResponse
        {
            var tcs = new TaskCompletionSource<TResponse>();
            serviceMethod(request, result => {
                if (result.Exception != null)
                    tcs.TrySetException(result.Exception);
                else
                    tcs.TrySetResult(result.Response);
            });
            return tcs.Task;
        }
    }
}
