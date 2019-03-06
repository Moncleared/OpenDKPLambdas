
using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;
using OpenDKPShared.ClientModels;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace InsertOrUpdateCharacter
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
            if (pRequest == null) return HttpHelper.HandleError("Request appears to be null", 500);

            using (opendkpContext vDatabase = new opendkpContext())
            { 
                if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                    return HttpHelper.HandleError("You do not have permission to perform this action", 401);
                var vCognitoUser = CognitoHelper.GetCognitoUser(pRequest.Headers["cognitoinfo"]);

                //We need to retrieve the ClientId for multitenancy purposes
                var vClientId = pRequest.Headers["clientid"];

                if (pRequest.HttpMethod.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
                {
                    return HandleInsert(pRequest, pContext, vCognitoUser, vDatabase, vClientId);
                }
                else if (pRequest.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                {
                    return HandleUpdate(pRequest, pContext, vCognitoUser, vDatabase, vClientId);
                }
            }
            return HttpHelper.HandleError("Only Methods PUT,POST are supported by this lamdba", 500);
        }

        private APIGatewayProxyResponse HandleUpdate(APIGatewayProxyRequest pRequest, ILambdaContext pContext, CognitoUser pCognitoUser, opendkpContext pDatabase, string pClientId)
        {
            var vResponse = HttpHelper.HandleError("[Update] Unknown error backend...", 500);
            try
            {
                Characters vModel = JsonConvert.DeserializeObject<Characters>(pRequest.Body);
                if (vModel != null)
                { 
                    Characters vExists = pDatabase.Characters.FirstOrDefault(x => x.ClientId.Equals(pClientId) && 
                    x.IdCharacter == vModel.IdCharacter);
                    if (vExists != null && vExists.IdCharacter >= 0)
                    {
                        string vOld = JsonConvert.SerializeObject(vExists, 
                            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        
                        //CustomCharacter class is a subset model of Characters, design to update only specific properties
                        CharacterModel vSimplifiedModel = new CharacterModel(vModel);
                        pDatabase.Entry(vExists).CurrentValues.SetValues(vSimplifiedModel);
                        pDatabase.SaveChanges();
                        AuditHelper.InsertAudit(pDatabase, pClientId, vOld, pRequest.Body, pCognitoUser.Username, Audit.ACTION_CHAR_UPDATE);
                        vResponse = HttpHelper.HandleResponse(vSimplifiedModel, 200);
                    } else
                    {
                        vResponse = HttpHelper.HandleError("Character not found in database", 500);
                    }
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(vException.Message, 500);
            }
            return vResponse;
        }

        private APIGatewayProxyResponse HandleInsert(APIGatewayProxyRequest pRequest, ILambdaContext pContext, CognitoUser pCognitoUser,opendkpContext pDatabase, string pClientId)
        {
            var vResponse = HttpHelper.HandleError("[Insert] Unknown error backend...", 500);

            try
            {
                Characters vModel = JsonConvert.DeserializeObject<Characters>(pRequest.Body);
                if (vModel != null)
                {
                    Characters vExists = pDatabase.Characters.FirstOrDefault(x => x.ClientId.Equals(pClientId) && 
                                            x.Name.Equals(vModel.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (vExists != null && vExists.IdCharacter >= 0)
                    {
                        vResponse = HttpHelper.HandleError("Character found in database, can't duplicate. Character Name must be unique", 500);
                    }
                    else
                    {
                        //Enforce ClientId serverside, don't trust what the client sends us
                        vModel.ClientId = pClientId;

                        pDatabase.Add(vModel);
                        pDatabase.SaveChanges();
                        AuditHelper.InsertAudit(pDatabase,pClientId, string.Empty, vModel, pCognitoUser.Username, Audit.ACTION_CHAR_INSERT);
                        vResponse = HttpHelper.HandleResponse(vModel, 200, true);
                    }
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError(vException.Message, 500);
            }

            return vResponse;
        }
    }
}
