namespace CargoParcelTracker.DTOs;

public class VesselDto
{
    public int Id { get; set; }
    public string VesselName { get; set; } = string.Empty;
    public string ImoNumber { get; set; } = string.Empty;
    public decimal Dwt { get; set; }
    public string VesselType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int VoyageAllocationCount { get; set; }
}

public class VesselCreateDto
{
    public string VesselName { get; set; } = string.Empty;
    public string ImoNumber { get; set; } = string.Empty;
    public decimal Dwt { get; set; }
    public string VesselType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
}

public class VesselUpdateDto
{
    public string VesselName { get; set; } = string.Empty;
    public decimal Dwt { get; set; }
    public string VesselType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
}
