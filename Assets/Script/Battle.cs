using System;

public class Battle
{
    BattleWorld _world;
    CheckSumMgr _checkSumMgr;
    public CheckSumMgr CheckSumMgr => _checkSumMgr;

    LocalFrame _localFrame;
    DumpServer _dumpServer;
    DumpNetworkTransferLayer _transLayer;

    public Battle(float tick, float pingSec, bool usePlaybackInput, int i)
    {
        _dumpServer = new DumpServer();
        _localFrame = new LocalFrame();
        _transLayer = new DumpNetworkTransferLayer(pingSec);

        _localFrame.Init(tick, _transLayer.Send);
        _dumpServer.Init(tick, _transLayer.Receive);

        if(!usePlaybackInput)
        {
            _transLayer.Init(_dumpServer.AddMessage, _localFrame.OnReceive);
        }
        else
        {
            _localFrame.LoadPlayBackInfo();
        }

        _checkSumMgr = new CheckSumMgr();
        _world = new BattleWorld("new " + i, _checkSumMgr, tick, _localFrame, 1);
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
        _dumpServer.Update();
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