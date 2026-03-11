FROM mcr.microsoft.com/dotnet/aspnet:10.0.0 AS base
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for EF Core support
COPY ["src/AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj", "AgileConfig.Server.Apisite/"]
COPY ["src/AgileConfig.Server.Data.Entity/AgileConfig.Server.Data.Entity.csproj", "AgileConfig.Server.Data.Entity/"]
COPY ["src/AgileConfig.Server.Data.EFCore/AgileConfig.Server.Data.EFCore.csproj", "AgileConfig.Server.Data.EFCore/"]
COPY ["src/AgileConfig.Server.Data.Repository.EFCore/AgileConfig.Server.Data.Repository.EFCore.csproj", "AgileConfig.Server.Data.Repository.EFCore/"]
COPY ["src/Agile.Config.Protocol/Agile.Config.Protocol.csproj", "Agile.Config.Protocol/"]
COPY ["src/AgileConfig.Server.Service/AgileConfig.Server.Service.csproj", "AgileConfig.Server.Service/"]
COPY ["src/AgileConfig.Server.IService/AgileConfig.Server.IService.csproj", "AgileConfig.Server.IService/"]
COPY ["src/AgileConfig.Server.Data.Freesql/AgileConfig.Server.Data.Freesql.csproj", "AgileConfig.Server.Data.Freesql/"]
COPY ["src/AgileConfig.Server.Data.Abstraction/AgileConfig.Server.Data.Abstraction.csproj", "AgileConfig.Server.Data.Abstraction/"]
COPY ["src/AgileConfig.Server.Common/AgileConfig.Server.Common.csproj", "AgileConfig.Server.Common/"]
COPY ["src/AgileConfig.Server.OIDC/AgileConfig.Server.OIDC.csproj", "AgileConfig.Server.OIDC/"]

RUN dotnet restore "AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj"
COPY src/. .
WORKDIR "/src/AgileConfig.Server.Apisite"
RUN dotnet build "AgileConfig.Server.Apisite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgileConfig.Server.Apisite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install EF Core tools for migrations
RUN apt-get update && apt-get install -y wget
RUN dotnet tool install --global dotnet-ef --version 10.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "AgileConfig.Server.Apisite.dll"]