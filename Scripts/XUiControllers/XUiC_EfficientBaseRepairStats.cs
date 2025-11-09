using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EfficientBaseRepairStats : XUiController
{
	private XUiController btnRefresh;

	private XUiV_Button btnRefresh_Background;

	private XUiController btnOn;

	private XUiV_Button btnOn_Background;

	private XUiController btnUpgrade;

	private XUiV_Button btnUpgrade_Background;

	private XUiV_Label lblOnOff;

	private XUiV_Sprite sprOnOff;

	private XUiV_Label lblUpgrade;

	private XUiV_Sprite sprUpgrade;

	private Color32 onColor = new Color32((byte)250, byte.MaxValue, (byte)163, byte.MaxValue);

	private Color32 offColor = (Color32)Color.white;

	private string turnOff => Localization.Get("xuiTurnOff");

	private string turnOn => Localization.Get("xuiTurnOn");

	private string UpgradeOnText => "Upgrade On";

	private string UpgradeOffText => "Upgrade Off";

	private bool lastOn;

	public TileEntityEfficientBaseRepair TileEntity { get; set; }

	public override void Init()
	{
		base.Init();

		btnRefresh = GetChildById("btnRefresh");
		btnRefresh_Background = (XUiV_Button)btnRefresh.GetChildById("clickable").ViewComponent;
		btnRefresh_Background.Controller.OnPress += BtnRefresh_OnPress;

		btnOn = GetChildById("btnOn");
		btnOn_Background = (XUiV_Button)btnOn.GetChildById("clickable").ViewComponent;
		btnOn_Background.Controller.OnPress += BtnOn_OnPress;

		btnUpgrade = GetChildById("btnUpgrade");
		btnUpgrade_Background = (XUiV_Button)btnUpgrade.GetChildById("clickable").ViewComponent;
		btnUpgrade_Background.Controller.OnPress += BtnUpgrade_OnPress;

		lblOnOff = (XUiV_Label)GetChildById("lblOnOff").ViewComponent;
		sprOnOff = (XUiV_Sprite)GetChildById("sprOnOff").ViewComponent;

		lblUpgrade = (XUiV_Label)GetChildById("lblUpgrade").ViewComponent;
		sprUpgrade = (XUiV_Sprite)GetChildById("sprUpgrade").ViewComponent;

		((XUiV_Label)GetChildById("lblRefresh").ViewComponent).Text = Localization.Get("xuiServerBrowserRefreshList");
	}

	public void SetUpgradeEnabled(bool enabled)
	{
		if (enabled != btnUpgrade.ViewComponent.forceHide)
			return;

		btnUpgrade.ViewComponent.forceHide = !enabled;

		if (enabled)
		{
			btnRefresh.viewComponent.Position -= new Vector2i(0, btnUpgrade.viewComponent.size.y);
		}
		else
		{
			btnRefresh.viewComponent.Position += new Vector2i(0, btnUpgrade.viewComponent.size.y);
		}
	}

	public void SetWidth(int width)
	{

		int max_width = Mathf.Max(width, 230);

		this.viewComponent.ParseAttribute("width", max_width.ToString(), this);

		this.GetChildById("header").ViewComponent.ParseAttribute("width", max_width.ToString(), this);

		XUiController content = GetChildById("content");

		content
			.GetChildById("backgroundMain")
			.ViewComponent.ParseAttribute("width", max_width.ToString(), this);

		content
			.GetChildById("background")
			.ViewComponent.ParseAttribute("width", max_width.ToString(), this);

		content
			.GetChildById("statGrid")
			.GetChildById("stats")
			.ViewComponent.ParseAttribute("cell_width", (max_width - 5).ToString(), this);

		GetChildById("lblBlocksToRepair").ViewComponent.ParseAttribute("width", (max_width - 5).ToString(), this);
		GetChildById("lblBlocksToUpgrade").ViewComponent.ParseAttribute("width", (max_width - 5).ToString(), this);
		GetChildById("lblTotalDamages").ViewComponent.ParseAttribute("width", (max_width - 5).ToString(), this);
		GetChildById("lblVisitedBlocks").ViewComponent.ParseAttribute("width", (max_width - 5).ToString(), this);
		GetChildById("lblIterations").ViewComponent.ParseAttribute("width", (max_width - 5).ToString(), this);
		GetChildById("lblTimer").ViewComponent.ParseAttribute("width", (max_width - 5).ToString(), this);

		const int SPRITE_OFFSET = 80;

		// button ON
		XUiController btnOn = content.GetChildById("btnOn");

		btnOn
			.GetChildById("backgroundMain")
			.ViewComponent.ParseAttribute("width", max_width.ToString(), this);

		btnOn
			.GetChildById("background")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnOn
			.GetChildById("buttonRect")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnOn
			.GetChildById("buttonRect")
			.GetChildById("clickable")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnOn
			.GetChildById("buttonRect")
			.GetChildById("lblOnOff")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnOn
			.GetChildById("buttonRect")
			.GetChildById("sprOnOff")
			.ViewComponent.ParseAttribute("pos", $"{max_width / 2 - SPRITE_OFFSET}, -2", this);

		// button UPGRADE
		XUiController btnUpgrade = content.GetChildById("btnUpgrade");

		btnUpgrade
			.GetChildById("backgroundMain")
			.ViewComponent.ParseAttribute("width", max_width.ToString(), this);

		btnUpgrade
			.GetChildById("background")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnUpgrade
			.GetChildById("buttonRect")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnUpgrade
			.GetChildById("buttonRect")
			.GetChildById("clickable")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnUpgrade
			.GetChildById("buttonRect")
			.GetChildById("lblUpgrade")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnUpgrade
			.GetChildById("buttonRect")
			.GetChildById("sprUpgrade")
			.ViewComponent.ParseAttribute("pos", $"{max_width / 2 - SPRITE_OFFSET}, -2", this);

		// button REFRESH
		XUiController btnRefresh = content.GetChildById("btnRefresh");

		btnRefresh
			.GetChildById("backgroundMain")
			.ViewComponent.ParseAttribute("width", max_width.ToString(), this);

		btnRefresh
			.GetChildById("background")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnRefresh
			.GetChildById("buttonRect")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnRefresh
			.GetChildById("buttonRect")
			.GetChildById("clickable")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnRefresh
			.GetChildById("buttonRect")
			.GetChildById("lblRefresh")
			.ViewComponent.ParseAttribute("width", (max_width - 4).ToString(), this);

		btnRefresh
			.GetChildById("buttonRect")
			.GetChildById("sprRefresh")
			.ViewComponent.ParseAttribute("pos", $"{max_width / 2 - SPRITE_OFFSET}, -2", this);
	}

	private void BtnRefresh_OnPress(XUiController _sender, int _mouseButton)
	{
		TileEntity.ForceRefresh();
		Manager.PlayInsidePlayerHead("UseActions/chest_tier4_open");
	}

	private void BtnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		TileEntity.Switch();
	}

	private void BtnUpgrade_OnPress(XUiController _sender, int _mouseButton)
	{
		TileEntity.SwitchUpgrade();
	}

	private void RefreshUpgradeOn(bool upgradeOn)
	{
		if (upgradeOn)
		{
			lblUpgrade.Text = UpgradeOffText;
			if (sprUpgrade != null)
			{
				sprUpgrade.Color = onColor;
			}
		}
		else
		{
			lblUpgrade.Text = UpgradeOnText;
			if (sprUpgrade != null)
			{
				sprUpgrade.Color = offColor;
			}
		}
	}

	private void RefreshIsOn(bool isOn)
	{
		if (isOn)
		{
			lblOnOff.Text = turnOff;
			if (sprOnOff != null)
			{
				sprOnOff.Color = onColor;
			}
		}
		else
		{
			lblOnOff.Text = turnOn;
			if (sprOnOff != null)
			{
				sprOnOff.Color = offColor;
			}
		}
	}

	private XUiV_Label GetLabel(string labelName)
	{
		return (XUiV_Label)GetChildById(labelName).ViewComponent;
	}

	private void RefreshStats()
	{
		GetLabel("lblBlocksToRepair").Text = $"{TileEntity.DamagedBlockCount:N0} damaged blocks found.";
		GetLabel("lblBlocksToUpgrade").Text = $"{TileEntity.UpgradableBlockCount:N0} upgradable blocks found.";
		GetLabel("lblTotalDamages").Text = $"{TileEntity.TotalDamagesCount:N0} damages points to repair.";
		GetLabel("lblVisitedBlocks").Text = $"{TileEntity.VisitedBlocksCount:N0} blocks visited.";
		GetLabel("lblIterations").Text = $"{TileEntity.BfsIterationsCount} iterations done.";
		GetLabel("lblTimer").Text = $"Repair time {TileEntity.CalcRepairTime()}";
	}

	public override void Update(float _dt)
	{
		if ((GameManager.Instance != null || GameManager.Instance.World != null) && TileEntity != null)
		{
			base.Update(_dt);
			if (lastOn != TileEntity.IsOn)
			{
				lastOn = TileEntity.IsOn;
				RefreshIsOn(TileEntity.IsOn);
			}
			RefreshUpgradeOn(TileEntity.UpgradeOn);
			RefreshStats();
			RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		TileEntity.SetUserAccessing(_bUserAccessing: true);
		RefreshIsOn(TileEntity.IsOn);
		RefreshUpgradeOn(TileEntity.UpgradeOn);
		RefreshBindings();
	}

	public override void OnClose()
	{
		GameManager instance = GameManager.Instance;
		Vector3i blockPos = TileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			TileEntity.SetUserAccessing(_bUserAccessing: false);
			instance.TEUnlockServer(TileEntity.GetClrIdx(), blockPos, TileEntity.entityId, false);
			TileEntity.SetModified();
		}
		base.OnClose();
	}
}
