# Eventix: השכרת ציוד לאירועים

## תיאור המערכת

**Eventix** היא מערכת Web API להשכרת ציוד לאירועים (רמקולים, מיקרופונים, תאורה, מקרנים ובמות). מפיקים נרשמים למערכת ומזמינים ציוד לטווח תאריכים של עד 7 ימים, ומנהל המחסן מנהל את הקטלוג: קטגוריות, פריטי ציוד, מחירים וכמויות.

הציוד מוגבל בכמות, ולכן הכמות הפנויה של כל פריט בכל יום היא **המשאב שמשתמשים מתחרים עליו**. כששני מפיקים מזמינים את היחידה האחרונה באותו רגע, רק אחד מהם יקבל אותה, והשני יקבל תשובת `409 Conflict`. הסבר מלא על כך נמצא בסעיף "המשאב המוגבל והתחרות עליו".

### מה המערכת עושה

- **הרשמה והתחברות** עם JWT. כל מי שנרשם הוא מפיק (`Producer`). מנהל המחסן (`Admin`) קיים כמשתמש דמו.
- **קטלוג ציוד** פתוח לקריאה לכולם: סינון לפי קטגוריה, חיפוש, טווח מחירים, מיון ועמודים (pagination אמיתי מול בסיס הנתונים). רק `Admin` יכול להוסיף, לעדכן ולהסיר.
- **זמינות** של פריט לטווח תאריכים: כמה יחידות פנויות בכל ימי הטווח.
- **הזמנה אחת עם כמה פריטים** לטווח תאריכים. ההזמנה היא "הכול או כלום": אם פריט אחד אינו זמין באחד הימים, שום דבר לא נשמר.
- **ביטול והחזרה:** מפיק מבטל הזמנה לפני תחילתה. מנהל המחסן מסמן החזרה, וגם החזרה מוקדמת, שמשחררת את הציוד מהיום שאחרי.
- **לוגים** עם NLog ו-`X-Correlation-Id` לכל בקשה, ופורמט שגיאות אחיד (`ProblemDetails`).

## טכנולוגיות

| תחום | בחירה |
|---|---|
| פלטפורמה | .NET 8, ASP.NET Core Web API |
| בסיס נתונים | PostgreSQL 16 (ב-Docker), EF Core 8 עם Npgsql, גישת Code-First |
| אימות והרשאה | JWT Bearer, שני תפקידים (`Producer`, `Admin`), סיסמאות עם BCrypt |
| מיפוי | AutoMapper 14 (DTOs נפרדים לבקשה ולתשובה) |
| לוגים | NLog (קונסול וקובץ יומי) |
| תיעוד | Swagger, כולל כפתור Authorize ל-JWT |
| בדיקות | xUnit, Moq, ו-SQLite בזיכרון |

## מבנה ה-Solution

```
Eventix.slnx
├── Eventix.Domain          ישויות, enums, IVersioned. אין אף חבילה חיצונית.
├── Eventix.Application     DTOs, ממשקים (מאגרים, IUnitOfWork, IPasswordHasher...),
│                           שירותים (הלוגיקה העסקית), מיפוי, חריגות, Options
├── Eventix.Infrastructure  AppDbContext, הגדרות Fluent API, Migrations, מאגרים,
│                           UnitOfWork, JWT ו-BCrypt, Seed
├── Eventix.API             Controllers, Middleware, Program.cs, nlog.config
└── Eventix.Tests           בדיקות xUnit
```

### כיוון התלויות

```
API ───────────────► Application ───────► Domain
 │                        ▲                  ▲
 └──► Infrastructure ─────┴──────────────────┘
        (מממש את הממשקים של Application)
```

- `Domain` לא מפנה לאף פרויקט ולא לאף חבילה.
- `Application` מפנה רק ל-`Domain`.
- `Infrastructure` מפנה ל-`Application` ול-`Domain`, ומממש את הממשקים שמוגדרים ב-`Application`.
- `API` מפנה ל-`Application` ול-`Infrastructure`, ואת `Infrastructure` הוא מכיר **רק כדי לרשום שירותים** ב-`Program.cs` (`AddApplication()` ו-`AddInfrastructure()`).
- `Eventix.Tests` מפנה ל-`Application` ול-`Infrastructure`.

