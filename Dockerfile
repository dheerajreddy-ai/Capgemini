FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/EduVoice.API/EduVoice.API.csproj", "src/EduVoice.API/"]
COPY ["src/EduVoice.Application/EduVoice.Application.csproj", "src/EduVoice.Application/"]
COPY ["src/EduVoice.Infrastructure/EduVoice.Infrastructure.csproj", "src/EduVoice.Infrastructure/"]
COPY ["src/EduVoice.Domain/EduVoice.Domain.csproj", "src/EduVoice.Domain/"]
RUN dotnet restore "src/EduVoice.API/EduVoice.API.csproj"
COPY . .
WORKDIR "/src/src/EduVoice.API"
RUN dotnet build "EduVoice.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EduVoice.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser
EXPOSE 8080
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "EduVoice.API.dll"]
