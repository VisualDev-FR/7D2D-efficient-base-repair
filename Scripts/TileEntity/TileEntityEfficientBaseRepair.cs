using Audio;
using UnityEngine;
using System.Collections.Generic;
using System;
using static Block;

public class TileEntityEfficientBaseRepair : TileEntitySecureLootContainer // TODO: Implement IPowered interface
{
	private static readonly Logging.Logger logger = Logging.CreateLogger<TileEntityEfficientBaseRepair>();

	public override TileEntityType GetTileEntityType() => Config.tileEntityType;

	private const string propAmmoGasCan = "ammoGasCan";

	private const float tickDuration_s = 2f;

	public int DamagedBlockCount { get; private set; }

	public int UpgradableBlockCount { get; private set; }

	public int VisitedBlocksCount { get; private set; }

	public int BfsIterationsCount { get; private set; }

	public int TotalDamagesCount { get; private set; }

	public int ElapsedTicksSinceLastRefresh { get; private set; }

	public bool IsOn { get; private set; }

	public bool UpgradeOn { get; private set; }

	public bool IsPowered { get; private set; }

	private bool forceFullRefresh;

	private bool forceRefreshMaterials;

	private string RepairSound(BlockValue block) => string.Format("ImpactSurface/metalhit{0}", block.Block.blockMaterial.SurfaceCategory);

	private static World world => GameManager.Instance.World;

	public readonly List<Vector3i> blocksToRepair = new List<Vector3i>();

	public readonly List<Vector3i> blocksToUpgrade = new List<Vector3i>();

	public readonly List<Vector3i> blocksToRefuel = new List<Vector3i>();

	public readonly List<Vector3i> blocksToReload = new List<Vector3i>();

	private readonly List<BlockChangeInfo> blockChangeInfos = new List<BlockChangeInfo>();

	public readonly Dictionary<string, int> requiredMaterials = new Dictionary<string, int>();

	public TileEntityEfficientBaseRepair(Chunk _chunk) : base(_chunk) { }

	public bool HasOwner() => this.ownerID != null;

	public string RepairTime()
	{
		float repairTime_s = 0f;
		float upgradeTime_s = 0f;

		if (Config.repairRate > 0)
			repairTime_s = (float)(TotalDamagesCount * tickDuration_s) / Config.repairRate;

		if (Config.upgradeRate > 0 && UpgradeOn)
			upgradeTime_s = (float)(UpgradableBlockCount * tickDuration_s) / Config.upgradeRate;

		return TimeSpan.FromSeconds(repairTime_s + upgradeTime_s).ToString(@"hh\:mm\:ss");
	}

	public Dictionary<string, int> ItemsToDict()
	{
		Dictionary<string, int> itemsDict = new Dictionary<string, int>();

		if (items == null)
			return itemsDict;

		foreach (ItemStack stack in items)
		{

			if (stack.itemValue.ItemClass == null)
				continue;

			string itemName = stack.itemValue.ItemClass.Name;

			if (!itemsDict.ContainsKey(itemName))
				itemsDict[itemName] = 0;

			itemsDict[itemName] += stack.count;
		}

		return itemsDict;
	}

