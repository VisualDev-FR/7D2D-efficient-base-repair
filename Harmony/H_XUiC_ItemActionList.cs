using ItemActionListTypes = XUiC_ItemActionList.ItemActionListTypes;


public class XUiC_ItemActionList_SetCraftingActionList
{
    public static void Postfix(ItemActionListTypes _actionListType, XUiController itemController, XUiC_ItemActionList __instance)
    {
        if (!(itemController is XUiC_EBRMaterialEntry materialEntry) || materialEntry.ItemClass is null)
            return;

        var itemClass = materialEntry.ItemClass;
        var itemValue = ItemClass.GetItem(itemClass.Name);

        __instance.AddActionListEntry(new ItemActionEntryScrap(itemController));
        __instance.AddActionListEntry(new ItemActionEntryRecipes(itemController));
    }
}