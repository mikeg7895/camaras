# Dockerfile para contenedor de desarrollo .NET 9 (estable)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base

# Variables de entorno recomendadas para desarrollo
ENV DOTNET_USE_POLLING_FILE_WATCHER=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NUGET_XMLDOC_MODE=skip \
    PATH=/home/vscode/.dotnet/tools:/root/.dotnet/tools:$PATH

# Usuario para desarrollo (id 1000 por defecto para VS Code)
ARG USERNAME=vscode
ARG UID=1000

# Instala utilidades comunes y crea usuario no root
RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
      ca-certificates git curl wget unzip procps iproute2 sudo gnupg2 tzdata build-essential \
      openssh-client && \
    rm -rf /var/lib/apt/lists/* && \
    useradd -m -u ${UID} -s /bin/bash ${USERNAME} && \
    echo "${USERNAME} ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/${USERNAME} && \
    chmod 0440 /etc/sudoers.d/${USERNAME}

# Directorio de trabajo
WORKDIR /workspace

# (Opcional) Instalar herramientas dotnet globales en /usr/local/share/dotnet/tools
# descomentar si desea preinstalar herramientas globales:
# RUN dotnet tool install --tool-path /usr/local/share/dotnet/tools dotnet-ef && \
#     ln -s /usr/local/share/dotnet/tools/dotnet-ef /usr/local/bin/dotnet-ef

# Exponer puertos típicos de ASP.NET Core (puede ajustar según su proyecto)
EXPOSE 5000 5001 5173

# Usar usuario no root por defecto
USER ${USERNAME}    

# Comando por defecto (mantener contenedor vivo; el IDE normalmente ejecuta sus comandos)
CMD ["tail", "-f", "/dev/null"] 