	private List<Vector3i> GetNeighbors(Vector3i pos)
	{
		return new List<Vector3i>
		{
			new Vector3i(pos.x + 1, pos.y, pos.z),
			new Vector3i(pos.x - 1, pos.y, pos.z),

			new Vector3i(pos.x, pos.y + 1, pos.z),
			new Vector3i(pos.x, pos.y - 1, pos.z),

			new Vector3i(pos.x, pos.y, pos.z + 1),
			new Vector3i(pos.x, pos.y, pos.z - 1),

			new Vector3i(pos.x, pos.y + 1, pos.z + 1),
			new Vector3i(pos.x, pos.y - 1, pos.z + 1),
			new Vector3i(pos.x, pos.y + 1, pos.z - 1),
			new Vector3i(pos.x, pos.y - 1, pos.z - 1),

			new Vector3i(pos.x + 1, pos.y, pos.z + 1),
			new Vector3i(pos.x - 1, pos.y, pos.z + 1),
			new Vector3i(pos.x + 1, pos.y, pos.z - 1),
			new Vector3i(pos.x - 1, pos.y, pos.z - 1),

			new Vector3i(pos.x + 1, pos.y + 1, pos.z),
			new Vector3i(pos.x + 1, pos.y - 1, pos.z),
			new Vector3i(pos.x - 1, pos.y + 1, pos.z),
			new Vector3i(pos.x - 1, pos.y - 1, pos.z),

			new Vector3i(pos.x + 1, pos.y + 1, pos.z + 1),
			new Vector3i(pos.x + 1, pos.y - 1, pos.z + 1),
			new Vector3i(pos.x + 1, pos.y + 1, pos.z - 1),
			new Vector3i(pos.x + 1, pos.y - 1, pos.z - 1),

			new Vector3i(pos.x - 1, pos.y + 1, pos.z + 1),
			new Vector3i(pos.x - 1, pos.y - 1, pos.z + 1),
			new Vector3i(pos.x - 1, pos.y + 1, pos.z - 1),
			new Vector3i(pos.x - 1, pos.y - 1, pos.z - 1),
		};
	}

	private bool IsBlockIgnored(BlockValue block)
	{
		if (block.damage > 0)
			return false;

		return (
			block.isair
			|| block.isWater
			|| block.Block.shape.IsTerrain()
			|| block.Block.IsPlant()
			|| block.Block.IsTerrainDecoration
		);
	}

	private Dictionary<string, int> GetUpgradeMaterialsForPos(Vector3i pos)
	{
		if (world.GetChunkFromWorldPos(pos) == null)
			return null;

		BlockValue block = world.GetBlock(pos);
		DynamicProperties upgradeProperties = block.Block.Properties;

		if (block.isair || block.isWater || block.ischild)
			return null;

		var itemName = GetUpgradeItemName(block.Block);
		var itemCount = upgradeProperties.GetInt("UpgradeBlock.ItemCount");

		if (itemName is null || itemName == "" || itemCount <= 0)
			return null;

		return new Dictionary<string, int>(){
			{
				itemName,
				itemCount
			}
		};
	}

	private Dictionary<string, int> GetRepairMaterialsForPos(Vector3i pos)
	{
		if (world.GetChunkFromWorldPos(pos) == null)
			return null;

		BlockValue block = world.GetBlock(pos);

		if (!(block.Block.RepairItems is List<SItemNameCount> repair_items))
			return null;

		Dictionary<string, int> missing_materials = new Dictionary<string, int>();
		float damage_perc = (float)block.damage / block.Block.MaxDamage;

		foreach (SItemNameCount item in repair_items)
		{
			int required_item_count = (int)Mathf.Ceil(item.Count * damage_perc);

			missing_materials.Add(item.ItemName, required_item_count);
		}

		return missing_materials.Count > 0 ? missing_materials : null;
	}

	private int TakeRepairMaterial(string item_name, int itemCount)
	{
		// TODO: optimize this function, by caching 'this.items' in a Hashed structure
		// -> purpose: prevents from iterating over each 'this.items'

		int neededItemCount = itemCount;
		int totalTaken = 0;

		for (int i = 0; i < this.items.Length; i++)
		{
			ItemStack stack = this.items[i];

			if (stack.IsEmpty())
				continue;

			// TODO: see ItemClass.GetItemClass to get an idea of how expensive is this call for `GetItem(string)`
			// TODO: check if `stack.itemValue.ItemClass.Name` can do the job in a more efficient way
			if (stack.itemValue.type != ItemClass.GetItem(item_name).type)
				continue;

			int takenItemsCount = Math.Min(stack.count, neededItemCount);

			neededItemCount -= takenItemsCount;
			stack.count -= takenItemsCount;
			requiredMaterials[item_name] -= takenItemsCount;

			totalTaken += takenItemsCount;

			if (neededItemCount < 0)
				logger.Error($"needed_item_count < 0 (={neededItemCount})");

			if (stack.count < 0)
				logger.Error($"stack.count  < 0 (={stack.count})");

			UpdateSlot(i, stack.Clone());

			if (neededItemCount == 0)
				break;
		}

		return totalTaken;
	}

