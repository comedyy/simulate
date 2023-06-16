using System;

public class Battle
{
    BattleWorld _world;
    CheckSumMgr _checkSumMgr;
    public CheckSumMgr CheckSumMgr => _checkSumMgr;

    LocalFrame _localFrame;
    DumpNetworkTransferLayer _transLayer;

    public Battle(float tick, float pingSec, bool randomFixedCount, bool usePlaybackInput, int i, DumpServer _dumpServer, int userCount)
    {
        _localFrame = new LocalFrame();
        _transLayer = new DumpNetworkTransferLayer(pingSec);

        _localFrame.Init(tick, _transLayer.Send);
        _dumpServer?.AddCallback(_transLayer.Receive);

        if(!usePlaybackInput)
        {
            _transLayer.Init(_dumpServer.AddMessage, _localFrame.OnReceive);
        }
        else
        {
            _localFrame.LoadPlayBackInfo();
        }

        _checkSumMgr = new CheckSumMgr();
        _world = new BattleWorld("new " + i, _checkSumMgr, randomFixedCount, tick, _localFrame, i, userCount);
    }

    public bool IsEnd => _world.IsEnd;

    public void Update(float pingSec)
    {
        if(!_world.IsEnd)
        {
            _world.Update();
        }

        _transLayer.Update(pingSec);
        _localFrame.Update();
    }

    internal void SavePlayback()
    {
        _localFrame.SavePlayback();
    }

    internal void Dispose()
    {
        _world.Dispose();
    }
}