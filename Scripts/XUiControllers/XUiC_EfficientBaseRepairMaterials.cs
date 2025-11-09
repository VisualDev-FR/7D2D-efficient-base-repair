using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XUiC_EfficientBaseRepairMaterials : XUiController
{
    public TileEntityEfficientBaseRepair TileEntity { get; set; }

    private XUiC_EBRMaterialEntry[] MaterialEntries { get; set; }

    private XUiController MaterialsPanel { get; set; }

    private XUiC_Paging Pager { get; set; }

    public override void Init()
    {
        base.Init();

        Pager = GetChildByType<XUiC_Paging>();
        Pager.OnPageChanged += HandlePageChanged;

        MaterialsPanel = GetChildById("materialsPanel");
        MaterialsPanel.OnScroll += HandleOnScroll;

        MaterialEntries = GetChildrenByType<XUiC_EBRMaterialEntry>();
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
        var endIndex = Math.Min(entries.Count, (currentPage + 1) * pageSize);

        for (int i = startIndex; i < endIndex; i++)
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
            string text = Localization.Get(entry.Key);
            string iconName = ItemClass.GetItem(entry.Key).ItemClass.GetIconName();

            int availableMaterialsCount = itemsDict.ContainsKey(entry.Key) ? itemsDict[entry.Key] : 0;
            int requiredMaterialsCount = entry.Value;

            if (requiredMaterialsCount <= 0)
                continue;

            if (index >= MaterialEntries.Length)
                break;

            MaterialEntries[index].SetIcon(iconName);
            MaterialEntries[index].SetQuantity(availableMaterialsCount, requiredMaterialsCount);

            index++;
        }

        for (int i = index; i < MaterialEntries.Length; i++)
        {
            MaterialEntries[i].SetEmpty();
        }

        Pager.LastPageNumber = lastPageNumber;
    }

    public void HandlePageChanged()
    {
        // Do nothing because the materials are updated at each frame
        // (which might need to be changed to increase performances, but requires complex state management)
    }

    public void HandleOnScroll(XUiController _sender, float _delta)
    {
        if (_delta > 0f)
        {
            Pager?.PageDown();
        }
        else
        {
            Pager?.PageUp();
        }
    }
}
