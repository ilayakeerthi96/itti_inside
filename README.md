# ITTI Inside

The ITTI intranet portal rebuilt on .NET 10 with an updated UI. The header and landing page sections follow the current site, and the landing page opens straight away, without signing in. The Applications, QA and Logout menu items go to the pages of the current intranet. The HR menu has three pages of its own: Corporate Medical Insurance, Holidays and Organization Training. Every link opens in a new tab. Admins sign in to the master page to update the landing page and the HR pages, and changes show straight away. This version is a prototype: the admin sign-in is a demo, and the starting content is dummy data.

## Stack

| Area | Choice |
| --- | --- |
| Runtime | .NET 10 (LTS, supported until November 2028) |
| Web | ASP.NET Core MVC with Razor views |
| Data | SQL Server 2016 or newer, through Entity Framework Core |
| UI | Bootstrap 5.3 plus a custom theme in `wwwroot/css/portal.css` |
| Tests | xUnit with in-memory integration tests, which need no database server |

Nothing loads from a CDN, so the site works on a closed network.

## Run it

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and SQL Server 2016 or newer.

In Development the site uses the default SQL Server instance on the same PC with Windows sign-in, so the database needs no password. The connection is `ConnectionStrings:Portal` in `src/Itti.Inside.Web/appsettings.Development.json`.

Passwords never go in the project or in source control. Save the admin password once in your own user secrets instead. Run this in PowerShell, with your own value in place of the capitals:

```bash
dotnet user-secrets set "Authentication:Admin:Password" 'ADMIN_PASSWORD' --project src/Itti.Inside.Web
```

Then start the site:

```bash
dotnet run --project src/Itti.Inside.Web --launch-profile http
```

Open <http://localhost:5010>: the landing page opens straight away. To update it, open <http://localhost:5010/master> and sign in with the admin user ID from `src/Itti.Inside.Web/appsettings.Development.json`, under `Authentication:Admin`, and the password you saved above.

The first run creates the `IttiInsidePortal` database and copies the starting content into it from `Seed/portal-content.json`. It creates only that database and leaves any others on the server alone. After that, the database is what counts: changing the seed file no longer changes the site.

To see the tables, open SQL Server Management Studio, connect to the server with Windows Authentication, and open **Databases**, then **IttiInsidePortal**, then **Tables**.

Run the tests:

```bash
dotnet test
```

## Master page

The admin keeps the portal up to date on the master page. Its **Main page** group has a section for everything on the landing page: Hero Slides, Latest News, CEO's Desk, Customer Feedback, Birthdays, Rockstar Awards, Wedding Bells and New Born. Its **HR pages** group has Medical Insurance, Holidays and Organization Training. Every employee sees each save straight away.

CEO's Desk is a list of dated messages. The landing page shows the start of the newest, and the CEO's Desk page shows it in full, with the earlier ones below.

### Pictures

A CEO's Desk message and a Rockstar Award can each have a picture, uploaded on its master form: JPG, PNG or WebP, up to 5 MB. The CEO's picture takes the place of the drawing on the card and the CEO's Desk page. An award's picture fills the top of its card and of its entry on the awards page. The content of each file is checked, not just its name, and SVG is refused, because it can carry script. Pictures live in the `PortalImages` table, apart from the messages and awards, and `/media/<id>` serves them. A replaced or removed picture is deleted, and so is the picture of a deleted item. Each new picture gets a new Id, so browsers may keep a picture for a year.

### HR pages

| Page | The admin enters | Employees see |
| --- | --- | --- |
| `/hr/corporate-medical-insurance` | One record: insurer, TPA, policy number, validity, sum insured, who is covered, benefits and claim steps (one per line), network hospitals link, helpdesk. Plus a policy document to download. | The policy with an "In force" badge, the benefits, the claim steps, the helpdesk and the download |
| `/hr/holidays` | Holidays one at a time, the notes under the list, or HR's Word file. A Word file with a table of dates (for example SL # / PUBLIC HOLIDAY / DATE / DAY) replaces that year's holidays. PDF and Excel files are offered for download only. | This year's holidays, the next one highlighted, optional ones marked, the notes and the file to download |
| `/hr/organization-training` | Trainings: title, category, date, time, mode, venue or link, trainer, audience, description and how to register | Upcoming trainings, soonest first, then past ones |

Uploaded files are stored in the database, so the server needs no folder for them. Each file's content is checked against its type, and downloads are sent as attachments. `WordHolidayListReader` reads the Word file without any extra library.

