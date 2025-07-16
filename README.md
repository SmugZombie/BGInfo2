# 🖥️ BgInfo2

BgInfo2 is a Windows Forms application that overlays custom system and API data onto your desktop wallpaper. Inspired by Sysinternals BGInfo, this version allows draggable, formatted overlays with API integration, text styling, and auto-updating.

---

## 📸 Features

- ✅ Drag-and-drop overlay positioning (top-left aligned)
- 🎨 Font and color selection for text
- 🔧 Custom text formatting with system variables:
  - `{hostname}`, `{user}`, `{ip}`, `{os}`, `{cores}`
- 🌐 API integrations with support for:
  - JSON parsing (dot notation path)
  - Regex extraction from plaintext
  - Auth support: Basic / Bearer
- 🖼️ Live wallpaper preview (scaled to screen size)
- 💾 Config save/load
- 🕒 Auto-update wallpaper every 30 minutes
- 🧭 Real-time status bar showing:
  - Preview coordinates
  - Scaled (real) coordinates
- 🗂 Menu bar with:
  - API Manager
  - Select Wallpaper
  - Update Wallpaper
  - Save/Load Config
  - Exit

---

## 🚀 Getting Started

### 💻 Requirements

- Windows 10/11
- .NET 6.0+ SDK

---

### 🛠 Build

```bash
git clone https://github.com/smugzombie/BgInfo2.git
cd BgInfo2
dotnet build
```

To generate a standalone executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true
```

Executable will be in:  
`bin\Release\net6.0\win-x64\publish\BgInfoClone.exe`

---

## 🧪 Usage

1. **Select Wallpaper** from the menu
2. Drag the red box to where you want the text
3. Use the `Template Format` box to set your desired output using placeholders
4. Use `Manage APIs` to add custom data
5. Click **Update Wallpaper** to apply it
6. Wallpaper is saved and set using Windows API

---

## 📁 Config Files

- `bginfo_config.json`: Stores font, color, position, format
- `api_connections.json`: Stores API endpoints and auth

---

## 🔒 API Format

Use this placeholder:

```
{API:YourAPIName}
```

It will be replaced by the result from the corresponding API definition.

### Example API JSON Config:

```json
[
  {
    "Name": "demo",
    "Url": "https://api.example.com/data",
    "Method": "GET",
    "AuthType": "Bearer",
    "PasswordOrToken": "your-token",
    "ContentType": "json",
    "JsonKey": "data.status"
  }
]
```

---

## 💡 Tips

- Only the **top-left corner** of the red drag area is used to calculate wallpaper placement
- Works best with 1920x1080 wallpapers
- If using scaling, check your Windows DPI settings for accurate alignment

---

## 📃 License

MIT License

---

## 🙋‍♂️ Author

**Ron Egli**  
💻 GitHub: [@smugzombie](https://github.com/smugzombie)
