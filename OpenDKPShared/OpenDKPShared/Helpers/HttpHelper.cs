using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using OpenDKPShared.DBModels;

namespace OpenDKPShared.Helpers
{
    /// <summary>
    /// Simple HTTP helper methods
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Objective is to determine if this request was triggered from CloudWatch or similar, AKA NOT from API Gateway
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public static bool TriggeredLambda(APIGatewayProxyRequest pRequest)
        {
            return pRequest != null && pRequest.Resource == null && pRequest.Path == null && pRequest.HttpMethod == null;
        }
        public static APIGatewayProxyResponse HandleError(string pErrorMessage, int pStatusCode)
        {
            var vHeaders = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" }
            };
            return new APIGatewayProxyResponse
            {
                Headers = vHeaders,
                Body = JsonConvert.SerializeObject(new { ErrorMessage = pErrorMessage, StatusCode = pStatusCode }),
                StatusCode = pStatusCode,
            };
        }
        /// <summary>
        /// Generates a APIGatewayProxyResponse by packaging the body, status code
        /// </summary>
        /// <param name="pBody">The object to serialize using JsonConvert.Serialize</param>
        /// <param name="pStatusCode">Status Code to return</param>
        /// <param name="pIgnoreCircularRef">Whether or not  to include circular references, default is false</param>
        /// <returns></returns>
        public static APIGatewayProxyResponse HandleResponse(object pBody, int pStatusCode, bool pIgnoreCircularRef = false)
        {
            var settings = new JsonSerializerSettings();
            if ( pIgnoreCircularRef )
            {
                settings.ReferenceLoopHandling= ReferenceLoopHandling.Ignore;
            }
            var vHeaders = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" }
            };
            return new APIGatewayProxyResponse
            {
                Headers = vHeaders,
                Body = JsonConvert.SerializeObject(pBody, settings),
                StatusCode = pStatusCode,
            };
        }
        /// <summary>
        /// A simple response for lambda warmups
        /// </summary>
        /// <returns></returns>
        public static APIGatewayProxyResponse WarmupResponse()
        {
            Console.WriteLine("Warmup Detected");
            using (opendkpContext vDatabase = new opendkpContext())
            {
                vDatabase.Pools.ToList<Pools>();
            }
            var vHeaders = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" }
            };
            return new APIGatewayProxyResponse
            {
                Headers = vHeaders,
                Body = JsonConvert.SerializeObject("Warmup Complete"),
                StatusCode = 200,
            };
        }
    }
}
