using System;
using System.Linq;
using Amazon.Lambda.Core;
using OpenDKPShared.DBModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BatchOps
{
    public class Function
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="pInput">Incoming API Gateway request object, used to pull parameter from url</param>
        /// <param name="pContext">Incoming Lambda Context object, not used currently</param>
        /// <returns></returns>
        public void FunctionHandler(string pInput, ILambdaContext pContext)
        {
            try
            {
                using (opendkpContext vDatabase = new opendkpContext())
                {
                    var vChanges = new Dictionary<string, string>();
                    var vCharacters = vDatabase.Characters.Include("TicksXCharacters.IdTickNavigation.Raid");
                    foreach(Characters vCharacter in vCharacters)
                    {
                        var vTickCount = vCharacter.TicksXCharacters.Where(x => DateTime.Compare(x.IdTickNavigation.Raid.Timestamp.Date, 
                                                                                 DateTime.Now.AddDays(-60).Date) >= 0).Count();

                        if (vTickCount > 0 && vCharacter.Active == 0)
                        {
                            vCharacter.Active = 1;
                            Console.WriteLine(string.Format("{0} Marked ACTIVE, Total Ticks = {1}", vCharacter.Name, vTickCount));
                            vChanges.Add(vCharacter.Name, "Marked Active, Total Ticks = "+ vTickCount);
                        }
                        if (vTickCount <= 0 && vCharacter.Active == 1)
                        {
                            vCharacter.Active = 0;
                            Console.WriteLine(string.Format("{0} Marked INACTIVE, Total Ticks = {1}", vCharacter.Name, vTickCount));
                            vChanges.Add(vCharacter.Name, "Marked Inactive, Total Ticks = " + vTickCount);
                        }
                    }
                    vDatabase.SaveChanges();
                }
            }
            catch(Exception vException)
            {
                Console.WriteLine(vException);
            }
        }
    }
}
