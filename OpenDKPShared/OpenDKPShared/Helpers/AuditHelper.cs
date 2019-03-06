using Newtonsoft.Json;
using OpenDKPShared.DBModels;
using System;

namespace OpenDKPShared.Helpers
{
    public class AuditHelper
    {
        /// <summary>
        /// Insert an audit entry into the audit table. This function will Serialize the two objects passed into JSON format for storage
        /// </summary>
        /// <param name="pDBContext">The database context to which the audit will be stored</param>
        /// <param name="pOldModel">An object representing an old value (changed from)</param>
        /// <param name="pNewModel">An object representing a new value (changed to)</param>
        /// <param name="pUsername">The username who made the change</param>
        /// <param name="pAction">The action being performed</param>
        public static void InsertAudit(opendkpContext pDBContext, string pClientId, object pOldModel, object pNewModel, string pUsername, string pAction)
        {
            try
            {
                InsertAudit(pDBContext,
                    pClientId,
                    JsonConvert.SerializeObject(pOldModel, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                    JsonConvert.SerializeObject(pNewModel, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                    pUsername, 
                    pAction);
            }
            catch(Exception vException)
            {
                Console.WriteLine("Failed to log audit for some reason");
                Console.WriteLine(vException);
            }
        }
        /// <summary>
        /// Insert an audit entry into the audit table. This function will store the text passed in
        /// </summary>
        /// <param name="pDBContext">The database context to which the audit will be stored</param>
        /// <param name="pOldModel">A string representing an old value (changed from)</param>
        /// <param name="pNewModel">A string representing a new value (changed to)</param>
        /// <param name="pUsername">The username who made the change</param>
        /// <param name="pAction">The action being performed</param>
        public static void InsertAudit(opendkpContext pDBContext, string pClientId, string pOldModel, string pNewModel, string pUsername, string pAction)
        {
            try
            {
                Audit vAudit = new Audit
                {
                    OldValue = pOldModel,
                    NewValue = pNewModel,
                    Timestamp = DateTime.Now,
                    CognitoUser = pUsername,
                    Action = pAction,
                    ClientId = pClientId
                };
                pDBContext.Add(vAudit);
                pDBContext.SaveChanges();
            }
            catch (Exception vException)
            {
                Console.WriteLine("Failed to log audit for some reason");
                Console.WriteLine(vException);
            }
        }
    }
}
