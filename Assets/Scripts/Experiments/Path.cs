using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField] List<Vector3> points;
    [SerializeField] bool isClosed;
    [SerializeField] bool autoSetTangents;

    public Path(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.left,
            center + (Vector3.left + Vector3.forward) * 0.5f,
            center + (Vector3.right + Vector3.back) * 0.5f,
            center + Vector3.right
        };
    }

    public Vector3 this[int i]
    {
        get
        {
            return points[i];
        }
        set
        {
            //if (au)
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if (isClosed != value)
            {
                isClosed = value;

                if (isClosed)
                {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                    if (autoSetTangents)
                    {
                        AutoSetControlPoints(0);
                        AutoSetControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    if (autoSetTangents)
                    {
                        AutoSetControlStartEnd();
                    }
                }
            }
        }
    }

    public bool AutoSetTangents
    {
        get
        {
            return autoSetTangents;
        }
        set
        {
            if (autoSetTangents != value)
            {
                autoSetTangents = value;
                if (autoSetTangents)
                {
                    AutoSetAllControls();
                }
            }
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public void AddSegment(Vector3 anchorPos)
    {
        points.Add(Vector3.Lerp(points[points.Count - 1], anchorPos, 0.25f));
        points.Add(Vector3.Lerp(points[points.Count - 1], anchorPos, 0.75f));
        points.Add(anchorPos);

        if (autoSetTangents)
        {
            AutoSetAffectedControls(points.Count - 1);
        }
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
        if (autoSetTangents)
        {
            AutoSetAffectedControls(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetControlPoints(segmentIndex * 3 + 3);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || (!isClosed && NumSegments > 1))
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[]
        {
            points[i * 3],
            points[i * 3 + 1],
            points[i * 3 + 2],
            points[LoopIndex(i * 3 + 3)]
        };
    }

    public void MovePoint(int i, Vector3 pos)
    {
        Vector3 deltaMove = pos - points[i];

        if (i % 3 != 0 && autoSetTangents)
            return;

        points[i] = pos;

        if (autoSetTangents)
        {
            AutoSetAffectedControls(i);
        }
        else
        {
            if (i % 3 == 0)
            {
                if (i + 1 < points.Count || isClosed)
                {
                    points[LoopIndex(i + 1)] += deltaMove;
                }
                if (i - 1 >= 0 || isClosed)
                {
                    points[LoopIndex(i - 1)] += deltaMove;
                }
            }
            else
            {
                bool nextIsAnchor = (i + 1) % 3 == 0;
                int linkControl = nextIsAnchor ? i + 2 : i - 2;
                int anchorIndex = nextIsAnchor ? i + 1 : i - 1;

                if (linkControl >= 0 && linkControl < points.Count || isClosed)
                {
                    float dist = (points[LoopIndex(anchorIndex)] - points[LoopIndex(linkControl)]).magnitude;
                    Vector3 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;
                    points[LoopIndex(linkControl)] = points[LoopIndex(anchorIndex)] + dir * dist;
                }
            }

        }
    }

    void AutoSetAffectedControls(int updatedIndex)
    {
        for (int i = updatedIndex - 3; i < updatedIndex + 3; i+=3)
        {
            if (i >= 0 && i < points.Count || isClosed)
            {
                AutoSetControlPoints(i);
            }
        }
        AutoSetControlStartEnd();
    }

    void AutoSetAllControls()
    {
        for (int i = 0; i < points.Count; i+=3)
        {
            AutoSetControlPoints(i);
        }
        AutoSetControlStartEnd();
    }

    void AutoSetControlPoints(int anchorIndex)
    {
        Vector3 anchorPos = points[LoopIndex(anchorIndex)];
        Vector3 dir = Vector3.zero;
        float[] neighborDist = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighborDist[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighborDist[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighborDist[i] * 0.5f;
            }
        }
    }

    void AutoSetControlStartEnd()
    {
        if (isClosed)
            return;
        points[1] = (points[0] + points[2]) * 0.5f;
        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}
