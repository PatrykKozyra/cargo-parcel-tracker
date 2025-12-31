# Cargo Parcel Tracker - Web API Documentation

## Overview

The Cargo Parcel Tracker provides a comprehensive RESTful API alongside the MVC web interface, showcasing .NET's ability to serve both web UI and API from the same codebase.

## API Base URL

- **Development**: `https://localhost:5001/api` or `http://localhost:5000/api`

## Swagger/OpenAPI Documentation

Interactive API documentation is available via Swagger UI:

- **Swagger UI**: `https://localhost:5001/api/docs` or `http://localhost:5000/api/docs`
- **OpenAPI Spec**: `https://localhost:5001/swagger/v1/swagger.json`

## API Endpoints

### Cargo Parcels API (`/api/parcels`)

#### Get All Parcels (with Pagination & Filtering)
```http
GET /api/parcels?pageNumber=1&pageSize=10&status=Nominated&crudeGrade=Brent&sortBy=ParcelName&sortOrder=asc
```

**Query Parameters:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status (Nominated, Confirmed, Loading, InTransit, Discharged, Cancelled)
- `crudeGrade` (string): Filter by crude grade
- `sortBy` (string): Sort field (ParcelName, QuantityBbls, LaycanStart)
- `sortOrder` (string): Sort direction (asc, desc)

**Response:** `PagedResultDto<CargoParcelDto>`

#### Get Parcel by ID
```http
GET /api/parcels/{id}
```

**Response:** `CargoParcelDto`

#### Create New Parcel
```http
POST /api/parcels
Content-Type: application/json

{
  "ParcelName": "BRENT-2025-001",
  "CrudeGrade": "Brent Crude",
  "QuantityBbls": 1000000,
  "LoadingPort": "Sullom Voe",
  "DischargePort": "Rotterdam",
  "LaycanStart": "2025-02-01T00:00:00Z",
  "LaycanEnd": "2025-02-05T00:00:00Z",
  "Status": "Nominated"
}
```

**Response:** `201 Created` with `CargoParcelDto`

#### Update Parcel
```http
PUT /api/parcels/{id}
Content-Type: application/json

{
  "ParcelName": "BRENT-2025-001-UPDATED",
  "CrudeGrade": "Brent Crude",
  "QuantityBbls": 1050000,
  "LoadingPort": "Sullom Voe",
  "DischargePort": "Rotterdam",
  "LaycanStart": "2025-02-01T00:00:00Z",
  "LaycanEnd": "2025-02-05T00:00:00Z",
  "Status": "Confirmed"
}
```

**Response:** `204 No Content`

#### Delete Parcel
```http
DELETE /api/parcels/{id}
```

**Response:** `204 No Content`

---

### Vessels API (`/api/vessels`)

#### Get All Vessels (with Pagination & Filtering)
```http
GET /api/vessels?pageNumber=1&pageSize=10&vesselType=VLCC&status=Available&sortBy=VesselName&sortOrder=asc
```

**Query Parameters:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `vesselType` (string): Filter by type (VLCC, Suezmax, Aframax, Panamax)
- `status` (string): Filter by status (Available, OnVoyage, InMaintenance, Chartered)
- `sortBy` (string): Sort field (VesselName, Dwt, VesselType)
- `sortOrder` (string): Sort direction (asc, desc)

**Response:** `PagedResultDto<VesselDto>`

#### Get Vessel by ID
```http
GET /api/vessels/{id}
```

**Response:** `VesselDto`

#### Get Available Vessels
```http
GET /api/vessels/available
```

**Response:** `IEnumerable<VesselDto>`

#### Create New Vessel
```http
POST /api/vessels
Content-Type: application/json

{
  "VesselName": "MT Pacific Voyager",
  "ImoNumber": "IMO9876543",
  "Dwt": 320000,
  "VesselType": "VLCC",
  "CurrentStatus": "Available"
}
```

**Response:** `201 Created` with `VesselDto`

#### Update Vessel
```http
PUT /api/vessels/{id}
Content-Type: application/json

{
  "VesselName": "MT Pacific Voyager",
  "Dwt": 320000,
  "VesselType": "VLCC",
  "CurrentStatus": "OnVoyage"
}
```

**Response:** `204 No Content`

#### Delete Vessel
```http
DELETE /api/vessels/{id}
```

**Response:** `204 No Content`

---

### Analytics API (`/api/analytics`)

#### Get Volume by Crude Grade
```http
GET /api/analytics/volume-by-grade
```

**Response:**
```json
[
  {
    "CrudeGrade": "Brent Crude",
    "TotalVolumeBbls": 15000000,
    "ParcelCount": 15,
    "AverageVolumeBbls": 1000000
  }
]
```

#### Get Parcels by Status
```http
GET /api/analytics/parcels-by-status
```

**Response:**
```json
[
  {
    "Status": "Nominated",
    "Count": 25,
    "TotalVolumeBbls": 25000000,
    "Percentage": 33.33
  }
]
```

#### Get Vessel Utilization
```http
GET /api/analytics/vessel-utilization
```

**Response:**
```json
[
  {
    "VesselType": "VLCC",
    "TotalVessels": 20,
    "AvailableVessels": 5,
    "InUseVessels": 15,
    "UtilizationPercentage": 75.00
  }
]
```

