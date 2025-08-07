FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8081

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Services/VocabularyService/VocabularyService.csproj", "Services/VocabularyService/"]
RUN dotnet restore "Services/VocabularyService/VocabularyService.csproj"
COPY . .
WORKDIR "/src/Services/VocabularyService"
RUN dotnet build "VocabularyService.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "VocabularyService.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VocabularyService.dll"]
