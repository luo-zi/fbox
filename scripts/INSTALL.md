# fbox 文件收集箱 — 安装指南

fbox 是一个 Windows 桌面小工具，配合 AI 使用。
把文件拖到小窗口里，AI 就能通过 CLI 命令看到你拖了哪些文件。

---

## 安装步骤

### 1. 运行安装脚本

右键 **`setup.ps1`** → 选择 **"使用 PowerShell 运行"**

安装脚本会自动完成：
- 把 `fbox.exe` 和 `fbox-gui.exe` 复制到 `%LOCALAPPDATA%\fbox`
- 把工具目录加到系统 PATH（之后可以直接在终端里输 `fbox` 命令）
- 安装 opencode 配套技能到 `~/.config/opencode/skills/fbox/`
- 在开始菜单创建 fbox 快捷方式

### 2. 重启终端

安装后需要**重新打开终端**（PowerShell / CMD），PATH 才会生效。

---

## 使用方式

### 启动桌面窗口

- 从开始菜单找到 **fbox** → 点击 **fbox-gui**
- 或双击 `fbox-gui.exe`

窗口会**始终置顶**显示在桌面上，把文件拖进虚线框即可收集。

### AI 查看文件

在终端中使用：
```bash
fbox list --new     # 只看新拖入的文件（最常用）
fbox list           # 看全部文件
fbox count          # 看总数和未读数
fbox clear          # 清空收集箱
```

每次 `fbox list` 或 `fbox list --new` 之后，收集箱会标记"已读"，
下次 `--new` 只会显示新拖入的文件。

---

## 卸载

开始菜单 → fbox → 运行 **uninstall.ps1**
或者直接删除 `%LOCALAPPDATA%\fbox` 目录。

---

## 数据位置

收集箱数据：`%APPDATA%\fbox\fbox.db`（SQLite 文件，删掉即清空）
