FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY Services/AuthService/AuthService.csproj Services/AuthService/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore Services/AuthService/AuthService.csproj

COPY . .
WORKDIR /src/Services/AuthService
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release -o /out /p:UseAppHost=false /p:DebugSymbols=false /p:DebugType=none

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /out/ .
ENTRYPOINT ["dotnet","AuthService.dll"]
