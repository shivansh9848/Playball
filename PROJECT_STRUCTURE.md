# Playball: Beginner's Guide to Project Structure & Concepts

This document explains **how the project works**, the **technologies used**, and **where to find things**. It is written for someone with **zero experience** in .NET or C#.

---

## 1. High-Level Concept: "The Restaurant Analogy"

To understand this backend application, think of a **Restaurant**:

1.  **Request**: A customer (The Frontend/Postman) sends an order.
2.  **Controller (Waiter)**: Takes the order, checks if it's valid, and passes it to the kitchen.
3.  **Service (Chef)**: The "brain" of the operation. It knows the recipes (business rules), cooks the food (logic), and asks for ingredients.
4.  **Repository (Pantry Manager)**: The only one allowed to go into the storage room (Database) to get ingredients (Data).
5.  **Database (Storage Room)**: Where raw data is stored.
6.  **Response**: The finished dish (JSON Data) is served back to the customer.

---

## 2. Key Concepts & Terms

### A. Dependency Injection (DI)
*   **Concept**: Instead of "building" tools yourself, you **ask for them**.
*   **Example**: A Chef (Service) doesn't build a stove. They ask for a stove.
*   **In Code**:
    ```csharp
    // Constructor Injection
    public GameService(IGameRepository gameRepository) 
    {
        _gameRepository = gameRepository; // "I asked for a repository, and the system gave me one."
    }
    ```

### B. DTO (Data Transfer Object)
*   **Concept**: A simple box to carry data.
*   **Why**: We don't want to expose our internal Database tables to the public. We use DTOs to only send/receive exactly what is needed.
*   **Example**:
    *   `RegisterUserRequest`: Contains only Name, Email, Password.
    *   `User`: The actual database table (contains ID, PasswordHash, CreatedAt, etc.).

### C. Entity Framework Core (EF Core)
*   **Concept**: A "Translator". It translates your C# code into SQL (Database language).
*   **How it works**: You write `_context.Users.Add(user)`, and EF Core runs `INSERT INTO Users...` automatically.

### D. JWT (JSON Web Token)
*   **Concept**: A "Digital ID Card".
*   **Flow**:
    1.  You Login -> System checks password -> System gives you a **Token string**.
    2.  For future requests (like "Create Game"), you attach this Token.
    3.  System checks the Token -> "Ah, this is John Doe, and he is an Admin." -> Access Granted.

---

## 3. Folder Structure Explained

### 📂 `Controllers` (The Waiters)
*   **Purpose**: Receive HTTP Requests (GET, POST, DELETE).
*   **Files**: `GamesController.cs`, `AuthController.cs`.
*   **Job**: Validate inputs -> Call Service -> Return `200 OK` or `400 Bad Request`.

### 📂 `Application/Services` (The Chefs)
*   **Purpose**: The Business Logic. **This is where the magic happens.**
*   **Files**:
    *   `GameService.cs`: Logic for "Is this time slot free?", "Is the user allowed to join?".
    *   `AuthService.cs`: Logic for "Hash password", "Generate Token".

### 📂 `Infrastructure/Repositories` (The Pantry Managers)
*   **Purpose**: Talk to the Database.
*   **Files**: `Repository.cs` (Generic helper), `GameRepository.cs`.
*   **Job**: `GetByIdAsync`, `AddAsync`, `SaveAsync`.

### 📂 `Domain/Entities` (The Blueprints)
*   **Purpose**: Defines what our data looks like.
*   **Files**:
    *   `User.cs`: Defines that a User has an ID, Name, Email.
    *   `Game.cs`: Defines that a Game has a StartTime, MaxPlayers.

### 📂 `Services` (Root Folder) - Background Workers
*   **Purpose**: Tasks that run automatically in the background, like a cleanup crew.
*   **Files**:
    *   `GameAutoCancelService.cs`: Runs every minute to cancel games that didn't get enough players.
    *   `SlotLockExpiryService.cs`: Unlocks slots if a user didn't pay in time.

---

## 4. Implementation Spotlights

### How Authentication Works (`AuthService.cs`)
1.  **Register**:
    *   Takes `RegisterUserRequest`.
    *   Hashes the password (converts "Password123" into random gibberish for security).
    *   Saves to DB.
    *   Creates a `Wallet` for the user.
2.  **Login**:
    *   Finds user by Email.
    *   Verifies Hash matches Password.
    *   Generates a **JWT Token** containing UserID and Role.

### How Validation Works (`CreateGameRequest.cs`)
We use **Data Annotations** (Rules written right above the variable):
```csharp
[Required] // Must be provided
[Range(2, 100)] // Must be between 2 and 100
public int MaxPlayers { get; set; }
```
If you send `MaxPlayers: 1`, the API automatically rejects it with "MaxPlayers must be between 2 and 100".

### How Waitlist Works (`WaitlistService.cs`)
1.  **Join**: Checks if Game is Full. If yes -> Adds to `Waitlist` table.
2.  **Invite**: Owner picks a user -> System moves them from `Waitlist` table to `GameParticipants` table -> Notifies user.

---

## 5. How to Read the Code
If you want to understand a specific feature, follow this path:
1.  Start at the **Controller** (e.g., `GamesController -> CreateGame`).
2.  Ctrl+Click on the **Service Method** (e.g., `_gameService.CreateGameAsync`).
3.  Read the **Logic** inside the Service.
4.  See how it uses the **Repository** (`_gameRepository.AddAsync`) to save data.
