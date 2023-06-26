
using System;

public  class CheckSumMgr
{
    public CheckSum positionChecksum = new CheckSum();
    public CheckSum hpCheckSum = new CheckSum();
    public CheckSum targetFindCheckSum = new CheckSum();
    public CheckSum preRVO = new CheckSum();

    public int Count => positionChecksum.GetHistory().Count;
}