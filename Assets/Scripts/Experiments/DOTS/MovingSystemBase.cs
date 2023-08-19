using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class MovingSystemBase : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref LocalTransform transform, in Speed speed) => {
            transform.Position += new float3(SystemAPI.Time.DeltaTime, 0, 0);
        }).Run();
    }
}
