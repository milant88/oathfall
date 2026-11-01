using NUnit.Framework;
using System.Linq;
using Oathfall.Core;
namespace Oathfall.Tests.EditMode
{
    public class ExpeditionContentTests
    {
        [Test] public void WardRequiresLivingPlayerInsideAndCannotOverflow()
        {
            var ward=new WardProgress(75);ward.Tick(100,false,true);ward.Tick(100,true,false);ward.Tick(-10,true,true);Assert.That(ward.Elapsed,Is.Zero);
            ward.Tick(74,true,true);Assert.That(ward.Complete,Is.False);ward.Tick(100,true,true);Assert.That(ward.Complete,Is.True);Assert.That(ward.Elapsed,Is.EqualTo(75));
        }
        [Test] public void RouteHasUniqueContentAndBoundedCombatBudgets()
        {
            var stops=ExpeditionContent.Stops;Assert.That(stops.Select(s=>s.Id).Distinct().Count(),Is.EqualTo(stops.Length));Assert.That(stops.Last().Kind,Is.EqualTo(EncounterKind.Guardian));
            Assert.That(stops.Count(s=>s.Kind==EncounterKind.Ward),Is.EqualTo(3));Assert.That(stops.Count(s=>s.Kind==EncounterKind.Sanctuary),Is.EqualTo(2));
            foreach(var stop in stops){Assert.That(stop.Count,Is.InRange(0,6));if(stop.Kind==EncounterKind.Skirmish)Assert.That(stop.Waves,Is.InRange(1,4));}
        }
        [Test] public void DiscardFreesSlotWithoutAllowingDuplicateRecollection()
        {
            var bag=new InventoryState();var item=new ItemInstance{InstanceId="discard-regression",Slot=ItemSlot.Weapon};bag.TryCollect(item);Assert.That(bag.Discard(-1),Is.False);Assert.That(bag.Discard(0),Is.True);Assert.That(bag.Items.Count,Is.Zero);Assert.That(bag.TryCollect(item),Is.EqualTo(PickupResult.Duplicate));Assert.That(bag.Discard(0),Is.False);
        }
    }
}
