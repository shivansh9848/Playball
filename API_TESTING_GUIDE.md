# Playball API Testing Guide

This guide provides tested and verified JSON examples for the core workflows of the Playball application.

**Base URL**: `http://localhost:5000` (or `https://localhost:5001`)

---

## 1. User Registration & Auth

### Register User 1 (Game Owner)
**POST** `/api/Auth/register`
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "1234567890",
  "password": "Password@123",
  "confirmPassword": "Password@123"
}
```

### Register User 2 (Player A)
**POST** `/api/Auth/register`
```json
{
  "fullName": "Alice Smith",
  "email": "alice@example.com",
  "phoneNumber": "1234567891",
  "password": "Password@123",
  "confirmPassword": "Password@123"
}
```

### Login
**POST** `/api/Auth/login`
```json
{
  "email": "john@example.com",
  "password": "Password@123"
}
```
**Response**: Save the `token` from the response. You must use it in the Authorization header (`Bearer <token>`) for all subsequent requests.

---

## 2. Venue & Court Setup (Admin/Owner)

### Create Venue
**POST** `/api/Venues`
```json
{
  "name": "Downtown Sports Center",
  "address": "123 Main St, Cityville",
  "sportsSupported": [0, 1] 
}
```
*Note: 0 = Cricket, 1 = Football, 2 = Badminton, 3 = Tennis*

### Create Court
**POST** `/api/Courts`
```json
{
  "venueId": 1,
  "name": "Central Court 1",
  "sportType": 0,
  "slotDurationMinutes": 60,
  "basePrice": 500.00,
  "openTime": "06:00",
  "closeTime": "22:00"
}
```

---

## 3. Game Management (Public/Private)

### Create Public Game
**POST** `/api/Games`
```json
{
  "title": "Weekend Cricket Bash",
  "description": "Friendly match for beginners",
  "venueId": 1,
  "courtId": 1,
  "startTime": "2026-03-10T09:00:00Z",
  "endTime": "2026-03-10T11:00:00Z",
  "minPlayers": 2,
  "maxPlayers": 12,
  "isPublic": true
}
```

### Create Private Game (Invite Only)
**POST** `/api/Games`
```json
{
  "title": "Secret Pro League",
  "description": "Invite only game",
  "venueId": 1,
  "courtId": 1,
  "startTime": "2026-03-11T18:00:00Z",
  "endTime": "2026-03-11T20:00:00Z",
  "minPlayers": 4,
  "maxPlayers": 10,
  "isPublic": false
}
```

---


---

## 4. Wallet Management

### Add Funds (Required before Booking)
**POST** `/api/Wallet/add-funds`
```json
{
  "amount": 1000.00,
  "currency": "INR",
  "paymentMethod": "UPI"
}
```

---

## 5. Game Actions (Join/Leave/Approve)

### Join a Game
**POST** `/api/Games/{gameId}/join`
*   No body required.
*   **Effect**: Adds you as a participant (Accepted for Public, Pending for Private).

### Approve Participant (Private Games Only)
**POST** `/api/Games/{gameId}/approve/{participantId}`
*   Only the Game Owner can do this.

---

## 6. Bookings & Payments

### Lock Slot (Check Availability)
**POST** `/api/Bookings/lock-slot`
```json
{
  "courtId": 1,
  "date": "2026-03-10",
  "startTime": "09:00",
  "endTime": "10:00"
}
```
**Response**: Returns a `lockId` and `totalPrice`.

### Confirm Booking
**POST** `/api/Bookings/confirm`
```json
{
  "lockId": "guid-from-previous-step",
  "paymentMethod": "CreditCard"
}
```

---

## 7. Waitlist Workflow (Detailed Walkthrough)

**Scenario**: A game has `MaxPlayers = 2`. Users try to join until it is full, then pile up in the waitlist. A spot opens, and someone is invited.

### Step 1: Create a Small Game
**User**: User 1 (Owner)
**POST** `/api/Games`
```json
{
  "title": "Small 2-Player Match",
  "description": "Testing waitlist",
  "venueId": 1,
  "courtId": 1,
  "startTime": "2026-04-01T10:00:00Z",
  "endTime": "2026-04-01T11:00:00Z",
  "minPlayers": 2,
  "maxPlayers": 2,
  "isPublic": true
}
```
*   **Result**: Game created. ID = 100 (example).
*   **Status**: `CurrentPlayers = 1/2` (Owner auto-joined).

### Step 2: User 2 Joins (Game Becomes Full)
**User**: User 2
**POST** `/api/Games/100/join`
*   **Result**: "You have successfully joined".
*   **Status**: `CurrentPlayers = 2/2` (FULL).

### Step 3: User 3 Tries to Join (Fails -> Joins Waitlist)
**User**: User 3
**POST** `/api/Games/100/join`
*   **Result**: **Error 400**: "Game is full".
**POST** `/api/games/100/Waitlist`
*   **Result**: "You have joined the waitlist at position #1".

### Step 4: User 4 Joins Waitlist
**User**: User 4
**POST** `/api/games/100/Waitlist`
*   **Result**: "You have joined the waitlist at position #2".

### Step 5: Verify Waitlist Order
**User**: Any
**GET** `/api/games/100/Waitlist`
*   **Result**:
    ```json
    [
      { "userId": 3, "position": 1, "userName": "User 3" },
      { "userId": 4, "position": 2, "userName": "User 4" }
    ]
    ```

### Step 6: User 2 Leaves the Game
**User**: User 2
**POST** `/api/Games/100/leave`
*   **Result**: Game `CurrentPlayers = 1/2`. Spot Opened!

### Step 7: Owner Invites User 3 (From Waitlist)
**User**: User 1 (Owner)
**POST** `/api/games/100/Waitlist/invite/3`
*   **Result**: "Player 3 invited and added to game".
*   **Effect**:
    1.  User 3 is **Moved** from Waitlist -> Game Participant.
    2.  Waitlist Re-shuffles: User 4 becomes Position #1.
    3.  Game is Full again `2/2`.

### Step 8: Verify Final State
**GET** `/api/Games/100` -> Shows User 1 and User 3 as participants.
**GET** `/api/games/100/Waitlist` -> Shows only User 4 (Position 1).
