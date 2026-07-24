namespace AntEmpire.Economy
{
    /// <summary>
    /// Result of a spend attempt. Lets callers (UI, upgrade services) show a
    /// meaningful message instead of guessing why a purchase failed.
    /// </summary>
    public enum ResourceTransactionResult
    {
        Success,
        NotEnoughResources,
        InvalidAmount
    }
}
