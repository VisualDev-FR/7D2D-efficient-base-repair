using UnityEngine;

public class XUiC_EBRMaterialEntry : XUiController
{
    private XUiV_Label label;

    private XUiV_Sprite sprite;

    private Color validColor = Color.green;

    private Color invalidColor = Color.red;

    public override void Init()
    {
        base.Init();

        label = GetChildById("label").viewComponent as XUiV_Label;
        sprite = GetChildById("icon").viewComponent as XUiV_Sprite;

        EBRUtils.Assert(label != null);
        EBRUtils.Assert(sprite != null);
    }

    public void SetIcon(string iconName)
    {
        sprite.ParseAttribute("sprite", iconName, this);
    }

    public void SetQuantity(int available, int required)
    {
        label.Text = $"{available} / {required}";
        label.Color = available >= required ? validColor : invalidColor;
    }

    public void SetEmpty()
    {
        label.Text = "";
        sprite.ParseAttribute("sprite", "", this);
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