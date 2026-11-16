# Service coverage baseline

Measured on 16 November 2026 against commit 20e0b4e.

## Test result

15 tests passed, 0 failed, 0 skipped.

## Reproduce the report

Run from the folder containing FleetLog.sln:

dotnet test FleetLog.sln --collect:"XPlat Code Coverage" --results-directory ./TestResults

The command prints the path to coverage.cobertura.xml.
Generated reports are ignored by Git.

## Service line coverage

These figures count executable source lines in the four service files,
including their result records. Duplicate line numbers within each file
are counted once.

| File | Covered lines | Total lines | Coverage |
| --- | ---: | ---: | ---: |
| FuelEntryService.cs | 0 | 35 | 0% |
| RepairService.cs | 36 | 38 | 94.74% |
| VehicleService.cs | 29 | 29 | 100% |
| MileageRules.cs | 3 | 3 | 100% |
| Total | 68 | 105 | 64.76% |

The service baseline is below the assignment's 65% target.
The whole-application line coverage is 10.99%; it also includes
controllers, views and other code.

Line coverage does not prove that every decision or scenario is tested.
The suite includes mileage unit tests and SQLite service integration tests.
SQLite tests do not verify all SQL Server-specific behaviour.

## Priority testing gaps

1. FuelEntryService: successful saving and mileage update; missing vehicle;
   future date; mileage below the current reading.
2. RepairService: missing vehicle; lower mileage; trimming a nonblank
   service centre.
3. VehicleService: editing a vehicle to use another vehicle's registration
   or VIN; ensuring another vehicle's history does not affect the selected
   vehicle.

Browser checks are useful but are not included in this coverage report.
Recalculate coverage as services and tests change.
