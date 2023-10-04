namespace Utilities
{
    public static class Utils
    {
        public static void CopyItemStack(ItemStack to, ItemStack from)
        {
            to.itemID = from.itemID;
            to.number = from.number;
        }
    }
}