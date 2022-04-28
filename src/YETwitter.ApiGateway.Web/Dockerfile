#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["YETwitter.ApiGateway.Web/YETwitter.ApiGateway.Web.csproj", "YETwitter.ApiGateway.Web/"]
RUN dotnet restore "YETwitter.ApiGateway.Web/YETwitter.ApiGateway.Web.csproj"
COPY . .
WORKDIR "/src/YETwitter.ApiGateway.Web"
RUN dotnet build "YETwitter.ApiGateway.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YETwitter.ApiGateway.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YETwitter.ApiGateway.Web.dll"]