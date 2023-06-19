using System;

public class Battle
{
    BattleWorld _world;
    CheckSumMgr _checkSumMgr;
    public CheckSumMgr CheckSumMgr => _checkSumMgr;

    LocalFrame _localFrame;
    DumpGameClientSocket _transLayer;

    public Battle(float tick, float pingSec, bool randomFixedCount, bool usePlaybackInput, int i, Server _dumpServer, int userCount)
    {
        _localFrame = new LocalFrame();

        if(usePlaybackInput)
        {
            _localFrame.LoadPlayBackInfo();
        }
        else if(_dumpServer != null)
        {
            _localFrame.Init(tick, pingSec, _dumpServer.ServerDumpSocket);
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