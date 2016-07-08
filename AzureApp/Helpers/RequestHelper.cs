// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class RequestHelper
    {
        const string JsonApplication = "application/json";

        public static Task<T> SendRequest<T>(HttpMethod method, string uri, string authToken, string version, object payload, List<HttpStatusCode> ignoredStatusCodes)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(authToken))
            {
                headers.Add("Authorization", authToken);
            }
            if (!string.IsNullOrEmpty(version))
            {
                headers.Add("x-ms-version", version);
            }
            return SendRequest<T>(method, uri, headers, payload, ignoredStatusCodes);
        }

        public static async Task<T> SendRequest<T>(HttpMethod method, string uri, IDictionary<string, string> headers, object payload, List<HttpStatusCode> ignoredStatusCodes)
        {
            var response = await SendRequestInternal(method, uri, headers, payload);
            if (response == null)
            {
                return default(T);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }

                // Return default value if expecting certain status codes
                if (ignoredStatusCodes != null && ignoredStatusCodes.Contains(response.StatusCode))
                {
                    return default(T);
                }

                // Get Error message - Try Json parsing first
                ErrorDetails error = new ErrorDetails();
                try
                {
                    var result = JsonConvert.DeserializeObject<JObject>(responseContent);
                    error.Message = result.Value<JObject>("error") != null
                    ? (string)result.SelectToken("error").SelectToken("message")
                    : (string)result.Value<JObject>("odata.error").SelectToken("message").SelectToken("value");
                }
                catch (JsonReaderException)
                {
                    // Try xml for RDFE responses
                    try
                    {
                        var serializer = new XmlSerializer(typeof(ErrorDetails));
                        using (var reader = new StringReader(responseContent))
                        {
                            error = (ErrorDetails)serializer.Deserialize(reader);
                        }
                    }
                    catch
                    {
                        // Default to raw body
                        error = new ErrorDetails() { Message = responseContent };
                    }
                }
                throw new Exception(error.Message);
            }
            catch (JsonReaderException)
            {
                throw new Exception("Unable to parse response from API.\nResource: {0}.\nResponse content: {1}".FormatInvariant(uri, responseContent));
            }
        }

        static Task<HttpResponseMessage> SendRequestInternal(HttpMethod method, string uri, IDictionary<string, string> headers, object payload, int maxRetries = 15)
        {
            return RetryWebRequest(
                () =>
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonApplication));

                    var request = new HttpRequestMessage(method, uri);
                    foreach (string name in headers.Keys)
                    {
                        request.Headers.Add(name, headers[name]);
                    }

                    if (payload != null)
                    {
                        request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, JsonApplication);
                    }

                    return client.SendAsync(request);
                },
                uri,
                maxRetries);
        }

        static async Task<HttpResponseMessage> RetryWebRequest(Func<Task<HttpResponseMessage>> httpRequest, string requestUri, int maxRetries = 15)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException("httpRequest");
            }

            HttpResponseMessage responseMessage = default(HttpResponseMessage);
            int retryCount = maxRetries;
            do
            {
                string statusCode = string.Empty;
                string exceptionMessage = string.Empty;
                string exceptionType = string.Empty;
                try
                {
                    responseMessage = await httpRequest();
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        // continue on success
                        break;
                    }

                    switch (responseMessage.StatusCode)
                    {
                        case HttpStatusCode.GatewayTimeout:
                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.ServiceUnavailable:
                        case HttpStatusCode.InternalServerError:
                            break;

                        // non retriable status codes
                        default:
                            return responseMessage;
                    }
                    statusCode = responseMessage.StatusCode.ToString();
                    exceptionType = responseMessage.ReasonPhrase;
                    exceptionMessage = await responseMessage.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    exceptionType = ex.GetType().ToString();
                    exceptionMessage = ex.Message;
                    if (!HandleHttpRequestException(ex))
                    {
                        throw ex;
                    }
                }
                catch (AggregateException aex)
                {
                    aex.Flatten().Handle(ex =>
                    {
                        if (retryCount == 0)
                        {
                            return false;
                        }

                        exceptionType = ex.GetType().ToString();
                        exceptionMessage = ex.Message;
                        if (ex is HttpRequestException)
                        {
                            return HandleHttpRequestException((HttpRequestException)ex);
                        }

                        // Retry ObjectDisposedException - happens on network issues
                        if (ex is ObjectDisposedException)
                        {
                            return true;
                        }

                        // Retry TaskCanceledException - request timeout
                        if (ex is TaskCanceledException)
                        {
                            return true;
                        }

                        return false;
                    });
                }

                // sleep on handled error conditions
                await Task.Delay(TimeSpan.FromSeconds((maxRetries - retryCount) * 2));
            }
            while (retryCount-- > 0);
            return responseMessage;
        }

        static bool HandleHttpRequestException(HttpRequestException ex)
        {
            WebException webException = ex.GetBaseException() as WebException;
            if (webException != null)
            {
                // Don't retry secure channel failures
                if (webException.Status == WebExceptionStatus.SecureChannelFailure)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
