using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XUiC_EfficientBaseRepairMaterials : XUiController
{
    public TileEntityEfficientBaseRepair TileEntity { get; set; }

    private XUiC_EBRMaterialEntry[] materialEntries;

    private XUiController materialsPanel;

    private XUiC_Paging pager;

    public override void Init()
    {
        base.Init();

        pager = GetChildByType<XUiC_Paging>();
        pager.OnPageChanged += HandlePageChanged;

        materialsPanel = GetChildById("materialsPanel");
        materialsPanel.OnScroll += HandleOnScroll;

        materialEntries = GetChildrenByType<XUiC_EBRMaterialEntry>();
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

        var lastPageNumber = Mathf.CeilToInt(TileEntity.requiredMaterials.Count / materialEntries.Length);
        var currentPageNumber = Math.Min(lastPageNumber, pager.CurrentPageNumber);

        var itemsDict = TileEntity.ItemsToDict();
        var requiredMaterials = GetPagedMaterials(currentPageNumber, materialEntries.Length);
        var index = 0;

        foreach (var entry in requiredMaterials)
        {
            string text = Localization.Get(entry.Key);
            string iconName = ItemClass.GetItem(entry.Key).ItemClass.GetIconName();

            int availableMaterialsCount = itemsDict.ContainsKey(entry.Key) ? itemsDict[entry.Key] : 0;
            int requiredMaterialsCount = entry.Value;

            if (requiredMaterialsCount <= 0)
                continue;

            if (index >= materialEntries.Length)
                break;

            materialEntries[index].SetIcon(iconName);
            materialEntries[index].SetQuantity(availableMaterialsCount, requiredMaterialsCount);

            index++;
        }

        for (int i = index; i < materialEntries.Length; i++)
        {
            materialEntries[i].SetEmpty();
        }

        pager.LastPageNumber = lastPageNumber;
    }

    public void HandlePageChanged()
    {
        Log.Out($"Page: {pager.currentPageNumber}");
    }

    public void HandleOnScroll(XUiController _sender, float _delta)
    {
        if (_delta > 0f)
        {
            pager?.PageDown();
        }
        else
        {
            pager?.PageUp();
        }
    }
}
