using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XUiC_EfficientBaseRepairMaterials : XUiController
{
    public TileEntityEfficientBaseRepair TileEntity { get; set; }

    private XUiC_EBRMaterialEntry[] MaterialEntries { get; set; }

    public XUiC_ItemInfoWindow ItemInfoWindow { get; private set; }

    private XUiC_Paging Pager { get; set; }

    public void PageUp() => Pager?.PageUp();

    public void PageDown() => Pager?.PageDown();

    public override void Init()
    {
        base.Init();

        Pager = GetChildByType<XUiC_Paging>();
        Pager.OnPageChanged += HandlePageChanged;

        ItemInfoWindow = Parent.GetChildByType<XUiC_ItemInfoWindow>();
        MaterialEntries = GetChildrenByType<XUiC_EBRMaterialEntry>();

        EBRUtils.Assert(ItemInfoWindow != null);
        EBRUtils.Assert(MaterialEntries != null);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        UpdateMaterials();
    }

    public override void Update(float _dt)
    {
        if (windowGroup.isShowing)
        {
            UpdateMaterials();
            base.Update(_dt);
        }
    }

    private IEnumerable<KeyValuePair<string, int>> GetPagedMaterials(int currentPage, int pageSize)
    {
        var entries = TileEntity.requiredMaterials.Select(e => e).ToList();
        var startIndex = currentPage * pageSize;
        var endIndex = (currentPage + 1) * pageSize;

        for (int i = startIndex; i < Math.Min(entries.Count, endIndex); i++)
        {
            yield return entries[i];
        }
    }

    private void UpdateMaterials()
    {
        if (TileEntity == null)
            return;

        var lastPageNumber = Mathf.CeilToInt(TileEntity.requiredMaterials.Count / MaterialEntries.Length);
        var currentPageNumber = Math.Min(lastPageNumber, Pager.CurrentPageNumber);

        var itemsDict = TileEntity.ItemsToDict();
        var requiredMaterials = GetPagedMaterials(currentPageNumber, MaterialEntries.Length);
        var index = 0;

        foreach (var entry in requiredMaterials)
        {
            var itemClass = ItemClass.GetItem(entry.Key).ItemClass;

            int availableMaterialsCount = itemsDict.ContainsKey(entry.Key) ? itemsDict[entry.Key] : 0;
            int requiredMaterialsCount = entry.Value;

            if (requiredMaterialsCount > 0)
            {
                MaterialEntries[index].SetItemClass(itemClass);
                MaterialEntries[index].SetQuantity(availableMaterialsCount, requiredMaterialsCount);
                index++;
            }
        }

        for (int i = index; i < MaterialEntries.Length; i++)
        {
            MaterialEntries[i].SetEmpty();
        }

        Pager.LastPageNumber = lastPageNumber;
    }

    public void SetRecipeInfo(ItemClass itemClass)
    {
        Log.Out($"Set Recipe Info: '{itemClass?.Name}'");

        if (itemClass == null)
        {
            ItemInfoWindow.ShowEmptyInfo();
            return;
        }

        var itemValue = ItemClass.GetItem(itemClass.Name);
        var itemStack = new ItemStack(itemValue, 0);

		ItemInfoWindow.makeVisible(true);
		ItemInfoWindow.selectedItemStack = null;
		ItemInfoWindow.selectedEquipmentStack = null;
		ItemInfoWindow.selectedPartStack = null;
		ItemInfoWindow.selectedTraderItemStack = null;
		ItemInfoWindow.selectedTurnInItemStack = null;
		ItemInfoWindow.SetInfo(itemStack, this, XUiC_ItemActionList.ItemActionListTypes.None);
    }

    public void HandlePageChanged()
    {
        // Do nothing because the materials are updated at each frame
        // (which might need to be changed to increase performances, but requires complex state management)
    }
}