	public int TakeRepairMaterials(Dictionary<string, int> materials, bool needMaterials)
	{
		int totalTaken = 0;

		foreach (KeyValuePair<string, int> entry in materials)
		{
			// can happen if the structure was modified since the last refresh
			if (!requiredMaterials.ContainsKey(entry.Key))
				requiredMaterials[entry.Key] = entry.Value;

			if (needMaterials)
			{
				totalTaken += TakeRepairMaterial(entry.Key, entry.Value);
			}
			else
			{
				totalTaken += entry.Value;
				requiredMaterials[entry.Key] -= entry.Value;
			}
		}

		return totalTaken;
	}

	private int ComputeRepairableDamages(BlockValue block, int maxRepairableDamages, List<SItemNameCount> repairItems)
	{
		int targetRepairedDamages = Mathf.Min(block.damage, maxRepairableDamages);

		// repair for free of blocks which can't be repaired in vanilla game
		// TODO: add param ForceNonRepairableBlocks
		// TODO: if (block.ischild) ...
		if (repairItems == null)
			return targetRepairedDamages;

		float targetRepairPerc = (float)targetRepairedDamages / block.Block.MaxDamage;
		float blockDamagePerc = (float)block.damage / block.Block.MaxDamage;

		int totalRequired = 0;

		Dictionary<string, int> itemsDict = ItemsToDict();
		Dictionary<string, int> materialsToTake = new Dictionary<string, int>();

		foreach (SItemNameCount item in repairItems)
		{
			int targetItemCount = (int)Mathf.Ceil(item.Count * targetRepairPerc);
			int availableItemCount = itemsDict.TryGetValue(item.ItemName, out availableItemCount) ? availableItemCount : 0;

			// stop trying to repair the block if one material is missing
			if (Config.needsMaterialsForRepair && availableItemCount < targetItemCount)
			{
				return 0;
			}

			materialsToTake[item.ItemName] = targetItemCount;
			totalRequired += (int)Mathf.Ceil(item.Count * blockDamagePerc);
		}

		int totalTaken = TakeRepairMaterials(materialsToTake, Config.needsMaterialsForRepair);

		float repairedDamages = (float)block.damage * totalTaken / totalRequired;

		return (int)Mathf.Ceil(repairedDamages);
	}

	private void RepairBlock(int repairAmount, int clrIdx, BlockValue block, Vector3i pos, string audioClipName)
	{
		block.Block.DamageBlock(GameManager.Instance.World, clrIdx, pos, block, -repairAmount, 0);

		if (!Config.playRepairSound || audioClipName == string.Empty)
			return;

		// play material specific sound (copied from ocbClaimAutoRepair)
		world.GetGameManager().PlaySoundAtPositionServer(
			_pos: pos.ToVector3(),
			_audioClipName: audioClipName,
			_mode: AudioRolloffMode.Logarithmic,
			_distance: 100
		);
	}

	private int TryRepairBlock(World world, Vector3i pos, int maxRepairableDamages)
	{
		BlockValue block = world.GetBlock(pos);
		List<SItemNameCount> repairItems = block.Block.RepairItems;

		int repairableDamages = ComputeRepairableDamages(block, maxRepairableDamages, repairItems);

		if (repairableDamages <= 0)
			return 0;

		TotalDamagesCount -= repairableDamages;

		RepairBlock(repairableDamages, chunk.ClrIdx, block, pos, RepairSound(block));

		return repairableDamages;
	}

