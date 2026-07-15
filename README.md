# XYZ Inc. Billing Application

React frontend and ASP.NET Core API for submitting orders and processing payments through mocked gateways.

## Prerequisites

- .NET 10 SDK
- Node.js 18+

## Run locally

**Backend**

```powershell
cd backend/Billing.Api
dotnet run
```

API: http://localhost:5012  
Swagger: http://localhost:5012/swagger

**Frontend**

```powershell
cd frontend
npm install
npm start
```

UI: http://localhost:3000

**Tests**

```powershell
dotnet test backend/BillingApplication.slnx
```

## Docker

```powershell
docker compose up --build
```

## Configuration

| Setting | Default |
|---------|---------|
| `REACT_APP_API_URL` | `http://localhost:5012` |
| `Cors:Origins` in `appsettings.json` | `http://localhost:3000` |

No external API keys are required.

## API

`POST /api/orders`

```json
{
  "orderNumber": "ORD-1001",
  "userId": "user-123",
  "payableAmount": 10.00,
  "paymentGatewayId": "stripe",
  "description": "Optional note"
}
```

Success returns a receipt with `orderNumber`, `amount`, `timestamp`, and `paymentConfirmation`.

## Mock gateways

| ID | Behavior |
|----|----------|
| `stripe` | Succeeds; amount `12.34` fails transiently before succeeding |
| `paypal` | Declines amounts ending in `.99` |

## Structure

```
backend/Billing.Api/
├── Endpoints/
├── Extensions/
├── Interfaces/
├── Middleware/
├── Models/
├── PaymentGateways/
├── Services/
└── Validation/

backend/Billing.Api.Tests/
backend/Billing.Api.IntegrationTests/
frontend/src/
```
