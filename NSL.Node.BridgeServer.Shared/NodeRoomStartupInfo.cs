﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NSL.Node.BridgeServer.Shared
{
    public class NodeRoomStartupInfo
    {
        public const string SystemVariablePrefix = "system__room";

        public const string RoomWaitReadyVariableName = SystemVariablePrefix + "__wait_ready";
        public const string RoomNodeCountVariableName = SystemVariablePrefix + "__node_count";
        public const string RoomStartupTimeoutVariableName = SystemVariablePrefix + "__startup_timeout";
        public const string RoomShutdownOnMissedReadyVariableName = SystemVariablePrefix + "__shutdown_on_missed_ready";

        internal Dictionary<string, string> collection;

        public NodeRoomStartupInfo() : this(new Dictionary<string, string>())
        {
            SetRoomWaitReady(false);
            SetRoomNodeCount(0);
            SetRoomStartupTimeout(60_000);
            SetRoomShutdownOnMissed(false);
        }

        public NodeRoomStartupInfo(IEnumerable<KeyValuePair<string, string>> collection)
        {
            this.collection = collection.ToDictionary(x => x.Key, x => x.Value);
        }

        public NodeRoomStartupInfo SetValue(string key, string value)
        {
            if (collection.ContainsKey(key))
            {
                collection[key] = value;
                return this;
            }

            collection.Add(key, value);
            return this;
        }


        public string GetValue(string key)
        {
            collection.TryGetValue(key, out var value);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue">IConvertible be converted to string with InvariantCulture</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public NodeRoomStartupInfo SetValue<TValue>(string key, TValue value) where TValue : IConvertible
            => SetValue(key, value.ToString(CultureInfo.InvariantCulture));

        public TValue GetValue<TValue>(string key)
            where TValue : IConvertible
            => (TValue)Convert.ChangeType(GetValue(key), typeof(TValue), CultureInfo.InvariantCulture);

        public void CopyTo(NodeRoomStartupInfo other)
            => other.collection = collection;

        public IEnumerable<KeyValuePair<string, string>> GetCollection()
            => collection;

        public NodeRoomStartupInfo SetRoomNodeCount(int value)
            => SetValue(RoomNodeCountVariableName, value);

        public int GetRoomNodeCount()
            => GetValue<int>(RoomNodeCountVariableName);

        public NodeRoomStartupInfo SetRoomWaitReady(bool value)
            => SetValue(RoomWaitReadyVariableName, value);

        public bool GetRoomWaitReady()
            => GetValue<bool>(RoomWaitReadyVariableName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">time(ms.)</param>
        /// <returns></returns>
        public NodeRoomStartupInfo SetRoomStartupTimeout(int value)
            => SetValue(RoomStartupTimeoutVariableName, value);

        public int GetRoomStartupTimeout()
            => GetValue<int>(RoomStartupTimeoutVariableName);

        public NodeRoomStartupInfo SetRoomShutdownOnMissed(bool value)
            => SetValue(RoomShutdownOnMissedReadyVariableName, value);

        public bool GetRoomShutdownOnMissed()
            => GetValue<bool>(RoomShutdownOnMissedReadyVariableName);
    }
}
