using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CognitoAPIs
{
    public class Function
    {
        private static string ADMIN_GROUP = "DKP_ADMIN";
        /// <summary>
        /// Lambda to fetch all characters from the character table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, not used currently</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers !=null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[CognitoAPIs] Unknown Error in backend", 500);
            try
            {
                string pUserPool = string.Empty;
                //The request should be from an authorized user, if yes then also grab the UserPool for the associated client
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    //Authorized Users only for Audit information
                    if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                    !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                        return HttpHelper.HandleError("You do not have permission to perform this action", 401);

                    //No validation on header because it's pre-performed above in the CognitoHelper
                    pUserPool = vDatabase.Clients.
                        FirstOrDefault(x => x.ClientId.Equals(pRequest.Headers["clientid"],
                                            StringComparison.InvariantCultureIgnoreCase)).UserPool;
                }

                if ( pRequest.HttpMethod == "GET")
                {
                    Task<object> vTask = GetAllUsersAsync(pUserPool);
                    vResponse = HttpHelper.HandleResponse(vTask.Result, 200);
                }
                if ( pRequest.HttpMethod == "POST")
                {
                    dynamic vModel = JsonConvert.DeserializeObject(pRequest.Body);
                    string vAction = vModel.Action;
                    string vData = vModel.Data;

                    if ( vAction.Equals("add-admin") )
                    {
                        Task<string> vTask = AddDkpAdminAsync(vData, pUserPool);
                        vResponse = HttpHelper.HandleResponse(vTask.Result, 200);
                    }
                    else if ( vAction.Equals("remove-admin") )
                    {
                        Task<string> vTask = RemoveDkpAdminAsync(vData, pUserPool);
                        vResponse = HttpHelper.HandleResponse(vTask.Result, 200);
                    }
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError("[CognitoAPIs] " + vException.Message, 500);
            }
            return vResponse;
        }

        private async Task<object> GetAllUsersAsync(string pUserPool)
        {
            AmazonCognitoIdentityProviderClient provider =
                new AmazonCognitoIdentityProviderClient(RegionEndpoint.USEast2);

            ListUsersRequest vRequest = new ListUsersRequest
            {
                UserPoolId = pUserPool
            };
            ListUsersInGroupResponse vGroupResponse = await provider.ListUsersInGroupAsync(new ListUsersInGroupRequest
            {
                GroupName = ADMIN_GROUP,
                UserPoolId = pUserPool
            });
            ListUsersResponse vResponse = await provider.ListUsersAsync(vRequest);
            var vReturn = new
            {
                Admins = vGroupResponse.Users,
                AllUsers = vResponse.Users
            };
            return vReturn;
        }

        private async Task<string> AddDkpAdminAsync(string pUsername, string pUserPool)
        {
            AmazonCognitoIdentityProviderClient provider =
                new AmazonCognitoIdentityProviderClient(RegionEndpoint.USEast2);
            AdminAddUserToGroupRequest vRequest = new AdminAddUserToGroupRequest
            {
                GroupName = ADMIN_GROUP,
                Username = pUsername,
                UserPoolId = pUserPool
            };
            var vResponse = await provider.AdminAddUserToGroupAsync(vRequest);
            return vResponse.HttpStatusCode.ToString();
        }

        private async Task<string> RemoveDkpAdminAsync(string pUsername, string pUserPool)
        {
            AmazonCognitoIdentityProviderClient provider =
                new AmazonCognitoIdentityProviderClient(RegionEndpoint.USEast2);
            AdminRemoveUserFromGroupRequest vRequest = new AdminRemoveUserFromGroupRequest
            {
                GroupName = ADMIN_GROUP,
                Username = pUsername,
                UserPoolId = pUserPool
            };
            var vResponse = await provider.AdminRemoveUserFromGroupAsync(vRequest);
            return vResponse.HttpStatusCode.ToString();
        }
    }
}
