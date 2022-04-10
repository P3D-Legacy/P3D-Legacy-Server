# Stage 1: Build application
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build-env

WORKDIR /build

COPY ["src/P3D.Legacy.Common/P3D.Legacy.Common.csproj", "src/P3D.Legacy.Common/"]
COPY ["src/P3D.Legacy.Server.Abstractions/P3D.Legacy.Server.Abstractions.csproj", "src/P3D.Legacy.Server.Abstractions/"]
COPY ["src/P3D.Legacy.Server.Application/P3D.Legacy.Server.Application.csproj", "src/P3D.Legacy.Server.Application/"]
COPY ["src/P3D.Legacy.Server.Client.P3D/P3D.Legacy.Server.Client.P3D.csproj", "src/P3D.Legacy.Server.Client.P3D/"]
COPY ["src/P3D.Legacy.Server.CommunicationAPI/P3D.Legacy.Server.CommunicationAPI.csproj", "src/P3D.Legacy.Server.CommunicationAPI/"]
COPY ["src/P3D.Legacy.Server.DiscordBot/P3D.Legacy.Server.DiscordBot.csproj", "src/P3D.Legacy.Server.DiscordBot/"]
COPY ["src/P3D.Legacy.Server.GameCommands/P3D.Legacy.Server.GameCommands.csproj", "src/P3D.Legacy.Server.GameCommands/"]
COPY ["src/P3D.Legacy.Server.Infrastructure/P3D.Legacy.Server.Infrastructure.csproj", "src/P3D.Legacy.Server.Infrastructure/"]
COPY ["src/P3D.Legacy.Server.InternalAPI/P3D.Legacy.Server.InternalAPI.csproj", "src/P3D.Legacy.Server.InternalAPI/"]
COPY ["src/P3D.Legacy.Server.Statistics/P3D.Legacy.Server.Statistics.csproj", "src/P3D.Legacy.Server.Statistics/"]
COPY ["src/P3D.Legacy.Server.UI.Shared/P3D.Legacy.Server.UI.Shared.csproj", "src/P3D.Legacy.Server.UI.Shared/"]
COPY ["src/P3D.Legacy.Server.GUI/P3D.Legacy.Server.GUI.csproj", "src/P3D.Legacy.Server.GUI/"]
COPY ["src/P3D.Legacy.Server/P3D.Legacy.Server.csproj", "src/P3D.Legacy.Server/"]

RUN dotnet restore "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj"

COPY ["src/P3D.Legacy.Common/", "src/P3D.Legacy.Common/"]
COPY ["src/P3D.Legacy.Server.Abstractions/", "src/P3D.Legacy.Server.Abstractions/"]
COPY ["src/P3D.Legacy.Server.Application/", "src/P3D.Legacy.Server.Application/"]
COPY ["src/P3D.Legacy.Server.Client.P3D/", "src/P3D.Legacy.Server.Client.P3D/"]
COPY ["src/P3D.Legacy.Server.CommunicationAPI/", "src/P3D.Legacy.Server.CommunicationAPI/"]
COPY ["src/P3D.Legacy.Server.DiscordBot/", "src/P3D.Legacy.Server.DiscordBot/"]
COPY ["src/P3D.Legacy.Server.GameCommands/", "src/P3D.Legacy.Server.GameCommands/"]
COPY ["src/P3D.Legacy.Server.Infrastructure/", "src/P3D.Legacy.Server.Infrastructure/"]
COPY ["src/P3D.Legacy.Server.InternalAPI/", "src/P3D.Legacy.Server.InternalAPI/"]
COPY ["src/P3D.Legacy.Server.Statistics/", "src/P3D.Legacy.Server.Statistics/"]
COPY ["src/P3D.Legacy.Server.UI.Shared/", "src/P3D.Legacy.Server.UI.Shared/"]
COPY ["src/P3D.Legacy.Server.GUI/", "src/P3D.Legacy.Server.GUI/"]
COPY ["src/P3D.Legacy.Server/", "src/P3D.Legacy.Server/"]

COPY [".git", ".git"]

RUN /bin/bash -c 'set -ex && \
    ARCH=`uname -m` && \
    if [[ "$ARCH" == "amd64" || "$ARCH" == "x86_64" ]]; then \
       echo "x86_64" && \
       dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -c Release -o ./output --arch x64 --os debian --self-contained true /p:PublishReadyToRun=true /p:PublishReadyToRunShowWarnings=true; \
    elif [[ "$ARCH" == "aarch64" || "$ARCH" == "arm64" ]]; then \
       echo "aarch64" && \
       dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -c Release -o ./output --arch arm64 --os debian --self-contained true; \
    else \
       echo "unknown arch" && \
       dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -c Release -o ./output --arch $ARCH --os debian --self-contained true; \
    fi'

# Stage 2: Copy application artifacts into a smaller runtime environment, which is then used as our final image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-bullseye-slim

WORKDIR /app

COPY --from=build-env /build/output .

# https://github.com/opencontainers/image-spec/blob/main/annotations.md
# org.opencontainers.image.base.digest
# org.opencontainers.image.ref.name
# org.opencontainers.image.revision
# org.opencontainers.image.version
# org.opencontainers.image.created
LABEL org.opencontainers.image.title="P3D Legacy Server" \
      org.opencontainers.image.description="Server software for P3D Legacy" \
      org.opencontainers.image.url="https://github.com/P3D-Legacy/P3D-Legacy-Server" \
      org.opencontainers.image.source="https://github.com/P3D-Legacy/P3D-Legacy-Server" \
      org.opencontainers.image.documentation="https://github.com/P3D-Legacy/P3D-Legacy-Server" \
      org.opencontainers.image.author="Aragas <personal@aragas.org>" \
      org.opencontainers.image.vendor="P3D-Legacy Team" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.base.name="mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine"

EXPOSE 80/tcp
EXPOSE 15124/tcp
ENTRYPOINT ["./P3D.Legacy.Server"]