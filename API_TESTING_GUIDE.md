# Playball API — End-to-End Testing Guide

> **Base URL:** `http://localhost:5262`  
> **Swagger UI:** `http://localhost:5262` (opens automatically)  
> **Auth:** After login, copy the `token` value and use it as `Bearer <token>` in the Authorization header.

---

## 🔑 Legend

| Badge | Meaning |
|---|---|
| 🟢 Public | No token needed |
| 🔵 Any Auth | Any logged-in user |
| 🟡 User | Role = `User` |
| 🟠 VenueOwner | Role = `VenueOwner` |
| 🔴 Admin | Role = `Admin` |

---

## Step-by-Step Testing Flow

Follow this order — each step produces IDs/tokens needed for the next.

---

## 1. Auth Endpoints

### 1.1 🟢 Register a User
```bash
curl -X POST http://localhost:5262/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "email": "user@test.com",
    "phoneNumber": "9876543210",
    "password": "Password@123"
  }'
```
**Expected:** `200 OK` with token. Save the `token`.

---

### 1.2 🟢 Login
```bash
curl -X POST http://localhost:5262/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@test.com",
    "password": "Password@123"
  }'
```
**Expected:** `200 OK` with `token`. **Copy this token — you'll use it everywhere below.**

> Set a variable for convenience:
> ```bash
> TOKEN="eyJhbGci..."   # paste your token here
> ```

---

### 1.3 🔵 Get My Profile
```bash
curl -X GET http://localhost:5262/api/auth/profile \
  -H "Authorization: Bearer $TOKEN"
```
**Expected:** `200 OK` with your user details.

---

### 1.4 🔵 Get Any User's Profile by ID
```bash
curl -X GET http://localhost:5262/api/auth/users/1/profile \
  -H "Authorization: Bearer $TOKEN"
```
**Expected:** `200 OK` with user profile. Change `1` to any valid userId.

---

## 2. Wallet Endpoints

### 2.1 🔵 Get Wallet Balance
```bash
curl -X GET http://localhost:5262/api/wallet/balance \
  -H "Authorization: Bearer $TOKEN"
```
**Expected:** `200 OK` with balance (starts at 0).

---

### 2.2 🔵 Add Funds (Mock Payment)
```bash
curl -X POST http://localhost:5262/api/wallet/add-funds \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 5000,
    "idempotencyKey": "test-key-001"
  }'
```
**Expected:** `200 OK` with updated balance. Add at least ₹5000 for booking tests.

---

### 2.3 🔵 Get Transaction History
```bash
curl -X GET "http://localhost:5262/api/wallet/transactions?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```
**Expected:** `200 OK` with list of transactions.

---

## 3. Admin Setup — Assign Roles

> ⚠️ **First**, register a user and then use an **Admin token** to promote them.  
> Your first registered user has role `User`. To test VenueOwner/Admin flows, you need an Admin account.  
> **Tip:** Directly update the database: `UPDATE "Users" SET "Role" = 0 WHERE "Email" = 'admin@test.com';` (0 = Admin)

### 3.1 🔴 Assign Role (Admin only)
```bash
curl -X POST http://localhost:5262/api/users/assign-role \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 2,
    "newRole": "VenueOwner"
  }'
```
**Valid roles:** `Admin`, `VenueOwner`, `GameOwner`, `User`  
**Expected:** `200 OK` with updated user profile.

---

### 3.2 🔴 Get All Users (Admin only)
```bash
curl -X GET http://localhost:5262/api/users \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
**Expected:** `200 OK` with list of all users.

---

### 3.3 🔴 Deactivate a User (Admin only)
```bash
curl -X POST http://localhost:5262/api/users/3/deactivate \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
**Expected:** `200 OK` with `true`.

---

