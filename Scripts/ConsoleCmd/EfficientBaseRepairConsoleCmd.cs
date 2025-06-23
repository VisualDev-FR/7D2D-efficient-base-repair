using System;
using System.Collections.Generic;
using System.Reflection;

public class EfficientBaseRepairConsoleCmd : ConsoleCmdAbstract
{
    private static readonly Logging.Logger logger = Logging.CreateLogger("EfficientBaseRepairConsoleCmd");

    public static readonly List<string> activeBoxNames = new List<string>();

    public override string[] getCommands()
    {
        return new string[] { "efficientbaserepair", "ebr" };
    }

    public override string getDescription()
    {
        return "efficientbaserepair ebr => command line tools for the mod EfficientBaseRepair.";
    }

    public override string getHelp()
    {
        return @"EfficientBaseRepair commands:
            - isChild: log the selected blockValue.isChild
            - neighbors: select all neighbors of the selected block
            - clear: clear all selection boxes added by command 'neighbors'
            - material, mat: fill the opened EfficientBaseRepair crate with required materials
            - setfuel <value>: set the given fuel amount into the opened powerSource item. If no value is given, a random value is choosen.
            - getconfig <name>: show the value of the given ebr parameter. The name is case-sensitive and must be defined in ModConfig.xml, ex: `get repairRate`
            - setconfig <name> <value>: set the value of the given ebr parameter, ex: `set repairRate 100`
        ";
    }

    public static SelectionCategory GetSelectionCategory()
    {
        var selectionBoxCategory = "BlockSelectionUtils";
        var sbm = SelectionBoxManager.Instance;

        if (!sbm.categories.ContainsKey(selectionBoxCategory))
        {
            sbm.CreateCategory(
                _name: selectionBoxCategory,
                _colSelected: SelectionBoxManager.ColSelectionActive,
                _colUnselected: SelectionBoxManager.ColSelectionInactive,
                _colFaceSelected: SelectionBoxManager.ColSelectionFaceSel,
                _bCollider: false,
                _tag: null
            );
        }

        return sbm.categories[selectionBoxCategory];
    }

    private void SelectBlock(Vector3i pos)
    {
        var selectionCat = GetSelectionCategory();
        var boxName = pos.ToString();

        SelectionBox box = selectionCat.AddBox(boxName, pos, Vector3i.one);
        box.SetVisible(true);
        box.SetSizeVisibility(_visible: true);

        selectionCat.SetVisible(true);

        activeBoxNames.Add(boxName);
    }

    private void CmdIsChild()
    {
        var position = BlockToolSelection.Instance.m_selectionStartPoint;
        var isChild = GameManager.Instance.World.GetBlock(position).ischild;

        logger.Info(isChild);
    }

    private void CmdNeighbors()
    {
        var position = BlockToolSelection.Instance.m_selectionStartPoint;
        var blockValue = GameManager.Instance.World.GetBlock(position);

        SelectionBoxManager.Instance.Deactivate();

        foreach (var pos in TileEntityEfficientBaseRepair.GetNeighbors(position, blockValue))
        {
            SelectBlock(pos);
        }
    }

    private void CmdClearBoxes()
    {
        var selectionCat = GetSelectionCategory();

        foreach (var name in activeBoxNames)
        {
            selectionCat.RemoveBox(name);
        }

        activeBoxNames.Clear();
    }

    private void CmdMaterial()
    {
        var xuiController = EBRUtils.GetXuiController<XUiC_EfficientBaseRepair>();

        if (xuiController is null || !xuiController.IsOpen)
        {
            logger.Error("No EfficientBaseRepair crate is open.");
            return;
        }

        var tileEntity = xuiController.TileEntity;

        foreach (var material in tileEntity.requiredMaterials)
        {
            var itemName = material.Key;
            var itemCount = material.Value;

            var itemType = ItemClass.nameToItem[itemName].Id;
            var itemValue = new ItemValue(itemType);
            var itemStack = new ItemStack(itemValue, itemCount);

            tileEntity.AddItem(itemStack);
        }
    }

    private void CmdSetFuel(string[] args)
    {
        var xuiController = EBRUtils.GetXuiController<XUiC_PowerSourceStats>();

        if (xuiController is null || !xuiController.IsOpen)
        {
            logger.Error("No power source item is open");
            return;
        }

        if (args.Length < 2 || !int.TryParse(args[1], out var value))
        {
            value = new System.Random().Next(xuiController.tileEntity.MaxFuel);
        }

        xuiController.tileEntity.CurrentFuel = (ushort)value;
    }

    private void CmdSetConfig(string[] args)
    {
        ModConfig.SetField<Config>(args[1], args[2]);
    }

    private void CmdGetConfig(string[] args)
    {
        var fieldName = args.Length > 1 ? args[1] : "";
        var value = ModConfig.GetField<Config>(fieldName);

        if (value != null)
        {
            Log.Out(value.ToString());
        }
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var args = _params.ToArray();

        if (args.Length == 0)
        {
            Log.Out(getHelp());
            return;
        }

        switch (args[0].ToLower())
        {
            case "ischild":
                CmdIsChild();
                break;

            case "neighbor":
            case "neighbors":
                CmdNeighbors();
                break;

            case "clear":
                CmdClearBoxes();
                break;

            case "material":
            case "mat":
                CmdMaterial();
                break;

            case "setfuel":
                CmdSetFuel(args);
                break;

            case "setconfig":
            case "set":
                CmdSetConfig(args);
                break;

            case "getconfig":
            case "get":
                CmdGetConfig(args);
                break;

            default:
                logger.Error($"Invalid or not implemented command: '{_params[0]}'");
                break;
        }
    }
}