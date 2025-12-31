namespace CargoParcelTracker.ViewModels;

/// <summary>
/// Parcel summary with aggregated statistics
/// </summary>
public class ParcelSummaryViewModel
{
    public string Status { get; set; } = string.Empty;
    public int ParcelCount { get; set; }
    public decimal TotalVolumeBbls { get; set; }
    public decimal AverageVolumeBbls { get; set; }
    public decimal MinVolumeBbls { get; set; }
    public decimal MaxVolumeBbls { get; set; }
    public int AllocationCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

/// <summary>
/// Vessel utilization with calculated metrics
/// </summary>
public class VesselUtilizationViewModel
{
    public int VesselId { get; set; }
    public string VesselName { get; set; } = string.Empty;
    public string ImoNumber { get; set; } = string.Empty;
    public string VesselType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int TotalVoyages { get; set; }
    public int DaysAtSea { get; set; }
    public int DaysIdle { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public decimal TotalCargoCarriedBbls { get; set; }
    public decimal AverageFreightRate { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Crude grade analysis with market insights
/// </summary>
public class CrudeGradeAnalysisViewModel
{
    public string CrudeGrade { get; set; } = string.Empty;
    public int ParcelCount { get; set; }
    public decimal TotalVolumeBbls { get; set; }
    public decimal AverageVolumeBbls { get; set; }
    public decimal MarketSharePercentage { get; set; }
    public int AllocationCount { get; set; }
    public decimal AverageFreightRate { get; set; }
    public decimal MinFreightRate { get; set; }
    public decimal MaxFreightRate { get; set; }
    public string MostUsedPort { get; set; } = string.Empty;
    public string MostCommonStatus { get; set; } = string.Empty;
}

/// <summary>
/// Laycan calendar entry for upcoming loading windows
/// </summary>
public class LaycanCalendarViewModel
{
    public int ParcelId { get; set; }
    public string ParcelName { get; set; } = string.Empty;
    public string CrudeGrade { get; set; } = string.Empty;
    public decimal QuantityBbls { get; set; }
    public string LoadingPort { get; set; } = string.Empty;
    public string DischargePort { get; set; } = string.Empty;
    public DateTime LaycanStart { get; set; }
    public DateTime LaycanEnd { get; set; }
    public int LaycanDuration { get; set; }
    public int DaysUntilLaycan { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsAllocated { get; set; }
    public string? VesselName { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
}

/// <summary>
/// Port activity summary
/// </summary>
public class PortActivityViewModel
{
    public string PortName { get; set; } = string.Empty;
    public int LoadingCount { get; set; }
    public int DischargeCount { get; set; }
    public int TotalOperations { get; set; }
    public decimal TotalVolumeLoaded { get; set; }
    public decimal TotalVolumeDischarged { get; set; }
    public List<string> TopCrudeGrades { get; set; } = new();
    public decimal AverageTurnaroundDays { get; set; }
}

/// <summary>
/// Financial summary report
/// </summary>
public class FinancialSummaryViewModel
{
    public decimal TotalFreightRevenue { get; set; }
    public decimal TotalDemurrage { get; set; }
    public decimal AverageFreightRate { get; set; }
    public decimal AverageDemurrageRate { get; set; }
    public int TotalAllocations { get; set; }
    public int ActiveVoyages { get; set; }
    public decimal TotalVolumeTransported { get; set; }
    public Dictionary<string, decimal> RevenueByVesselType { get; set; } = new();
    public Dictionary<string, decimal> RevenueByGrade { get; set; } = new();
}

/// <summary>
/// Comprehensive dashboard view combining all reports
/// </summary>
public class DashboardReportViewModel
{
    public List<ParcelSummaryViewModel> ParcelSummaries { get; set; } = new();
    public List<VesselUtilizationViewModel> TopVessels { get; set; } = new();
    public List<CrudeGradeAnalysisViewModel> GradeAnalysis { get; set; } = new();
    public List<LaycanCalendarViewModel> UpcomingLaycans { get; set; } = new();
    public FinancialSummaryViewModel FinancialSummary { get; set; } = new();
    public int TotalParcels { get; set; }
    public int TotalVessels { get; set; }
    public int TotalAllocations { get; set; }
}

/// <summary>
/// Route analysis - most common shipping routes
/// </summary>
public class RouteAnalysisViewModel
{
    public string LoadingPort { get; set; } = string.Empty;
    public string DischargePort { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public int ShipmentCount { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal AverageVolume { get; set; }
    public decimal AverageFreightRate { get; set; }
    public List<string> CrudeGrades { get; set; } = new();
    public int AverageTransitDays { get; set; }
}
