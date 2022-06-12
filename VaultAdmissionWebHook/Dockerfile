FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "VaultAdmissionWebHook.dll"]

# eval $(minikube -p minikube docker-env)
# docker build -t canpolatoral/vault-admission-webhook:0.2 .
# docker build -t canpolatoral/vault-admission-webhook .

# docker push canpolatoral/vault-admission-webhook:0.2
# docker push canpolatoral/vault-admission-webhook