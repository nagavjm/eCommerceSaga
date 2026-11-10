# Deploy E-Commerce Platform to Local / Dev Kubernetes using Helm
param (
    [string]$Namespace = "ecommerce-dev",
    [string]$ReleaseName = "ecommerce-dev"
)

$chartPath = "$PSScriptRoot\ecommerce-platform"
$valuesDev = "$chartPath\values.dev.yaml"

Write-Host "Linting Helm chart at $chartPath..." -ForegroundColor Cyan
helm lint $chartPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Helm chart linting failed. Aborting."
    exit 1
}

Write-Host "Deploying $ReleaseName to namespace $Namespace using values.dev.yaml..." -ForegroundColor Cyan
helm upgrade --install $ReleaseName $chartPath `
    -f $valuesDev `
    --namespace $Namespace `
    --create-namespace

Write-Host "Deployment triggered. Checking rollout status..." -ForegroundColor Green
kubectl get pods -n $Namespace