### 3.4 🔴 Activate a User (Admin only)
```bash
curl -X POST http://localhost:5262/api/users/3/activate \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
**Expected:** `200 OK` with `true`.

---

## 4. Venue Endpoints

> Use a **VenueOwner** token for these. Login as a VenueOwner user.

### 4.1 🟠 Create a Venue
```bash
curl -X POST http://localhost:5262/api/venues \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Grand Sports Arena",
    "address": "123 MG Road, Bangalore",
    "sportsSupported": [1, 2, 3]
  }'
```
**Expected:** `200 OK` with venue details. Save the `venueId`.

---

### 4.2 🟢 Get All Approved Venues
```bash
curl -X GET http://localhost:5262/api/venues
```
**Expected:** `200 OK` with list. (Empty until Admin approves your venue.)

---

### 4.3 🟢 Get Venues with Filters
```bash
curl -X GET "http://localhost:5262/api/venues?location=Bangalore&sportsSupported=1"
```
**Expected:** `200 OK` with filtered venues.

---

### 4.4 🟠 Get My Venues
```bash
curl -X GET http://localhost:5262/api/venues/my \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN"
```
**Expected:** `200 OK` with your venues (status = Pending until approved).

---

### 4.5 🔴 Get Pending Venues (Admin only)
```bash
curl -X GET http://localhost:5262/api/venues/pending \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
**Expected:** `200 OK` with list of pending venues.

---

### 4.6 🔴 Approve a Venue (Admin only)
```bash
curl -X POST http://localhost:5262/api/venues/approve/1 \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "approvalStatus": 1,
    "rejectionReason": null
  }'
```
**`approvalStatus`:** `1` = Approved, `2` = Rejected  
**Expected:** `204 No Content`.

---

## 5. Court Endpoints

> Use **VenueOwner** token. Venue must be approved first.

### 5.1 🟠 Create a Court
```bash
curl -X POST http://localhost:5262/api/courts \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "venueId": 1,
    "name": "Court A",
    "sportType": 1,
    "slotDurationMinutes": 60,
    "basePrice": 500,
    "openTime": "06:00",
    "closeTime": "22:00"
  }'
```
**Expected:** `200 OK` with court details. Save the `courtId`.

---

### 5.2 🟢 Get Courts by Venue
```bash
curl -X GET http://localhost:5262/api/courts/venue/1
```
**Expected:** `200 OK` with list of courts for venue ID 1.

---

### 5.3 🟠 Update a Court
```bash
curl -X PUT http://localhost:5262/api/courts/1 \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Court A - Premium",
    "basePrice": 600,
    "isActive": true
  }'
```
**Expected:** `200 OK` with updated court.

---

### 5.4 🟠 Delete a Court
```bash
curl -X DELETE http://localhost:5262/api/courts/1 \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN"
```
**Expected:** `204 No Content`. (Fails if active bookings exist.)

---

## 6. Slots Endpoints

### 6.1 🟢 Get Available Slots for a Court
```bash
curl -X GET "http://localhost:5262/api/slots/available/1/2026-02-20"
```
Replace `1` with your `courtId` and `2026-02-20` with a future date.  
**Expected:** `200 OK` with list of available time slots and prices.

---

### 6.2 🟢 Get Slot Details (with Pricing Breakdown)
```bash
curl -X GET "http://localhost:5262/api/slots/details/1/2026-02-20T09:00:00/2026-02-20T10:00:00"
```
**Expected:** `200 OK` with pricing breakdown (base price, demand multiplier, discounts, final price).

---

## 7. Discount Endpoints

### 7.1 🟠 Create a Discount
```bash
curl -X POST http://localhost:5262/api/discounts \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "scope": 0,
    "venueId": 1,
    "courtId": null,
    "percentOff": 20,
    "validFrom": "2026-02-18T00:00:00Z",
    "validTo": "2026-03-31T23:59:59Z"
  }'
```
**`scope`:** `0` = Venue-wide, `1` = Court-specific  
**Expected:** `200 OK` with discount details.

---

