# Kubernetes Deployment with Helm

This project standardizes **exclusively on Helm** for packaging, configuring, and deploying the complete Enterprise E-Commerce microservices ecosystem on Kubernetes and Azure Kubernetes Service (AKS).

---

## Intuitive Kubernetes Folder Structure

The Helm chart templates are partitioned into clean, sequenced functional domains so that anyone inspecting the architecture can easily understand the resource dependencies and topology:

```
deploy/kubernetes/
├── README.md                                    # This architectural and operational guide
└── helm/
    ├── deploy-dev.ps1                           # 1-Click local/Minikube deployment automation
    ├── deploy-prod.ps1                          # 1-Click AKS production deployment automation
    └── ecommerce-platform/                      # Complete Helm Chart Package
        ├── Chart.yaml                           # Chart metadata (v1.0.0)
        ├── values.yaml                          # Default production-ready baseline values
        ├── values.dev.yaml                      # Development / Minikube configuration overlay
        ├── values.prod.yaml                     # AKS Production high-availability overlay
        └── templates/
            ├── _helpers.tpl                     # Chart name, label, and selector helpers
            │
            ├── 00-base/                         # Cluster foundations
            │   ├── namespace.yaml               # Kubernetes namespace isolation
            │   ├── configmap.yaml               # Shared non-sensitive environment variables
            │   ├── secret.yaml                  # Database passwords, JWT keys, connection strings
            │   └── serviceaccount.yaml          # Azure Workload Identity / ServiceAccount binding
            │
            ├── 01-infrastructure/               # In-cluster dependencies (Dev / Testing)
            │   └── infrastructure-dev.yaml      # Embedded SQL Server 2022 & Redis deployments
            │
            ├── 02-services/                     # Core .NET 9 Clean Architecture Microservices
            │   ├── service-identity.yaml        # Identity Service (JWT Auth, RBAC)
            │   ├── service-product.yaml         # Product Catalog Service
            │   ├── service-cart.yaml            # Distributed Shopping Cart Service
            │   ├── service-inventory.yaml       # Inventory & Stock Reservation Service
            │   ├── service-order.yaml           # Order Processing Service
            │   ├── service-payment.yaml         # Payment Gateway Adapter Service
            │   ├── service-shipping.yaml        # Shipping & Dispatch Service
            │   ├── service-notification.yaml    # Email / Event Notification Service
            │   └── service-saga-orchestrator.yaml # State Machine Saga Orchestrator Service
            │
            ├── 03-gateway/                      # Edge Reverse Proxy
            │   └── service-gateway.yaml         # Ocelot API Gateway (ClusterIP :8080)
            │
            ├── 04-frontend/                     # Single Page Application
            │   └── service-frontend.yaml        # Angular 19 + Nginx production runtime (:80)
            │
            └── 05-policies/                     # Traffic routing, resilience & autoscaling
                ├── ingress.yaml                 # NGINX / AGIC Ingress (routes /api/ and /)
                ├── hpa.yaml                     # Horizontal Pod Autoscalers (CPU-based scaling)
                └── pdb.yaml                     # Pod Disruption Budgets (ensures HA during node upgrades)
```

---

## Deployment Instructions

### 1. Lint the Chart
```bash
helm lint deploy/kubernetes/helm/ecommerce-platform
```

### 2. Local / Development Deployment (Minikube / Docker Desktop / K3s)
Deploys 1 replica per service, with in-cluster SQL Server and Redis:
```bash
# Using PowerShell:
.\deploy\kubernetes\helm\deploy-dev.ps1

# Or using Helm CLI directly:
helm upgrade --install ecommerce-dev deploy/kubernetes/helm/ecommerce-platform \
  -f deploy/kubernetes/helm/ecommerce-platform/values.dev.yaml \
  --namespace ecommerce-dev \
  --create-namespace
```

### 3. Production Deployment (Azure Kubernetes Service - AKS)
Deploys 3+ HA replicas per service, connects to Azure SQL and Azure Service Bus, configures Application Gateway Ingress Controller (AGIC), and enables Horizontal Pod Autoscaling (HPA):
```bash
# Using PowerShell:
.\deploy\kubernetes\helm\deploy-prod.ps1

# Or using Helm CLI directly:
helm upgrade --install ecommerce-prod deploy/kubernetes/helm/ecommerce-platform \
  -f deploy/kubernetes/helm/ecommerce-platform/values.prod.yaml \
  --namespace ecommerce-prod \
  --create-namespace
```

### 4. Template Rendering / Dry Run
```bash
helm template ecommerce-prod deploy/kubernetes/helm/ecommerce-platform \
  -f deploy/kubernetes/helm/ecommerce-platform/values.prod.yaml
```

### 5. Teardown / Uninstall
```bash
helm uninstall ecommerce-dev --namespace ecommerce-dev
# or
helm uninstall ecommerce-prod --namespace ecommerce-prod
```
