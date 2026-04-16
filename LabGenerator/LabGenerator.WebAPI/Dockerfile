# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./LabGenerator.sln ./
COPY ./LabGenerator.Domain/LabGenerator.Domain.csproj ./LabGenerator.Domain/
COPY ./LabGenerator.Application/LabGenerator.Application.csproj ./LabGenerator.Application/
COPY ./LabGenerator.Infrastructure/LabGenerator.Infrastructure.csproj ./LabGenerator.Infrastructure/
COPY ./LabGenerator.WebAPI/LabGenerator.WebAPI.csproj ./LabGenerator.WebAPI/

RUN dotnet restore ./LabGenerator.WebAPI/LabGenerator.WebAPI.csproj

COPY . .
RUN dotnet publish ./LabGenerator.WebAPI/LabGenerator.WebAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LabGenerator.WebAPI.dll"]