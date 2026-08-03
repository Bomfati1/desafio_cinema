# ============================================================
# Estágio 1: Build (SDK completo — ~800 MB, só usado pra compilar)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia só o .csproj primeiro (camada de cache — se as dependências
# não mudarem, o Docker reaproveita esta camada)
COPY ["Cinema.Api/Cinema.Api.csproj", "Cinema.Api/"]
RUN dotnet restore "Cinema.Api/Cinema.Api.csproj"

# Agora copia o resto do código e compila
COPY . .
WORKDIR "/src/Cinema.Api"
RUN dotnet publish "Cinema.Api.csproj" -c Release -o /app/publish --no-restore

# ============================================================
# Estágio 2: Runtime (imagem enxuta — ~120 MB, só o necessário)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Cria usuário não-root (boa prática de segurança)
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

# Porta que o Render vai expor (Render injeta a variável PORT)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copia os binários compilados do estágio 1
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Cinema.Api.dll"]