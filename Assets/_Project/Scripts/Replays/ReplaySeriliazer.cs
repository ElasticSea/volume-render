using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ElasticSea.Framework.Extensions;
using Newtonsoft.Json;
using Replays;
using static ElasticSea.Framework.Util.Conversions.EndianBitConverter;

namespace _Project.Scripts.Gameplay.Replays
{
    public class ReplaySeriliazer
    {
        private const int floatLength = 4;
        private const int vectorLength = floatLength * 3;
        private const int stateLength = vectorLength * 3;
        
        public static Replay Read(byte[] bytes)
        {
            var nullByteIndex = bytes.IndexOf(b => b == 0);
            
            var metadataJson = Encoding.UTF8.GetString(bytes, 0, nullByteIndex);
            var replayFile = JsonConvert.DeserializeObject<Replay>(metadataJson);

            var payload = new byte[bytes.Length - (nullByteIndex + 1)];
            Array.Copy(bytes, nullByteIndex + 1, payload, 0, payload.Length);

            replayFile.Snapshots = ReadSnapshots(payload, replayFile.Transforms.Length);
            
            return replayFile;
        }

        public static byte[] Write(Replay replay)
        {
            var metadataJson = JsonConvert.SerializeObject(replay, Formatting.Indented);
            var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);

            var stateCount = replay.Snapshots.First().States.Length;
            var snapshotSize = 4 + stateCount * stateLength;
            var payloadByteCount = replay.Snapshots.Count * snapshotSize;

            var metadataDelimiter = new byte[] {0};
            var output = new byte[metadataBytes.Length + 1 + payloadByteCount];
            Array.Copy(metadataBytes, 0, output, 0, metadataBytes.Length);
            Array.Copy(metadataDelimiter, 0, output, metadataBytes.Length, 1);
            WritePayload(output, metadataBytes.Length + 1, replay.Snapshots);
            return output;
        }

        private static List<Snapshot> ReadSnapshots(byte[] payload, int entitiesCount)
        {
            var snapshotSize = floatLength + entitiesCount * stateLength;
            var snapshots = new List<Snapshot>(payload.Length / snapshotSize);
            
            for (int i = 0; i < snapshots.Capacity; i++)
            {
                var states = new TransformState[entitiesCount];
                for (int j = 0; j < states.Length; j++)
                {
                    var byteIndex = i * snapshotSize + 4 + j * stateLength;
                    states[j] = ReadState(payload, byteIndex);
                }

                snapshots.Add(new Snapshot()
                {
                    Time = Little.ToSingle(payload, i * snapshotSize),
                    States = states
                });
            }

            return snapshots;
        }

        private static TransformState ReadState(byte[] payload, int byteIndex)
        {
            return new TransformState
            {
                Position = ReadVector(payload, byteIndex + vectorLength * 0),
                Rotation = ReadVector(payload, byteIndex + vectorLength * 1),
                Scale = ReadVector(payload, byteIndex + vectorLength * 2)
            };
        }

        private static Vector3 ReadVector(byte[] payload, int byteIndex)
        {
            var x = Little.ToSingle(payload, byteIndex + floatLength * 0);
            var y = Little.ToSingle(payload, byteIndex + floatLength * 1);
            var z = Little.ToSingle(payload, byteIndex + floatLength * 2);
            return new Vector3(x, y, z);
        }

        public static void WriteVector(byte[] payload, int offset, Vector3 v)
        {
            var bx = Little.GetBytes(v.x);
            var by = Little.GetBytes(v.y);
            var bz = Little.GetBytes(v.z);
            Array.Copy(bx, 0, payload, offset + floatLength * 0, floatLength);
            Array.Copy(by, 0, payload, offset + floatLength * 1, floatLength);
            Array.Copy(bz, 0, payload, offset + floatLength * 2, floatLength);
        }

        public static void WriteState(byte[] payload, int offset, TransformState s)
        {
            WriteVector(payload, offset + vectorLength * 0, s.Position);
            WriteVector(payload, offset + vectorLength * 1, s.Rotation);
            WriteVector(payload, offset + vectorLength * 2, s.Scale);
        }

        public static void WriteSnapshot(byte[] payload, int offset, Snapshot s)
        {
            var time = Little.GetBytes(s.Time);
            Array.Copy(time, 0, payload, offset, 4);
            
            for (int i = 0; i < s.States.Length; i++)
            {
                WriteState(payload, offset + 4 + i * stateLength, s.States[i]);
            }
        }

        public static void WritePayload(byte[] payload, int offset, List<Snapshot> snapshots)
        {
            var snapshotCount = snapshots.Count;
            var stateCount = snapshots.First().States.Length;
            var snapshotSize = 4 + stateCount * stateLength;
            
            for (var i = 0; i < snapshotCount; i++)
            {
                WriteSnapshot(payload, offset + i * snapshotSize, snapshots[i]);
            }
        }
    }
}