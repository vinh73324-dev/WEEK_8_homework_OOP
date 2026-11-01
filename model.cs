/***********************
Mã sinh viên: 202419018
Họ tên: Phạm Văn Vinh
************************/

using System;
using System.Data;
using System.Globalization;

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
        // Trang thai khong thay doi thi khong lam gi
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

        // Muc pin khong doi thi khong lam gi
        if (BatteryLevel == newLevel)
            return;

        int oldLevel = BatteryLevel;
        BatteryLevel = newLevel;

        bool wasWarning = oldLevel < 20 && oldLevel > 0;
        bool wasCritical = oldLevel == 0;

        bool isWarning = oldLevel < 20 && oldLevel > 0;
        bool isCritical = oldLevel == 0;

        if (isCritical && !wasCritical)
            RaiseAlert($"Pin may chieu {Id} da bang 0%", AlertSeverity.Critical);

        if (isWarning && !wasWarning)
            RaiseAlert($"Pin may chieu {Id} da xuong duoi muc 20% (Hien tai: {newLevel})", AlertSeverity.Warning);
    }
    #endregion
}

public class AirConditionerRemote: TeachingDevice
{
    public string AssignedRoom {get;}
    public bool IsResponsive {get; private set;}

    #region Constructor
    public AirConditionerRemote(
        string id,
        string name,
        string assignedRoom,
        bool isResponsive
    ): base(id, name)
    {
        AssignedRoom = assignedRoom;
        IsResponsive = true;
    }
    #endregion

    #region Public method
    public void UpdateResponseState(bool isResponsive)
    {
        if (IsResponsive = isResponsive)
            return;
        bool oldState = IsResponsive;
        IsResponsive = isResponsive;

        if (oldState && !IsResponsive)
            RaiseAlert($"Dieu khien dieu hoa {Id} phong {AssignedRoom} khong phan hoi", AlertSeverity.Critical);
    }
    #endregion
}
public class WirelessMicrophone: TeachingDevice
{
    public int BatteryLevel {get; private set;}
    public int SignalQuality {get; private set;}
    
    #region Constructor
    public WirelessMicrophone(
        string id,
        string name,
        int batteryLevel,
        int signalQuality
    ): base(id, name)
    {
        BatteryLevel = batteryLevel;
        SignalQuality = signalQuality;
    }
    #endregion

    #region  Public method
    public void UpdateBatteryLevel(int newLevel)
    {
        if (newLevel < 0 || newLevel > 100)
            throw new InvalidDeviceReadingException("Muc pin phai tu 0 den 100.", Id, newLevel);
        
        if (BatteryLevel == newLevel)
            return;
        
        string prop = "Battery";    //Ten thuoc tinh truyen vao ham kiem tra

        int oldLevel = BatteryLevel;
        BatteryLevel = newLevel;

        // Thuc hien kiem tra trang thai va phat canh cao
        EvaluateAndRaiseAlert(oldLevel, BatteryLevel, 30, prop);
    }

    public void UpdateSignalQuality(int newQuality)
    {
        if (newQuality < 0 || newQuality > 100)
            throw new InvalidDeviceReadingException("Chat luong tin hieu phai tu 0 den 100.", Id, newQuality);
        
        if (SignalQuality == newQuality)
            return;

        string prop = "Signal"; //Ten thuoc tinh truyen vao ham kiem tra

        int oldQuality = SignalQuality;
        SignalQuality = newQuality;

        EvaluateAndRaiseAlert(oldQuality, SignalQuality, 40, prop);
    }
    #endregion
    
    #region Additional method
    // Method kiem tra trang thai cu va moi de dua ra canh cao
    private void EvaluateAndRaiseAlert(int oldValue, int newValue, int threshold, string prop)
    {
        bool wasWarning = oldValue < threshold;
        bool isWarning = newValue < threshold;

        bool wasCritical = oldValue == 0;
        bool isCritical = newValue == 0;

        if (isWarning && !wasWarning)
        {   
            string reason = "";
            if (prop == "Battery")
                reason = $"Dieu khien dieu hoa {Id} co pin yeu (Hien tai: {newValue})";
            if (prop == "Signal")
                reason = $"Dieu khien dieu hoa {Id} co tin hieu yeu (Hien tai: {newValue})";
            
            RaiseAlert(reason, AlertSeverity.Warning);
        }

        if (isCritical && !wasCritical)
        {
            string reason = "";
            if (prop == "Battery")
                reason = $"Dieu khien dieu hoa {Id} het pin";
            if (prop == "Signal")
                reason = $"Dieu khien dieu hoa {Id} khong co tin hieu";
            
            RaiseAlert(reason, AlertSeverity.Critical);
        }
    }
    #endregion
}
#endregion

