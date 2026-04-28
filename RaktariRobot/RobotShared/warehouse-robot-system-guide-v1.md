# Warehouse Robot Diagnostic System Guide

This document describes the planned operation of our warehouse robot diagnostic client-server application.

The goal is to keep the project simple, understandable, and easy to implement. The system is not a real robot routing simulator. It is a small diagnostic dashboard for autonomous robots moving on a simple square grid.

---

# 1. System overview

The warehouse is represented as a simple 2D grid. The robots move packages from the inside of the warehouse to the outside dropoff point.

Version 1:

```text
10 x 7 warehouse grid

. W . . . . . S . .
. . . S . . . . . .
S . . . . . R . . .
. . . . . . . . . C
. . . . S . . . . .
S . . . . . . . . .
. . . . . . . . D .

R = Robot spawn
S = Shelf
D = Dropoff
C = Charger
W = Service (Warning fix point)
```

The robots move on discrete integer coordinates:

```text
X = 0..9
Y = 0..6
(0,0) = top-left corner
```

Robots are allowed to overlap each other and move through obstacles. We do not handle collisions. This keeps the movement logic simple and avoids complex path planning.

Default robot program, which the robot executes autonomously:

```text
1. Robot chooses a random shelf.
2. Robot moves to that shelf.
3. Robot picks up a package.
4. Robot moves to the dropoff point.
5. Robot drops off the package.
6. Robot repeats the process.
```

If the robot has low battery (`BatteryLevel <= 20`), it finishes its current delivery first, then goes to the charger. If it is not carrying a package, it goes straight to the charger.

```
Note: The shelf placements on the grid are deliberate,
      even from the furthest shelf the robot can reach
      the charger in time.
```

Multiple robots can charge at the same time. Visually, they can overlap on the same grid cell.

After full charge, the robots continue their default program.

Warnings and errors can happen during operation:

- **Warning**: not severe; the robot can continue moving.
- **Error**: severe; the robot stops until the user fixes or removes it.

The user does not control the warehouse workflow. The user acts as an operator or maintenance technician:

```text
Monitor robots.
Pause/resume robots.
Send robots to charger/service.
Manually move robots if needed.
Clear warnings.
Fix errors.
Remove broken robots.
```

---

# 2. Client-server communication model

The Client and Server communicate only through HTTP.

```text
Client  --HTTP request-->   Server
Client  <--HTTP response--  Server
```

The Client always initiates the request. The Server responds with DTOs (Data Transfer Objects).

Important rule:

```text
Client = UI + button clicks + timer
Server = robot state + warehouse state + simulation logic

RobotShared project = shared DTOs and enums used by both Client and Server
```

The Client should not directly access server memory, server services, or robot objects.

---

# 3. Simulation tick concept

The simulation only advances when the Client sends a tick request.

```text
POST api/simulation/tick
```

The Server does not run the simulation by itself in the background.

The Client has a timer. When the simulation is running, the Client sends a tick request every 1000 ms.

One tick means:

```text
1. Server checks every robot.
2. Server updates robot states.
3. Server moves robots by one grid cell if needed.
4. Server decreases battery during movement.
5. Server charges robots if they are charging.
6. Server may generate random warnings/errors for the robots.
7. Server returns the updated robot DTO list to the Client.
8. Client redraws the warehouse grid and displays robot info.
```

The Start/Stop simulation buttons can be client-only buttons:

```text
Start simulation = Client starts sending tick requests
Stop simulation  = Client stops sending tick requests
```

---

# 4. Manual commands and tick behavior

Sometimes the user may want to pause a robot, send a robot to the charger before low battery, or set a manual target location.

Manual commands take effect immediately on the Server, **but movement only happens on the next tick**.

Example:

```text
User clicks: Move to charger
Client sends: POST api/robots/ROBOT-001/move-to-charger
Server immediately sets:
    TargetPosition = ChargerPosition
    State = MovingToCharger

The robot position does not change immediately.
The robot moves one cell toward the charger on the next simulation tick.
```

General rule:

```text
Commands set intent/state/target.
Ticks perform physical simulation steps.
```

---

# 5. Client buttons

## Simulation

- **Start simulation**

  - Starts the Client timer.
  - The Client begins sending ticks repeatedly to the Server at a fixed interval.
- **Stop simulation**

  - Stops the Client timer.
  - The Server state remains unchanged until another command/tick arrives.
- **Reset simulation**

  - Stops the Client timer.
  - Sends `POST api/simulation/reset`.
  - Removes all robots from the Server.
  - Resets the robot ID counter.

## Robot management

- **Refresh button**

  - Synchronizes the frontend with the backend.
  - Sends `GET api/robots`.
  - Useful for debugging and demonstration, but not strictly required because ticks already return all robot details.
- **Add robot**

  - Creates a new robot on the Server.
  - The Server generates a robot ID and default display name, for example `001` and `ROBOT-001` accordingly.
