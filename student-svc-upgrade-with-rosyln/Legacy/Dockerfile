# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /stu-svc

# copy csproj and restore as distinct layers
COPY ./ct.stu/ct.stu.csproj .

RUN dotnet restore
ARG CT_ENV
ENV CT_ENV=$CT_ENV

ARG CT_ENV=Test

ENV ASPNETCORE_ENVIRONMENT ${CT_ENV}

# copy everything else and build app
COPY ./ct.stu .

RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /ct-stu

COPY --from=build /app .

#COPY ./ct_sc.pfx .
ARG CT_ENV
ENV CT_ENV=$CT_ENV

ARG CT_ENV=Test

ENV ASPNETCORE_ENVIRONMENT ${CT_ENV}

ENTRYPOINT ["dotnet", "ct.stu.dll"]

EXPOSE 5190
