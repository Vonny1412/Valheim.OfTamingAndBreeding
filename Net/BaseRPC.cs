using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Net
{

    internal class BaseRPC
    {
        public static readonly WaitForSeconds OneSecondWait = new WaitForSeconds(1f);

        private readonly string _rpcName;
        protected CustomRPC _rpc;

        protected Func<ZPackage, ZPackage, bool> onServerReceiveCB = null;
        protected Func<ZPackage, bool> onClientReceiveCB = null;

        public void OnServerReceive(Func<ZPackage, ZPackage, bool> cb) => onServerReceiveCB = cb;
        public void OnClientReceive(Func<ZPackage, bool> cb) => onClientReceiveCB = cb;
        
        private bool serverReady = false;
        public void SetServerReady() => serverReady = true;

        public void ResetState(bool clearCallbacks = false)
        {
            Plugin.LogDebug($"[{nameof(BaseRPC)}:{_rpcName}] {nameof(ResetState)}()");
            serverReady = false;
            if (clearCallbacks)
            {
                OnServerReceive(null);
                OnClientReceive(null);
            }
        }

        public BaseRPC(string rpcName)
        {
            _rpcName = rpcName;
            _rpc = NetworkManager.Instance.AddRPC(_rpcName, RPCOnServerReceive, RPCOnClientReceive);
            if (_rpc == null)
                throw new Exception($"[{_rpcName}] AddRPC returned null");
        }

        public void RequestFromServer()
        {
            if (_rpc == null)
            {
                Plugin.LogFatal($"[{_rpcName}] RequestFromServer: RPC not registered yet (Network not ready?)");
                return;
            }
            _rpc.Initiate();
        }


        private readonly Dictionary<long, long> _lastRequestTicks = new Dictionary<long, long>();
        private int _antiSpamOps;

        private bool AntiSpam(long sender)
        {
            long now = DateTime.UtcNow.Ticks;
            long window = TimeSpan.FromSeconds(2).Ticks;

            if (_lastRequestTicks.TryGetValue(sender, out var last) && (now - last) < window)
                return false;

            _lastRequestTicks[sender] = now;

            // prune every 200 ops
            if ((++_antiSpamOps % 200) == 0)
            {
                long keep = TimeSpan.FromMinutes(5).Ticks;
                var toRemove = new List<long>();
                foreach (var kv in _lastRequestTicks)
                    if (now - kv.Value > keep)
                        toRemove.Add(kv.Key);

                for (int i = 0; i < toRemove.Count; i++)
                    _lastRequestTicks.Remove(toRemove[i]);
            }

            return true;
        }

        private IEnumerator RPCOnServerReceive(long sender, ZPackage inPkg)
        {
            if (ZNet.instance == null || !ZNet.instance.IsServer())
                yield break;

            if (!AntiSpam(sender))
            {
                yield break;
            }

            int tries = 10;
            while (!serverReady && tries-- > 0)
            {
                yield return OneSecondWait;
            }
            if (!serverReady)
            {
                yield break;
            }

            var outPkg = new ZPackage();

            if (onServerReceiveCB != null)
            {
                if (!onServerReceiveCB(inPkg, outPkg))
                    yield break;
            }

            _rpc.SendPackage(sender, outPkg);
            yield break;
        }

        private IEnumerator RPCOnClientReceive(long sender, ZPackage inPkg)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
                yield break;

            if (onClientReceiveCB != null)
            {
                if (!onClientReceiveCB(inPkg))
                    yield break;
            }

            yield break;
        }

    }

}
