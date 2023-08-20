#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 7001
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["kafkaconsumer/kafkaconsumer.csproj", "kafkaconsumer/"]
RUN dotnet restore "kafkaconsumer/kafkaconsumer.csproj"
COPY . .
WORKDIR "/src/kafkaconsumer"
RUN dotnet build "kafkaconsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "kafkaconsumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:50001
ENTRYPOINT ["dotnet", "kafkaconsumer.dll"]