### 7.2 🟢 Get Discounts by Venue
```bash
curl -X GET http://localhost:5262/api/discounts/venue/1
```
**Expected:** `200 OK` with active discounts for venue 1.

---

### 7.3 🟢 Get Discounts by Court
```bash
curl -X GET http://localhost:5262/api/discounts/court/1
```
**Expected:** `200 OK` with active discounts for court 1.

---

### 7.4 🟠 Get My Discounts
```bash
curl -X GET http://localhost:5262/api/discounts \
  -H "Authorization: Bearer $VENUE_OWNER_TOKEN"
```
**Expected:** `200 OK` with all discounts you created.

---

## 8. Booking Endpoints

> Use a **User** role token. Court must exist and be active.

### 8.1 🟡 Lock a Slot (Step 1 of Booking)
```bash
curl -X POST http://localhost:5262/api/bookings/lock-slot \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "courtId": 1,
    "slotStartTime": "2026-02-20T09:00:00Z",
    "slotEndTime": "2026-02-20T10:00:00Z"
  }'
```
**Expected:** `200 OK` with booking in `Locked` status and `priceLocked`. Save `bookingId`.  
> ⏱️ Lock expires in ~10 minutes — confirm quickly!

---

### 8.2 🟡 Confirm Booking (Step 2 of Booking)
```bash
curl -X POST http://localhost:5262/api/bookings/confirm \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "bookingId": 1
  }'
```
**Expected:** `200 OK` with booking in `Confirmed` status. Wallet balance deducted.

---

### 8.3 🔵 Get My Bookings
```bash
curl -X GET http://localhost:5262/api/bookings/my \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `200 OK` with list of your bookings.

---

### 8.4 🔵 Get Booking by ID
```bash
curl -X GET http://localhost:5262/api/bookings/1 \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `200 OK` with booking details.

---

### 8.5 🔵 Cancel a Booking
```bash
curl -X POST http://localhost:5262/api/bookings/cancel/1 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '"Changed my plans"'
```
**Expected:** `204 No Content`. Refund processed to wallet based on cancellation timing.

---

## 9. Games Endpoints

### 9.1 🟡 Create a Game
```bash
curl -X POST http://localhost:5262/api/games \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Friday Football",
    "description": "Casual 5-a-side game",
    "venueId": 1,
    "courtId": 1,
    "startTime": "2026-02-21T16:00:00Z",
    "endTime": "2026-02-21T17:00:00Z",
    "minPlayers": 4,
    "maxPlayers": 10,
    "isPublic": true
  }'
```
**Expected:** `200 OK` with game details. Save `gameId`.

---

### 9.2 🟢 Get Public Games
```bash
curl -X GET http://localhost:5262/api/games/public
```
**Expected:** `200 OK` with list of public games.

---

### 9.3 🟢 Get Game by ID
```bash
curl -X GET http://localhost:5262/api/games/1
```
**Expected:** `200 OK` with game details and participant list.

---

### 9.4 🔵 Get My Games
```bash
curl -X GET http://localhost:5262/api/games/my \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `200 OK` with games you created or joined.

---

### 9.5 🟡 Join a Game
```bash
curl -X POST http://localhost:5262/api/games/1/join \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `204 No Content`. (Fails if game is full.)

---

### 9.6 🟡 Leave a Game
```bash
curl -X POST http://localhost:5262/api/games/1/leave \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `204 No Content`.

---

## 10. Waitlist Endpoints

> Used when a game is full.

### 10.1 🟡 Join Waitlist
```bash
curl -X POST http://localhost:5262/api/games/1/waitlist \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `200 OK` with your waitlist position.

---

### 10.2 🔵 Get Waitlist for a Game
```bash
curl -X GET http://localhost:5262/api/games/1/waitlist \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `200 OK` with ordered waitlist (sorted by player rating).

---

### 10.3 🟡 Invite User from Waitlist (Game Owner)
```bash
curl -X POST http://localhost:5262/api/games/1/waitlist/invite/3 \
  -H "Authorization: Bearer $USER_TOKEN"
