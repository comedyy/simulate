
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
}