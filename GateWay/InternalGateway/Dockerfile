# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 8080
USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG configuration=Release
WORKDIR /src

# copy đúng đường dẫn TỪ root (chú ý đúng chữ hoa/thường)
COPY GateWay/InternalGateway/InternalGateway.csproj GateWay/InternalGateway/

RUN --mount=type=cache,id=nugetcache,target=/root/.nuget/packages \
    dotnet restore GateWay/InternalGateway/InternalGateway.csproj

# copy source còn lại
COPY . .

WORKDIR /src/GateWay/InternalGateway
RUN --mount=type=cache,id=nugetcache,target=/root/.nuget/packages \
    --mount=type=cache,id=dotnetobj,target=/src/GateWay/InternalGateway/obj \
    dotnet publish InternalGateway.csproj -c $configuration -o /app/publish \
      /p:UseAppHost=false /p:DebugSymbols=false /p:DebugType=none

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InternalGateway.dll"]