	private bool TryUpgradeBlock(World world, Vector3i pos)
	{
		if (!(world.GetChunkFromWorldPos(pos) is Chunk chunk))
			return false;

		Dictionary<string, int> availableMaterials = ItemsToDict();
		Dictionary<string, int> upgradeMaterials = GetUpgradeMaterialsForPos(pos);

		if (upgradeMaterials == null)
			return false;

		foreach (var entry in upgradeMaterials)
		{
			if (!Config.needsMaterialsForUpgrade)
				continue;

			if (!availableMaterials.ContainsKey(entry.Key))
				return false;

			int availableItemCount = availableMaterials[entry.Key];
			int requiredItemCount = upgradeMaterials[entry.Key];

			if (availableItemCount < requiredItemCount)
				return false;
		}

		TakeRepairMaterials(upgradeMaterials, Config.needsMaterialsForUpgrade);

		BlockValue currentBlock = world.GetBlock(pos);
		Vector3i localPos = World.toBlock(pos);

		var textureFull = chunk.GetTextureFull(localPos.x, localPos.y, localPos.z);

		RepairBlock(1, chunk.ClrIdx, currentBlock, pos, Config.upgradeSound);
		SetBlockUpgradable(pos);

		if (Config.keepPaintAfterUpgrade)
		{
			blockChangeInfos.Add(new BlockChangeInfo(
				chunk.ClrIdx,
				pos,
				chunk.GetBlock(localPos.x, localPos.y, localPos.z),
				chunk.GetDensity(localPos.x, localPos.y, localPos.z),
				textureFull
			));
		}

		return true;
	}

	private string GetUpgradeItemName(Block block)
	{
		// NOTE: copied from ItemActionRepair.GetUpgradeItemName()

		string text = block.Properties.Values["UpgradeBlock.Item"];
		if (text != null && text.Length == 1 && text[0] == 'r')
		{
			text = block.RepairItems[0].ItemName;
		}

		return text;
	}

	private bool CanUpgradeBlock(BlockValue block)
	{
		return block.Block.Properties.Values.ContainsKey("UpgradeBlock.UpgradeHitCount");
	}

	private bool CanRefuelBlock(TileEntity te)
	{
		if (!(te is TileEntityPowerSource tileEntity))
			return false;

		// don't handle tileEntities which don't belong to the efficientBaseRepair block owner
		if (tileEntity.ownerID != null && !tileEntity.ownerID.Equals(this.ownerID))
			return false;

		return tileEntity.CurrentFuel < tileEntity.MaxFuel;
	}

	private bool CanReloadBlock(TileEntity te)
	{
		if (!(te is TileEntityPoweredRangedTrap tileEntity))
			return false;

		// don't handle tileEntities which don't belong to the efficientBaseRepair block owner
		if (tileEntity.ownerID != null && !tileEntity.ownerID.Equals(this.ownerID))
			return false;

		foreach (var itemStack in tileEntity.ItemSlots)
		{
			if (itemStack.IsEmpty())
				return true;

			var stackSize = itemStack.itemValue.ItemClass.Stacknumber.Value;
			var itemCount = itemStack.count;

			if (itemCount < stackSize)
				return true;
		}

		return false;
	}

	public void AnalyseStructure(Vector3i initial_pos)
	{
		blocksToRepair.Clear();
		blocksToUpgrade.Clear();
		blocksToRefuel.Clear();
		blocksToReload.Clear();

		List<Vector3i> neighbors = GetNeighbors(initial_pos);
		Dictionary<string, int> visited = new Dictionary<string, int>();

		int maxBfsIterations = Config.maxBfsIterations;
		int iterations = maxBfsIterations;

		while (neighbors.Count > 0 && iterations-- > 0)
		{
			List<Vector3i> neighbors_temp = new List<Vector3i>(neighbors);
			neighbors = new List<Vector3i>();

			foreach (Vector3i pos in neighbors_temp)
			{
				BlockValue block = world.GetBlock(pos);

				bool isIgnored = IsBlockIgnored(block);
				bool isVisited = visited.ContainsKey(pos.ToString());

				if (!isVisited)
					visited.Add(pos.ToString(), 0);

				if (isIgnored || isVisited || block.ischild)
					continue;

				if (block.damage > 0)
				{
					blocksToRepair.Add(pos);
					TotalDamagesCount += block.damage;
				}
				else if (CanUpgradeBlock(block))
				{
					blocksToUpgrade.Add(pos);
				}

				var tileEntity = world.GetTileEntity(pos);

				if (CanRefuelBlock(tileEntity))
					blocksToRefuel.Add(pos);

				if (CanReloadBlock(tileEntity))
					blocksToReload.Add(pos);

				neighbors.AddRange(GetNeighbors(pos));
			}
		}

		DamagedBlockCount = blocksToRepair.Count;
		BfsIterationsCount = maxBfsIterations - iterations;
		VisitedBlocksCount = visited.Count;
		UpgradableBlockCount = blocksToUpgrade.Count;

		logger.Info($"blocksToRepair  : {blocksToRepair.Count}");
		logger.Info($"blocksToUpgrade : {blocksToUpgrade.Count}");
		logger.Info($"blocksToRefuel  : {blocksToRefuel.Count}");
		logger.Info($"blocksToReload  : {blocksToReload.Count}");
		logger.Info($"Iterations      : {maxBfsIterations - iterations}/{maxBfsIterations}");
		logger.Info($"visited_blocks  : {visited.Count}");
	}

