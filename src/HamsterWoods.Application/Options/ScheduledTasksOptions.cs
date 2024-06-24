namespace HamsterWoods.Options;

/// <summary>
/// Options for scheduled tasks functionality.
/// </summary>
public class ScheduledTasksOptions
{
    // Constants for default values
    private const int DefaultExecutionHour = 8;
    private const int DefaultExpiredDeviceInfoFromDays = 60;
    private const int DefaultExpiredDeviceInfoLimit = 200;
    private const int DefaultDelayFromMilliseconds = 200;
    
    // Fields to store the configuration values
    private int _executionHour;
    private int _expiredDeviceInfoFromDays;
    private int _expiredDeviceInfoLimit;
    private int _delayFromMilliseconds;
    
    /// <summary>
    /// Gets or sets the hour of the day when the scheduled tasks should be executed.
    /// If not set, defaults to DefaultExecutionHour.
    /// </summary>
    public int ExecutionHour
    {
        get => _executionHour > 0 ? _executionHour : DefaultExecutionHour;
        set => _executionHour = value;
    }
    
    /// <summary>
    /// Gets or sets the number of days before a device info is considered expired.
    /// If not set, defaults to DefaultExpiredDeviceInfoFromDays.
    /// </summary>
    public int ExpiredDeviceInfoFromDays {
        get => _expiredDeviceInfoFromDays > 0 ? _expiredDeviceInfoFromDays : DefaultExpiredDeviceInfoFromDays;
        set => _expiredDeviceInfoFromDays = value;
    }
    
    /// <summary>
    /// Gets or sets the limit for the number of expired device info to be processed at once.
    /// If not set, defaults to DefaultExpiredDeviceInfoLimit.
    /// </summary>
    public int ExpiredDeviceInfoLimit {
        get => _expiredDeviceInfoLimit > 0 ? _expiredDeviceInfoLimit : DefaultExpiredDeviceInfoLimit;
        set => _expiredDeviceInfoLimit = value;
    }
    
    /// <summary>
    /// Gets or sets the delay in milliseconds between batches of processing expired device info.
    /// If not set, defaults to DefaultDelayFromMilliseconds.
    /// </summary>
    public int DelayFromMilliseconds {
        get => _delayFromMilliseconds > 0 ? _delayFromMilliseconds : DefaultDelayFromMilliseconds;
        set => _delayFromMilliseconds = value;
    }
}