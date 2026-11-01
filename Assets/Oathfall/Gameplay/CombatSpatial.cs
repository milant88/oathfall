using UnityEngine;
namespace Oathfall.Gameplay
{
    public static class CombatSpatial
    {
        // World geometry uses layer 8; actors and pickups never block visibility queries.
        public const int WorldMask = 1 << 8;
        public static bool Clear(Vector3 from, Vector3 to) => !Physics.Linecast(from, to, WorldMask, QueryTriggerInteraction.Ignore);
        public static bool TryNavigableSpawn(Vector3 origin,Vector3 target,out Vector3 position)
        {
            // The player's smaller CharacterController can stand outside agent clearance.
            // Validate reachability to the nearby navigable endpoint, without moving the player
            // or accepting another floor or an arbitrarily distant disconnected region.
            if(!UnityEngine.AI.NavMesh.SamplePosition(target,out var targetSurface,1.5f,UnityEngine.AI.NavMesh.AllAreas)||Mathf.Abs(targetSurface.position.y-target.y)>.5f){position=default;return false;}
            target=targetSurface.position;
            var path=new UnityEngine.AI.NavMeshPath();
            for(int attempt=0;attempt<32;attempt++)
            {
                float angle=attempt*2.399f,radius=attempt==0?0:.5f+attempt*.18f;
                var candidate=origin+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);
                if(!UnityEngine.AI.NavMesh.SamplePosition(candidate,out var hit,2,UnityEngine.AI.NavMesh.AllAreas))continue;
                if(Mathf.Abs(hit.position.y-origin.y)>.5f)continue; // Candidate origins are on the intended encounter floor, not roofs/rocks.
                if(Physics.CheckCapsule(hit.position+Vector3.up*.5f,hit.position+Vector3.up*1.5f,.4f,WorldMask,QueryTriggerInteraction.Ignore))continue;
                if(!UnityEngine.AI.NavMesh.CalculatePath(hit.position,target,UnityEngine.AI.NavMesh.AllAreas,path)||path.status!=UnityEngine.AI.NavMeshPathStatus.PathComplete)continue;
                position=hit.position;return true;
            }
            position=default;return false;
        }
        public static Vector3 Ground(Vector3 position)
        {
            // Start near the drop, below overhead arches/bridges, rather than on their roof.
            if(Physics.Raycast(position + Vector3.up * .25f, Vector3.down, out var hit, 5, WorldMask, QueryTriggerInteraction.Ignore)) return hit.point;
            return position;
        }
    }
}
