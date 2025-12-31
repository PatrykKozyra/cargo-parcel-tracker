namespace CargoParcelTracker.DTOs;

public class VolumeByGradeDto
{
    public string CrudeGrade { get; set; } = string.Empty;
    public decimal TotalVolumeBbls { get; set; }
    public int ParcelCount { get; set; }
    public decimal AverageVolumeBbls { get; set; }
}

public class ParcelsByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalVolumeBbls { get; set; }
    public decimal Percentage { get; set; }
}

public class VesselUtilizationDto
{
    public string VesselType { get; set; } = string.Empty;
    public int TotalVessels { get; set; }
    public int AvailableVessels { get; set; }
    public int InUseVessels { get; set; }
    public decimal UtilizationPercentage { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalVessels { get; set; }
    public int TotalParcels { get; set; }
    public int TotalAllocations { get; set; }
    public int AvailableVessels { get; set; }
    public decimal TotalVolumeBbls { get; set; }
    public IEnumerable<VolumeByGradeDto> VolumeByGrade { get; set; } = new List<VolumeByGradeDto>();
    public IEnumerable<ParcelsByStatusDto> ParcelsByStatus { get; set; } = new List<ParcelsByStatusDto>();
}
