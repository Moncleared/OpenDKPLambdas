using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetAllCharacters
{
    public class Function
    {
        /// <summary>
        /// Lambda to fetch all characters from the character table
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, not used currently</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers !=null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[GetAllCharacters] Unknown Error in backend", 500);
            try
            {
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    //We need to retrieve the ClientId for multitenancy purposes
                    var vClientId = pRequest.Headers["clientid"];
                    if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                    {
                        string vAccountName = pRequest.PathParameters["account"].Trim();
                        var vTmpList = vDatabase.UserXCharacter
                                            .Where(x => x.ClientId.Equals(vClientId) && x.User.Trim().Equals(vAccountName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        var vResult = new List<Characters>();
                        var vList = new List<int>();
                        foreach(var vCharacter in vTmpList)
                        {
                            vList.Add(vCharacter.IdCharacter);
                        }
                        vResult = vDatabase.Characters.Where(x => x.ClientId.Equals(vClientId) && vList.Contains(x.IdCharacter)).ToList();
                        vResponse = HttpHelper.HandleResponse(vResult, 200, true);
                    }
                    else
                    {
                        object vResult = null;
                        if (pRequest.QueryStringParameters != null && pRequest.QueryStringParameters.Count > 0 &&
                            pRequest.QueryStringParameters.ContainsKey("IncludeInactives"))
                        {
                            vResult = vDatabase.Characters.Where(x=>x.ClientId.Equals(vClientId)).ToList<Characters>();
                        }
                        else
                        {
                            vResult = vDatabase.Characters.Where(x => x.ClientId.Equals(vClientId) && 
                                                                      x.Active == 1).ToList<Characters>();
                        }
                        vResponse = HttpHelper.HandleResponse(vResult, 200);
                    }
                }
            }
            catch(Exception vException)
            {
                vResponse = HttpHelper.HandleError("[GetAllCharacters] " + vException.Message, 500);
            }
            return vResponse;
        }
    }
}
