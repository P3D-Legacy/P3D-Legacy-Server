# Stage 1: Restore, Build an Publish on x64
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS publish
ARG TARGETPLATFORM
WORKDIR /build

COPY ["src/P3D.Legacy.Common/P3D.Legacy.Common.csproj", "src/P3D.Legacy.Common/"]
COPY ["src/P3D.Legacy.Server.CQERS/P3D.Legacy.Server.CQERS.csproj", "src/P3D.Legacy.Server.CQERS/"]
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
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=debian-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=debian-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=debian-arm ; \
    fi \
    && dotnet restore "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -r $RID;

COPY ["src/P3D.Legacy.Common/", "src/P3D.Legacy.Common/"]
COPY ["src/P3D.Legacy.Server.CQERS/", "src/P3D.Legacy.Server.CQERS/"]
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

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=debian-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=debian-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=debian-arm ; \
    fi \
    && dotnet build "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" --no-restore -c Release -r $RID --self-contained true;

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=debian-x64 ; \
    RTR="/p:PublishReadyToRun=false" ; \
    #RTR="/p:PublishReadyToRun=true /p:PublishReadyToRunShowWarnings=true" ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=debian-arm64 ; \
    RTR="/p:PublishReadyToRun=false" ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=debian-arm ; \
    RTR="/p:PublishReadyToRun=false" ; \
    fi \
    && dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" --no-build -c Release -o ./output -r $RID --self-contained true $RTR;


# Stage 6: Copy application artifacts into a smaller runtime environment, which is then used as our final image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime-deps:6.0-bullseye-slim

WORKDIR /app

COPY --from=publish /build/output .

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
      org.opencontainers.image.authors="Vitaly Mikhailov (Aragas) <personal@aragas.org>" \
      org.opencontainers.image.vendor="P3D-Legacy Team" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.base.name="mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine"

EXPOSE 80/tcp
EXPOSE 15124/tcp
ENTRYPOINT ["./P3D.Legacy.Server"]