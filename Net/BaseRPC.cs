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

        protected Func<ZPackage, ZPackage, bool> _onServerReceive = null;
        protected Func<ZPackage, bool> _onClientReceive = null;

        public void OnServerReceive(Func<ZPackage, ZPackage, bool> cb) => _onServerReceive = cb;
        public void OnClientReceive(Func<ZPackage, bool> cb) => _onClientReceive = cb;

        private bool serverReady = false;
        public void SetServerReady() => serverReady = true;

        public void ResetClientState()
        {
            serverReady = false;
            _onServerReceive = null;
            _onClientReceive = null;
        }

        public BaseRPC(string rpcName)
        {
            _rpcName = rpcName;
            // Don't assume Jotunn networking is ready here.
            TryRegisterRpc();
        }

        public bool TryRegisterRpc()
        {
            if (_rpc != null) return true;

            // Jotunn/Valheim networking might not be ready yet on client startup.
            if (NetworkManager.Instance == null || ZRoutedRpc.instance == null)
                return false;

            _rpc = NetworkManager.Instance.AddRPC(_rpcName, _OnServerReceive, _OnClientReceive);

            //if (_rpc != null)
            //Plugin.Log.LogWarning($"[OTAB] RPC '{_rpcName}' registered");
            //else
            //Plugin.Log.LogWarning($"[OTAB] RPC '{_rpcName}' NOT registered (NetworkManager/ZRoutedRpc not ready?)");

            return _rpc != null;
        }

        public void RequestFromServer()
        {
            if (_rpc == null) return;
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

        private IEnumerator _OnServerReceive(long sender, ZPackage inPkg)
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

            if (_onServerReceive != null)
            {
                if (!_onServerReceive(inPkg, outPkg))
                    yield break;
            }

            _rpc.SendPackage(sender, outPkg);
            yield break;
        }

        private IEnumerator _OnClientReceive(long sender, ZPackage inPkg)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
                yield break;

            if (_onClientReceive != null)
            {
                if (!_onClientReceive(inPkg))
                    yield break;
            }

            yield break;
        }

    }

}
