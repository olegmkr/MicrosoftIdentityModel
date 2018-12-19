using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;

namespace MicrosoftIdentityModel
{
    public class RetryingHandler : DelegatingHandler
    {
        private readonly TimeSpan _perRequestTimeout;
        private readonly RetryPolicy _retryPolicy;
        //private readonly ILog _logger;

        public RetryingHandler(
            HttpMessageHandler innerHandler,
            TimeSpan perRequestTimeout
        )
            : base(innerHandler)
        {
            if (innerHandler == null) throw new ArgumentNullException(nameof(innerHandler));
            if (perRequestTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(perRequestTimeout));

            //_logger = LogManager.GetLogger(GetType());

            _retryPolicy = new RetryPolicy(new ErrorDetectionStrategy(), new FixedInterval(3, TimeSpan.FromSeconds(0)));

            _perRequestTimeout = perRequestTimeout;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentRetryCount = 0;
            EventHandler<RetryingEventArgs> handler = (sender, e) => currentRetryCount = e.CurrentRetryCount;
            _retryPolicy.Retrying += handler;

            try
            {
                return await _retryPolicy.ExecuteAsync(
                    async () =>
                    {
                        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        cts.CancelAfter(_perRequestTimeout);
                        var token = cts.Token;

                        try
                        {
                            return await base.SendAsync(request, token).ConfigureAwait(false);
                        }
                        catch (Exception exception)
                        {
                            //_logger.Error(JsonConvert.SerializeObject(new
                            //{
                            //    Mesage = $"Retrying failed request. Iteration: {currentRetryCount}",
                            //    Request = JsonConvert.SerializeObject(request),
                            //    ExceptionMessage = exception.Message,
                            //    StackTrace = exception.StackTrace
                            //}));

                            var wex = exception as WebException;
                            if (token.IsCancellationRequested || (wex != null && wex.Status == WebExceptionStatus.UnknownError))
                            {
                                throw new Exception("Timed out or disconnected", exception);
                            }

                            throw;
                        }
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _retryPolicy.Retrying -= handler;
            }
        }

        private class ErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                return true;
            }
        }
    }
}
