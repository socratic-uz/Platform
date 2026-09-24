---
name: socratic-devops
description: Infrastructure, deployment, and git repository orchestration guide for Socratic. USE FOR: managing the 9 Git submodules (UI, SharedKernel, Identifying, Shopping, Ordering, Paying, gitops, docs, IaC), staging/committing changes to child repos and bumping parent pointers; packaging deployments with Aspirate (aspirate generate for Docker Compose / Kubernetes Helm); building Windows POS desktop installers with InnoSetup; and configuring CI/CD workflows (.github/workflows/Socratic.yml). DO NOT USE FOR: writing C# business logic.
---

# Socratic DevOps: Git Submodules, Aspirate, Kubernetes & Installers

The Socratic deployment and version-control ecosystem manages 9 independent Git repositories, automated Aspirate translation to Kubernetes/Helm, and offline POS installers.

---

## 1. 🌳 Git 9-Submodule Protocol

> **CRITICAL RULE**: Never make blind root commits without verifying submodule state. Do NOT commit or push automatically unless explicitly instructed by the user.

### Inspect Status Across All Submodules:
```bash
git submodule foreach --recursive "git status -s"
```

### Two-Stage Commit & Push Workflow:
1. **Commit & push inside the child submodule first**:
   ```bash
   git -C src/Frontend status -s
   git -C src/Frontend add .
   git -C src/Frontend commit -m "feat(kiosk): add responsive variant picker"
   git -C src/Frontend push origin HEAD:main
   ```
2. **Commit the updated submodule SHA pointer in root**:
   ```bash
   git add src/Frontend
   git commit -m "chore(submodule): bump Frontend to latest commit"
   git push origin HEAD:main
   ```

### Resolving Detached HEAD:
```bash
git -C <submodule_path> checkout main
git -C <submodule_path> pull origin main
```

---

## 2. ☸️ Aspirate & Kubernetes Helm Deployment

Socratic translates `.NET Aspire` AppHost service definitions into production manifests using **Aspirate**:

```powershell
# Generate Docker Compose manifest:
aspirate generate --output-format compose

# Generate Kubernetes Helm charts:
aspirate generate --output-format helm
```

The output manifests live in `gitops/` and are version-controlled in the `gitops` submodule.

---

## 3. 🖥️ POS Desktop Installer (Inno Setup)

For retail cashier terminals and standalone kiosks running on Windows 10/11 IoT Enterprise:
```powershell
iscc scripts/installer.iss
```
Generates `Output/Socratic-POS-Setup.exe` with bundled offline runtime, auto-start, watchdog scripts, and kiosk-mode lockdown policies.
