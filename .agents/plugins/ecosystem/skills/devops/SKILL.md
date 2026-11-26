---
name: devops
description: >-
  CI/CD pipelines (Argo Workflows & ArgoCD), container orchestration, Kubernetes/Helm deployment with Aspirate, and Windows POS desktop installer packaging for Socratic.
  USE FOR: packaging deployments with Aspirate (aspirate generate for Docker Compose / Kubernetes Helm), configuring Argo Workflows / Argo Events / ArgoCD CI/CD pipelines (infrastructure/kubernetes/infrastructure/base/charts/argo-workflows), and building offline Windows POS installers with InnoSetup.
  DO NOT USE FOR: multi-tier Git submodule commit sequencing (use git-submodules) or C# business logic.
---

# Socratic DevOps: Aspirate, Kubernetes Helm, CI/CD & POS Installers

This skill covers containerization, Kubernetes/Helm generation via Aspirate, Argo Workflows CI/CD pipelines, and offline Windows POS installer packaging.
For multi-tier Git submodule commit sequencing, use [git-submodules](../git-submodules/SKILL.md).

---

## 1. ☸️ Aspirate & Kubernetes Helm Deployment

Socratic translates `.NET Aspire` AppHost service definitions (`src/Shared/AppHost/AppHost.cs`) into production manifests using **Aspirate**:

```powershell
# Generate Docker Compose manifest:
aspirate generate --output-format compose

# Generate Kubernetes Helm charts:
aspirate generate --output-format helm
```

The generated manifests and Helm charts are stored in `infrastructure/` and deployed to the cluster.

---

## 2. 🚀 Argo Workflows & ArgoCD CI/CD

- CI/CD pipelines, build workflows, and event-driven triggers are managed via **Argo Workflows**, **Argo Events**, and **ArgoCD** (`infrastructure/kubernetes/infrastructure/base/charts/argo-workflows`).
- Standard build configurations across all solutions and projects: `Debug`, `Release`.

---

## 3. 🖥️ POS Desktop Installer (Inno Setup)

For retail cashier terminals and standalone kiosks running on Windows 10/11 IoT Enterprise:
```powershell
iscc scripts/installer.iss
```
Generates `Output/Socratic-POS-Setup.exe` with bundled offline runtime, auto-start, watchdog scripts, and kiosk-mode lockdown policies.
