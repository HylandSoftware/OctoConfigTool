FROM microsoft/dotnet:2.1.5-aspnetcore-runtime-alpine3.7
LABEL stage=run maintainer="Hyland Software, Inc." \
    org.opencontainers.image.title="OctoConfigTool" \
    org.opencontainers.image.description="An ASP.NET Core app to generate Octopus variables from json config files" \
    org.opencontainers.image.authors="Hyland Software, Inc." \
    org.opencontainers.image.vendor="Hyland Software, Inc." \
    org.opencontainers.image.source=https://github.com/HylandSoftware/OctoConfigTool \
    org.opencontainers.image.documentation=https://github.com/HylandSoftware/OctoConfigTool

WORKDIR /app

COPY build/docker .

ENTRYPOINT ["dotnet", "OctoConfigTool.dll"]
