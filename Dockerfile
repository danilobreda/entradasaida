# Dockerfile para EntradaSaida
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Instalar dependências do sistema para OpenCV
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    libxext-dev \
    libxrender-dev \
    libxtst-dev \
    libgtk-3-dev \
    libglib2.0-dev \
    libcairo2-dev \
    libpango1.0-dev \
    libatk1.0-dev \
    libgdk-pixbuf2.0-dev \
    libsoup2.4-dev \
    libwebkit2gtk-4.0-dev \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/EntradaSaida.Api/EntradaSaida.Api.csproj", "src/EntradaSaida.Api/"]
COPY ["src/EntradaSaida.Core/EntradaSaida.Core.csproj", "src/EntradaSaida.Core/"]
COPY ["src/EntradaSaida.Infrastructure/EntradaSaida.Infrastructure.csproj", "src/EntradaSaida.Infrastructure/"]
COPY ["src/EntradaSaida.ML/EntradaSaida.ML.csproj", "src/EntradaSaida.ML/"]
COPY ["EntradaSaida.sln", "./"]

# Restaurar dependências
RUN dotnet restore "EntradaSaida.sln"

# Copiar código fonte
COPY . .

WORKDIR "/src/src/EntradaSaida.Api"
RUN dotnet build "EntradaSaida.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EntradaSaida.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/models /app/recordings /app/data

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EntradaSaida.Api.dll"]
