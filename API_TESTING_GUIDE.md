# Playball API — End-to-End Testing Guide

This guide provides a step-by-step workflow to verify the **Application Flow**, **Database Consistency**, and **Business Logic** of the Playball platform.

**Prerequisites:**

* **Base URL:** `http://localhost:5262`
* **Tool:** Postman, Insomnia, or Curl.
* **Database:** Ensure database isn't empty (or let the app seed default data on startup).

---

## 🏗️ Phase 1: User Setup (Authentication)

We need 3 distinct users for a complete test.

### 1. Register Admin / System Setup

*If the app just started, a default admin is seeded: `admin@playball.com` / `Password@123`.*

* **Login as Admin**:
  * `POST /api/auth/login`
  * Body: `{ "email": "admin@playball.com", "password": "Password@123" }`
  * **Save Token as:** `{{ADMIN_TOKEN}}`

### 2. Register Venue Owner

* **Register**:
  * `POST /api/auth/register`
  * Body:
    ```json
    {
      "fullName": "John Owner",
      "email": "owner@test.com",
      "phoneNumber": "9876543210",
      "password": "Password@123"
    }
    ```
* **Login & Save Token as:** `{{OWNER_TOKEN}}`
* **Promote to Venue Owner (Admin Action)**:
  * `POST /api/users/assign-role` (with `{{ADMIN_TOKEN}}`)
  * Body: `{ "userId": 3, "newRole": "2" }` (Assuming ID 2 is the owner)

### 3. Register Player

* **Register**:
  * `POST /api/auth/register`
  * Body:
    ```json
    {
      "fullName": "Alice Player",
      "email": "player@test.com",
      "phoneNumber": "9123456780",
      "password": "Password@123"
    }
    ```
* **Login & Save Token as:** `{{PLAYER_TOKEN}}`

---

## 🏟️ Phase 2: Venue & Court Creation

### 1. Create Venue (as Owner)

* **Endpoint**: `POST /api/venues` (with `{{OWNER_TOKEN}}`)
* **Body**:
  ```json
  {
    "name": "Super Sports Arena",
    "address": "Downtown, City",
    "sportsSupported": [1, 2]
  }
  ```
* **Response**: Note `venueId` (e.g., `1`). Status will be `Pending`.

### 2. Approve Venue (as Admin)

* **Endpoint**: `POST /api/venues/approve/1` (with `{{ADMIN_TOKEN}}`)
* **Body**: `{ "approvalStatus": 2 }`

### 3. Create Court (as Owner)

* **Endpoint**: `POST /api/courts` (with `{{OWNER_TOKEN}}`)
* **Body**:
  ```json
  {
    "venueId": 1,
    "name": "Court A",
    "sportType": 1,
    "slotDurationMinutes": 60,
    "basePrice": 1000,
    "openTime": "06:00",
    "closeTime": "22:00"
  }
  ```
* **Response**: Note `courtId` (e.g., `1`).

---

## 💳 Phase 3: Wallet Funds

### 1. Add Funds to Player Wallet (as Player)

* **Endpoint**: `POST /api/wallet/add-funds` (with `{{PLAYER_TOKEN}}`)
* **Body**:
  ```json
  {
    "amount": 5000,
    "idempotencyKey": "unique-params-001"
  }
  ```

---

## 📅 Phase 4: Booking Flow (The foundation for Games)

### 1. Search Slots

* **Endpoint**: `GET /api/slots/availability/1/2026-06-01`
  *(Use a future date)*

### 2. Lock a Slot (as Player)

* **Endpoint**: `POST /api/bookings/lock-slot` (with `{{PLAYER_TOKEN}}`)
* **Body**:
  ```json
  {
    "courtId": 1,
    "slotStartTime": "2026-06-01T18:00:00Z",
    "slotEndTime": "2026-06-01T19:00:00Z"
  }
  ```
* **Response**: Note `bookingId` (e.g., `1`).

### 3. Confirm Booking (as Player)

* **Endpoint**: `POST /api/bookings/confirm` (with `{{PLAYER_TOKEN}}`)
* **Body**: `{ "bookingId": 1 }`
* **Result**: Booking status becomes `Confirmed`.

---

## ⚽ Phase 5: Game Lifecycle (Linked to Booking)

**Crucial Change**: You cannot create a game *without* a confirmed booking for that slot.

### 1. Create Game (as Player / Game Owner)

* **Endpoint**: `POST /api/games` (with `{{PLAYER_TOKEN}}`)
* **Body**:
  ```json
  {
    "title": "Evening Football Match",
    "description": "Casual 5v5",
    "venueId": 1,
    "courtId": 1,
    "startTime": "2026-06-01T18:00:00Z", // MUST Match Booking
    "endTime": "2026-06-01T19:00:00Z",   // MUST Match Booking
    "minPlayers": 2,
    "maxPlayers": 10,
    "isPublic": false // TEST PRIVATE GAME FLOW
  }
  ```
* **Response**: Returns `gameId` (e.g., `1`).

### 2. Join Game (as Another User)

* *Create another user "Bob" and get token `{{BOB_TOKEN}}`.*
* **Endpoint**: `POST /api/games/1/join` (with `{{BOB_TOKEN}}`)
* **Result**:
  * Because `isPublic` is false, Bob's status is `Pending`.
  * `CurrentPlayers` does NOT increase yet.

### 3. Approve Participant (as Game Owner / Player)

* *Bob needs approval.*
* **Endpoint**: `POST /api/games/1/approve/{bobUserId}` (with `{{PLAYER_TOKEN}}`)
  * *Note: You need Bob's UserID.*
* **Result**: Bob's status becomes `Accepted`. `CurrentPlayers` increases.

---

## 💰 Phase 6: Game Completion & Payouts

### 1. Complete Game (as Owner/Admin)

* **Endpoint**: `POST /api/games/1/complete` (with `{{PLAYER_TOKEN}}`)
* **Result**:
  * Game Status -> `Completed`.
  * **Payout Triggered**: The Venue Owner (John) receives the booking amount into their wallet.

### 2. Verify Payout (as Venue Owner)

* **Endpoint**: `GET /api/wallet` (with `{{OWNER_TOKEN}}`)
* **Result**: Balance should have increased by `1000` (Court Base Price).

---

## ⭐️ Phase 7: Ratings

### 1. Rate Venue

* **Endpoint**: `POST /api/ratings/venue/1` (with `{{PLAYER_TOKEN}}`)
* **Body**:
  ```json
  {
    "score": 5,
    "comment": "Great turf!"
  }
  ```

### 2. Rate Transaction (Peer Review)

* **Endpoint**: `POST /api/ratings/user/{bobUserId}`
* **Body**: `{ "score": 4, "comment": "Good team player" }`

---

## 🧪 Special Scenarios

### Auto-Refund on Game Cancellation

1. Create a Game with `minPlayers: 10`.
2. Only 2 people join.
3. Wait for `GameAutoCancelService` (runs every minute in background) OR manually trigger (if endpoint existed).
4. **Verify**:
   * Game Status: `Cancelled`
   * Booking Status: `Cancelled`
   * Player Wallet: Refunded `1000`.
