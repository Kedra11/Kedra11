namespace AntEmpire.OfflineProgress
{
    /// <summary>
    /// Computes what the colony gathered while the app was closed.
    /// Runs once at boot, after SaveManager.LoadOrCreate() and before
    /// gameplay starts.
    /// </summary>
    public class OfflineProgressService
    {
        // TODO: elapsed = nowUnix - SaveData.lastSaveUnixTime (clamp to a max,
        //       e.g. 8–24h, so very long absences don't explode the economy).
        // TODO: reward = workerCount × workerLevelMultiplier × ratePerSecond × elapsed.
        // TODO: cap rewards by Food Storage capacity.
        // TODO: return a summary object for OfflineRewardPopup
        //       ("Пока тебя не было, колония собрала 1240 Food и 330 Leaves").
        // TODO: apply via ResourceManager.Add, then SaveManager.Save().
    }
}
