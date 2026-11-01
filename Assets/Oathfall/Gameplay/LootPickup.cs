using Oathfall.Core;
using UnityEngine;
namespace Oathfall.Gameplay
{
    public sealed class LootPickup : MonoBehaviour
    {
        public ItemInstance Item { get; private set; }
        private Transform visual;
        private float nextAttempt;
        private bool collected;
        public void Initialize(ItemInstance item)
        {
            Item = item;
            transform.position = CombatSpatial.Ground(transform.position);
            foreach(var collider in GetComponentsInChildren<Collider>()) collider.enabled=false;
            visual = transform.childCount > 0 ? transform.GetChild(0) : null;
        }
        private void Update()
        {
            if(visual != null) { visual.localPosition = Vector3.up * (.3f + Mathf.Sin(Time.time*2)*.06f); visual.Rotate(0,35*Time.deltaTime,0); }
            var game=GameplaySession.Current;
            if(collected || Item == null || game?.Player == null || !game.RunActive || Time.time < nextAttempt) return;
            var delta=game.Player.transform.position-transform.position;
            if(!PickupRules.InReach(delta.x*delta.x+delta.z*delta.z,delta.y)) return;
            if(!CombatSpatial.Clear(transform.position+Vector3.up*.5f,game.Player.transform.position+Vector3.up*.5f)) return;
            var result=game.TryCollect(Item);
            nextAttempt=Time.time+1;
            if(result==PickupResult.Collected || result==PickupResult.Duplicate) { collected=true; Destroy(gameObject); }
        }
    }
}
