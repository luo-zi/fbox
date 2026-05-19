---
name: fbox
description: >-
  fbox（文件收集箱）is a Windows desktop utility the user uses to share files with the AI.
  The user drags files onto a small always-on-top window, and the AI checks fbox via CLI.
  Load this skill whenever the user mentions sharing files, says "看看这个文件" / "看下文件",
  says "fbox", or when you need to access files the user has prepared for you.
---

# fbox — 文件收集箱

fbox 是一个 Windows 桌面工具。用户将文件拖入 GUI 小窗口，程序记录文件路径。
AI 通过 CLI 命令查看收集箱中的文件引用，从而知道用户想让你处理哪些文件。

---

## 核心机制：队列 & 增量

| 概念 | 说明 |
|------|------|
| **全量文件** | 所有被拖入过的文件路径，永久保留 |
| **队列（未读）** | 每次拖入新文件 → 追加到队列；有 `--new` 参数时只显示队列中的文件 |
| **消费（标记已读）** | 调用 `fbox list` 或 `fbox list --new` 后，队列被清空（全部标记已读） |
| **下次增量** | 只有在上次消费之后新拖入的文件才会出现在 `list --new` 中 |

### 工作流示例

```
# 用户拖入 a.txt, b.txt
你: fbox list --new
  → a.txt    # 队列中
  → b.txt    # 队列中，然后队列清空

# 用户拖入 c.txt, d.txt
你: fbox list --new
  → c.txt    # 仅显示新增的
  → d.txt

# 用户没拖新文件
你: fbox list --new
  → 暂无新文件  (退出码 1)
```

---

## CLI 命令

所有命令在**终端**中执行。fbox.exe 位于项目目录下。

| 命令 | 作用 | 是否消费队列 | 示例输出 |
|------|------|-------------|---------|
| `fbox list` | 显示**全部**文件路径，标记已读 | 是 | `C:\Users\...\a.txt` (每行一个) |
| `fbox list --new` | 仅显示**未读**文件路径，标记已读 | 是 | `C:\Users\...\c.txt` |
| `fbox count` | 显示总数和未读数 | 否 | `总数: 12  \|  未读: 3` |
| `fbox paths` | 显示全部路径，**不**标记已读 | 否 | `C:\Users\...\a.txt` |
| `fbox clear` | 清空收集箱 | — | `已清空收集箱` |

### 退出码

- `0` — 成功，有结果
- `1` — 无文件或错误，输出到 stderr

---

## AI 使用流程

每当用户提到"看文件"、"帮我处理这个文件"、"拖进 fbox 了"、"看看这些"等场景时：

1. **先查增量** — `fbox list --new`
   - 如果有输出：这些是新文件，用户想让你处理它们
   - 如果退出码为 1 (暂无新文件)：没有新提交的文件

2. **再查全量** — `fbox list`
   - 查看收集箱中所有文件（会消费队列）

3. **确认状态** — `fbox count`
   - 快速查看收集箱是否为空

### 重要原则

- ⚠️ 当你不知道用户指的是哪些文件时，第一件事就是 `fbox list --new`
- ✅ `list --new` 是最常用的命令，因为它只显示用户拖入后你还没看过的文件
- 处理完文件后不需要额外操作，fbox 只是路径引用，不存储文件内容
- 如果用户说"再看一下 fbox"，意味着可能又有新文件拖入了，再次调用 `fbox list --new`

---

## 安装

### 方式一：分布包安装（推荐，无需 .NET SDK）

收到 `fbox-dist.zip` 后：

1. 解压到任意目录
2. 右键 `setup.ps1` → **使用 PowerShell 运行**
3. 安装脚本会：
   - 复制 `fbox.exe` / `fbox-gui.exe` 到 `%LOCALAPPDATA%\fbox`
   - 将工具目录加入用户 PATH
   - 安装 opencode 技能到 `~/.config/opencode/skills/fbox/`
   - 创建开始菜单快捷方式

安装后重启终端即可使用 `fbox` 命令。

### 方式二：从源码构建

```bash
# 编译
cd D:\Users\zjw\source\fbox && dotnet build

# 打包分发 (生成 dist/ 目录)
.\scripts\build-dist.ps1

# 运行 GUI
dotnet run --project src\Fbox.Gui

# 运行 CLI
dotnet run --project src\Fbox.Cli -- list --new
```

### CLI 发布产物（不依赖 .NET 运行时）

```bash
.\scripts\build-dist.ps1
# 输出: dist/fbox.exe, dist/fbox-gui.exe, dist/setup.ps1, dist/skill/SKILL.md
```

`fbox.exe` 和 `fbox-gui.exe` 是自包含单文件发布，可在没有 .NET SDK 的 Windows 机器上直接运行。

---

## 数据存储位置

SQLite 数据库位于：
```
%APPDATA%\fbox\fbox.db
```
删除这个文件会清空所有数据。

窗口位置设置文件：
```
%APPDATA%\fbox\window.json
```
