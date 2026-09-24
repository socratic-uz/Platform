---
description: Steps for committing and pushing changes across Socratic nested submodules, stream-aligned feature repositories, and the root meta-repository.
---

# Socratic Multi-Tier Submodule Commit Workflow

Because Socratic is structured as a **Federated Meta-Repository** with multi-level nested submodules, commits and pushes must strictly follow a **bottom-up sequence** (innermost submodules first, followed by intermediate meta-repositories, and finally the root repository).

> [!IMPORTANT]
> **DO NOT automatically commit or push.** Only perform Git commits or pushes when the user explicitly requests to do so.

---

## 1. Blast Radius & Breaking Change Analysis (CodeGraph)

Before committing changes across submodules, inspect public symbol modifications to verify blast radius and dependent contracts:
- Run `codegraph_impact(symbol="<ChangedSymbol>")` on any modified public interfaces, DTOs, or domain services.
- If using CLI: `codegraph affected [files...]` to identify all affected tests and downstream modules.
- Ensure no unexpected breaking changes in public contracts between submodules.

---

## 2. Pre-Commit Status Check

Check all submodules recursively for uncommitted changes:
```bash
git submodule foreach --recursive "git status -s"
```

---

## 3. Multi-Tier Bottom-Up Commit Sequence

### Tier 1: Platform, Hardware & Shared Submodules (Innermost)
If changes were made to platform or hardware submodules (`Platform`, `Hardware/*`, `SharedKernel`):
```bash
# Example for Platform:
git -C Platform status
git -C Platform add .
git -C Platform commit -m "<type>(platform): <message>"
git -C Platform push origin HEAD
```

### Tier 2: Stream-Aligned Feature Repositories
If a feature repository (`Commerce`, `POS`, `Kiosk`, `Vision`) has code changes or modified submodule pointers:
```bash
# Example for Commerce:
git -C src/Frontend/Retail/Commerce status
git -C src/Frontend/Retail/Commerce add .
git -C src/Frontend/Retail/Commerce commit -m "<type>(commerce): <message>"
git -C src/Frontend/Retail/Commerce push origin HEAD
```

### Tier 3: Frontend Umbrella Meta-Repository
If changes occurred within `src/Frontend` (or to feature pointers):
```bash
git -C src/Frontend status
git -C src/Frontend add .
git -C src/Frontend commit -m "chore(features): update feature submodule pointers"
git -C src/Frontend push origin HEAD
```

### Tier 4: Root Umbrella Meta-Repository
Finally, commit updated `src/Frontend` or `src/Backend/*` pointers in the root repository:
```bash
git status
git add src/Frontend src/Backend/*
git commit -m "chore(ecosystem): synchronize submodule pointers"
git push origin HEAD
```

---

## 4. Post-Commit Verification

Verify clean working trees across all submodules:
```bash
git status
git submodule foreach --recursive "git status"
```

