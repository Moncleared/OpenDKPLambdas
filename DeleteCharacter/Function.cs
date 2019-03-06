using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.Helpers;
using OpenDKPShared.DBModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeleteCharacter
{
    public class Function
    {
        /// <summary>
        /// Lambda to insert character to character table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, should be a PUT or POST with a BODY</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[DeleteCharacter] Unknown Backend error", 500);
            try
            {
                if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                {
                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];
                    
                    string vCharacterName = pRequest.PathParameters["character"];
                    using (opendkpContext vDatabase = new opendkpContext())
                    {
                        //Authorized Users only for Deleting Characters
                        if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                        !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                            return HttpHelper.HandleError("You do not have permission to perform this action", 401);
                        var vCognitoUser = CognitoHelper.GetCognitoUser(pRequest.Headers["cognitoinfo"]);

                        var vResult = vDatabase.
                            Characters.
                            FirstOrDefault(x => x.ClientId.Equals(vClientId) &&
                                                x.Name.Equals(vCharacterName, StringComparison.InvariantCultureIgnoreCase));

                        if ( vResult != null )
                        {
                            vDatabase.Characters.Remove(vResult);
                            vDatabase.SaveChanges();
                            AuditHelper.InsertAudit(vDatabase, vClientId, vResult, string.Empty, vCognitoUser.Username, Audit.ACTION_CHAR_DELETE);
                            vResponse = HttpHelper.HandleResponse(vResult, 200);
                        }
                        else
                        {
                            vResponse = HttpHelper.HandleError("[DeleteCharacter] Character doesn't exist in the DB", 500);
                        }
                    }
                }
            }
            catch
            {
                vResponse = HttpHelper.HandleError("[DeleteCharacter] Issue with DB or Query", 500);
            }

            return vResponse;
        }
    }
}
