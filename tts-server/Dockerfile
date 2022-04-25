FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /tts-server

COPY . ./
RUN dotnet restore tts-server.csproj

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /tts-server
COPY --from=build-env /tts-server/out .
ENV ASPNETCORE_URLS=http://+:80;ws://+:80  
ENTRYPOINT ["dotnet", "tts-server.dll"]