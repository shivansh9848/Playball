# Playball API — End-to-End Testing Guide (Milestones 1-5)

This guide provides a sequential workflow to test the entire application. Follow the phases in order to verify all milestones.

> **Base URL:** `http://localhost:5262`  
> **Swagger UI:** `http://localhost:5262` (opens automatically)  
> **Auth:** Most endpoints require a Bearer Token. Copy the `token` from login response.

---

## 🏗️ Phase 1: User & Admin Setup (Milestone 1)

We need three actors:
1.  **Admin:** To approve venues and manage users.
2.  **Venue Owner:** To create venues and courts.
3.  **Player:** To book slots and join games.

### 1. Register Users
Run this **3 times** with different emails/passwords to create your test accounts.

**Request:** `POST /api/auth/register`
```json
{
  "fullName": "Admin User",  // Change to "Venue Owner" and "Player One" for subsequent calls
  "email": "admin@test.com", // Change to "owner@test.com" and "player@test.com"
  "phoneNumber": "9876543210",
  "password": "Password@123"
}
```
> **Note IDs:** Assume ID 1 = Admin, ID 2 = Owner, ID 3 = Player.

### 2. Promote Admin (Manual Step)
The first user is just a standard "User". Promote them to Admin directly in SQL or using a pre-seeded admin if available.
```sql
-- Run in Database
UPDATE "Users" SET "Role" = 0 WHERE "Email" = 'admin@test.com';
-- Role Enum: 0=Admin, 1=VenueOwner, 2=GameOwner, 3=User
```

### 3. Login as Admin
**Request:** `POST /api/auth/login`
```json
{ "email": "admin@test.com", "password": "Password@123" }
```
> **Action:** Copy the token. We'll call this **$ADMIN_TOKEN**.

### 4. Assign Roles (Using Admin Token)
Promote User 2 to Venue Owner.

**Request:** `POST /api/users/assign-role`
**Header:** `Authorization: Bearer $ADMIN_TOKEN`
```json
{
  "userId": 2,
  "newRole": "VenueOwner"
}
```

---

## 🏟️ Phase 2: Venue Management (Milestone 2)

### 1. Login as Venue Owner
**Request:** `POST /api/auth/login`
```json
{ "email": "owner@test.com", "password": "Password@123" }
```
> **Action:** Copy token. This is **$OWNER_TOKEN**.

### 2. Create a Venue
**Request:** `POST /api/venues`
**Header:** `Authorization: Bearer $OWNER_TOKEN`
```json
{
  "name": "Grand Sports Arena",
  "address": "123 MG Road, Bangalore",
  "sportsSupported": [1, 2] // 1=Football, 2=Basketball
}
```
> **Note:** Venue ID is returned (e.g., `1`). Status is `Pending`.

### 3. Approve Venue (Admin)
Switch back to Admin to approve the venue.

**Request:** `POST /api/venues/approve/1`
**Header:** `Authorization: Bearer $ADMIN_TOKEN`
```json
{
  "approvalStatus": 1, // 1=Approved
  "rejectionReason": null
}
```

### 4. Create a Court (Venue Owner)
Using **$OWNER_TOKEN**.

**Request:** `POST /api/courts`
**Header:** `Authorization: Bearer $OWNER_TOKEN`
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
> **Note:** Court ID is returned (e.g., `1`).

---

## 💳 Phase 3: Wallet & Discounts (Milestone 3 & 4)

### 1. Create a Discount (Venue Owner)
Create a discount to test pricing logic.

**Request:** `POST /api/discounts`
**Header:** `Authorization: Bearer $OWNER_TOKEN`
```json
{
  "scope": 0, // Venue-wide
  "venueId": 1,
  "percentOff": 20,
  "validFrom": "2024-01-01T00:00:00Z",
  "validTo": "2030-12-31T23:59:59Z"
}
```

### 2. Login as Player
**Request:** `POST /api/auth/login`
```json
{ "email": "player@test.com", "password": "Password@123" }
```
> **Action:** Copy token. This is **$PLAYER_TOKEN**.

### 3. Add Funds to Wallet
**Request:** `POST /api/wallet/add-funds`
**Header:** `Authorization: Bearer $PLAYER_TOKEN`
```json
{
  "amount": 5000,
  "idempotencyKey": "txn-001"
}
```

### 4. Check Slot Availability & Pricing
Verify slots appear and discount is applied.

**Request:** `GET /api/slots/details/1/2026-05-20T10:00:00/2026-05-20T11:00:00`
> **Check:** Response should show `basePrice: 1000` and `finalPrice: 800` (20% off).

---

## 📅 Phase 4: Booking Flow (Milestone 3)

### 1. Lock a Slot
**Request:** `POST /api/bookings/lock-slot`
**Header:** `Authorization: Bearer $PLAYER_TOKEN`
```json
{
  "courtId": 1,
  "slotStartTime": "2026-05-20T10:00:00Z",
  "slotEndTime": "2026-05-20T11:00:00Z"
}
```
> **Note:** Returns `bookingId` (e.g., `1`).

### 2. Confirm Booking
**Request:** `POST /api/bookings/confirm`
**Header:** `Authorization: Bearer $PLAYER_TOKEN`
```json
{ "bookingId": 1 }
```
> **Verify:** Response status `Confirmed`. Wallet balance deducted.

### 3. View My Bookings
**Request:** `GET /api/bookings/my`
**Header:** `Authorization: Bearer $PLAYER_TOKEN`

---

## ⚽ Phase 5: Games & Social (Milestone 4 & 5)

### 1. Create a Game (Player)
The player creates a public game on their booked slot (or a new slot, but let's use a new one for simplicity).

**Request:** `POST /api/games`
**Header:** `Authorization: Bearer $PLAYER_TOKEN`
```json
{
  "title": "Weekend Football",
  "venueId": 1,
  "courtId": 1,
  "startTime": "2026-05-21T18:00:00Z",
  "endTime": "2026-05-21T19:00:00Z",
  "minPlayers": 2,
  "maxPlayers": 10,
  "isPublic": true
}
```
> **Note:** Game ID returned (e.g., `1`). Player is auto-joined.

### 2. Join a Game (Another User)
Register/Login as a 4th user ("Player Two") and join.

**Request:** `POST /api/games/1/join`
**Header:** `Authorization: Bearer $PLAYER_TWO_TOKEN`

### 3. Rate the Venue (After Game)
**Request:** `POST /api/ratings/venue/1`
**Header:** `Authorization: Bearer $PLAYER_TOKEN`
```json
{
  "score": 5,
  "comment": "Amazing turf!"
}
```

### 4. Get Player Profile
**Request:** `GET /api/players/3/profile`
> **Verify:** Profile shows updated stats (games played, bookings).

---

## ✅ Completed!
You have successfully tested the entire Playball workflow covering all milestones.
