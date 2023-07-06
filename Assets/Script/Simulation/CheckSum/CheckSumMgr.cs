
using System;

public  class CheckSumMgr
{
    public CheckSum positionChecksum = new CheckSum();
    public CheckSum hpCheckSum = new CheckSum();
    public CheckSum targetFindCheckSum = new CheckSum();
    public CheckSum preRVO = new CheckSum();

    public int Count => positionChecksum.GetHistory().Count;

    public int CurrentCheckSum{
        get{
            var hash = 0;
            hash = CheckSum.CombineHashCode(hash, positionChecksum.GetHistoryCheckSums());
            hash = CheckSum.CombineHashCode(hash, hpCheckSum.GetHistoryCheckSums());
            hash = CheckSum.CombineHashCode(hash, targetFindCheckSum.GetHistoryCheckSums());
            hash = CheckSum.CombineHashCode(hash, preRVO.GetHistoryCheckSums());
            return hash;
        }
    }

    public int CurrentCheckSumEvery(out int frame)
    {
        var step = positionChecksum.GetHistory().Count / 60;
        frame = step * 60;

        if(frame >= positionChecksum.GetHistory().Count)
        {
            return -1;
        }

        var hash = 0;
        hash = CheckSum.CombineHashCode(hash, positionChecksum.GetHistory()[frame]);
        hash = CheckSum.CombineHashCode(hash, hpCheckSum.GetHistory()[frame]);
        hash = CheckSum.CombineHashCode(hash, targetFindCheckSum.GetHistory()[frame]);
        hash = CheckSum.CombineHashCode(hash, preRVO.GetHistory()[frame]);
        return hash;
    }
}