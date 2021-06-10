using System.Collections.Generic;
using Newtonsoft.Json;

namespace Replays
{
    public class Replay
    {
        public string[] Transforms { get; set; }
        
        [JsonIgnore]
        public List<Snapshot> Snapshots { get; set; }
    }
}