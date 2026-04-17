# RobotShared DTO and API Structure

This document defines the planned shared data structures and REST API contract for the robot diagnostic system.

The goal is to keep the **Client** and **Server** aligned on the same DTOs, enums, and endpoint structure through the `RobotShared` project.

---

## Robot Data Model

### Core robot data

- **RobotId** (`string`)
- **IsOnline** (`bool`)
- **LastSeenAt** (`DateTime`)

### State

- **State** (`enum`)
  - `Ready`
  - `Moving`
  - `Loading`
  - `Unloading`
  - `Charging`
  - `Paused`
  - `EmergencyStop`
  - `Error`
  - `Disconnected`

- **AssignedTask** (`string`, optional)

### Position and movement

- **Position** `{ x, y }` (`float`)
- **TargetPosition** `{ x, y }` (`float`)
- **CarryingLoad** (`bool`)

### Battery and runtime

- **BatteryLevel** (`int`, `0-100`)
- **EstimatedRemainingMinutes** (`int`)
- **LowBatteryWarning** (`bool`)

### Diagnostics

- **DiagnosticLevel** (`enum`)
  - `Normal`
  - `Warning`
  - `Error`

- **MotorStatus** (`enum`)
  - `Normal`
  - `Warning`
  - `Error`

- **SensorStatus** (`enum`)
  - `Normal`
  - `Warning`
  - `Error`

### Error and uptime

- **LastErrorCode** (`string`)
- **LastErrorMessage** (`string`)
- **UptimeSeconds** (`int`)

### Safety

- **EmergencyStopActive** (`bool`)

### Rule

- `EmergencyStopActive = true` only when `State = EmergencyStop`

---

## HTTP Methods

### GET endpoints

- `GET /api/robots`
- `GET /api/robots/{robotId}`
- `GET /api/robots/{robotId}/state`
- `GET /api/robots/{robotId}/position`
- `GET /api/robots/{robotId}/battery`
- `GET /api/robots/{robotId}/errors`
- `GET /api/robots/{robotId}/history`

### POST endpoints

- `POST /api/robots/{robotId}/move`
- `POST /api/robots/{robotId}/pause`
- `POST /api/robots/{robotId}/charge`
- `POST /api/robots/{robotId}/emergency-stop`
- `POST /api/robots/{robotId}/reset-error`

### Simulation endpoints

- `POST /api/robots/{robotId}/simulate-error`
- `POST /api/robots/{robotId}/simulate-low-battery`
- `POST /api/robots/{robotId}/simulate-disconnect`

---

## DTOs for the `RobotShared` Project

## Enums

### `RobotState`

- `Ready`
- `Moving`
- `Loading`
- `Unloading`
- `Charging`
- `Paused`
- `EmergencyStop`
- `Error`
- `Disconnected`

### `DiagnosticLevel`

- `Normal`
- `Warning`
- `Error`

### `ComponentStatus` for (MotorStatus and SensorStatus)

- `Normal`
- `Warning`
- `Error`

---

## Basic DTOs

### `PositionDto`

- `X` (`float`)
- `Y` (`float`)

### `RobotSummaryDto`

Useful for the **robot list page**.

- `RobotId` (`string`)
- `IsOnline` (`bool`)
- `State` (`RobotState`)
- `BatteryLevel` (`int`)
- `DiagnosticLevel` (`DiagnosticLevel`)

### `RobotDetailsDto`

Useful for the **robot diagnostics/details page**.

- `RobotId` (`string`)
- `IsOnline` (`bool`)
- `LastSeenAt` (`DateTime`)
- `State` (`RobotState`)
- `AssignedTask` (`string`, optional)
- `Position` (`PositionDto`)
- `TargetPosition` (`PositionDto`)
- `CarryingLoad` (`bool`)
- `BatteryLevel` (`int`)
- `EstimatedRemainingMinutes` (`int`)
- `LowBatteryWarning` (`bool`)
- `DiagnosticLevel` (`DiagnosticLevel`)
- `MotorStatus` (`ComponentStatus`)
- `SensorStatus` (`ComponentStatus`)
- `LastErrorCode` (`string`)
- `LastErrorMessage` (`string`)
- `UptimeSeconds` (`int`)
- `EmergencyStopActive` (`bool`)

---

## GET Response DTOs

### `RobotStateDto`

- `RobotId` (`string`)
- `State` (`RobotState`)
- `DiagnosticLevel` (`DiagnosticLevel`)
- `EmergencyStopActive` (`bool`)

### `RobotPositionDto`

- `RobotId` (`string`)
- `Position` (`PositionDto`)
- `TargetPosition` (`PositionDto`)
- `CarryingLoad` (`bool`)

### `RobotBatteryDto`

- `RobotId` (`string`)
- `BatteryLevel` (`int`)
- `EstimatedRemainingMinutes` (`int`)
- `LowBatteryWarning` (`bool`)
- `State` (`RobotState`)

### `RobotErrorDto`

- `RobotId` (`string`)
- `LastErrorCode` (`string`)
- `DiagnosticLevel` (`DiagnosticLevel`)
- `ErrorMessage` (`string`)

### `RobotHistoryEventDto`

- `Timestamp` (`DateTime`)
- `EventType` (`string`)
- `Message` (`string`)

### `RobotHistoryDto`

- `RobotId` (`string`)
- `Events` (`List<RobotHistoryEventDto>`)

---

## POST Request DTOs

### `MoveRobotRequestDto`

- `TargetPosition` (`PositionDto`)

### `PauseRobotRequestDto`

- `Status = Paused`

### `ChargeRobotRequestDto`

- `Status = Charging`

### `EmergencyStopRequestDto`

- `Status = EmergencyStop`

### `ResetErrorRequestDto`

- `Error -> Normal`

### `SimulateErrorRequestDto`

- `ErrorCode` (`string`)
- `ErrorMessage` (`string`)

### `SimulateLowBatteryRequestDto`

- `BatteryLevel` (`int`)

### `SimulateDisconnectRequestDto`

- `DurationSeconds` (`int`, optional)

---

## POST Response DTOs

### `RobotCommandResultDto`

- `Success` (`bool`)
- `Message` (`string`)
- `RobotId` (`string`)
- `State` (`RobotState`)
- `DiagnosticLevel` (`DiagnosticLevel`)

---

## Notes

- The `RobotShared` project should contain the shared enums and DTO classes used by both the **Client** and the **Server**.
- The **Client** should rely on these DTOs when calling the API and rendering data.
- The **Server** should rely on the same DTOs for request handling and API responses.
- This helps avoid duplicate model definitions and reduces mismatch risk between frontend and backend.
