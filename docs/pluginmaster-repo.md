# pluginmaster 仓库使用说明

`pluginmaster` 是本仓库的自定义插件仓库索引文件：`pluginmaster.json`。

## 在 XIVLauncherCN / Dalamud 中导入

导入这个链接即可：

`https://raw.githubusercontent.com/LienChan666/AutoRetainer/master/pluginmaster.json`

## 发版流程

1. 先生成插件包（`latest.zip`）并上传到 GitHub Release 资产。
2. 确保发布页可访问：`releases/latest/download/latest.zip`。
3. 发布 Release（`published`）后，GitHub Actions 会自动更新 `pluginmaster.json`：
   - `AssemblyVersion`（从 `AutoRetainer.csproj` 读取）
   - `RepoUrl`（当前仓库地址）
   - `DownloadLinkInstall / Update / Testing`
   - `LastUpdate`（Unix 时间戳）

工作流文件：`.github/workflows/Update Pluginmaster Repo.yml`
