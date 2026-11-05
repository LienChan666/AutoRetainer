# AutoRetainer 简体中文汉化版

本仓库是 [PunishXIV/AutoRetainer](https://github.com/PunishXIV/AutoRetainer) 的简体中文汉化维护分支。

## 分支说明

- `master`：当前维护分支，跟随上游更新；每次推送后都会自动构建并发布插件。
- `deprecated`：旧版历史分支，仅供查看，不再同步、构建或发布。

## 插件仓库

在 Dalamud 的自定义插件仓库中添加：

```text
https://raw.githubusercontent.com/LienChan666/DalamudPlugins/refs/heads/master/pluginmaster.json
```

## 子模块补丁与源码准备

ECommons 和 AutoRetainerAPI 使用官方上游 URL，gitlink 固定到补丁清单的基准提交；完整本地化差异保存在 `patches/submodules/`，不再依赖汉化 fork 获取构建源码。原有两个汉化 fork 已删除；完整补丁和锁定清单保留所需差异，原提交 SHA 仅供历史溯源，应用、测试和构建均无需访问这些提交。

```bash
git clone --recurse-submodules https://github.com/LienChan666/AutoRetainer.git
cd AutoRetainer
python3 scripts/apply-submodule-patches.py
```

已有检出先执行 `git submodule sync --recursive` 和 `git submodule update --init --recursive`，再运行脚本。需要 Python 3 和 Git。CI 在构建前运行同一脚本。

清单 `patches/submodules/manifest.json` 记录官方 URL、基准、补丁 SHA-256、原汉化来源提交及预期结果树。当前 ECommons 基准为 `6e3524d0c6d229b854d300d949b6a315aab97ab3`，比根仓上游记录的 `e6be8f0` 新 67 个提交；不得为了匹配根仓记录而降级。AutoRetainerAPI 基准为 `7ccf0f6b4c7923821a43ed1e92456c9d5d7132f2`。

脚本要求两个子模块的 HEAD、主仓索引中的 gitlink 和真实子模块索引均匹配锁定基准；工作目录必须是干净基准或完整已应用的目标树。任何暂存修改、其他文件修改、未跟踪文件和忽略文件（包括构建产生的 `bin/obj`）都会阻止应用。请使用干净检出，或自行保管并移走这些文件；脚本不会自动清理它们。重复执行会重新验证补丁和完整结果树，只有精确匹配才报告已应用。

所有子模块先在临时索引中验证补丁及结果树，全部通过后才改工作目录；正常应用错误或可捕获中断会回滚本次已修改的子模块，保留原已应用状态。应用不移动 HEAD、不修改真实索引，因此主仓会显示子模块有工作目录修改，这是预期状态。运行期间不要并发修改子模块；掉电、强制终止或文件系统损坏无法提供跨目录事务保证，发生后请重新检出基准再应用，脚本会拒绝部分状态。

更新时先在临时分支合入最新根仓上游，分别确认子模块官方默认分支及原锁定基准。迁移完整差异时保留已有非文案调整（如枚举中文显示的支持），但不得顺带新增业务行为。重新生成补丁、更新 checksum 与预期树，并验证结果与审定后的汉化树完全一致。补丁必须来自固定基准到目标树的完整差异：

```bash
git -C ECommons diff --binary --full-index BASE LOCALIZED_COMMIT -- . > patches/submodules/ECommons.patch
```

同步维护另一补丁和清单，运行 `python3 scripts/test-submodule-patches.py` 做本地源码准备测试（不构建插件）。最终仍将根仓压为最新 `upstream/master` 的一个“添加中文汉化”提交，通过 Actions 构建发布。

工作流拒绝覆盖已发布的版本 ZIP。需要重新构建并发布时，先递增汉化修订号；直接手动重跑相同已发布版本会在发布步骤被拒绝。
