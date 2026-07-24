namespace AntEmpire.Ants
{
    /// <summary>
    /// The collecting logic of a worker ant: how much it carries, which
    /// resource node it targets, and depositing into the ResourceManager.
    /// </summary>
    public class WorkerAntCollector
    {
        // TODO: carryCapacity from AntTypeData × evolution multiplier.
        // TODO: pick nearest resource node (surface scene).
        // TODO: on deposit: ResourceManager.Add(type, carriedAmount).
        // TODO: hook for the future "Colony Memory" trail-speed mechanic —
        //       count trips per route so often-used routes get faster.
    }
}