- **Rename robot**

  - Changes only the display name of the robot on the Server.
  - The technical robot ID cannot be changed for stability reasons.
- **Remove robot**

  - Deletes the selected robot from the Server.
  - Disappears from the frontend too.

## Robot operation

- **Resume robot**

  - When a new robot is spawned, it will stand still until this button is pressed.
  - Resumes a paused robot.
  - The robot continues autonomous operation from the next tick.
- **Pause robot**

  - Sets the robot state to `Paused`.
  - A paused robot is ignored by simulation ticks.
- **Move to charger**

  - Manually sends the robot to the charger, before it reaches low battery.
  - The robot starts moving on the next tick.
- **Move to service**

  - Sends the robot to the service point.
  - Useful for clearing warnings. The robot gets fixed there.
  - Should not be allowed if the robot is in `Error`, because an error means the robot cannot move.
- **Move to location**

  - Manual override.
  - The user gives an X,Y coordinate.
  - The Server validates the coordinate and sets the robot state to `ManualMoving`.
  - Once the robot arrives, it stays there until a new location is set or resume is pressed.

## Diagnostics

- **Clear warning**

  - Clears warning states.
  - Sets robot motor/sensor status to normal again.
- **Fix error in place**

  - Press this if the robot stopped due to an error.
  - Simulates a technician walking to the robot at its current position and fixing it.
  - Clears the error and lets the robot continue.
- **Simulate fault**

  - Creates a random warning or error for one of the components (motor, sensor) of the robot.
  - Used for testing/demo purposes.

## View buttons

- **Simple view**

  - Shows only basic robot information:
    - display name
    - state
    - position
    - battery
    - diagnostic level
- **Detailed view**

  - Shows all robot details:
    - ID
    - display name
    - state
    - position
    - manual override active
    - target position
    - target shelf ID
    - carrying load
    - battery level
    - low battery warning
    - diagnostic level
    - motor status
    - sensor status
    - last error message

```text
Note: All robot details are sent from Server to Client on request.
      Client filters the information to show simple or detailed view.
```

---

# 6. HTTP endpoints

## GET endpoints


| HTTP | Endpoint        |      Response DTO      | Description                                                                                                                                      |
| :----: | :---------------- | :-----------------------: | :------------------------------------------------------------------------------------------------------------------------------------------------- |
| GET | `api/warehouse` |     `WarehouseDto`     | Returns fixed warehouse data: grid size, spawn, shelves, dropoff, charger, and service location. Usually called once when the Client page loads. |
| GET | `api/robots`    | `List<RobotDetailsDto>` | Returns all details for all robots. Used for the robot list, warehouse graphics, and refresh button.                                             |

```text
Note: There is no individual robot detail request,
      because this is a small-scale demo project.
```

## Simulation endpoints


| HTTP | Endpoint               | Request DTO |         Response DTO         | Description                                                                                                                    |
| :----: | :----------------------- | :-----------: | :----------------------------: | :------------------------------------------------------------------------------------------------------------------------------- |
| POST | `api/simulation/tick`  |    none    |   `List<RobotDetailsDto>`   | Advances the simulation by one step. Moves robots, updates states, updates battery/charging, and may generate warnings/errors. |
| POST | `api/simulation/reset` |    none    | `SimulationResetResponseDto` | Resets the simulation. Clears all robots, resets the robot ID counter, and returns the empty robot list.                       |

## Robot command endpoints


|  HTTP  | Endpoint                                |        Request DTO        |      Response DTO      | Description                                                                      |
| :------: | :---------------------------------------- | :--------------------------: | :-----------------------: | :--------------------------------------------------------------------------------- |
|  POST  | `api/robots`                            |  `CreateRobotRequestDto`  | `RobotCommandResultDto` | Creates a new robot. The Server generates the`RobotId`.                          |
|  POST  | `api/robots/{robotId}/rename`           |  `RenameRobotRequestDto`  | `RobotCommandResultDto` | Changes the robot's display name. The`RobotId` does not change.                  |
|  POST  | `api/robots/{robotId}/resume`           |            none            | `RobotCommandResultDto` | Resumes a paused robot. The robot continues on the next tick.                    |
|  POST  | `api/robots/{robotId}/pause`            |            none            | `RobotCommandResultDto` | Pauses the robot. Simulation ticks ignore paused robots.                         |
|  POST  | `api/robots/{robotId}/move-to-charger`  |            none            | `RobotCommandResultDto` | Sets the robot target to the charging station. Movement starts on the next tick. |
|  POST  | `api/robots/{robotId}/move-to-service`  |            none            | `RobotCommandResultDto` | Sets the robot target to the service station. Used mainly for clearing warnings. |
|  POST  | `api/robots/{robotId}/move-to-location` | `MoveToLocationRequestDto` | `RobotCommandResultDto` | Manual override. Sets the robot target to a specific X,Y coordinate.             |
|  POST  | `api/robots/{robotId}/clear-warning`    |            none            | `RobotCommandResultDto` | Clears warning statuses if the robot has warnings but no severe error.           |
|  POST  | `api/robots/{robotId}/fix-error`        |            none            | `RobotCommandResultDto` | Clears an error in place and returns the robot to a usable state.                |
|  POST  | `api/robots/{robotId}/simulate-fault`   |            none            | `RobotCommandResultDto` | Creates a random warning or error on the robot for testing/demo.                 |
| DELETE | `api/robots/{robotId}`                  |            none            | `RobotCommandResultDto` | Removes one robot from the Server.                                               |

