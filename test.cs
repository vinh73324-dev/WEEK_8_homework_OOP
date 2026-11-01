/***********************
Mã sinh viên: 202419018
Họ tên: Phạm Văn Vinh
************************/

using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // 1. Theo dõi sự kiện khi trạng thái thiết bị thay đổi
        SeperateLine();
        Console.WriteLine("1. Theo dõi sự kiện khi trạng thái thiết bị thay đổi");

        var projector = new ProjectorRemote("P01", "Điều khiển máy chiếu Sony", 80);

        projector.StatusChanged += (sender, e) =>
        {
            Console.WriteLine($"Mã thiết bị: {e.DeviceId}");
            Console.WriteLine($"Trạng thái: {e.OldStatus} -> {e.NewStatus}");
            Console.WriteLine($"Thời gian thay đổi: {e.ChangeAt}");
        };
        projector.ChangeStatus(DeviceStatus.Borrowed);
        projector.ChangeStatus(DeviceStatus.Borrowed);

        // 2. Cập nhật pin điều khiển máy chiếu và phát cảnh báo
        SeperateLine();
        Console.WriteLine("2. Cập nhật pin điều khiển máy chiếu và phát cảnh báo");
        projector.AlertRaised += (sender, e) =>
        {
            Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message} - {e.OccurredAt}");
        };
        projector.UpdateBatteryLevel(19);
        projector.UpdateBatteryLevel(10);

        // 3. Kiểm tra giá trị pin không hợp lệ
        SeperateLine();
        Console.WriteLine("3. Kiểm tra giá trị pin không hợp lệ");
        try
        {
            projector.UpdateBatteryLevel(101);
        }
        catch (InvalidDeviceReadingException ex)
        {
            Console.WriteLine($"{ex.Message} (Mã thiết bị: {ex.DeviceId}, Giá trị: {ex.InvalidValue})");
        }

        // 4. Cập nhật trạng thái phản hồi của điều khiển điều hòa
        SeperateLine();
        Console.WriteLine("4. Cập nhật trạng thái phản hồi của điều khiển điều hòa");
        var airConditionerRemote = new AirConditionerRemote(
            "A01", "Điều khiển điều hòa Daikin", "D3-101", true);
        airConditionerRemote.AlertRaised += (sender, e) =>
        {
            Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message} - {e.OccurredAt}");
        };
        airConditionerRemote.UpdateResponseState(false);
        airConditionerRemote.UpdateResponseState(false);

        // 5. Theo dõi pin và chất lượng tín hiệu micro không dây
        SeperateLine();
        Console.WriteLine("5. Theo dõi pin và chất lượng tín hiệu micro không dây");
        var microphone = new WirelessMicrophone("M01", "Micro không dây Shure", 80, 90);
        microphone.AlertRaised += (sender, e) =>
        {
            Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message} - {e.OccurredAt}");
        };
        microphone.UpdateBatteryLevel(25);
        microphone.UpdateSignalQuality(30);

        // 6. Gửi cảnh báo đến dịch vụ thông báo và dịch vụ bảo trì
        SeperateLine();
        Console.WriteLine("6. Gửi cảnh báo đến dịch vụ thông báo và dịch vụ bảo trì");
        var notificationService = new ConsoleNotificationService();
        var maintenanceService = new MaintenanceService();
        var criticalRemote = new AirConditionerRemote(
            "A02", "Điều khiển điều hòa Panasonic", "D5-201", true);
        criticalRemote.AlertRaised += notificationService.HandleAlert;
        criticalRemote.AlertRaised += maintenanceService.HandleCriticalAlert;
        criticalRemote.UpdateResponseState(false);
        Console.WriteLine("Danh sách yêu cầu bảo trì:");
        foreach (var request in maintenanceService.Requests)
        {
            Console.WriteLine($"  {request}");
        }

        // 7. Lọc và xử lý riêng cảnh báo nghiêm trọng bằng delegate
        SeperateLine();
        Console.WriteLine("7. Lọc và xử lý riêng cảnh báo nghiêm trọng bằng delegate");
        AlertFilter criticalFilter = alert => alert.Severity == AlertSeverity.Critical;
        Action<DeviceAlertEventArgs> printCriticalAlert = alert =>
            Console.WriteLine($"Cảnh báo cần xử lý: {alert.DeviceId} - {alert.Message}");
        var filterSubscriber = new FilterAlertSubscriber(criticalFilter, printCriticalAlert);
        var filteredRemote = new AirConditionerRemote(
            "A03", "Điều khiển điều hòa LG", "D2-101", true);
        filteredRemote.AlertRaised += filterSubscriber.HandleAlert;
        filteredRemote.UpdateResponseState(false);

        // 8. Ghi nhận lịch sử thay đổi trạng thái thiết bị
        SeperateLine();
        Console.WriteLine("8. Ghi nhận lịch sử thay đổi trạng thái thiết bị");
        var auditLogger = new DeviceAuditLogger();
        var auditRemote = new ProjectorRemote("P02", "Điều khiển máy chiếu Epson", 60);
        auditRemote.StatusChanged += auditLogger.HandleStatusChanged;
        auditRemote.ChangeStatus(DeviceStatus.UnderMaintenance);
        foreach (var entry in auditLogger.Entries)
        {
            Console.WriteLine($"  {entry}");
        }

        // 9. Đăng ký bộ điều phối theo dõi danh sách thiết bị
        SeperateLine();
        Console.WriteLine("9. Đăng ký bộ điều phối theo dõi danh sách thiết bị");
        var coordinatorDevices = new List<TeachingDevice>
        {
            new ProjectorRemote("P03", "Điều khiển máy chiếu BenQ", 70),
            new AirConditionerRemote("A04", "Điều khiển điều hòa Mitsubishi", "D4-101", true),
            new WirelessMicrophone("M02", "Micro không dây AKG", 75, 85)
        };
        var coordinatorAuditLogger = new DeviceAuditLogger();
        var coordinatorNotifications = new ConsoleNotificationService();
        var coordinatorMaintenance = new MaintenanceService();
        using (var coordinator = new DeviceMonitoringCoordinator(
            coordinatorDevices,
            coordinatorNotifications,
            coordinatorAuditLogger,
            coordinatorMaintenance))
        {
            coordinator.Subscribe();
            coordinatorDevices[0].ChangeStatus(DeviceStatus.Borrowed);
            ((AirConditionerRemote)coordinatorDevices[1]).UpdateResponseState(false);
            Console.WriteLine($"Số sự kiện trạng thái đã ghi nhận: {coordinatorAuditLogger.Entries.Count}");
            Console.WriteLine($"Số yêu cầu bảo trì đã tạo: {coordinatorMaintenance.Requests.Count}");
            coordinator.Unsubscribe();
        }
    }

    static void SeperateLine()
    {
        Console.WriteLine(new string('-', 40));
    }
}