```
Replace `3` with the userId to invite.  
**Expected:** `204 No Content`.

---

### 10.4 🔵 Leave Waitlist
```bash
curl -X DELETE http://localhost:5262/api/games/1/waitlist \
  -H "Authorization: Bearer $USER_TOKEN"
```
**Expected:** `204 No Content`.

---

## 11. Ratings Endpoints

> Rate after a game has been played.

### 11.1 🟡 Rate a Venue
```bash
curl -X POST http://localhost:5262/api/ratings/venue/1 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "score": 5,
    "comment": "Excellent facilities!"
  }'
```
**Expected:** `200 OK` with rating details.

---

### 11.2 🟡 Rate a Court
```bash
curl -X POST http://localhost:5262/api/ratings/court/1 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "score": 4,
    "comment": "Good surface, well maintained."
  }'
```
**Expected:** `200 OK` with rating details.

---

### 11.3 🟡 Rate a Player
```bash
curl -X POST http://localhost:5262/api/ratings/player/2 \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "score": 5,
    "comment": "Great teammate!"
  }'
```
**Expected:** `200 OK` with rating details.

---

### 11.4 🟢 Get Venue Ratings
```bash
curl -X GET http://localhost:5262/api/ratings/venue/1
```
**Expected:** `200 OK` with all ratings for venue 1.

---

### 11.5 🟢 Get Court Ratings
```bash
curl -X GET http://localhost:5262/api/ratings/court/1
```
**Expected:** `200 OK` with all ratings for court 1.

---

### 11.6 🟢 Get Player Ratings
```bash
curl -X GET http://localhost:5262/api/ratings/player/1
```
**Expected:** `200 OK` with all ratings for player (user) 1.

---

## 12. Players Endpoint

### 12.1 🟢 Get Player Profile
```bash
curl -X GET http://localhost:5262/api/players/1/profile
```
**Expected:** `200 OK` with player stats, aggregated rating, games played, and recent reviews.

---

## 📋 Complete Endpoint Summary

| # | Method | Endpoint | Auth | Role |
|---|---|---|---|---|
| 1 | POST | `/api/auth/register` | 🟢 Public | — |
| 2 | POST | `/api/auth/login` | 🟢 Public | — |
| 3 | GET | `/api/auth/profile` | 🔵 Any | — |
| 4 | GET | `/api/auth/users/{id}/profile` | 🔵 Any | — |
| 5 | POST | `/api/wallet/add-funds` | 🔵 Any | — |
| 6 | GET | `/api/wallet/balance` | 🔵 Any | — |
| 7 | GET | `/api/wallet/transactions` | 🔵 Any | — |
| 8 | POST | `/api/users/assign-role` | 🔴 Admin | Admin |
| 9 | GET | `/api/users` | 🔴 Admin | Admin |
| 10 | POST | `/api/users/{id}/deactivate` | 🔴 Admin | Admin |
| 11 | POST | `/api/users/{id}/activate` | 🔴 Admin | Admin |
| 12 | POST | `/api/venues` | 🟠 VenueOwner | VenueOwner |
| 13 | GET | `/api/venues` | 🟢 Public | — |
| 14 | GET | `/api/venues/my` | 🟠 VenueOwner | VenueOwner |
| 15 | GET | `/api/venues/pending` | 🔴 Admin | Admin |
| 16 | POST | `/api/venues/approve/{id}` | 🔴 Admin | Admin |
| 17 | POST | `/api/courts` | 🟠 VenueOwner | VenueOwner |
| 18 | GET | `/api/courts/venue/{id}` | 🟢 Public | — |
| 19 | PUT | `/api/courts/{id}` | 🟠 VenueOwner | VenueOwner |
| 20 | DELETE | `/api/courts/{id}` | 🟠 VenueOwner | VenueOwner |
| 21 | GET | `/api/slots/available/{courtId}/{date}` | 🟢 Public | — |
| 22 | GET | `/api/slots/details/{courtId}/{start}/{end}` | 🟢 Public | — |
| 23 | POST | `/api/discounts` | 🟠 VenueOwner | VenueOwner |
| 24 | GET | `/api/discounts` | 🟠 VenueOwner | VenueOwner/Admin |
| 25 | GET | `/api/discounts/venue/{id}` | 🟢 Public | — |
| 26 | GET | `/api/discounts/court/{id}` | 🟢 Public | — |
| 27 | POST | `/api/bookings/lock-slot` | 🟡 User | User |
| 28 | POST | `/api/bookings/confirm` | 🟡 User | User |
| 29 | GET | `/api/bookings/my` | 🔵 Any | — |
| 30 | GET | `/api/bookings/{id}` | 🔵 Any | — |
| 31 | POST | `/api/bookings/cancel/{id}` | 🔵 Any | — |
| 32 | POST | `/api/games` | 🟡 User/GameOwner | User/GameOwner |
| 33 | GET | `/api/games/public` | 🟢 Public | — |
| 34 | GET | `/api/games/my` | 🔵 Any | — |
| 35 | GET | `/api/games/{id}` | 🟢 Public | — |
| 36 | POST | `/api/games/{id}/join` | 🟡 User/GameOwner | User/GameOwner |
| 37 | POST | `/api/games/{id}/leave` | 🟡 User/GameOwner | User/GameOwner |
| 38 | POST | `/api/games/{id}/waitlist` | 🟡 User/GameOwner | User/GameOwner |
| 39 | GET | `/api/games/{id}/waitlist` | 🔵 Any | — |
| 40 | POST | `/api/games/{id}/waitlist/invite/{userId}` | 🟡 User/GameOwner | User/GameOwner |
| 41 | DELETE | `/api/games/{id}/waitlist` | 🔵 Any | — |
| 42 | POST | `/api/ratings/venue/{id}` | 🟡 User/GameOwner/VenueOwner | — |
| 43 | POST | `/api/ratings/court/{id}` | 🟡 User/GameOwner/VenueOwner | — |
| 44 | POST | `/api/ratings/player/{id}` | 🟡 User/GameOwner | — |
| 45 | GET | `/api/ratings/venue/{id}` | 🟢 Public | — |
| 46 | GET | `/api/ratings/court/{id}` | 🟢 Public | — |
| 47 | GET | `/api/ratings/player/{id}` | 🟢 Public | — |
| 48 | GET | `/api/players/{id}/profile` | 🟢 Public | — |

---

## 🚀 Quick Smoke Test (Minimal Path)

Run these 10 calls in order to verify the core booking flow works end-to-end:

1. `POST /api/auth/register` → get token
2. `GET /api/auth/profile` → verify auth works
3. `GET /api/wallet/balance` → check balance = 0
4. `POST /api/wallet/add-funds` → add ₹5000
5. `GET /api/venues` → browse venues (need an approved one)
6. `GET /api/slots/available/{courtId}/2026-02-20` → see available slots
7. `POST /api/bookings/lock-slot` → lock a slot
8. `POST /api/bookings/confirm` → confirm booking
9. `GET /api/bookings/my` → verify booking appears
10. `GET /api/wallet/balance` → verify amount deducted

---

## ⚠️ Common Errors

| Error | Cause | Fix |
|---|---|---|
| `401 Unauthorized` | Missing/invalid token | Paste token without `Bearer ` prefix in Swagger (it adds it automatically now) |
| `403 Forbidden` | Wrong role | Use correct account (User/VenueOwner/Admin) |
| `400 Bad Request` | Validation failed | Check required fields and value ranges |
| `404 Not Found` | Wrong ID | Check the ID exists in DB |
| Slot not available | Already booked or outside hours | Try a different date/time |
| Booking confirm fails | Lock expired (>10 min) | Lock the slot again |
