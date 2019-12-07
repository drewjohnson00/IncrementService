FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["IncrementService/IncrementService.csproj", "IncrementService/"]
RUN dotnet restore "IncrementService/IncrementService.csproj"
COPY . .
WORKDIR "/src/IncrementService"
RUN dotnet build "IncrementService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IncrementService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IncrementService.dll"]