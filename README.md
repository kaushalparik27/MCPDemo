# MCPDemo - Employee Search MCP Server

A small demo in C# / .NET 10. It lets an AI app (for example Claude Desktop) search company employees by talking to your own database.

You ask in normal language, for example *"Who works in Finance in Mumbai?"*. The AI app decides to call a **tool**, our server runs the search, and the AI app writes the answer.

## How it works

```
AI app (Claude Desktop / MCP Inspector)
        |
        |  MCP over HTTP   (http://localhost:5072/mcp)
        v
EmployeeMcpServer   - MCP server. Has 3 tools. No database inside.
        |
        |  HTTP + JSON     (http://localhost:5232)
        v
EmployeeApi         - REST API. Uses EF Core + LINQ.
        |
        v
SQL Server database "MCPDemo"
```

| Project | What it is |
|---|---|
| `EmployeeApi` | ASP.NET Core Web API. Owns the data. Has Swagger and daily log files (log4net). |
| `EmployeeMcpServer` | ASP.NET Core app with the MCP tools. It calls `EmployeeApi` with `HttpClient`. |
| `database/` | SQL scripts: tables and 100 sample employees. |

## The tools

| Tool | Input | What it does |
|---|---|---|
| `search_employees` | `name`, `department`, `city` (all optional) | Search employees. Filters can be combined. |
| `get_employee_by_id` | `id` | Details of one employee. |
| `get_employees_by_manager` | `managerName` | The people who report directly to a manager. |

The AI app reads the **descriptions** of the tools to decide when to use them. They are in `EmployeeMcpServer/Tools/EmployeeTools.cs`.

## Database

Four tables: `Employee`, `Department`, `Location` and `LocationDepartment`. A department can exist in many locations, so `LocationDepartment` links them. An employee works in one `LocationDepartment` and can have a manager (another employee).

## What you need

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server (any edition, for example Express or LocalDB) and a way to run SQL scripts (SSMS, `sqlcmd`, ...)
- [Node.js](https://nodejs.org/) (for MCP Inspector and the `mcp-remote` bridge)
- Claude Desktop (optional, for the last step)

## 1. Set up the database

1. Create an empty database named `MCPDemo`.
2. Run these scripts in the `MCPDemo` database, in this order:
   - `database/01_create_tables.sql`
   - `database/02_sample_data.sql`
3. Open `EmployeeApi/appsettings.json` and check the connection string. By default it uses the default local instance with Windows sign-in:
   ```json
   "ConnectionStrings": {
     "MCPDemo": "Server=.;Database=MCPDemo;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   Change `Server=.` if your SQL Server has another name (for example `.\\SQLEXPRESS`).

## 2. Run the API

```bash
dotnet run --project EmployeeApi --launch-profile http
```

- API: `http://localhost:5232`
- Swagger: `http://localhost:5232/swagger`
- Health check: `http://localhost:5232/health`
- Log files: `EmployeeApi/logs/` (one file per day)

Quick test: open `http://localhost:5232/api/employees?department=Finance&city=Mumbai`.

## 3. Run the MCP server

Open a **second terminal** and keep the API running:

```bash
dotnet run --project EmployeeMcpServer --launch-profile http
```

The MCP endpoint is `http://localhost:5072/mcp`. If your API uses another address, change `EmployeeApi:BaseUrl` in `EmployeeMcpServer/appsettings.json`.

> A running server locks its `.exe` file. Stop it with `Ctrl+C` before you build again.

## 4. Test with MCP Inspector

MCP Inspector is a test page for MCP servers. You do not need to install it.

1. Open a **third terminal** and run:
   ```bash
   npx @modelcontextprotocol/inspector
   ```
   Type `y` if it asks to proceed. Open the full link it prints (usually `http://localhost:6274`, with a token).
2. Add or select a server and set:
   - **Transport type:** Streamable HTTP
   - **URL:** `http://localhost:5072/mcp`
3. Click **Connect**. The status turns **Connected**.
4. Open the **Tools** tab. You should see the 3 tools.
5. Select `search_employees`, type `Finance` in `department` and `Mumbai` in `city`, and click **Execute Tool**. You should get a JSON list of employees.

Tips:
- Type exact values. `Finance` works, `Finance only` does not.
- `get_employee_by_id` with an id that does not exist returns a tool error. This is expected.

## 5. Connect Claude Desktop

Claude Desktop cannot connect to an HTTP address on `localhost` directly. It starts local programs, so we use the small **`mcp-remote`** bridge:

```
Claude Desktop --(stdio)--> npx mcp-remote --(HTTP)--> http://localhost:5072/mcp
```

1. Make sure the API and the MCP server are running (steps 2 and 3).
2. In Claude Desktop open **Settings -> Developer -> Edit Config**.
   If you do not see **Developer**, turn it on first: **Help -> Troubleshooting -> Enable Developer Mode**.
3. In `claude_desktop_config.json`, add this inside `"mcpServers"`. Keep any servers you already have, and put a comma between entries:
   ```json
   {
     "mcpServers": {
       "employee-mcp-server": {
         "command": "npx",
         "args": ["-y", "mcp-remote", "http://localhost:5072/mcp"]
       }
     }
   }
   ```
4. Save the file. **Quit Claude Desktop completely** (right-click its icon in the system tray, then Quit) and open it again. The config is read only at startup.
5. Check **Settings -> Developer**: `employee-mcp-server` should show as running.
6. Start a new chat and ask:
   - *"Who works in Finance in Mumbai?"*
   - *"Show me employee 8."*
   - *"Who works under Priya Shah?"*

   Claude asks for your permission the first time it uses a tool.

If you add or change a tool, restart the MCP server and then restart Claude Desktop, so it reads the new tool list.

## Troubleshooting

| Problem | What to check |
|---|---|
| `address already in use` | Another copy is running on that port. Stop it, or check with `Get-NetTCPConnection -LocalPort 5072`. |
| `dotnet build` fails: file is being used by another process | Stop the running server (`Ctrl+C`), then build again. |
| Tool returns an error or no data | Is `EmployeeApi` running? Is the connection string correct? Check `EmployeeApi/logs/`. |
| No **Tools** tab in Inspector | The server must be running the latest code. Reconnect. |
| `employee-mcp-server` is not running in Claude Desktop | Check the JSON (commas, quotes), that `http://localhost:5072/mcp` is up, and that Node.js (`npx`) is installed. |
| Claude does not show the new tools | Quit Claude Desktop from the system tray and open it again. |

## Notes

- This is a local demo. There is **no login**. Do not put it on a public address with real employee data before you add authentication.
- All employees in `database/02_sample_data.sql` are made up.

## Ideas for later

- Public HTTPS URL with a tunnel (ngrok or Dev Tunnels) to try other AI apps.
- A second project with the stdio transport, as a console app.
- Authentication, and a limit on how many results a search returns.
