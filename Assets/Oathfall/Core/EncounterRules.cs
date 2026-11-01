namespace Oathfall.Core
{
    public static class EncounterRules
    {
        private const float FirstAttackBaseDelay = .55f;
        private const float FirstAttackSlotSpacing = .28f;
        private const int FirstAttackSlots = 6;

        public static float GetFirstAttackDelay(int encounterIndex)
        {
            var slot = encounterIndex < 0 ? 0 : encounterIndex % FirstAttackSlots;
            return FirstAttackBaseDelay + (slot * FirstAttackSlotSpacing);
        }

        public static float GetIncomingDamageMultiplier(bool isBlocking, bool isDodging)
        {
            if (isDodging)
            {
                return 0f;
            }

            return isBlocking ? .35f : 1f;
        }
    }
}
