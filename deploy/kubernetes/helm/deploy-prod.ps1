# Deploy E-Commerce Platform to AKS Production using Helm
param (
    [string]$Namespace = "ecommerce-prod",
    [string]$ReleaseName = "ecommerce-prod"
)

$chartPath = "$PSScriptRoot\ecommerce-platform"
$valuesProd = "$chartPath\values.prod.yaml"

Write-Host "Linting Helm chart at $chartPath..." -ForegroundColor Cyan
helm lint $chartPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Helm chart linting failed. Aborting."
    exit 1
}

Write-Host "Deploying $ReleaseName to namespace $Namespace using values.prod.yaml..." -ForegroundColor Cyan
helm upgrade --install $ReleaseName $chartPath `
    -f $valuesProd `
    --namespace $Namespace `
    --create-namespace

Write-Host "Production deployment triggered. Checking rollout status..." -ForegroundColor Green
kubectl get pods -n $Namespace