## ארכיטקטורה: Clean Architecture

הפרויקט בנוי לפי **Clean Architecture**, החלוקה המודרנית של שכבות ב-.NET. הרעיון: **הלוגיקה העסקית במרכז ואינה תלויה בטכנולוגיה**. התלויות מצביעות תמיד פנימה, מהשכבות הטכניות (בסיס נתונים, HTTP) אל הליבה.

| שכבה | תפקיד | מה אסור בה |
|---|---|---|
| **Domain** | הישויות של העולם העסקי (`Equipment`, `Reservation`, `DailyStock`...) | חבילות חיצוניות, EF Core |
| **Application** | הלוגיקה העסקית (`ReservationService`, `StockAllocator`) והחוזים שהיא צריכה (`IReservationRepository`, `IUnitOfWork`) | EF Core, גישה ישירה לבסיס הנתונים |
| **Infrastructure** | המימוש הטכני: `AppDbContext`, מאגרים, JWT, BCrypt | לוגיקה עסקית |
| **API** | HTTP בלבד: Controllers, Middleware, הגדרות | `DbContext` או מאגר בתוך Controller, `new` על שירות |

**איך זה נראה בקוד:**

- **Application מגדיר ממשק, Infrastructure מממש אותו.** `IReservationRepository` ו-`IUnitOfWork` מוגדרים ב-`Application`, ו-`ReservationRepository` ו-`UnitOfWork` יושבים ב-`Infrastructure`. השירות לא יודע שמאחורי הממשק יש PostgreSQL, ולכן אפשר לבדוק אותו עם Moq.
- **ה-Controllers דקים.** `ReservationsController` מקבל בקשה, קורא ל-`IReservationService` ומחזיר סטטוס. כל כלל עסקי (בדיקת תאריכים, מלאי, בעלות) יושב ב-Service.
- **כל התלויות מוזרקות דרך ה-constructor** (Dependency Injection), והרישום נעשה במקום אחד לכל שכבה: `AddApplication()` ו-`AddInfrastructure()`.
- **משך החיים (lifetime) נבחר במודע:** `DbContext`, המאגרים והשירותים הם `Scoped`, כלומר מופע אחד לכל בקשת HTTP. עזרים חסרי מצב (`BCryptPasswordHasher`, `JwtTokenGenerator`, שעון) הם `Singleton`. אסור ש-Singleton יחזיק `DbContext`: ה-`DbContext` אינו בטוח לשימוש מכמה בקשות במקביל, והמופע היחיד היה חי כל חיי האפליקציה.

### היחס למבנה שהמורה הציגה

| מבנה בדרישות המורה | המקביל אצלנו |
|---|---|
| `Core` (ישויות, ממשקים, DTOs, enums) | `Domain` (ישויות, enums) ו-`Application` (ממשקים, DTOs) |
| `Data` (DbContext, מאגרים, migrations) | `Infrastructure` |
| `Service` (לוגיקה עסקית) | `Application` (השירותים) |
| `API` | `API` |

הדרישה המרכזית נשמרת: **`Core`/`Domain` אינו תלוי בשום דבר חיצוני** (אין EF Core בשכבות הפנימיות), ואף Controller אינו מזריק `DbContext`.

## המשאב המוגבל והתחרות עליו

**המשאב המוגבל** הוא **הכמות הפנויה של פריט ציוד ביום מסוים**. לכל פריט יש `TotalQuantity` (למשל 2 מקרנים), והמלאי נשמר בטבלת `DailyStock`: שורה לכל זוג (פריט, יום) עם `ReservedQuantity`. כמות פנויה ביום = `TotalQuantity - ReservedQuantity`.

**איפה נוצרת התחרות:** שני מפיקים שמזמינים באותו רגע את היחידה האחרונה, או טווחים חופפים חלקית (1 עד 3 מול 3 עד 5, שנוגעים שניהם בשורת היום 3), קוראים את אותו מלאי, שניהם רואים שיש פנוי, ושניהם מנסים לתפוס אותו.

