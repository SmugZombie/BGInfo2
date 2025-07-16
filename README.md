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
git clone https://github.com/SmugZombie/BgInfo2.git
cd BgInfoClone
dotnet build
