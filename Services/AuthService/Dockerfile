# syntax=docker/dockerfile:1.7

# ===== Build & Publish (self-contained, single-file, trimmed) =====
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
ENV RUNTIME_ID=linux-musl-${TARGETARCH:-x64}
WORKDIR /src

# Cache nuget để build nhanh và layer gọn
COPY Services/AuthService/AuthService.csproj Services/AuthService/
RUN --mount=type=cache,id=nugetcache,target=/root/.nuget/packages \
    dotnet restore Services/AuthService/AuthService.csproj -r $RUNTIME_ID

# Copy code và publish
COPY . .
WORKDIR /src/Services/AuthService
RUN --mount=type=cache,id=nugetcache,target=/root/.nuget/packages \
    --mount=type=cache,id=dotnetobj,target=/src/Services/AuthService/obj \
    dotnet publish -c Release -r $RUNTIME_ID -o /out \
      --self-contained true \
      /p:PublishSingleFile=true \
      /p:PublishTrimmed=false \
      /p:TrimMode=partial \
      /p:InvariantGlobalization=true \
      /p:DebugSymbols=false /p:DebugType=none \
      /p:IncludeNativeLibrariesForSelfExtract=true

# ===== Final (siêu nhỏ, bảo mật tốt) =====
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-noble-chiseled AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 8080

# Chiseled chạy non-root mặc định (không cần USER/Shell)
COPY --from=build /out/ ./
ENTRYPOINT ["./AuthService"]
