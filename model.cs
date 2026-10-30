/***********************
Mã sinh viên: 202419018
Họ tên: Phạm Văn Vinh
************************/

using System;

#region Enumeration
public enum DeviceStatus
{
    Available,
    Borrowed,
    UnderMaintenance
}

public enum AlertSeverity
{
    Information,
    Warning,
    Critical
}
#endregion

#region Event Class
public class DeviceStatusChangedEventArgs: EventArgs
{
    public string DeviceId {get;}
    public DeviceStatus OldStatus {get;}
    public DeviceStatus NewStatus {get;}
    public DateTime ChangeAt {get;}

    public DeviceStatusChangedEventArgs(
        string deviceId,
        DeviceStatus oldStatus,
        DeviceStatus newStatus,
        DateTime changeAt
    )
    {
        DeviceId = deviceId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangeAt = changeAt;
    }
}

public class DeviceAlertEventArgs : EventArgs
{
    public string DeviceId { get; }
    public string Message { get; }
    public AlertSeverity Severity { get; }
    public DateTime OccurredAt { get; }

    public DeviceAlertEventArgs(
        string deviceId,
        string message,
        AlertSeverity severity,
        DateTime occurredAt
    )
    {
        DeviceId = deviceId;
        Message = message;
        Severity = severity;
        OccurredAt = occurredAt;
    }
}
#endregion

#region System Device
public abstract class TeachingDevice
{
    public string Id { get; }
    public string Name { get; }
    public DeviceStatus Status { get; private set; }
    public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<DeviceAlertEventArgs>? AlertRaised;

    #region Constructor
    protected TeachingDevice(string id, string name)
    {
        Id = id;
        Name = name;
        Status = DeviceStatus.Available;
    }
    #endregion

    #region Public method
    public void ChangeStatus(DeviceStatus newStatus)
    {
        if (newStatus == Status)
            return;
        
        DeviceStatus oldStatus = Status;
        Status = newStatus;

        OnStatusChanged(
            new DeviceStatusChangedEventArgs(
                Id,
                oldStatus,
                newStatus,
                DateTime.Now
            )
        );
    }
    #endregion

    #region Protected method
    protected void RaiseAlert(
        string message,
        AlertSeverity severity)
    {
        OnAlertRaised(
            new DeviceAlertEventArgs(
                Id,
                message,
                severity,
                DateTime.Now
            )
        );
    }
    #endregion

    #region Virtual method
    protected virtual void OnStatusChanged(DeviceStatusChangedEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }
    protected virtual void OnAlertRaised(DeviceAlertEventArgs e)
    {
        AlertRaised?.Invoke(this, e);
    }
    #endregion
}

public class ProjectorRemote: TeachingDevice
{
    public int BatteryLevel {get; private set;}

    #region Constructor
    public ProjectorRemote(
        string id, 
        string name,
        int batteryLevel
    ): base(id, name)
    {
        if (batteryLevel < 0 || batteryLevel > 100)
            throw new InvalidDeviceReadingException("Muc pin phai tu 0 den 100.", Id, batteryLevel);
        BatteryLevel = batteryLevel;
    }
    #endregion

    #region Public method
    public void UpdateBatteryLevel(int newLevel)
    {
        if (newLevel < 0 || newLevel > 100)
            throw new InvalidDeviceReadingException("Muc pin phai tu 0 den 100.", Id, newLevel);
    }
    #endregion
}
#endregion