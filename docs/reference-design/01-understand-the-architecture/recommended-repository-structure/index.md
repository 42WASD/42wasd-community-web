# Recommended repository structure

For a full-stack Bolero application:

```text
community-platform/
├── README.md
├── global.json
├── src/
│   │
│   ├── Community.Shared/
│   │   ├── Domain/
│   │   │   ├── Common.fs
│   │   │   ├── Community.fs
│   │   │   ├── Event.fs
│   │   │   ├── Project.fs
│   │   │   └── Member.fs
│   │   │
│   │   └── Contracts/
│   │       └── CommunityApi.fs
│   │
│   ├── Community.Client/
│   │   ├── App/
│   │   │   ├── Routing.fs
│   │   │   └── App.fs
│   │   │
│   │   ├── State/
│   │   │   └── Shared.fs
│   │   │
│   │   ├── Pages/
│   │   │   ├── Page.fs
│   │   │   ├── Home.fs
│   │   │   ├── About.fs
│   │   │   ├── Events.fs
│   │   │   ├── Projects.fs
│   │   │   └── Members.fs
│   │   │
│   │   ├── Ui/
│   │   │   ├── Layout.fs
│   │   │   ├── Navbar.fs
│   │   │   ├── Footer.fs
│   │   │   └── Primitives.fs
│   │   │
│   │   ├── Infrastructure/
│   │   │   ├── CommunityApi.fs
│   │   │   └── Browser.fs
│   │   │
│   │   ├── Main.fs
│   │   └── wwwroot/
│   │
│   └── Community.Server/
│       ├── Program.fs
│       ├── Services/
│       │   ├── CommunityService.fs
│       │   └── EventService.fs
│       └── Persistence/
│
└── tests/
    ├── Community.Client.Tests/
    └── Community.Server.Tests/
```

## Dependency direction

```text
Community.Shared
      ↑
      │
Client + Server

Client:
Domain/contracts
      ↑
Infrastructure / State
      ↑
Pages / features
      ↑
App composition
```

Do **not** create circular feature dependencies.