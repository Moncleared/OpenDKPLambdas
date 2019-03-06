using OpenDKPShared.DBModels;
using System;

namespace OpenDKPShared.Helpers
{
    /// <summary>
    /// This class is designed to help consolidate the User Request actions on the database
    /// </summary>
    public static class UserRequestHelper
    {
        /// <summary>
        /// Inserts a UserRequest to the database
        /// </summary>
        /// <param name="pDBContext">The database context</param>
        /// <param name="pRequest">The user request</param>
        /// <returns></returns>
        public static bool InsertRequest(opendkpContext pDBContext, UserRequests pRequest)
        {
            bool vReturn = false;
            try
            {
                pDBContext.Add(pRequest);
                pDBContext.SaveChanges();
                vReturn = true;
            }
            catch(Exception vException)
            {
                Console.WriteLine("Error Inserting AdminRequest: " + vException);
            }
            return vReturn;
        }
        /// <summary>
        /// Deletes a UserRequest from the Database
        /// </summary>
        /// <param name="pDBContext">The database in which to delete the UserREquest from</param>
        /// <param name="pRequest">The UserRequest to delete</param>
        /// <returns></returns>
        public static bool DeleteRequest(opendkpContext pDBContext, UserRequests pRequest)
        {
            bool vReturn = false;
            try
            {
                pDBContext.Remove(pRequest);
                pDBContext.SaveChanges();
                vReturn = true;
            }
            catch (Exception vException)
            {
                Console.WriteLine("Error Inserting AdminRequest: " + vException);
            }
            return vReturn;
        }
    }
}
