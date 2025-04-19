using System.Collections.Generic;
using System.Linq;


class DistributionDataGroup
{
    private readonly List<DistributionData> distribDatas = new List<DistributionData>();

    private readonly ItemClass itemClass;

    private readonly int totalAvailable;

    public DistributionDataGroup(ItemClass itemClass, int totalAvailable)
    {
        this.itemClass = itemClass;
        this.totalAvailable = totalAvailable;
    }

    public void AddDatas(TileEntity tileEntity, int itemCount)
    {
        distribDatas.Add(new DistributionData()
        {
            tileEntity = tileEntity,
            itemClass = itemClass,
            itemCount = itemCount,
        });
    }

    public List<DistributionData> CalcDistributionDatas()
    {
        var distribDatas = new List<DistributionData>();
        var totalRequired = distribDatas.Sum(data => data.itemCount);
        var balance = totalAvailable;

        return distribDatas;
    }
}