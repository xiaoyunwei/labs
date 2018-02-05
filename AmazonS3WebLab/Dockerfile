FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY *.sln ./
COPY AmazonS3WebLab/AmazonS3WebLab.csproj AmazonS3WebLab/
RUN dotnet restore
COPY . .
WORKDIR /src/AmazonS3WebLab
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY credentials /root
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AmazonS3WebLab.dll"]