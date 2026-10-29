---
name: gitops-submodules
description: >-
  Multi-tier Federated Meta-Repository and Git submodule orchestration guide for Socratic.
  USE FOR: inspecting status across nested Git submodules, bottom-up multi-tier commits and pushes (Tier 1 Platform/Hardware -> Tier 2 Feature Repos -> Tier 3 Frontend Umbrella -> Tier 4 Root Umbrella), resolving detached HEAD in submodules, and running scripts/Sync-Agents.ps1.
  DO NOT USE FOR: Kubernetes Helm or InnoSetup installers (use devops).
---

# Socratic Federated Meta-Repository & Multi-Tier Git Submodules

Full architectural reference: [rules/git-submodules-workflow.md](../../../../rules/git-submodules-workflow.md).

> **CRITICAL RULE**: Never make blind root commits without verifying submodule state. **DO NOT** commit or push automatically unless explicitly instructed by the user.

---

## 1. 🔍 Pre-Commit Status & Blast Radius Check

Inspect all submodules recursively for uncommitted changes:
```powershell
git submodule foreach --recursive "git status -s"
```

If `.agents/` files were modified at the root, synchronize and verify all submodule `.agents/` directories first:
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Sync-Agents.ps1
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Sync-Agents.ps1 -VerifyOnly
```

---

## 2. 🌳 Multi-Tier Bottom-Up Commit Sequence

Commits and pushes must strictly follow a **bottom-up sequence** (innermost submodules first, followed by intermediate meta-repositories, and finally the root repository):

1. **Tier 1 — Innermost Platform & Shared Submodules** (`src/Frontend/Platform`, `src/Shared/SharedKernel`):
   ```powershell
   git -C src/Frontend/Platform status -s
   git -C src/Frontend/Platform add .
   git -C src/Frontend/Platform commit -m "<type>(platform): <message>"
   git -C src/Frontend/Platform push origin HEAD:main
   ```
2. **Tier 2 — Stream-Aligned Feature Repositories** (`src/Frontend/Retail/*`, `src/Frontend/Portal/*`, `src/Frontend/Studio/*`, `src/Frontend/Intelligence/*`, `src/Backend/*`):
   ```powershell
   git -C src/Frontend/Retail/Commerce add .
   git -C src/Frontend/Retail/Commerce commit -m "<type>(commerce): <message>"
   git -C src/Frontend/Retail/Commerce push origin HEAD:main
   ```
3. **Tier 3 — Frontend Umbrella Meta-Repository** (`src/Frontend`):
   ```powershell
   git -C src/Frontend add .
   git -C src/Frontend commit -m "chore(frontend): update submodule pointers"
   git -C src/Frontend push origin HEAD:main
   ```
4. **Tier 4 — Root Umbrella Meta-Repository**:
   ```powershell
   git add src/Frontend src/Backend/*
   git commit -m "chore(ecosystem): synchronize submodule pointers"
   git push origin HEAD:main
   ```

---

## 3. 🔧 Resolving Detached HEAD in Submodules

```powershell
git -C <submodule_path> checkout main
git -C <submodule_path> pull origin main
```