Only admins sign in. Everyone else opens the landing page without a sign-in page:

| | Visitor | Admin |
| --- | --- | --- |
| Signs in | Never | At `/master`, which leads to the Admin Sign In page, `/account/login` |
| Can open | The landing page and the portal pages | The master page, and the portal pages like everyone else |
| Header | The full menu | Master and Logout on the master pages, the full menu elsewhere |
| Logout in the header | Goes to the current intranet's logout page | On the master pages, ends the admin session |

The portal controllers say `[AllowAnonymous]`. Every other page, the master pages included, needs an admin session, so a new controller stays protected until it is opened up on purpose.

The demo also has an optional employee account, `Authentication:Demo`. It is switched off unless `Authentication:Demo:UserId` is set, and it opens nothing extra.

On the main page, the list cards page through their items with previous and next buttons: 2 feedback entries, 5 birthdays, 2 awards and 2 announcements at a time. The buttons appear only when a card has more than one page. The main page loads the newest 10 feedback entries and 12 awards. To change these numbers, edit `data-page-size` in the card's view and the constants in `HomeDashboardService`.

### New this month, or the last 2

Birthdays, Wedding Bells and New Born entries each have a **Posted on** date, which the master page sets to today unless the admin changes it. Their cards show what was posted this month. When nothing was posted this month, a card shows the last 2 posts instead, with a short note, so it never goes blank while there is something to show. `RecentUpdates` holds this rule. Rows that existed before the date was added count as posted on the day of that upgrade.

### Full pages

Every card links to a page with all of its items, opened in a new tab: `/customer-feedback`, `/birthdays` (grouped by the month they were posted in), `/rockstar-awards`, `/wedding-bells` and `/new-born`. They page through 10 items at a time, or 6 awards. **Read More** on an award opens `/rockstar-awards#award-<id>`, on the page that holds that award. `DetailsController` serves them, without signing in.

In the demo, the admin account is `Authentication:Admin`. A real sign-in must give the people who update the portal the `PortalRoles.Admin` role, in `AuthenticatedUser.Roles`.

Each change is logged with the user ID of the person who made it.

### Adding a field or section

1. Change the record in `Domain/Content`, and `PortalDbContext` if it is a new table.
2. Create a migration. It is applied the next time the site starts:

   ```bash
   dotnet tool restore
   dotnet ef migrations add <Name> --project src/Itti.Inside.Infrastructure --startup-project src/Itti.Inside.Infrastructure --output-dir Persistence/Migrations
   ```

3. Update the form in `Web/Models/Master/ContentForms.cs` and the view in `Web/Views/Master`.

## Project layout

```text
src/
  Itti.Inside.Domain/          Portal records: news, feedback, awards, menu items
  Itti.Inside.Application/     Interfaces and the landing page service
  Itti.Inside.Infrastructure/  SQL Server access and the dummy sign-in
    Persistence/               Database tables, migrations and first-run data
    Seed/portal-content.json   Starting content, the header menu and the pages
  Itti.Inside.Web/             Controllers, views, security and static files
tests/
  Itti.Inside.Tests/           Unit and integration tests
```

Dependencies point inward: `Web` and `Infrastructure` depend on `Application`, and `Application` depends on `Domain`. The UI never talks to a data source directly, so replacing the dummy data does not touch any page.

## Replacing the dummy parts

### Sign-in

1. Implement `ICredentialValidator` in `Itti.Inside.Infrastructure/Identity`, for example over Active Directory.
2. Add a case for it in the `switch` in `Itti.Inside.Infrastructure/DependencyInjection.cs`.
3. Set `Authentication:Provider` to the new name.

For single sign-on with Microsoft Entra ID, add the OpenID Connect handler in `Hosting/ServiceCollectionExtensions.cs` instead and retire the login form.

The cookie session, sign-in rate limit and page protection stay as they are.

### Header menu

The menu is the `menu` list in `Seed/portal-content.json`, with the QA and HR items of the current site. A path that starts with `/` is a page on this site and needs a matching entry in `pages`. A full `http://` or `https://` address goes to that site: Applications, the QA items and Logout go to the pages of the current intranet, on `inside.itti.com`. The Applications and QA placeholder pages are still in `pages`, but nothing links to them. On a computer with a mouse the dropdowns open on hover. On touch screens and in the phone menu they open on tap. To edit the menu from the site later, implement `INavigationRepository` over the database and register it in `Itti.Inside.Infrastructure/DependencyInjection.cs`.

### Links and new tabs

