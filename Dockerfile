FROM mcr.microsoft.com/dotnet/sdk:7.0.407
ARG servicename
WORKDIR /app
RUN apt-get update && apt-get install -y jq && rm -rf /var/lib/apt/lists/*
COPY out/$servicename .