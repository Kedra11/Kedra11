namespace AntEmpire.Economy
{
    /// <summary>
    /// The four MVP resources.
    ///
    /// Food   — main currency, hatches new ants.
    /// Leaves — building material for rooms.
    /// Soil   — tunnel expansion.
    /// DNA    — evolution and rare upgrades.
    /// </summary>
    public enum ResourceType
    {
        Food,
        Leaves,
        Soil,
        DNA
    }

    /// <summary>
    /// Stable string ids used in save files and configs.
    /// Never rename these once players have saves — display names live in
    /// ScriptableObject configs, ids stay fixed forever.
    /// </summary>
    public static class ResourceIds
    {
        public const string Food = "food";
        public const string Leaves = "leaves";
        public const string Soil = "soil";
        public const string DNA = "dna";

        public static string FromType(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Food: return Food;
                case ResourceType.Leaves: return Leaves;
                case ResourceType.Soil: return Soil;
                case ResourceType.DNA: return DNA;
                default: return Food;
            }
        }
    }
}