**איך המערכת מגיבה:** בדיוק אחד מצליח (`201`), והשני מקבל `409 Conflict` עם הודעה שמציינת את הפריט והיום, ו**לא נשמר דבר** מההזמנה שלו (הכול או כלום).

| מצב | מה מגן |
|---|---|
| שתי הזמנות על שורת יום קיימת | `Version` (Guid) ב-`DailyStock` הוא **concurrency token**. תנאי ה-UPDATE כולל אותו, ולכן השנייה מקבלת `DbUpdateConcurrencyException` |
| שתי הזמנות שיוצרות את אותה שורת (פריט, יום) חדשה | אינדקס ייחודי `IX_DailyStocks_EquipmentId_Date`, והפרתו (PostgreSQL `23505`) מתורגמת ל-409 |
| ביטול כפול או ביטול מול החזרה | `Version` גם על `Reservation`, ושחרור המלאי קורה פעם אחת |
| הכול או כלום | הבדיקה, תפיסת המלאי ויצירת ההזמנה נעשות בקריאה אחת ל-`SaveChangesAsync` (טרנזקציה אחת) |

**איפה זה בקוד:** `StockAllocator` (Application) הוא המקום היחיד שתופס ומשחרר מלאי (`Reserve`/`Release` על טווח), ו-`ReservationService.CreateAsync` משתמש בו. `UnitOfWork` (Infrastructure) תופס את `DbUpdateConcurrencyException` ואת הפרת האינדקס הייחודי וזורק `ConcurrencyConflictException`. ה-Service רושם `LogWarning` עם המשתמש, הפריט והתאריך, וה-`ExceptionHandlingMiddleware` מחזיר 409. אין retry אוטומטי: המשתמש מקבל הודעה ומנסה שוב.

**מה מוכיח את זה:** בפרויקט `Eventix.Tests`, בדיקת SQLite בזיכרון עם **שני `DbContext` נפרדים** קוראים את אותה שורת `DailyStock`, שניהם משנים, הראשון נשמר והשני מקבל `DbUpdateConcurrencyException` (ודרך `UnitOfWork`: `ConcurrencyConflictException`).

## מעברי סטטוס והרשאות

```
Confirmed ──► Cancelled   בעלים או Admin, רק לפני יום ההתחלה (עד יום לפני האירוע). משחרר מלאי.
Confirmed ──► Returned    Admin בלבד, מיום ההתחלה והלאה. החזרה מוקדמת משחררת מלאי מהיום שאחרי.
```

`Cancelled` ו-`Returned` הם מצבים סופיים, וכל מעבר אחר מחזיר 409. **רק `Producer` יכול ליצור הזמנה**: `Admin` מקבל 403 (והלקוח כבר לא מציע לו להזמין).

## הרצה מקומית

**דרישות:** .NET 8 SDK, ‏Docker Desktop (ל-PostgreSQL), ו-Node.js 20 ומעלה (ללקוח).

**1. בסיס נתונים (PostgreSQL 16 ב-Docker):**

```powershell
docker run --name eventix-pg -e POSTGRES_PASSWORD=<בחרו סיסמה> -p 5432:5432 -d postgres:16
```

**2. סודות (User Secrets, לא נכנסים ל-git):**

```powershell
cd Eventix.API
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=EventixDb;Username=postgres;Password=<אותה סיסמה>"
dotnet user-secrets set "Jwt:Key" "<מחרוזת אקראית של 32 תווים לפחות>"
```

**3. הרצת השרת:**

```powershell
dotnet run --launch-profile http
```

בהפעלה הראשונה המערכת **יוצרת את בסיס הנתונים בעצמה**, מריצה את כל ה-migrations ומוסיפה נתוני דמו (5 קטגוריות, 15 פריטי ציוד ו-3 משתמשים). אין צורך בשום פעולה ידנית בבסיס הנתונים. ה-Swagger נמצא ב-`http://localhost:5139/swagger` (כפתור Authorize לשם הדבקת ה-token).

**4. הלקוח (React) נמצא בריפו נפרד:** https://github.com/eti5232/eventix-client

