#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DevBlog.Api/DevBlog.Api.csproj", "src/DevBlog.Api/"]
COPY ["src/DevBlog.Core/DevBlog.Core.csproj", "src/DevBlog.Core/"]
COPY ["src/DevBlog.Data/DevBlog.Data.csproj", "src/DevBlog.Data/"]
RUN dotnet restore "./src/DevBlog.Api/DevBlog.Api.csproj"
COPY . .
WORKDIR "/src/src/DevBlog.Api"
RUN dotnet build "./DevBlog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DevBlog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevBlog.Api.dll"]