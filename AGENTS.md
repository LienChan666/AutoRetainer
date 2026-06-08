# AGENTS.md instructions for /home/MyDalamudPlugins/AutoRetainer

除非用户明确要求兼容旧行为，否则不要为了追求最小改动而保留已知有问题的旧逻辑，也不要在现有实现外叠加无必要的判断、兜底或分支。发现某段逻辑已经错误、失效、无价值或会制造死代码时，应优先直接删除或替换为新的明确实现，而不是通过额外兼容层掩盖问题。

除非用户明确要求，否则不要在输出中展示项目内的代码片段、行号、符号引用或文件内引用；请直接输出结果语义。项目外的路径、链接和外部引用可以直接展示。

更新插件到上游并推送发布时，如果合并后没有冲突且工作区状态正常，不要在本地执行构建验证；直接推送到远端，由已经配置好的远端自动构建和发布流程处理。

# RTK - Rust Token Killer (Codex CLI)

**Usage**: Token-optimized CLI proxy for shell commands.

## Rule

Always prefix shell commands with `rtk`.

Examples:

```bash
rtk git status
rtk cargo test
rtk npm run build
rtk pytest -q
```

## Meta Commands

```bash
rtk gain            # Token savings analytics
rtk gain --history  # Recent command savings history
rtk proxy <cmd>     # Run raw command without filtering
```

## Verification

```bash
rtk --version
rtk gain
which rtk
```

@/home/ylc/.codex/RTK.md
