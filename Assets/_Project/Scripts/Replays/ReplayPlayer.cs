using System.Collections.Generic;
using Replays;
using UnityEngine;

public class ReplayPlayer : MonoBehaviour
{
    [SerializeField] private float timeScale = 1;

    private List<Snapshot> snapshots;
    private List<Transform> transforms;

    public float TimeScale
    {
        get => timeScale;
        set => timeScale = value;
    }

    public int Frame
    {
        get => frame;
        set
        {
            frame = Mathf.Clamp(value, 1, snapshots.Count - 1);
            time = snapshots[frame].Time;
            SetFrame();
        }
    }

    public float Time
    {
        get => time;
        set
        {
            time = value;

            for (var i = 0; i < snapshots.Count; i++)
            {
                var sample = snapshots[i];
                if (sample.Time >= time)
                {
                    break;
                }
                frame = i;
            }
            
            SetFrame();
        }
    }

    public float MinTime => minTime;
    public float MaxTime => maxTime;
    public int Frames => frames;

    public void Load(List<Snapshot> snapshots, List<Transform> transforms)
    {
        time = 0;
        frame = 1;
        frames = snapshots.Count;
        minTime = snapshots[0].Time;
        maxTime = snapshots[snapshots.Count - 1].Time;
        this.snapshots = snapshots;
        this.transforms = transforms;
    }
    
    private int frame = 1;
    private float time = 0;
    private float minTime;
    private float maxTime;
    private int frames;

    private void Update()
    {
        if (snapshots == null || snapshots.Count == 0)
        {
            return;
        }
        
        time = Mathf.Clamp(time + UnityEngine.Time.deltaTime * TimeScale, minTime, maxTime);

        SetFrame();
    }

    private void SetFrame()
    {
        var (prev, next) = GetSnapshots();

        var snapshotDelta = (time - prev.Time) / (next.Time - prev.Time);

        for (var i = 0; i < transforms.Count; i++)
        {
            var transform = transforms[i];
            var prevState = prev.States[i];
            var nextState = next.States[i];
            transform.position = Vector3.Lerp(prevState.Position, nextState.Position, snapshotDelta);
            transform.eulerAngles = Quaternion.Lerp(Quaternion.Euler(prevState.Rotation), Quaternion.Euler(nextState.Rotation), snapshotDelta)
                .eulerAngles;
            transform.localScale = Vector3.Lerp(prevState.Scale, nextState.Scale, snapshotDelta);
        }
    }
    
    private (Snapshot prev, Snapshot next) GetSnapshots()
    {
        while (true)
        {
            if (TimeScale > 0)
            {
                var sample = snapshots[frame];
                if (sample.Time >= time)
                {
                    break;
                }

                frame++;
            }
            else
            {
                var sample = snapshots[frame];
                if (sample.Time <= time)
                {
                    break;
                }

                frame--;
            }
        }

        if (TimeScale > 0)
        {
            var prev = snapshots[frame - 1];
            var next = snapshots[frame];
            return (prev, next);
        }
        else
        {
            var prev = snapshots[frame + 1];
            var next = snapshots[frame];
            return (prev, next);
        }
    }
}