#### Get Dashboard Summary
```http
GET /api/analytics/dashboard
```

**Response:** `DashboardSummaryDto` with comprehensive statistics

#### Get Top Parcels by Volume
```http
GET /api/analytics/top-parcels?limit=10
```

**Query Parameters:**
- `limit` (int): Number of results (default: 10, max: 50)

**Response:** `IEnumerable<CargoParcelDto>`

#### Get Statistics for Specific Crude Grade
```http
GET /api/analytics/grade/Brent%20Crude
```

**Response:** `VolumeByGradeDto`

---

## Data Transfer Objects (DTOs)

### CargoParcelDto
```json
{
  "Id": 1,
  "ParcelName": "BRENT-2025-001",
  "CrudeGrade": "Brent Crude",
  "QuantityBbls": 1000000,
  "LoadingPort": "Sullom Voe",
  "DischargePort": "Rotterdam",
  "LaycanStart": "2025-02-01T00:00:00Z",
  "LaycanEnd": "2025-02-05T00:00:00Z",
  "Status": "Nominated",
  "CreatedDate": "2025-01-15T10:30:00Z",
  "VoyageAllocationCount": 1
}
```

### VesselDto
```json
{
  "Id": 1,
  "VesselName": "MT Atlantic Explorer",
  "ImoNumber": "IMO1234567",
  "Dwt": 320000,
  "VesselType": "VLCC",
  "CurrentStatus": "Available",
  "VoyageAllocationCount": 5
}
```

### PagedResultDto<T>
```json
{
  "Items": [...],
  "TotalCount": 100,
  "PageNumber": 1,
  "PageSize": 10,
  "TotalPages": 10,
  "HasPrevious": false,
  "HasNext": true
}
```

---

## Features

### 1. **RESTful Design**
- Standard HTTP methods (GET, POST, PUT, DELETE)
- Proper status codes (200, 201, 204, 400, 404, 500)
- Resource-based URLs

### 2. **Pagination**
- Configurable page size (max 100 items)
- Page number navigation
- Total count and page metadata

### 3. **Filtering**
- Filter by status, type, crude grade
- Case-insensitive string matching
- Multiple filter combinations

### 4. **Sorting**
- Sort by any major field
- Ascending/descending order
- Default sorting applied

### 5. **DTO Pattern**
- Separate DTOs for read/create/update operations
- No direct entity exposure
- Clean API contracts

### 6. **Analytics Endpoints**
- Aggregated data by grade and status
- Vessel utilization statistics
- Dashboard summary with KPIs

### 7. **Error Handling**
- Structured error responses
- Validation messages
- Logging for diagnostics

### 8. **Swagger/OpenAPI**
- Interactive API documentation
- Try-it-out functionality
- Schema definitions

---

## Architecture Patterns

### Repository Pattern with API Layer
```
API Controller → Repository → Entity Framework → Database
      ↓
    DTO Mapping
      ↓
  JSON Response
```

### Same Codebase, Dual Interface
- **MVC Views**: `/Home`, `/Vessels`, `/CargoParcels`, `/VoyageAllocations`
- **RESTful API**: `/api/vessels`, `/api/parcels`, `/api/analytics`

### Dependency Injection
All API controllers use constructor injection for repositories and logging:
```csharp
public CargoParcelsApiController(
    ICargoParcelRepository parcelRepository,
    ILogger<CargoParcelsApiController> logger)
{
    _parcelRepository = parcelRepository;
    _logger = logger;
}
```

---

## Example Usage

### Using cURL

```bash
# Get all parcels with filtering
curl -X GET "https://localhost:5001/api/parcels?pageNumber=1&pageSize=5&status=Nominated"

# Get analytics dashboard
curl -X GET "https://localhost:5001/api/analytics/dashboard"

# Create new parcel
curl -X POST "https://localhost:5001/api/parcels" \
  -H "Content-Type: application/json" \
  -d '{
    "ParcelName": "TEST-001",
    "CrudeGrade": "Brent Crude",
    "QuantityBbls": 1000000,
    "LoadingPort": "Sullom Voe",
    "DischargePort": "Rotterdam",
    "LaycanStart": "2025-02-01T00:00:00Z",
    "LaycanEnd": "2025-02-05T00:00:00Z",
    "Status": "Nominated"
  }'
```

### Using JavaScript/Fetch

```javascript
// Get vessels with pagination
const response = await fetch('https://localhost:5001/api/vessels?pageNumber=1&pageSize=10');
const data = await response.json();
console.log(data);

// Get volume by grade analytics
const analytics = await fetch('https://localhost:5001/api/analytics/volume-by-grade');
const volumeData = await analytics.json();
console.log(volumeData);
```

---

## Testing

Access the Swagger UI at `https://localhost:5001/api/docs` to:
- View all available endpoints
- See request/response schemas
- Test API calls directly in browser
- Download OpenAPI specification

---

## Notes

- All timestamps are in UTC
- Pagination maximum page size is 100 items
- IMO numbers must be unique for vessels
- Enum values are case-sensitive in requests
- All API responses use PascalCase for property names
