FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["EntrepreneurshipSchoolBackend.csproj", "EntrepreneurshipSchoolBackend/"]
RUN dotnet restore "EntrepreneurshipSchoolBackend/EntrepreneurshipSchoolBackend.csproj"
COPY . "EntrepreneurshipSchoolBackend/"
WORKDIR "/src/EntrepreneurshipSchoolBackend"
RUN dotnet build "EntrepreneurshipSchoolBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EntrepreneurshipSchoolBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EntrepreneurshipSchoolBackend.dll"]

#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /app
#COPY *.csproj ./
#RUN dotnet restore EntrepreneurshipSchoolBackend.csproj
#COPY . ./
#RUN dotnet publish EntrepreneurshipSchoolBackend.csproj -c Release -o out
#FROM microsoft/dotnet:7.0-aspnetcore-runtime AS runtime
#WORKDIR /app
#COPY -- from=build /app/out .
#ENTRYPOINT ["dotnet", "EntrepreneurshipSchoolBackend.dll"]