```powershell
git clone https://github.com/eti5232/eventix-client.git
cd eventix-client
npm install
npm run dev
```

האתר נפתח ב-`http://localhost:5173`. בפיתוח Vite מעביר את `/api` לשרת המקומי, ולכן אין צורך בהגדרות נוספות.

**בדיקות:** `dotnet test Eventix.slnx` (7 בדיקות, בלי Docker ובלי בסיס נתונים חיצוני).

## משתמשי דמו

נוצרים אוטומטית בהפעלה (`DbSeeder`):

| תפקיד | אימייל | סיסמה |
|---|---|---|
| `Admin` (מנהל המחסן) | `admin@eventix.dev` | `Admin123!` |
| `Producer` (מפיק) | `producer1@eventix.dev` | `Producer123!` |
| `Producer` (מפיק) | `producer2@eventix.dev` | `Producer123!` |

בלקוח, בעמוד ההתחברות, יש גם כפתורי "כניסה כלקוח" ו"כניסה כמנהל" שמכניסים את משתמשי הדמו בלחיצה אחת.

## ה-API בקצרה

| נתיב | הרשאה |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me` | פתוח (`me` דורש token). הרשמה יוצרת תמיד `Producer` |
| `GET /api/categories`, `GET /api/equipment`, `GET /api/equipment/{id}` | פתוח. סינון (`categoryId`, `search`, `minPrice`, `maxPrice`), מיון ו-`page`/`pageSize` (עד 50) |
| `GET /api/equipment/{id}/availability?from=&to=` | פתוח. הכמות הפנויה המינימלית בטווח (עד 7 ימים) |
| `POST/PUT /api/categories`, `POST/PUT/DELETE /api/equipment` | `Admin`. מחיקה היא רכה, ונחסמת (409) אם יש הזמנות פעילות |
| `POST /api/reservations` | `Producer` |
| `GET /api/reservations`, `GET /api/reservations/{id}` | מפיק רואה רק את שלו, `Admin` רואה הכול |
| `PATCH /api/reservations/{id}` (`Cancelled` / `Returned`) | לפי כללי המעברים למעלה |

סטטוסים: `200`, `201` (עם `Location`), `204`, `400`, `401`, `403` (כולל בעלות), `404`, `409`. כל השגיאות בפורמט `ProblemDetails` עם `correlationId`, וכל בקשה נושאת `X-Correlation-Id`.

## לוגים

NLog: כל בקשה נרשמת ברמת `Information`, התנגשות מלאי ברמת `Warning`, וחריגה לא צפויה ברמת `Error`. הקונטקסט כולל `CorrelationId`. סיסמאות, tokens וגוף בקשות **אינם** נרשמים. הלוגים נכתבים לקונסול ולקובץ יומי בתיקיית `logs/`.

## העלאה לענן (Render)

- **לקוח:** `https://eventix-client.onrender.com`
- **API:** `https://eventix-api-ifp2.onrender.com`

`render.yaml` מגדיר את שלושת השירותים: PostgreSQL, שרת (Docker, `Eventix.API/Dockerfile`) ולקוח (Static Site). הסודות (`Jwt__Key`, מחרוזת החיבור) מוגדרים כמשתני סביבה ולא בקוד, וה-CORS מאשר רק את כתובת הלקוח (`Cors__AllowedOrigins__0`). בתוכנית החינמית השרת "נרדם" אחרי חוסר פעילות, והבקשה הראשונה אחריו לוקחת כחצי דקה.

## מגבלות ידועות

- אין retry אוטומטי על 409 של תחרות. כשהרבה בקשות נוגעות באותן שורות, חלקן מקבלות 409 גם כשנשאר מלאי, והמשתמש מנסה שוב.
- אין refresh token, שחזור סיסמה או אימות אימייל.
- אין עריכה של הזמנה קיימת (רק ביטול והחזרה).
- חלק מבדיקות התחרות (מרוץ יצירת שורות, נעילות) נבדקו בסקריפטים מול PostgreSQL אמיתי ולא נשמרו כטסטים. בפרויקט נשמרו הבדיקות המינימליות שאושרו.
