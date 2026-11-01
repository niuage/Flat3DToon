# Project working rules

- Deliver polished, consistent user-facing tools and interfaces, with clear labels, useful feedback, usable interactions, and sensible defaults.
- Preserve unrelated user changes. Stage explicit paths and inspect the staged diff before committing.
- Complete, validate, review, commit, and push each meaningful step before starting the next independent step. Keep each independent bug fix in its own commit with its relevant validation. Do not accumulate completed fixes into one large commit.
- The user explicitly authorizes routine, non-force pushes of completed, reviewed task commits to `https://github.com/niuage/Flat3DToon` on the active task branch. This includes code, scenes, assets, and documentation changed for the requested task. Do not ask for confirmation again for these pushes.
- Before pushing, verify that `origin` points to that exact repository. This authorization does not cover changing remotes, force-pushing, deleting remote branches, or publishing elsewhere. If execution approval is required, cite this standing authorization and the verified destination.

## Unity automation

Use the official Unity CLI with the project's `com.unity.pipeline` package for Editor inspection, scene and asset changes, C# evaluation, compilation, tests, and captures.

Read these project-local guides when present:

- `.agents/skills/unity-cli/SKILL.md`
- `.agents/skills/unity-pipeline/SKILL.md`

Discover supported commands and parameters through CLI help and `unity command` rather than guessing. If `unity` is missing from a Windows session's PATH, try:

`& "$env:LOCALAPPDATA\Unity\bin\unity.exe"`

If the CLI cannot connect, check the Pipeline server and report any unresolved connection issue. Installing the CLI does not itself select the correct Editor.

### Editor targeting

- Before interacting with Unity, run `unity status --project-path "<absolute checkout path>" --json` and verify that the returned project path matches this checkout.
- Include an explicit `--project-path "<absolute checkout path>"` on every Editor command.
- Never fall back to another running Editor if this project's Editor is unavailable.
- Discover process IDs and ports from current status. Do not hardcode them.

### Verification

Wait for compilation and tests to finish, then inspect their actual results. A command that merely starts work is not verification.

Use targeted tests for affected behavior. Metadata-only and documentation-only changes may use static checks without running Unity tests.

## Unity Editor state

Before switching or replacing scenes, or starting any Unity test run:

1. Stop Play Mode if it is active.
2. Inspect all open scenes.
3. Save every modified scene through the Unity CLI.
4. Wait for each save to finish and verify that no open scene remains dirty.

For an untitled scene, use the CLI's C# evaluation support to save it to an explicit, unique path under `Assets/`. Do not open a Save As dialog or replace the scene to give it a name. Resolve save failures before switching scenes or running tests.

The user will not edit the Unity Editor while an agent is working and authorizes saving all existing scene changes without further confirmation. This permission covers saving scene state to disk; preserve unrelated changes and exclude them from task commits.

Always finish with Unity out of Play Mode, even if it was playing when the task began. After tests or Play Mode checks, stop Play Mode and verify that the Editor is stopped. Do not automatically enter or restore Play Mode as a handoff unless the user explicitly requests that exception.

These Editor state rules override any skill example that enters or restores Play Mode.
