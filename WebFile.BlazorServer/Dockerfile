FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WebFile.BlazorServer/WebFile.BlazorServer.csproj", "WebFile.BlazorServer/"]
RUN dotnet restore "WebFile.BlazorServer/WebFile.BlazorServer.csproj"
COPY . .
WORKDIR "/src/WebFile.BlazorServer"
RUN dotnet build "WebFile.BlazorServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebFile.BlazorServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV SQL=""
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebFile.BlazorServer.dll"]
