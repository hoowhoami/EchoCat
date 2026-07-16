# EchoCat

EchoCat 是一个桌面陪伴应用。它会以小猫的形式停留在桌面上，支持轻量互动、聊天和基础外观设置。

## 运行

```bash
DOTNET_CLI_HOME=.dotnet dotnet restore EchoCat.slnx
DOTNET_CLI_HOME=.dotnet dotnet run --project src/EchoCat.Desktop
```

如果在仓库根目录直接执行 `dotnet run` 提示找不到项目，请使用上面的 `--project` 命令。

## 基本操作

- 单击：摸摸小猫。
- 双击：打开聊天输入框。
- 拖动：移动小猫位置。
- 右键：打开菜单，可切换皮肤、置顶、隐藏、打开设置或退出。
- 托盘菜单：显示、隐藏或退出应用。

## AI 设置

默认使用离线模式。需要接入 AI 时，打开设置窗口并选择 `openai`，填写：

- Endpoint
- Model
- API Key
- 自定义提示词，可留空；留空时使用默认提示词。

设置保存后会在下一条消息生效。

## 设置

设置窗口中可以调整：

- 是否始终置顶
- 启动时是否显示欢迎气泡
- 启动后是否隐藏到托盘
- 皮肤
- AI 参数

## 备注

本项目仍在早期阶段，交互和功能会继续调整。
