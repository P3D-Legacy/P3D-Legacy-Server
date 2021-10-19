# Stage 1: Build application
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /build

COPY ["src/P3D.Legacy.Common/P3D.Legacy.Common.csproj", "src/P3D.Legacy.Common/"]
COPY ["src/P3D.Legacy.Server.Abstractions/P3D.Legacy.Server.Abstractions.csproj", "src/P3D.Legacy.Server.Abstractions/"]
COPY ["src/P3D.Legacy.Server.Infrastructure/P3D.Legacy.Server.Infrastructure.csproj", "src/P3D.Legacy.Server.Infrastructure/"]
COPY ["src/P3D.Legacy.Server.Application/P3D.Legacy.Server.Application.csproj", "src/P3D.Legacy.Server.Application/"]
COPY ["src/P3D.Legacy.Server.GameCommands/P3D.Legacy.Server.GameCommands.csproj", "src/P3D.Legacy.Server.GameCommands/"]
COPY ["src/P3D.Legacy.Server.Statistics/P3D.Legacy.Server.Statistics.csproj", "src/P3D.Legacy.Server.Statistics/"]
COPY ["src/P3D.Legacy.Server/P3D.Legacy.Server.csproj", "src/P3D.Legacy.Server/"]

RUN dotnet restore "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj"

COPY [".git/HEAD", ".git/HEAD"]
COPY [".git/config", ".git/config"]
COPY [".git/refs/heads/master", ".git/refs/heads/master"]

COPY ["src/P3D.Legacy.Common/", "src/P3D.Legacy.Common/"]
COPY ["src/P3D.Legacy.Server.Abstractions/", "src/P3D.Legacy.Server.Abstractions/"]
COPY ["src/P3D.Legacy.Server.Infrastructure/", "src/P3D.Legacy.Server.Infrastructure/"]
COPY ["src/P3D.Legacy.Server.Application/", "src/P3D.Legacy.Server.Application/"]
COPY ["src/P3D.Legacy.Server.GameCommands/", "src/P3D.Legacy.Server.GameCommands/"]
COPY ["src/P3D.Legacy.Server.Statistics/", "src/P3D.Legacy.Server.Statistics/"]
COPY ["src/P3D.Legacy.Server/", "src/P3D.Legacy.Server/"]

RUN /bin/bash -c 'set -ex && \
    ARCH=`uname -m` && \
    if [[ "$ARCH" == "amd64" || "$ARCH" == "x86_64" ]]; then \
       echo "x86_64" && \
       dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -c Release -o ./output -r alpine-x64 --self-contained true; \
    elif [[ "$ARCH" == "aarch64" || "$ARCH" == "arm64" ]]; then \
       echo "aarch64" && \
       dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -c Release -o ./output -r alpine-arm64 --self-contained true; \
    else \
       echo "unknown arch" && \
       dotnet publish "src/P3D.Legacy.Server/P3D.Legacy.Server.csproj" -c Release -o ./output --os alpine --arch $ARCH --self-contained true; \
    fi'

# Stage 2: Copy application artifacts into a smaller runtime environment, which is then used as our final image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine

WORKDIR /app

COPY --from=build-env /build/output .

LABEL org.opencontainers.image.title="P3D Legacy Server" \
      org.opencontainers.image.description="Server software for P3D Legacy" \
      org.opencontainers.image.url="https://github.com/P3D-Legacy/P3D-Legacy-Server" \
      org.opencontainers.image.source="https://github.com/P3D-Legacy/P3D-Legacy-Server" \
      org.opencontainers.image.documentation="https://github.com/P3D-Legacy/P3D-Legacy-Server" \
      org.opencontainers.image.author="Aragas <personal@aragas.org>" \
      org.opencontainers.image.vendor="P3D-Legacy Team" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.base.name="mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine"

EXPOSE 80/tcp
EXPOSE 15124/tcp
ENTRYPOINT ["./P3D.Legacy.Server"]