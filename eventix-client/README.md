# Eventix client

React + TypeScript (Vite) client for the Eventix API. RTL Hebrew, CSS Modules with CSS variables.

## Run locally

1. Start PostgreSQL (Docker container `eventix-pg`) and the API:
   ```powershell
   cd ..\Eventix.API
   dotnet run --launch-profile http   # http://localhost:5139
   ```
2. Start the client:
   ```powershell
   npm install
   npm run dev                        # http://localhost:5173
   ```
   In development Vite forwards `/api` to the API (`VITE_API_PROXY_TARGET`, default `http://localhost:5139`), so no CORS setup is needed.

## Regenerate API types

With the API running:

```powershell
npx openapi-typescript http://localhost:5139/swagger/v1/swagger.json -o src/types/api.d.ts
```

## Production

Set `VITE_API_URL` to the deployed API address and run `npm run build`. The API must allow the client's origin (CORS); it does not yet.

## Brand and photos

- Logo and icon: `public/brand/` (copied unchanged from the project's `logo` folder; `icon-32/192/512.png` are resized copies of the icon for the favicon).
- Home hero background: `public/images/hero-background.jpg`, from the brand asset `logo/eventix-hero-background.png` (see `public/images/ATTRIBUTION.md`).
- Category and equipment visuals are line icons in `src/components/CategoryIcon.tsx`.
- Palette and typography: `src/styles/tokens.css` (deep blue taken from the logo, white, orange `#FF7A00` as the only accent, Heebo).

## Demo accounts (seed)

- `producer1@eventix.dev` / `Producer123!`
- `admin@eventix.dev` / `Admin123!`