	public void ForceRefresh()
	{
		forceFullRefresh = true;
		setModified();
	}

	private void RefreshStats()
	{
		ElapsedTicksSinceLastRefresh = 0;
		DamagedBlockCount = 0;
		BfsIterationsCount = 0;
		VisitedBlocksCount = 0;
		TotalDamagesCount = 0;
		forceFullRefresh = false;

		AnalyseStructure(ToWorldPos());
		RefreshMaterialsStats();
	}

	private void RefreshRepairItems()
	{
		foreach (Vector3i position in blocksToRepair)
		{
			Dictionary<string, int> missingMaterials = GetRepairMaterialsForPos(position);

			if (missingMaterials is null)
				continue;

			foreach (KeyValuePair<string, int> entry in missingMaterials)
			{
				if (!requiredMaterials.ContainsKey(entry.Key))
					requiredMaterials.Add(entry.Key, 0);

				requiredMaterials[entry.Key] += entry.Value;
			}
		}
	}

	private void RefreshUpgradeItems()
	{
		if (!UpgradeOn)
			return;

		foreach (Vector3i position in blocksToUpgrade)
		{
			Dictionary<string, int> missingMaterials = GetUpgradeMaterialsForPos(position);

			if (missingMaterials is null)
				continue;

			foreach (KeyValuePair<string, int> entry in missingMaterials)
			{
				if (!requiredMaterials.ContainsKey(entry.Key))
					requiredMaterials.Add(entry.Key, 0);

				requiredMaterials[entry.Key] += entry.Value;
			}
		}
	}

	private void RefreshRefuelItems()
	{
		foreach (var pos in blocksToRefuel)
		{
			if (!(world.GetTileEntity(pos) is TileEntityPowerSource tileEntity))
				continue;

			int requiredFuel = tileEntity.MaxFuel - tileEntity.CurrentFuel;

			if (requiredFuel <= 0)
				continue;

			if (!requiredMaterials.ContainsKey(propAmmoGasCan))
				requiredMaterials[propAmmoGasCan] = 0;

			requiredMaterials[propAmmoGasCan] += requiredFuel;
		}
	}

	private void RefreshReloadItems()
	{
		foreach (var pos in blocksToReload)
		{
			if (!(world.GetTileEntity(pos) is TileEntityPoweredRangedTrap tileEntity))
				continue;

			var ammoType = tileEntity.AmmoItem;
			var itemName = ammoType.Name;
			var maxStackSize = ammoType.Stacknumber.Value;

			foreach (var itemStack in tileEntity.ItemSlots)
			{
				var requiredAmmos = itemStack.IsEmpty() ? maxStackSize : maxStackSize - itemStack.count;

				if (requiredAmmos <= 0)
					continue;

				if (!requiredMaterials.ContainsKey(itemName))
					requiredMaterials[itemName] = 0;

				requiredMaterials[itemName] += requiredAmmos;
			}
		}
	}

	private void RefreshMaterialsStats()
	{
		forceRefreshMaterials = false;
		requiredMaterials.Clear();

		RefreshRepairItems();
		RefreshUpgradeItems();
		RefreshRefuelItems();
		RefreshReloadItems();
	}

	public void Switch(bool forceRefresh_ = false)
	{
		if (forceRefresh_)
			forceFullRefresh = true;

		IsOn = !IsOn;

		Manager.PlayInsidePlayerHead(IsOn ? "switch_up" : "switch_down");
		setModified();
	}

