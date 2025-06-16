![Requirements Village](./assets/requirements-village-header.png)

# Requirements Village

*Where project ideas get laid to rest.*

Requirements Village is a full-stack application for organizing software project ideas — especially the ones you may never finish. It provides a structured home for your someday/maybe/wishlist projects and lets you define tech stacks, components, and deployment plans for each one.

---

## 🧰 Tech Stack

- **Frontend:** SvelteKit
- **Backend:** C# Minimal API
- **Authentication:** ASP.NET Core Identity  
- **Data Layer:** Entity Framework Core
- **Database:** SQLite

---

## 📂 Project Structure

```
RequirementsVillage/
├── client/          # SvelteKit frontend application
└── api/             # C# Minimal API backend
```

Each project contains its own README and setup instructions.

---

## 🚀 Getting Started

### Client (Frontend)

```bash
cd client
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`

### API (Backend)

```bash
cd api/RequirementsVillage.Api
dotnet restore
dotnet run
```

The API will be available at `https://localhost:7139` (or check console output for exact port)

---

## 🎨 Visual Design

The application features a Gothic aesthetic with polished irreverence, as demonstrated in the mockups found in `./assets/claude/`. The design balances professional SaaS functionality with a playful "project graveyard" theme.
