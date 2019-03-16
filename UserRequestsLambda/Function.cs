using System;
using System.Linq;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using OpenDKPShared.Helpers;
using OpenDKPShared.DBModels;
using OpenDKPShared.ClientModels;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace UserRequestsLambda
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
            if (pRequest.Headers != null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Unknown Backend error", 500);

            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            try
            {
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    //For these operations, we should have the CognitoInfo user available to us
                    if ( pRequest.Headers != null && !pRequest.Headers.ContainsKey("cognitoinfo"))
                        return HttpHelper.HandleError("You do not have permission to perform this action", 401);

                    CognitoUser vCognitoUser = CognitoHelper.GetCognitoUser(pRequest.Headers["cognitoinfo"]);
                    if (pRequest.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //If the GET request has PathParameter of the account, fetch all requests for given account
                        //Otherwise return all requests
                        if (pRequest != null && pRequest.PathParameters != null && pRequest.PathParameters.Count > 0)
                        {
                            string vAccountName = pRequest.PathParameters["account"];
                            vResponse = GetAccountRequests(vDatabase, vAccountName, vClientId);
                        }
                        else
                        {
                            vResponse = GetAllRequests(vDatabase, vClientId);
                        }
                    }

                    //PUT can come from any user, such as a standard user who wants to assign a character to themselves
                    //or credit for a raid tick. 
                    if (pRequest.HttpMethod.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (vCognitoUser == null) return HttpHelper.HandleError("You do not have permission to perform this action", 401);
                        UserRequests vUserRequest = JsonConvert.DeserializeObject<UserRequests>(pRequest.Body);
                        if (vUserRequest.RequestType == UserRequests.TYPE_CHARACTER_ASSIGN)
                        {
                            vResponse = HandleCharacterAssignRequest(vDatabase, vUserRequest, pRequest, vCognitoUser);
                        }
                        if (vUserRequest.RequestType == UserRequests.TYPE_RAIDTICK)
                        {
                            vResponse = HandleRaidTickRequest(vDatabase, vUserRequest, pRequest, vCognitoUser);
                        }
                    }

                    //POST can only come from admins, need to make sure cognito Groups contains "DKP_ADMIN" or "SITE_ADMIN"
                    //This would be for approving or denying requests
                    if (pRequest.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!CognitoHelper.IsDkpAdmin(pRequest) ||
                            !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                            return HttpHelper.HandleError("You do not have permission to perform this action", 401);
                        vCognitoUser = CognitoHelper.GetCognitoUser(pRequest.Headers["cognitoinfo"]);

                        UserRequests vUserRequest = JsonConvert.DeserializeObject<UserRequests>(pRequest.Body);
                        if (vUserRequest.RequestType == UserRequests.TYPE_CHARACTER_ASSIGN)
                        {
                            vResponse = UpdateCharacterAssignRequest(vDatabase, vClientId, vUserRequest, vCognitoUser);
                        }
                        if (vUserRequest.RequestType == UserRequests.TYPE_RAIDTICK)
                        {
                            vResponse = UpdateRaidTickRequest(vDatabase, vUserRequest, vCognitoUser, vClientId);
                        }
                    }
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query: "+vException.Message, 500);
            }

            return vResponse;
        }

        private APIGatewayProxyResponse GetAllRequests(opendkpContext pDatabase, string pClientId)
        {
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query", 500);
            var vResults = pDatabase.UserRequests.Where(x => x.ClientId.Equals(pClientId)).ToList();
            foreach(UserRequests vRequest in vResults)
            {
                InjectRequestDetails(pDatabase, pClientId, vRequest);
            }
            vResponse = HttpHelper.HandleResponse(vResults, 200, true);
            return vResponse;
        }

        private APIGatewayProxyResponse GetAccountRequests(opendkpContext pDatabase, string pAccount, string pClientId)
        {
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query", 500);
            var vResults = pDatabase.UserRequests.
                Where(x => x.ClientId.Equals(pClientId) && x.Requestor.ToLower().Equals(pAccount.ToLower(), StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            foreach (UserRequests vRequest in vResults)
            {
                InjectRequestDetails(pDatabase, pClientId, vRequest);
            }
            vResponse = HttpHelper.HandleResponse(vResults, 200, true);
            return vResponse;
        }

        private APIGatewayProxyResponse UpdateRaidTickRequest(opendkpContext pDatabase, UserRequests pUserRequest, CognitoUser pCognitoUser, string pClientId)
        {
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query", 500);

            if (!string.IsNullOrWhiteSpace(pUserRequest.Requestor) && !string.IsNullOrWhiteSpace(pUserRequest.RequestDetails))
            {
                if (pUserRequest.RequestStatus == UserRequests.STATUS_APPROVED)
                {
                    dynamic vModel = JsonConvert.DeserializeObject(pUserRequest.RequestDetails);
                    //`{characterName:${this.CharacterName},tickId=${this.SelectedData.TickId},reason:${this.LogData}}`;
                    string vCharacterName = vModel.characterName;
                    int vTickId = vModel.tickId;
                    string vReason = vModel.reason;

                    //I need to associate the character
                    //If character associate is successful, I need to update pending request to completed
                    using (var dbContextTransaction = pDatabase.Database.BeginTransaction())
                    {    
                        //First, lets make sure the Character Exists in the Database!
                        Characters vCharacter = CharacterHelper.GetCharacterByName(pDatabase, pClientId, vCharacterName);
                        if (vCharacter == null)
                        {
                            return HttpHelper.HandleError(string.Format("{0} does not exist in the database...", vCharacterName), 500);
                        }

                        //Determine if this character already has the raid tick
                        var vFound = pDatabase.TicksXCharacters.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.IdCharacter == vCharacter.IdCharacter && x.IdTick == vTickId);
                        if (vFound != null)
                        {
                            return HttpHelper.HandleError("You already have credit for this raid tick!", 500);
                        }

                        TicksXCharacters vTickToInsert = new TicksXCharacters();
                        vTickToInsert.IdCharacter = vCharacter.IdCharacter;
                        vTickToInsert.IdTick = vTickId;
                        vTickToInsert.ClientId = pClientId;
                        pDatabase.Add(vTickToInsert);

                        //Update the Request
                        UserRequests vRequest = pDatabase.UserRequests.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.Id == pUserRequest.Id);
                        if (vRequest == null)
                        {
                            return HttpHelper.HandleError("[UserRequestsLambda] Couldn't find the Request in DB to update...", 500);
                        }

                        vRequest.RequestApprover = pCognitoUser.Username;
                        vRequest.ReviewedTimestamp = DateTime.Now;
                        vRequest.RequestStatus = UserRequests.STATUS_APPROVED;
                        vRequest.ClientId = pClientId;

                        pDatabase.SaveChanges();
                        dbContextTransaction.Commit();

                        //Update Caches
                        int vStatus = CacheManager.UpdateSummaryCacheAsync(pClientId).GetAwaiter().GetResult();
                        Console.WriteLine("SummaryCacheResponse=" + vStatus);
                        vResponse = HttpHelper.HandleResponse(vRequest, 200, true);
                    }
                }
                else if (pUserRequest.RequestStatus == UserRequests.STATUS_DENIED)
                {
                    UserRequests vRequest = pDatabase.UserRequests.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.Id == pUserRequest.Id);
                    if (vRequest == null)
                    {
                        return HttpHelper.HandleError("[UserRequestsLambda] Couldn't find the Request in DB to update...", 500);
                    }
                    vRequest.RequestApprover = pCognitoUser.Username;
                    vRequest.ReviewedTimestamp = DateTime.Now;
                    vRequest.RequestStatus = UserRequests.STATUS_DENIED;
                    vRequest.RequestDetails = pUserRequest.RequestDetails;
                    vRequest.ClientId = pClientId;

                    pDatabase.SaveChanges();
                    vResponse = HttpHelper.HandleResponse(vRequest, 200, true);
                }
            }
            return vResponse;
        }

        private APIGatewayProxyResponse UpdateCharacterAssignRequest(opendkpContext pDatabase, string pClientId, UserRequests pUserRequest, CognitoUser pCognitoUser)
        {
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query", 500);
            if (!string.IsNullOrWhiteSpace(pUserRequest.Requestor) && !string.IsNullOrWhiteSpace(pUserRequest.RequestDetails))
            {
                if ( pUserRequest.RequestStatus == UserRequests.STATUS_APPROVED )
                {
                    dynamic vCharacterId = JsonConvert.DeserializeObject(pUserRequest.RequestDetails);
                    int vId = vCharacterId.characterId;

                    //I need to associate the character
                    //If character associate is successful, I need to update pending request to completed
                    using (var dbContextTransaction = pDatabase.Database.BeginTransaction())
                    {
                        UserXCharacter vAssociation = new UserXCharacter();
                        vAssociation.User = pUserRequest.Requestor;
                        vAssociation.IdCharacter = vId;
                        vAssociation.ApprovedBy = pCognitoUser.Username;
                        vAssociation.ClientId = pClientId;
                        pDatabase.Add(vAssociation);

                        UserRequests vRequest = pDatabase.UserRequests.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.Id == pUserRequest.Id);
                        if (vRequest == null)
                        {
                            return HttpHelper.HandleError("[UserRequestsLambda] Couldn't find the Request in DB to update...", 500);
                        }
                        vRequest.RequestApprover = pCognitoUser.Username;
                        vRequest.ReviewedTimestamp = DateTime.Now;
                        vRequest.RequestStatus = UserRequests.STATUS_APPROVED;
                        vRequest.ClientId = pClientId;

                        pDatabase.SaveChanges();
                        dbContextTransaction.Commit();
                        vResponse = HttpHelper.HandleResponse(vRequest, 200, true);
                    }
                }
                else if (pUserRequest.RequestStatus == UserRequests.STATUS_DENIED)
                {
                    UserRequests vRequest = pDatabase.UserRequests.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.Id == pUserRequest.Id);
                    if (vRequest == null)
                    {
                        return HttpHelper.HandleError("[UserRequestsLambda] Couldn't find the Request in DB to update...", 500);
                    }
                    vRequest.RequestApprover = pCognitoUser.Username;
                    vRequest.ReviewedTimestamp = DateTime.Now;
                    vRequest.RequestStatus = UserRequests.STATUS_DENIED;
                    vRequest.RequestDetails = pUserRequest.RequestDetails;
                    vRequest.ClientId = pClientId;

                    pDatabase.SaveChanges();
                    vResponse = HttpHelper.HandleResponse(vRequest, 200, true);
                }
            }
            return vResponse;
        }

        private APIGatewayProxyResponse HandleRaidTickRequest(opendkpContext pDatabase, UserRequests pUserRequest, APIGatewayProxyRequest pRequest, CognitoUser pCognitoUser)
        {
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query", 500);
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            if (!string.IsNullOrWhiteSpace(pUserRequest.RequestDetails))
            {
                dynamic vModel = JsonConvert.DeserializeObject(pUserRequest.RequestDetails);
                //`{characterName:${this.CharacterName},tickId=${this.SelectedData.TickId},reason:${this.LogData}}`;
                string vCharacterName = vModel.characterName;
                int vTickId = vModel.tickId;
                string vReason = vModel.reason;

                //First, lets make sure the Character Exists in the Database!
                Characters vCharacter = CharacterHelper.GetCharacterByName(pDatabase, vClientId,vCharacterName);
                if ( vCharacter == null )
                {
                    return HttpHelper.HandleError(string.Format("{0} does not exist in the database...",vCharacterName), 500);
                }

                //Second, lets confirm the Character is associated with the Account
                var vUser = pDatabase.UserXCharacter.FirstOrDefault(x => x.ClientId.Equals(vClientId) && x.IdCharacter == vCharacter.IdCharacter && x.User.Equals(pCognitoUser.Username));
                if ( vUser == null )
                {
                    return HttpHelper.HandleError(string.Format("{0} is not associated with your account", vCharacterName), 500);
                }

                //Determine if this character already has the raid tick
                var vFound = pDatabase.TicksXCharacters.FirstOrDefault(x => x.ClientId.Equals(vClientId) && x.IdCharacter==vCharacter.IdCharacter && x.IdTick==vTickId);
                if (vFound != null)
                {
                    return HttpHelper.HandleError("You already have credit for this raid tick!", 500);
                }

                //Verify that there is not already an open pending request
                var vPendingRequests = pDatabase.UserRequests.Where(x => x.ClientId.Equals(vClientId) && 
                                                                         x.RequestType == UserRequests.TYPE_RAIDTICK &&
                                                                         x.RequestStatus == UserRequests.STATUS_PENDING).ToList();
                foreach (var vRequest in vPendingRequests)
                {
                    dynamic vTmpModel = JsonConvert.DeserializeObject(vRequest.RequestDetails);
                    string vTmpName = vTmpModel.characterName;
                    int vTmpId = vTmpModel.tickId;
                    if (vTmpId == vTickId && vCharacterName.Trim().Equals(vTmpName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return HttpHelper.HandleError("There is already a pending request for this character & raid tick!", 500);
                    }
                }

                pUserRequest.Requestor = pCognitoUser.Username;
                pUserRequest.RequestStatus = UserRequests.STATUS_PENDING;
                pUserRequest.RequestTimestamp = DateTime.Now;
                pUserRequest.RequestApprover = "NONE";
                pUserRequest.ReviewedTimestamp = DateTime.Now;
                pUserRequest.ClientId = vClientId;
                pDatabase.Add(pUserRequest);
                pDatabase.SaveChanges();
                vResponse = HttpHelper.HandleResponse(pUserRequest, 200, true);
            }
            return vResponse;
        }

        private APIGatewayProxyResponse HandleCharacterAssignRequest(opendkpContext pDatabase, UserRequests pUserRequest, APIGatewayProxyRequest pRequest, CognitoUser pCognitoUser)
        {
            var vResponse = HttpHelper.HandleError("[UserRequestsLambda] Issue with DB or Query", 500);            
            
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];
            if (!string.IsNullOrWhiteSpace(pUserRequest.RequestDetails))
            {
                dynamic vCharacterId = JsonConvert.DeserializeObject(pUserRequest.RequestDetails);
                int vId = vCharacterId.characterId;

                //Determine if we've already got a character associated for this character Id
                var vFound = pDatabase.UserXCharacter.FirstOrDefault(x => x.ClientId.Equals(vClientId) && x.IdCharacter == vId);
                if (vFound != null)
                {
                    return HttpHelper.HandleError("[UserRequestsLambda] This character has already been associated to an account", 500);
                }

                //Verify that there is not already an open pending request for the specific character
                //Note: It's possible someone else tries to claim someone elses character and this request would block them
                //TODO: Review note above, determine if anything should be done regarding this scenario
                var vPendingRequests = pDatabase.UserRequests.Where(x => x.ClientId.Equals(vClientId) && 
                                                                         x.RequestType == UserRequests.TYPE_CHARACTER_ASSIGN &&
                														 x.RequestStatus == UserRequests.STATUS_PENDING).ToList();
                foreach(var vRequest in vPendingRequests)
                {
                    dynamic vTmpCharId = JsonConvert.DeserializeObject(vRequest.RequestDetails);
                    int vTmpId = vTmpCharId.characterId;
                    if ( vTmpId == vId)
                    {
                        return HttpHelper.HandleError("[UserRequestsLambda] There is already a pending request for this character", 500);
                    }
                }
                pUserRequest.Requestor = pCognitoUser.Username;
                pUserRequest.RequestStatus = UserRequests.STATUS_PENDING;
                pUserRequest.RequestTimestamp = DateTime.Now;
                pUserRequest.RequestApprover = "NONE";
                pUserRequest.ReviewedTimestamp = DateTime.Now;
                pUserRequest.ClientId = vClientId;
                pDatabase.Add(pUserRequest);
                pDatabase.SaveChanges();
                vResponse = HttpHelper.HandleResponse(pUserRequest, 200, true);
            }
            return vResponse;
        }
        public void InjectRequestDetails(opendkpContext pDatabase, string pClientId, UserRequests pRequest)
        {
            JsonSerializerSettings vSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            if (pRequest.RequestType == UserRequests.TYPE_CHARACTER_ASSIGN)
            {
                dynamic vModel = JsonConvert.DeserializeObject(pRequest.RequestDetails);
                int vId = vModel.characterId;
                Characters vCharacter = pDatabase.Characters.FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.IdCharacter == vId);
                vModel.character = JsonConvert.SerializeObject(vCharacter, vSettings);
                pRequest.RequestDetails = JsonConvert.SerializeObject(vModel, vSettings);
            }

            if (pRequest.RequestType == UserRequests.TYPE_RAIDTICK)
            {
                dynamic vModel = JsonConvert.DeserializeObject(pRequest.RequestDetails);
                string vCharacterName = vModel.characterName;
                int vTickId = vModel.tickId;
                string vReason = vModel.reason;
                
                var vFound = pDatabase.TicksXCharacters.
                    Include("IdTickNavigation.Raid").
                    FirstOrDefault(x => x.ClientId.Equals(pClientId) && x.IdTick == vTickId);
                object vCustomModel = null;
                if ( vFound !=null )
                {
                    vCustomModel = new
                    {
                        Character = vCharacterName,
                        Raid = vFound.IdTickNavigation.Raid.Name,
                        vFound.IdTickNavigation.Raid.Timestamp,
                        Tick = vFound.IdTickNavigation.Description,
                        DkpValue = vFound.IdTickNavigation.Value
                    };
                }
                else
                {
                    vCustomModel = new
                    {
                        Character = vCharacterName,
                        Raid = "UNKNOWN",
                        Timestamp = DateTime.Now,
                        Tick = "UNKNOWN",
                        DkpValue = 0
                    };
                }
                 
                vModel.raid = JsonConvert.SerializeObject(vCustomModel, vSettings);

                pRequest.RequestDetails = JsonConvert.SerializeObject(vModel, vSettings);
            }
        }
    }
}
