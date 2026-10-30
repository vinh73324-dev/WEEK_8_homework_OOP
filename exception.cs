/***********************
Mã sinh viên: 202419018
Họ tên: Phạm Văn Vinh
************************/

using System;

public class InvalidDeviceReadingException : Exception
{
    public string DeviceId { get; }
    public int InvalidValue { get; }

    public InvalidDeviceReadingException(
        string message,
        string deviceId,
        int invalidValue
    ): base(message)
    {
        DeviceId = deviceId;
        InvalidValue = invalidValue;
    }
}