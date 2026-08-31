# Petrik CleanTrack

A Petrik takarítói számára készülő, saját hardveres terminálokkal összeköthető munkaidő-nyilvántartó rendszer.

## Jelenlegi MVP

- ASP.NET Core 10 REST API
- PostgreSQL adatbázis
- React + TypeScript webes adminfelület
- Docker Compose alapú futtatás
- JWT admin hitelesítés
- dolgozók és napi elvárt munkaidő kezelése
- RFID UID hozzárendelés
- terminálok létrehozása és külön API-kulcsos hitelesítése
- automatikus BE/KI váltás kártyacsippantáskor
- offline terminál-szinkronhoz idempotens `eventId`
- 10 másodperces dupla-csippantás védelem
- kézi jelenléti esemény rögzítése audit naplóval
- mai jelenléti dashboard
- Europe/Budapest szerinti napi számítás, UTC adattárolás

## Architektúra

```text
M5Stack CoreS3 + RFID2
        | HTTPS
        v
ASP.NET Core API  <---->  PostgreSQL
        ^
        | /api
React admin ---- Nginx
```

A terminál **nem** futtat backendet vagy adatbázist. Csak leolvassa az RFID kártyát, ideiglenesen képes eseményeket tárolni, majd az API-val szinkronizál.

## Gyors indítás

1. Másold le a környezeti változókat:

```bash
cp .env.example .env
```

Windows PowerShellben:

```powershell
Copy-Item .env.example .env
```

2. A `.env` fájlban **mindenképp** változtasd meg legalább a `POSTGRES_PASSWORD`, `JWT_KEY` és `ADMIN_PASSWORD` értékét.

3. Indítás:

```bash
docker compose up --build
```

4. Adminfelület:

```text
http://localhost:8088
```

API:

```text
http://localhost:8080
```

Health check:

```text
http://localhost:8080/health
```

Az első induláskor létrejön a bootstrap admin a `.env` fájlban megadott adatokkal. A bootstrap csak üres felhasználói táblánál fut le.

## Terminál létrehozása

Az adminfelületen a **Terminálok → Új terminál** menüponttal hozz létre például egy ilyen eszközt:

```text
Device ID: PETRIK-CLEAN-01
Név: Főbejárat
```

A rendszer generál egy 256 bites eszközkulcsot. A nyers kulcsot csak létrehozáskor / újrageneráláskor mutatjuk, az adatbázisban SHA-256 hash kerül tárolásra.

## M5Stack API

### Kapcsolat ellenőrzése

```http
GET /api/terminal/ping
X-Device-Id: PETRIK-CLEAN-01
X-Device-Key: <device-api-key>
```

### Kártya csippantása

```http
POST /api/terminal/scan
Content-Type: application/json
X-Device-Id: PETRIK-CLEAN-01
X-Device-Key: <device-api-key>

{
  "eventId": "84d6bed0-8593-4b8f-acbe-0c61698ef405",
  "rfidUid": "04A793218F",
  "occurredAt": "2026-08-31T07:42:13+02:00"
}
```

Példa válasz:

```json
{
  "success": true,
  "employeeName": "Kovács Éva",
  "action": "CheckIn",
  "occurredAtUtc": "2026-08-31T05:42:13Z",
  "workedMinutesToday": 0,
  "duplicate": false
}
```

Az `eventId` a terminálon generált UUID. Ha hálózati hiba után ugyanazt az eseményt újraküldi az eszköz, a szerver felismeri, és nem hoz létre második jelenléti eseményt.

## Fontos MVP döntések

### RFID és nem biometria

A rendszer jelenleg kártya/kulcstartó UID-val számol. Ujjlenyomatot és arcfelismerést nem tárol.

### Időkezelés

Az adatbázis UTC időpontokat tárol. A napi jelenlét és munkaidő `Europe/Budapest` időzónával számolódik, így a téli/nyári időszámítás is kezelhető.

### Adatbázis séma

Az MVP jelenleg `EnsureCreatedAsync()` segítségével automatikusan létrehozza a sémát. Éles bevezetés előtt ezt EF Core migrációkra fogjuk átállítani, hogy verziózott adatbázis-frissítések legyenek.

## Következő tervezett lépések

- M5Stack CoreS3 + RFID2 firmware
- terminálos „kártya hozzárendelési mód” az adminból
- műszakok és munkarendek
- szünetek
- napi/havi munkaidő összesítés és túlóra
- hibás / hiányzó párok jelzése
- jelenléti esemény javítás teljes változásnaplóval
- Excel/PDF export
- szabadság és távollét
- több admin jogosultsági szint
- SignalR valós idejű dashboard
- OTA firmware-verzió és terminál állapotfigyelés

## Projektstruktúra

```text
petrik-cleantrack/
├── backend/        # ASP.NET Core API
├── frontend/       # React admin
├── docker-compose.yml
├── .env.example
└── README.md
```
