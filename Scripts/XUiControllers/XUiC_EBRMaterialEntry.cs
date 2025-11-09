using UnityEngine;

public class XUiC_EBRMaterialEntry : XUiController
{
    private static Color validColor = Color.green;

    private static Color invalidColor = Color.red;

    private XUiV_Label Label { get; set; }

    private XUiV_Sprite Sprite { get; set; }

    public override void Init()
    {
        base.Init();

        Label = GetChildById("label").viewComponent as XUiV_Label;
        Sprite = GetChildById("icon").viewComponent as XUiV_Sprite;

        EBRUtils.Assert(Label != null);
        EBRUtils.Assert(Sprite != null);
    }

    public void SetIcon(string iconName)
    {
        Sprite.ParseAttribute("sprite", iconName, this);
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