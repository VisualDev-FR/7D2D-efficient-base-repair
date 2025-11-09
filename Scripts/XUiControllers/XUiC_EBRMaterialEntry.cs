using Platform;
using UnityEngine;

public class XUiC_EBRMaterialEntry : XUiC_RecipeEntry
{
    private static Color validColor = Color.green;

    private static Color invalidColor = Color.red;

    public XUiC_EfficientBaseRepairMaterials ParentController;

    private XUiV_Label Label { get; set; }

    private XUiV_Sprite Sprite { get; set; }

    public ItemClass ItemClass { get; private set; }

    public override void Init()
    {
        InitXUIController();

        ParentController = GetParentByType<XUiC_EfficientBaseRepairMaterials>();

        Label = GetChildById("label").viewComponent as XUiV_Label;
        Sprite = GetChildById("icon").viewComponent as XUiV_Sprite;

        OnPress += HandleOnPressed;
        OnScroll += HandleOnScroll;

        EBRUtils.Assert(Label != null);
        EBRUtils.Assert(Sprite != null);
    }

    private void InitXUIController()
    {
        viewComponent?.InitView();

        for (int i = 0; i < children.Count; i++)
        {
            children[i].Init();
        }

        curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
    }

    public void HandleOnScroll(XUiController _sender, float _delta)
    {
        if (_delta > 0f)
        {
            ParentController.PageDown();
        }
        else
        {
            ParentController.PageUp();
        }
    }

    public void HandleOnPressed(XUiController _sender, int _mouseButton)
    {
        ParentController.SetRecipeInfo(ItemClass);
    }

    public void SetItemClass(ItemClass itemClass)
    {
        ItemClass = itemClass;
        Sprite.ParseAttribute("sprite", itemClass.GetIconName(), this);
    }

    public void SetQuantity(int available, int required)
    {
        Label.Text = $"{available} / {required}";
        Label.Color = available >= required ? validColor : invalidColor;
    }

    public void SetEmpty()
    {
        Label.Text = "";
        Sprite.ParseAttribute("sprite", "", this);
        ItemClass = null;
    }

    public override bool ParseAttribute(string name, string value, XUiController _parent)
    {
        if (base.ParseAttribute(name, value, _parent))
            return true;

        switch (name)
        {
            case "valid_materials_color":
                validColor = StringParsers.ParseColor32(value);
                return true;

            case "invalid_materials_color":
                invalidColor = StringParsers.ParseColor32(value);
                return true;
        }

        return false;
    }
}