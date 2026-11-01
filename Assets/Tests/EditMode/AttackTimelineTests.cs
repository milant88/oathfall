using NUnit.Framework;
using Oathfall.Core;

namespace Oathfall.Tests.EditMode
{
    public sealed class AttackTimelineTests
    {
        [Test]
        public void EncounterRules_StaggersFirstEnemyAttacksAndMakesDodgesInvulnerable()
        {
            Assert.That(EncounterRules.GetFirstAttackDelay(0), Is.EqualTo(.55f).Within(.0001f));
            Assert.That(EncounterRules.GetFirstAttackDelay(1), Is.GreaterThan(EncounterRules.GetFirstAttackDelay(0)));
            Assert.That(EncounterRules.GetFirstAttackDelay(3), Is.GreaterThan(EncounterRules.GetFirstAttackDelay(2)));
            Assert.That(EncounterRules.GetFirstAttackDelay(5), Is.GreaterThan(EncounterRules.GetFirstAttackDelay(4)));
            Assert.That(EncounterRules.GetIncomingDamageMultiplier(false, true), Is.EqualTo(0f));
            Assert.That(EncounterRules.GetIncomingDamageMultiplier(true, false), Is.EqualTo(.35f));
        }

        [Test]
        public void GetPhase_SeparatesWindupActiveAndRecoveryWindows()
        {
            var timing = new AttackTiming(.18f, .14f, .24f);
            Assert.That(timing.GetPhase(0f), Is.EqualTo(AttackPhase.Windup));
            Assert.That(timing.GetPhase(.179f), Is.EqualTo(AttackPhase.Windup));
            Assert.That(timing.GetPhase(.18f), Is.EqualTo(AttackPhase.Active));
            Assert.That(timing.GetPhase(.319f), Is.EqualTo(AttackPhase.Active));
            Assert.That(timing.GetPhase(.32f), Is.EqualTo(AttackPhase.Recovery));
            Assert.That(timing.GetPhase(.56f), Is.EqualTo(AttackPhase.None));
        }

        [Test]
        public void GetPhase_RejectsTimeOutsideTheAttack()
        {
            var timing = new AttackTiming(.1f, .1f, .1f);
            Assert.That(timing.GetPhase(-.01f), Is.EqualTo(AttackPhase.None));
            Assert.That(timing.GetPhase(.3f), Is.EqualTo(AttackPhase.None));
        }
    }
}