---

# 7. RobotShared enums

These enums should be placed in the `RobotShared` project.

## RobotState

The robot is always in one discrete state.

- `Idle`
- `MovingToShelf`
- `MovingToDropoff`
- `MovingToCharger`
- `MovingToService`
- `ManualMoving`
- `Charging`
- `Paused`
- `Error`

## DiagnosticLevel

Represents the overall diagnostic status of the robot.

- `Normal`
- `Warning`
- `CriticalWarning`
- `Error`

If the robot's motor and sensor both show warning, display `CriticalWarning`.

## ComponentStatus

Used for the robot's motor and sensor status.

- `Normal`
- `Warning`
- `Error`

---

# 8. RobotShared DTOs

These DTOs should be placed in the `RobotShared` project.

DTOs are not the internal robot objects. DTOs are communication objects that travel between Client and Server as JSON.

## PositionDto

```csharp
namespace RobotShared;

public class PositionDto
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

## ShelfDto

```csharp
namespace RobotShared;

public class ShelfDto
{
    public string ShelfId { get; set; } = "";
    public PositionDto Position { get; set; } = new();
}
```

## WarehouseDto

```csharp
namespace RobotShared;

public class WarehouseDto
{
    public int Width { get; set; }
    public int Height { get; set; }

    public PositionDto SpawnPosition { get; set; } = new();
    public PositionDto DropoffPosition { get; set; } = new();
    public PositionDto ChargerPosition { get; set; } = new();
    public PositionDto ServicePosition { get; set; } = new();

    public List<ShelfDto> Shelves { get; set; } = new();
}
```

## RobotDetailsDto

```csharp
namespace RobotShared;

public class RobotDetailsDto
{
    public int RobotId { get; set; }
    public string DisplayName { get; set; } = "";

    public RobotState State { get; set; }

    public PositionDto Position { get; set; } = new();
    public PositionDto? TargetPosition { get; set; }

    public string? TargetShelfId { get; set; }

    public bool CarryingLoad { get; set; }
    public int BatteryLevel { get; set; }
    public bool LowBatteryWarning { get; set; }

    public DiagnosticLevel DiagnosticLevel { get; set; }
    public ComponentStatus MotorStatus { get; set; }
    public ComponentStatus SensorStatus { get; set; }
 
    public string? LastErrorMessage { get; set; }
}
```

## CreateRobotRequestDto

```csharp
namespace RobotShared;

public class CreateRobotRequestDto
{
    public string DisplayName { get; set; } = "";
}
```

The Server generates the `RobotId` automatically.

Example:

```text
ROBOT-001
ROBOT-002
ROBOT-003
```

## RenameRobotRequestDto

```csharp
namespace RobotShared;

public class RenameRobotRequestDto
{
    public string NewDisplayName { get; set; } = "";
}
```

## MoveToLocationRequestDto

```csharp
namespace RobotShared;

public class MoveToLocationRequestDto
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

## RobotCommandResultDto

Use this for command endpoints such as add, rename, pause, resume, remove, clear warning, fix error, etc.

```csharp
namespace RobotShared;

public class RobotCommandResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? RobotId { get; set; }
}
```

## SimulationResetResponseDto

Use this for `POST api/simulation/reset`.

```csharp
namespace RobotShared;

public class SimulationResetResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public WarehouseDto Warehouse { get; set; } = new();
    public List<RobotDetailsDto> Robots { get; set; } = new();
}
```

---

# 9. Internal Server classes

The DTO is not the real robot object. The real robot object should be inside the `RobotServer` project.

Suggested structure:

```text
RobotShared
    Enums.cs
    Dtos.cs

RobotServer
    Models/Robot.cs
    Services/RobotSimulationService.cs
    Controllers/RobotController.cs

RobotClient
    UI pages/components
    HTTP API client service
```

---

# 10. Key project rules

```text
RobotId is generated by the Server and never changes.
DisplayName is editable by the user.

Client sends HTTP requests.
Server changes robot/warehouse state.
Server returns DTOs.
Client redraws the UI from DTOs.

Robot movement only happens during simulation tick.
Manual commands only set target/state immediately.

Warnings allow movement.
Errors stop movement.
```
