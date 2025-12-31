namespace CargoParcelTracker.DTOs;

public class CargoParcelDto
{
    public int Id { get; set; }
    public string ParcelName { get; set; } = string.Empty;
    public string CrudeGrade { get; set; } = string.Empty;
    public decimal QuantityBbls { get; set; }
    public string LoadingPort { get; set; } = string.Empty;
    public string DischargePort { get; set; } = string.Empty;
    public DateTime LaycanStart { get; set; }
    public DateTime LaycanEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int VoyageAllocationCount { get; set; }
}

public class CargoParcelCreateDto
{
    public string ParcelName { get; set; } = string.Empty;
    public string CrudeGrade { get; set; } = string.Empty;
    public decimal QuantityBbls { get; set; }
    public string LoadingPort { get; set; } = string.Empty;
    public string DischargePort { get; set; } = string.Empty;
    public DateTime LaycanStart { get; set; }
    public DateTime LaycanEnd { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CargoParcelUpdateDto
{
    public string ParcelName { get; set; } = string.Empty;
    public string CrudeGrade { get; set; } = string.Empty;
    public decimal QuantityBbls { get; set; }
    public string LoadingPort { get; set; } = string.Empty;
    public string DischargePort { get; set; } = string.Empty;
    public DateTime LaycanStart { get; set; }
    public DateTime LaycanEnd { get; set; }
    public string Status { get; set; } = string.Empty;
}
