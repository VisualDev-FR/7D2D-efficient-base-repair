using Audio;
using UnityEngine;

public class XUiC_EfficientBaseRepair : XUiController
{
	private const string windowName = "EfficientBaseRepair";

	private XUiC_LootWindow lootWindow;

	private XUiC_EfficientBaseRepairStats statsWindow;

	private XUiC_EfficientBaseRepairMaterials MaterialsWindow;

	private XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public TileEntityEfficientBaseRepair TileEntity { get; private set; }

	private bool isOpening;

	private float openTimeLeft;

	private XUiC_Timer timerWindow;

	private float totalOpenTime;

	private bool ignoreCloseSound;

	public override void Init()
	{
		base.Init();
		openTimeLeft = 0f;
		lootWindow = GetChildByType<XUiC_LootWindow>();
		timerWindow = base.xui.GetChildByType<XUiC_Timer>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();

		statsWindow = (XUiC_EfficientBaseRepairStats)GetChildById("WindowEfficientBaseRepairStats");
		MaterialsWindow = (XUiC_EfficientBaseRepairMaterials)GetChildById("windowEfficientBaseRepairMaterials");
	}

	public void SetTileEntityChest(string _lootContainerName, TileEntityEfficientBaseRepair _te)
	{
		TileEntity = _te;
		lootWindow.SetTileEntityChest(_lootContainerName, _te);
		statsWindow.SetUpgradeEnabled(TileEntity.UpgradeEnabled);
	}

	protected void OpenContainer()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("backpack", _bModal: false);
		lootWindow.ViewComponent.UiTransform.gameObject.SetActive(true);
		lootWindow.OpenContainer();

		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("Base Repair");
		}
		lootWindow.ViewComponent.IsVisible = true;
		xui.playerUI.windowManager.Close("timer");
		lootWindow.ViewComponent.ParseAttribute("position", "0, -348", this);
		isOpening = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.hasBeenAttackedTime > 0 && isOpening)
		{
			GUIWindowManager windowManager = base.xui.playerUI.windowManager;
			windowManager.Close("timer");
			isOpening = false;
			windowManager.Close("looting");
		}
		else
		{
			if (!isOpening)
			{
				return;
			}
			if (TileEntity.bWasTouched || openTimeLeft <= 0f)
			{
				if (!TileEntity.bWasTouched && !TileEntity.bPlayerStorage && !TileEntity.bPlayerBackpack)
				{
					base.xui.playerUI.entityPlayer.Progression.AddLevelExp(base.xui.playerUI.entityPlayer.gameStage, "_xpFromLoot", Progression.XPTypes.Looting);
				}
				openTimeLeft = 0f;
				OpenContainer();
			}
			else
			{
				if (timerWindow != null)
				{
					float fillAmount = openTimeLeft / totalOpenTime;
					timerWindow.UpdateTimer(openTimeLeft, fillAmount);
				}
				openTimeLeft -= _dt;
			}
		}
	}

	public override void OnOpen()
	{
		if (TileEntity.entityId != -1)
		{
			Entity entity = GameManager.Instance.World.GetEntity(TileEntity.entityId);
			if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, base.xui.playerUI.entityPlayer, null, entity.EntityClass.Tags) > 0f)
			{
				Manager.PlayInsidePlayerHead("twitch_no_attack");
				GUIWindowManager windowManager = base.xui.playerUI.windowManager;
				ignoreCloseSound = true;
				windowManager.Close("timer");
				isOpening = false;
				windowManager.Close("looting");
				return;
			}
		}
		else if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, base.xui.playerUI.entityPlayer, null, TileEntity.blockValue.Block.Tags) > 0f)
		{
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			GUIWindowManager windowManager2 = base.xui.playerUI.windowManager;
			ignoreCloseSound = true;
			windowManager2.Close("timer");
			isOpening = false;
			windowManager2.Close("looting");
			return;
		}
		ignoreCloseSound = false;
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		lootWindow.ViewComponent.UiTransform.gameObject.SetActive(false);

		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		totalOpenTime = 0f; //(openTimeLeft = EffectManager.GetValue(PassiveEffects.ScavengingTime, null, entityPlayer.IsCrouching ? (te.GetOpenTime() * 1.5f) : te.GetOpenTime(), entityPlayer));
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		base.xui.playerUI.windowManager.Open("timer", _bModal: false);
		timerWindow = base.xui.GetChildByType<XUiC_Timer>();
		timerWindow.currentOpenEventText = Localization.Get("xuiOpeningLoot");
		isOpening = true;
		LootContainer lootContainer = LootContainer.GetLootContainer(TileEntity.lootListName);
		if (lootContainer == null || lootContainer.soundClose == null)
		{
			return;
		}
		Vector3 position = TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
		if (TileEntity.entityId != -1 && GameManager.Instance.World != null)
		{
			Entity entity2 = GameManager.Instance.World.GetEntity(TileEntity.entityId);
			if (entity2 != null)
			{
				position = entity2.GetPosition();
			}
		}

		statsWindow.TileEntity = TileEntity;
		statsWindow.SetWidth(lootWindow.ViewComponent.Size.x);
		statsWindow.OnOpen();

		MaterialsWindow.TileEntity = TileEntity;
		MaterialsWindow.OnOpen();

		Manager.BroadcastPlayByLocalPlayer(position, "UseActions/chest_tier4_open");
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		TileEntity.ToWorldPos();
		if (isOpening)
		{
			base.xui.playerUI.windowManager.Close("timer");
		}
		if (openTimeLeft > 0f && !TileEntity.bWasTouched)
		{
			TileEntity.bTouched = false;
			TileEntity.SetModified();
		}
		lootWindow.CloseContainer(ignoreCloseSound);
		lootWindow.ViewComponent.IsVisible = false;
		isOpening = false;
	}

	public static void Open(LocalPlayerUI _playerUi, TileEntityEfficientBaseRepair tileEntity)
	{
		XUiC_EfficientBaseRepair instance = (XUiC_EfficientBaseRepair)_playerUi.xui.FindWindowGroupByName(windowName);

		if (instance == null)
		{
			Log.Error("[EfficientBaseRepair] null instance of XUiC_EfficientBaseRepair. aborting...");
			return;
		}

		tileEntity.ForceRefresh();

		instance.SetTileEntityChest("EfficientBaseRepair", tileEntity);

		_playerUi.windowManager.Open(windowName, _bModal: true);
	}
}