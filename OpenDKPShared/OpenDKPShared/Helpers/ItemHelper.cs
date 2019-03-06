using OpenDKPShared.DBModels;
using System;
using System.Linq;

namespace OpenDKPShared.Helpers
{
    /// <summary>
    /// Helper class for Items
    /// </summary>
    public class ItemHelper
    {
        /// <summary>
        /// Creates an item into the db if needed (not found already)
        /// </summary>
        /// <param name="pDBContext">The database context in which to look or create the item in</param>
        /// <param name="pItem">The item to create or lookup, checks for its Id first otherwise by name</param>
        /// <returns></returns>
        public static Items CreateItemIfNeeded(opendkpContext pDBContext, Items pItem)
        {
            Items vFoundItem = pDBContext.Items.FirstOrDefault<Items>(x => x.IdItem == pItem.IdItem);

            //If we don't find the item by it's ID
            if ( vFoundItem == null )
            {
                vFoundItem = pDBContext.Items
                                     .FirstOrDefault<Items>(x => x.Name.Trim().Equals(pItem.Name.Trim(), StringComparison.InvariantCultureIgnoreCase));

                //If we don't find the item by it's name
                if ( vFoundItem == null )
                {
                    var vNewItem = new Items
                    {
                        Name = pItem.Name
                    };
                    pDBContext.Add(vNewItem);
                    pDBContext.SaveChanges();
                    vFoundItem = vNewItem;
                }
            }
            
            return vFoundItem;
        }
    }
}