#region Subscriber
public class ConsoleNotificationService
{
    public void HandleAlert(
        object? sender,
        DeviceAlertEventArgs e
    )
    {
        Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message}");
    }
}

public class DeviceAuditLogger
{
    private readonly List<string> entries = new();
    public IReadOnlyList<string> Entries => entries.AsReadOnly();

    public void HandleStatusChanged(
        object? sender,
        DeviceStatusChangedEventArgs e
    )
    {
        entries.Add(
            $"{e.ChangeAt}: {e.DeviceId}," +
            $"{e.OldStatus} -> {e.NewStatus}");
    }
}

public class MaintenanceService
{
    private readonly List<string> requests = new();
    public IReadOnlyList<string> Requests => requests.AsReadOnly();

    public void HandleCriticalAlert(
        object? sender,
        DeviceAlertEventArgs e
    )
    {
        if (e.Severity != AlertSeverity.Critical)
        {
            return;
        }
        requests.Add($"Maintenance request for {e.DeviceId}: {e.Message}");
    }
}
#endregion

#region Delegate
public delegate bool AlertFilter(DeviceAlertEventArgs alert);
#endregion

#region Advanced Subscriber
public class FilterAlertSubscriber
{
    private readonly AlertFilter filter;
    private readonly Action<DeviceAlertEventArgs> action;

    #region Constructor
    public FilterAlertSubscriber(
        AlertFilter filter,
        Action<DeviceAlertEventArgs> action
    )
    {
        this.filter = filter;
        this.action = action;
    }
    #endregion

    #region Public method
    public void HandleAlert(
        object? sender,
        DeviceAlertEventArgs e
    )
    {
        if (filter(e))
        {
            action(e);
        }
    }
    #endregion

}
#endregion

#region Subscriber Coordinator
public class DeviceMonitoringCoordinator: IDisposable
{
    private readonly List<TeachingDevice> devices;
    private readonly ConsoleNotificationService notificationService;
    private readonly DeviceAuditLogger auditLogger;
    private readonly MaintenanceService maintenanceService;

    private bool isSubscribed = false;
    private bool isDisposed = false;

    #region Constructor
    public DeviceMonitoringCoordinator(
        IEnumerable<TeachingDevice> devices,
        ConsoleNotificationService notificationService,
        DeviceAuditLogger auditLogger,
        MaintenanceService maintenanceService
    )
    {
        this.devices = devices?.ToList() ?? new List<TeachingDevice>();
        this.notificationService = notificationService;
        this.auditLogger = auditLogger;
        this.maintenanceService = maintenanceService;
    }
    #endregion


    #region Public method
    public void Subscribe()
    {
        if (!isSubscribed)
            return;
        
        foreach (var device in devices)
        {
            device.StatusChanged += auditLogger.HandleStatusChanged;

            device.AlertRaised += notificationService.HandleAlert;
            device.AlertRaised += maintenanceService.HandleCriticalAlert;
        }

        isSubscribed = true;
    }

    public void Unsubscribe()
    {
        if(!isSubscribed)
            return;
        
        foreach (var device in devices)
        {
            device.StatusChanged -= auditLogger.HandleStatusChanged;
            device.AlertRaised -= notificationService.HandleAlert;
            device.AlertRaised -= maintenanceService.HandleCriticalAlert;
        }

        isSubscribed = false;
    }

    public void Dispose()
    {
        if (!isDisposed)
            return;
        
        Unsubscribe();
        isDisposed = true;
        // Goi garbage collector
        GC.SuppressFinalize(this);
    }
    #endregion


}
#endregion