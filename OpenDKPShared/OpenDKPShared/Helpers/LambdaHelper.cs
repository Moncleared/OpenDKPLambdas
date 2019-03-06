using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Threading.Tasks;

namespace OpenDKPShared.Helpers
{
    /// <summary>
    /// Lambda helper function to call lambdas from within our own execution environment
    /// </summary>
    public static class LambdaHelper
    {
        /// <summary>
        /// Calls a lambda function async mode (Type.Event)
        /// </summary>
        /// <param name="pFunctionName">The name of the Lambda function to call</param>
        /// <param name="pPayload">The payload to send to the Lambda</param>
        /// <returns></returns>
        public static async Task<InvokeResponse> InvokeLambdaAsync(string pFunctionName, string pPayload)
        {
            InvokeResponse vResponse = null;
            AmazonLambdaConfig vConfig = new AmazonLambdaConfig
            {
                RegionEndpoint = RegionEndpoint.USEast2
            };

            using (AmazonLambdaClient vClient = new AmazonLambdaClient(vConfig))
            {
                InvokeRequest vRequest = new InvokeRequest
                {
                    FunctionName = pFunctionName,
                    InvocationType = InvocationType.Event
                };
                vRequest.Payload = pPayload;
                vResponse = await vClient.InvokeAsync(vRequest);
            }
            return vResponse;
        }
    }
}
