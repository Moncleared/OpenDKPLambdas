using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using OpenDKPShared.ClientModels;
using OpenDKPShared.DBModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace OpenDKPShared.Helpers
{
    public static class CognitoHelper
    {
        /// <summary>
        /// Quickly check to see if the CognitoUser is in the appropriate Group 
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public static bool IsDkpAdmin(APIGatewayProxyRequest pRequest)
        {
            try
            {
                if ( pRequest.Headers != null )
                {
                    return IsDkpAdmin(GetCognitoUser(pRequest.Headers["cognitoinfo"]));
                }
            }
            catch
            {
                Console.WriteLine("Exception caught in IsDkpAdmin APIGatewayProxyRequest");
            }
            return false;
        }
        /// <summary>
        /// Quickly check to see if the CognitoUser is in the appropriate Group
        /// </summary>
        /// <param name="pCognitoUser"></param>
        /// <returns></returns>
        public static bool IsDkpAdmin(CognitoUser pCognitoUser)
        {
            try
            {
                if ( pCognitoUser != null )
                {
                    if ( pCognitoUser.Groups.Contains("DKP_ADMIN"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Exception caught in IsDkpAdmin CognitoUser");
            }
            return false;
        }
        /// <summary>
        /// This function is not yet complete, but it is designed to validate the CognitoUser submitting a request
        /// Technically we're trusting what the user sends to us, in the future we need to validate this against the Cognito
        /// AWS SDK
        /// TODO: https://aws.amazon.com/premiumsupport/knowledge-center/decode-verify-cognito-json-token/
        /// </summary>
        /// <param name="pJwtToken"></param>
        /// <returns></returns>
        public static CognitoUser GetCognitoUser(string pJwtToken)
        {
            CognitoUser vReturnValue = null;
            try
            {
                var vJwtHandler = new JwtSecurityTokenHandler();
                if (vJwtHandler.CanReadToken(pJwtToken))
                {
                    var vToken = vJwtHandler.ReadJwtToken(pJwtToken);
                    var vUsername = vToken.Payload["cognito:username"];
                    var vNickname = vToken.Payload["nickname"];
                    var vEmail = vToken.Payload["email"];
                    var expDate = vToken.ValidTo;
                    if (DateTime.UtcNow.AddMinutes(1) > expDate)
                    {
                        //return null; TODO; Renable this later when I figure out the timezone stuff taking place
                    }
                    CognitoUser vUser = new CognitoUser
                    {
                        Username = vUsername as string,
                        Email = vEmail as string,
                        Nickname = vNickname as string,
                        Groups = new List<string>()
                    };
                    if (vToken.Payload.ContainsKey("cognito:groups"))
                    {
                        JArray vGroups = vToken.Payload["cognito:groups"] as JArray;
                        vUser.Groups = vGroups.ToObject<List<string>>();
                    }
                    vReturnValue = vUser;
                }
            }
            catch (Exception vException)
            {
                vReturnValue = null;
                Console.WriteLine(vException.Message);
            }
            return vReturnValue;
        }

        /// <summary>
        /// This function is designed to Compare the IAM Role associated with the  UserGroup to the ClientID passed in
        /// If the Roles don't match, we don't allow the request to come through
        /// </summary>
        /// <param name="vDatabase">dbcontext</param>
        /// <param name="pArnString">The string passed in by the API Gateway stating who the user is</param>
        /// <param name="pClientId">The client id the user is requesting to update for</param>
        /// <returns></returns>
        public static bool IsAuthorizedAdmin(opendkpContext vDatabase, APIGatewayProxyRequest pRequest)
        {
            //The Context Contains the Identity & Request contains the header with the client id
            //string pArnString, string pClientId
            try
            {
                string[] vArnItems = pRequest.RequestContext.Identity.UserArn.Split('/');
                string vAssumedRole = string.Empty;
                string vClientId = pRequest.Headers["clientid"];
                if (vArnItems.Length > 1)
                    vAssumedRole = vArnItems[1];

                if (!string.IsNullOrWhiteSpace(vAssumedRole))
                {
                    var vClientModel = vDatabase.Clients.FirstOrDefault(x => x.ClientId.Equals(vClientId, StringComparison.InvariantCultureIgnoreCase));
                    if (vClientModel != null)
                    {
                        if (vClientModel.AssumedRole.Equals(vAssumedRole, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("IsAuthorizedAdmin exception encountered");
            }

            return false;
        }
    }
}
