namespace Utilities
{
    public static class Utils
    {
        public static void CopyItemStack(ItemStack to, ItemStack from)
        {
            to.itemID = from.itemID;
            to.number = from.number;
        }
        
        public static int GetSetBitCount(long lValue)
        {
            int iCount = 0;

            //Loop the value while there are still bits
            while (lValue != 0)
            {
                //Remove the end bit
                lValue = lValue & (lValue - 1);

                //Increment the count
                iCount++;
            }

            //Return the count
            return iCount;
        }
    }
}