Every link on the portal pages opens in a new tab, so the page stays open behind it: all menu items, Home and the logo included, the card links, the breadcrumbs and **Back to Home**. In a view, add `new-tab="true"` to an `<a>` for the same behaviour. `NewTabTagHelper` adds `target="_blank"`, `rel="noopener"` and a note for screen readers. A test checks every link on the portal pages, so a new link without it fails the build. Links within a page, e-mail addresses and the links on the master pages and the Admin Sign In page stay as they are.

### Inner pages

Everything except Home and Applications is rendered by `PagesController.Show` from the `pages` list. When a page gets real functionality, add a controller for it. Its route takes priority over the placeholder.

### Logo

`wwwroot/images/brand/` holds the official logos: `inside-logo.png` for the header and the sign-in page, and `inside-emblem.png` for the sign-in page. To update the logo, replace these files and keep the same names.

## Security already in place

- The portal pages are open on purpose, with `[AllowAnonymous]`. Every other page needs an admin session.
- Sign-in cookies are `HttpOnly`, `Secure` outside Development, and `SameSite=Lax`.
- All form posts carry anti-forgery tokens, including the admin Logout.
- The master page needs the `Admin` role. Every form is checked on the server.
- Text typed into the master page is shown as plain text, so it cannot run as code on the landing page.
- Sign-in is limited to 10 attempts a minute per IP address, and return URLs are checked to block open redirects.
- A strict Content Security Policy allows no inline or third-party scripts or styles.
- HSTS, HTTPS redirection, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` and `Permissions-Policy` are set.
- Pages are sent with `Cache-Control: no-store`, so the browser's back button cannot show them after logout.
- The build fails on compiler warnings and on packages with known vulnerabilities.
- **The demo sign-in refuses to start in Production**, so it cannot go live by accident.

## Deployment

The app reads settings from `appsettings.json`, environment variables and any other standard ASP.NET Core source. Keep secrets in environment variables or a vault, never in source control.

**HTTPS is required everywhere except Development.** Over plain HTTP the sign-in page returns an error, by design, because the cookies are marked `Secure`. Either terminate HTTPS at the site itself, or at a proxy that sends `X-Forwarded-Proto` and is listed in `ReverseProxy:KnownProxies`.

### Demo server for review

Use `Staging`, because the demo sign-in does not run in `Production`. Set the database and the admin account with environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Staging
ConnectionStrings__Portal=<your SQL Server connection string>
Authentication__Admin__UserId=<choose one>
Authentication__Admin__Password=<choose one>
```

The site also runs as an IIS application under another site, for example at `/inside` under the current intranet. Its links, cookies and sign-in then stay under `/inside`.

### Database

The site updates the database itself at startup, so its database account needs permission to change tables. If your DBA runs changes instead, set `Database:ApplyMigrationsOnStartup` to `false` and create a script for them:

```bash
dotnet ef migrations script --idempotent --project src/Itti.Inside.Infrastructure --startup-project src/Itti.Inside.Infrastructure --output portal-database.sql
```

The starting content is copied only outside Production. In Production the site starts empty, and content is entered on the master page.

### IIS

1. Install the [ASP.NET Core 10 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0) on the server.
2. Publish, then copy the output to the site folder:

   ```bash
   dotnet publish src/Itti.Inside.Web -c Release -o publish
   ```

3. Create a site with an HTTPS binding and an application pool set to **No Managed Code**.

### Docker

```bash
docker build -t itti-inside .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Staging -e ConnectionStrings__Portal="<your SQL Server connection string>" -e Authentication__Admin__UserId=<choose one> -e Authentication__Admin__Password=<choose one> itti-inside
```

### Go-live checklist

- [ ] Replace the demo sign-in and set `Authentication:Provider`.
- [ ] Give the `Admin` role to the people who update the portal.
- [ ] Set `ConnectionStrings:Portal` to the real database, kept outside source control. Use a dedicated SQL Server login that can reach only this database, not `sa`.
- [ ] Back up the database: it now holds the only copy of the portal content.
- [ ] Remove the sample landing page content from `Seed/portal-content.json`.
- [ ] Set `AllowedHosts` to the real host name.
- [ ] Set `DataProtection:KeysPath` to a shared folder, so people stay signed in across restarts and servers. Protect the folder, because these keys encrypt the sign-in cookies.
- [ ] Behind a load balancer or reverse proxy, list its IP addresses in `ReverseProxy:KnownProxies`.
- [ ] Point monitoring at `/health`.
- [ ] Replace the sample links on the sign-in page.
