using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDKPShared.DBModels;
using OpenDKPShared.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminSettingsLambda
{
    public class Function
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRequest">Incoming API Gateway request object, not used currently</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest pRequest, ILambdaContext pContext)
        {
            if (pRequest.Headers != null && pRequest.Headers.Count > 0 && pRequest.Headers.Keys.Contains("warmup")) return HttpHelper.WarmupResponse();
            var vResponse = HttpHelper.HandleError("[AdminSettings] Unknown Error in backend", 500);
            //We need to retrieve the ClientId for multitenancy purposes
            var vClientId = pRequest.Headers["clientid"];

            try
            {
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    if ( pRequest.HttpMethod == "GET")
                    {
                        string vSettingName = pRequest.PathParameters["setting"];
                        var vResult = vDatabase.AdminSettings.Where(x => x.ClientId.Equals(vClientId) && x.SettingName.Equals(vSettingName, 
                                                                    StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if ( vResult == null )
                        {
                            vResult = new AdminSettings();
                        }
                        return HttpHelper.HandleResponse(JsonConvert.SerializeObject(vResult), 200); 
                    }
                    //This is the Guild Dump Utility I quickly wrote
                    if ( pRequest.HttpMethod == "POST")
                    {
                        //Authorize User for POST requests only
                        if (!CognitoHelper.IsDkpAdmin(pRequest) || 
                            !CognitoHelper.IsAuthorizedAdmin(vDatabase, pRequest))
                            return HttpHelper.HandleError("You do not have permission to perform this action", 401);

                        //Process Guild Dump
                        dynamic vModel = JsonConvert.DeserializeObject(pRequest.Body);
                        string vAction = vModel.Action;
                        JArray vData = vModel.Data;
                        if ( !string.IsNullOrWhiteSpace(vAction) )
                        {
                            var vCharacters = vData.ToObject<Characters[]>();
                            List<string> vCharacterNames = new List<string>();
                            foreach (Characters vCharacter in vCharacters) vCharacterNames.Add(vCharacter.Name);
                            var vResults = vDatabase.Characters.Where(x => x.ClientId.Equals(vClientId) && vCharacterNames.Contains(x.Name));
                            foreach(var item in vResults)
                            {
                                var vChar = vCharacters.FirstOrDefault(x => x.Name.Equals(item.Name));
                                if ( vChar != null )
                                {
                                    item.Level = vChar.Level;
                                    item.Rank = vChar.Rank;
                                    item.Class = vChar.Class;
                                }
                            }
                            vDatabase.SaveChanges();
                            return HttpHelper.HandleResponse("Success", 200);
                        } else {
                            AdminSettings vInputModel = JsonConvert.DeserializeObject<AdminSettings>(pRequest.Body);
                            AdminSettings vAdminModel = vDatabase.AdminSettings
                                .FirstOrDefault(x => x.ClientId.Equals(vClientId) && x.SettingName.Equals(vInputModel.SettingName,
                                                                          StringComparison.InvariantCultureIgnoreCase));

                            if (vAdminModel == null)
                            {
                                vAdminModel = vInputModel;
                                vAdminModel.ClientId = vClientId;
                                vDatabase.Add(vAdminModel);
                            }
                            vAdminModel.SettingValue = vInputModel.SettingValue;
                            vAdminModel.UpdatedBy = vInputModel.UpdatedBy;
                            vAdminModel.UpdatedTimestamp = vInputModel.UpdatedTimestamp;
                            vAdminModel.ClientId = vClientId;
                            vDatabase.SaveChanges();
                            return HttpHelper.HandleResponse(JsonConvert.SerializeObject(vAdminModel), 200);
                        }
                    }
                }
            }
            catch (Exception vException)
            {
                vResponse = HttpHelper.HandleError("[AdminSettings] Error Connecting to DB: " + vException.Message, 500);
            }
            return vResponse;
        }
    }
}