	public void SwitchUpgrade()
	{
		UpgradeOn = !UpgradeOn;
		forceRefreshMaterials = true;
		setModified();
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		IsOn = _br.ReadBoolean();
		UpgradeOn = _br.ReadBoolean();
		IsPowered = _br.ReadBoolean();

		if (_eStreamMode == StreamModeRead.Persistency)
			return;

		forceRefreshMaterials = _br.ReadBoolean();
		forceFullRefresh = _br.ReadBoolean();
		DamagedBlockCount = _br.ReadInt32();
		TotalDamagesCount = _br.ReadInt32();
		VisitedBlocksCount = _br.ReadInt32();
		BfsIterationsCount = _br.ReadInt32();
		UpgradableBlockCount = _br.ReadInt32();

		// send requiredMaterials from server to client, to update Materials panel.
		requiredMaterials.Clear();

		int requiredMaterialsCount = _br.ReadInt32();
		if (requiredMaterialsCount > 0)
		{
			for (int i = 0; i < requiredMaterialsCount; i++)
			{
				string itemName = _br.ReadString();
				int itemCount = _br.ReadInt32();

				requiredMaterials[itemName] = itemCount;
			}
		}

		// NOTE: reading again items allows to bypass the bUserAccessing condition from base-classes
		// -> allows the TileEntity to take items from containers, even if the user is accessing the container
		// -> /!\ may cause unknown issues on concurency access to the container
		int itemsCount = _br.ReadInt32();
		if (itemsCount > 0)
		{
			for (int i = 0; i < itemsCount; i++)
			{
				items[i].Read(_br);
			}
		}

		if (_eStreamMode == StreamModeRead.FromServer)
			return;

		// force refresh on server side if he receives the param forceRefresh=true from client.
		if (forceFullRefresh)
		{
			logger.Info("Refresh forced from server.");
			RefreshStats();
			setModified();
		}
		else if (forceRefreshMaterials)
		{
			RefreshMaterialsStats();
			setModified();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(IsOn);
		_bw.Write(UpgradeOn);
		_bw.Write(IsPowered);

		if (_eStreamMode == StreamModeWrite.Persistency)
			return;

		_bw.Write(forceRefreshMaterials);
		_bw.Write(forceFullRefresh);
		_bw.Write(DamagedBlockCount);
		_bw.Write(TotalDamagesCount);
		_bw.Write(VisitedBlocksCount);
		_bw.Write(BfsIterationsCount);
		_bw.Write(UpgradableBlockCount);
		_bw.Write(requiredMaterials.Count);

		foreach (KeyValuePair<string, int> entry in requiredMaterials)
		{
			_bw.Write(entry.Key);
			_bw.Write(entry.Value);
		}

		// see the note in the read method upper
		_bw.Write(items.Length);
		foreach (ItemStack stack in items)
		{
			stack.Clone().Write(_bw);
		}

		if (_eStreamMode == StreamModeWrite.ToServer)
			return;

		// trigger forceRefresh=true in single player mode
		// TODO: try with SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer
		if (forceFullRefresh)
		{
			logger.Info("Refresh forced from Client.");
			RefreshStats();
		}
		else if (forceRefreshMaterials)
		{
			RefreshMaterialsStats();
		}
	}

	public bool BloodMoonActive(World _world)
	{
		if (Config.activeDuringBloodMoon)
			return false;

		if (_world.aiDirector == null)
			return false;

		bool bloodMoonActive = _world.aiDirector.BloodMoonComponent.BloodMoonActive;

		if (bloodMoonActive && IsOn)
			Switch();

		return bloodMoonActive;
	}

	private void SetBlockUpgradable(Vector3i position)
	{
		Dictionary<string, int> upgradeMaterials = GetUpgradeMaterialsForPos(position);

		if (upgradeMaterials == null)
			return;

		blocksToUpgrade.Add(position);
		UpgradableBlockCount++;

		if (!UpgradeOn)
			return;

		foreach (KeyValuePair<string, int> entry in upgradeMaterials)
		{
			if (!requiredMaterials.ContainsKey(entry.Key))
				requiredMaterials[entry.Key] = 0;

			requiredMaterials[entry.Key] += entry.Value;
		}
	}

	private bool RepairBlocks(World world)
	{
		int repairableDamages = Config.repairRate > 0 ? Config.repairRate : int.MaxValue;
		int totalRepairedDamages = 0;

		foreach (Vector3i position in new List<Vector3i>(blocksToRepair))
		{
			int repairedDamages = TryRepairBlock(world, position, repairableDamages);

			totalRepairedDamages += repairedDamages;
			repairableDamages -= repairedDamages;

			BlockValue block = world.GetBlock(position);

			if (block.damage == 0)
			{
				blocksToRepair.Remove(position);
				DamagedBlockCount--;
				SetBlockUpgradable(position);
			}

			if (repairableDamages <= 0 && Config.repairRate > 0)
				break;
		}

		if (totalRepairedDamages > 0)
			logger.Info($"{totalRepairedDamages} hit points repaired.");

		return totalRepairedDamages > 0;
	}

	private bool UpgradeBlocks(World world)
	{
		if (!UpgradeOn)
			return false;

		int upgradeCountTarget = Config.upgradeRate > 0 ? Config.upgradeRate : int.MaxValue;
		int upgradedBlocksCount = 0;

		foreach (Vector3i position in new List<Vector3i>(blocksToUpgrade))
		{
			if (!TryUpgradeBlock(world, position))
				continue;

			UpgradableBlockCount--;
			upgradeCountTarget--;
			upgradedBlocksCount++;

			blocksToUpgrade.Remove(position);

			if (upgradeCountTarget == 0)
				break;
		}

		if (upgradedBlocksCount > 0)
			logger.Info($"{upgradedBlocksCount} blocks upgraded.");

		return upgradedBlocksCount > 0;
	}

	private bool RefuelBlocks(World world)
	{
		bool wasModified = false;

		foreach (var pos in blocksToRefuel)
		{
			if (!(world.GetTileEntity(pos) is TileEntityPowerSource tileEntity))
				continue;

			var requiredFuel = tileEntity.MaxFuel - tileEntity.CurrentFuel;
			var fuelTaken = TakeRepairMaterial(propAmmoGasCan, requiredFuel);

			// no more fuel available, we stop here
			if (fuelTaken == 0)
				break;

			tileEntity.CurrentFuel += (ushort)fuelTaken;
			wasModified = true;
		}

		return wasModified;
	}

	private bool ReloadBlocks(World world)
	{
		bool wasModified = false;

		foreach (var pos in blocksToReload)
		{
			if (!(world.GetTileEntity(pos) is TileEntityPoweredRangedTrap tileEntity))
				continue;

			var ammoType = tileEntity.AmmoItem;
			var itemName = ammoType.Name;
			var maxStackSize = ammoType.Stacknumber.Value;
			var itemSlots = tileEntity.ItemSlots;
			var wasReloaded = false;

			foreach (var itemStack in itemSlots)
			{
				var requiredAmmos = itemStack.IsEmpty() ? maxStackSize : maxStackSize - itemStack.count;

				if (requiredAmmos <= 0)
					continue;

				var ammoTaken = TakeRepairMaterial(itemName, requiredAmmos);

				// no more ammo available, we stop here
				if (ammoTaken <= 0)
					break;

				itemStack.count += ammoTaken;
				itemStack.itemValue.type = ammoType.Id;

				wasModified = true;
				wasReloaded = true;
			}

			if (wasReloaded)
			{
				tileEntity.IsLocked = true;
				tileEntity.ItemSlots = itemSlots;
			}
		}

		return wasModified;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);

		blockChangeInfos.Clear();

		if (!IsOn || BloodMoonActive(world))
			return;

		if (ElapsedTicksSinceLastRefresh >= Config.refreshRate && Config.refreshRate > 0)
			RefreshStats();

		bool wasModified = false;

		wasModified |= RefuelBlocks(world);
		wasModified |= ReloadBlocks(world);
		wasModified |= RepairBlocks(world);
		wasModified |= UpgradeBlocks(world);

		if (blockChangeInfos.Count > 0)
		{
			GameManager.Instance.World.SetBlocksRPC(blockChangeInfos);
		}

		if (wasModified)
		{
			setModified();
		}
		else if (Config.autoTurnOff)
		{
			IsOn = false;
			setModified();
		}

		ElapsedTicksSinceLastRefresh++;
	}
}
