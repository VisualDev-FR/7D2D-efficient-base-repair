using System.Collections.Generic;
using UnityEngine;

public class XUiC_EfficientBaseRepairMaterials : XUiController
{
    public string[] MaterialNames;

    private int[] weights;

    public TileEntityEfficientBaseRepair tileEntity;

    private XUiController[] materialSprites;

    private XUiController[] materialWeights;

    private Color baseTextColor;

    private Color validColor = Color.green;

    private Color invalidColor = Color.red;

    public override void Init()
    {
        base.Init();
        materialSprites = GetChildrenById("material");
        materialWeights = GetChildrenById("weight");
        if (materialWeights[0] != null)
        {
            baseTextColor = ((XUiV_Label)materialWeights[0].ViewComponent).Color;
        }

        weights = null;
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

    private void UpdateMaterials()
    {
        if (tileEntity == null)
            return;

        int index = 0;
        Dictionary<string, int> itemsDict = tileEntity.ItemsToDict();

        foreach (KeyValuePair<string, int> entry in tileEntity.requiredMaterials)
        {
            string text = Localization.Get(entry.Key);
            string iconName = ItemClass.GetItem(entry.Key).ItemClass.GetIconName();

            int availableMaterialsCount = itemsDict.ContainsKey(entry.Key) ? itemsDict[entry.Key] : 0;
            int requiredMaterialsCount = entry.Value;

            if (requiredMaterialsCount <= 0)
                continue;

            // can happen if there more repair materials than the anticipated max amount of material sprites
            if (index >= materialSprites.Length)
                continue;

            XUiV_Sprite sprite = (XUiV_Sprite)materialSprites[index].ViewComponent;
            sprite.ParseAttribute("sprite", iconName, materialSprites[index]);

            XUiV_Label label = (XUiV_Label)materialWeights[index].ViewComponent;
            label.Text = $"{availableMaterialsCount} / {requiredMaterialsCount}";
            label.Color = availableMaterialsCount >= requiredMaterialsCount ? validColor : invalidColor;

            index++;
        }

        for (int i = index; i < materialWeights.Length; i++)
        {
            ((XUiV_Label)materialWeights[i].ViewComponent).Text = "";
            ((XUiV_Sprite)materialSprites[i].ViewComponent).ParseAttribute("sprite", "", materialSprites[i]);
        }
    }

    private void ResetWeightColors()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            ((XUiV_Label)materialWeights[i].ViewComponent).Color = baseTextColor;
        }
    }

    public void SetMaterialWeights(ItemStack[] stackList)
    {
        for (int i = 3; i < stackList.Length; i++)
        {
            if (weights != null && stackList[i] != null)
            {
                weights[i - 3] = stackList[i].count;
            }
        }
        // onForgeValuesChanged();
    }

    public override bool ParseAttribute(string name, string value, XUiController _parent)
    {
        bool flag = base.ParseAttribute(name, value, _parent);
        if (!flag)
        {
            switch (name)
            {
                case "materials":
                    // if (value.Contains(","))
                    // {
                    // 	MaterialNames = value.Replace(" ", "").Split(',', StringSplitOptions.None);
                    // 	weights = new int[MaterialNames.Length];
                    // }
                    // else
                    // {
                    // 	MaterialNames = new string[1] { value };
                    // }
                    MaterialNames = new string[1] { value };
                    return true;
                case "valid_materials_color":
                    validColor = StringParsers.ParseColor32(value);
                    return true;
                case "invalid_materials_color":
                    invalidColor = StringParsers.ParseColor32(value);
                    return true;
                default:
                    return false;
            }
        }
        return flag;
    }

    private float calculateWeightOunces(int materialIndex)
    {
        return weights[materialIndex];
    }

    private float calculateWeightPounds(int materialIndex)
    {
        return calculateWeightOunces(materialIndex) / 16f;
    }
}
