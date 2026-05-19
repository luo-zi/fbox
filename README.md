# 📦 fbox — 给 AI 看的文件收集箱

拖文件进桌面小窗口，AI 自动 `fbox list --new` 就能拿到路径。

## 工作流

```
你拖入文件 → fbox 存到 SQLite → AI 执行 fbox list --new → 拿到路径开始处理
```

每次 `list` 自动标记已读，下次 `--new` 只看增量。

## 快速开始

```bash
# 构建
dotnet build

# 运行 GUI
dotnet run --project src\Fbox.Gui

# 查询（AI 用）
dotnet run --project src\Fbox.Cli -- list --new
```

或直接下载 [releases](https://github.com/[user]/fbox/releases) 中的 dist 包，
运行 `setup.ps1` 一键安装。

## 打包分发

```bash
.\scripts\build-dist.ps1
# 产出 dist/fbox-dist.zip，可直接发给别人
```

## 配套 opencode 技能

`skills/fbox/SKILL.md` 是 opencode 配套技能。
安装后 AI 会自动识别并调用 `fbox` 命令。

手动安装：
```bash
# 复制到 opencode 技能目录
copy skills\fbox\SKILL.md %USERPROFILE%\.config\opencode\skills\fbox\
```

## 项目结构

```
├── src/Fbox.Shared/      # SQLite 数据层 + 队列逻辑
├── src/Fbox.Cli/         # CLI (fbox.exe)
├── src/Fbox.Gui/         # WPF 桌面窗口
├── scripts/              # 打包脚本 + 安装程序
├── skills/               # opencode 配套技能
└── dist/                 # 分发包（gitignored）
```

## License

MIT
