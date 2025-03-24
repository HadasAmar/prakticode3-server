# שלב בסיס - להריץ את האפליקציה
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# שלב בנייה - לבנות את הקוד
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app  # אנחנו משתמשים ב/app
COPY ["TodoApi.csproj", "/app"]  # העברנו את הקובץ לתיקיית /app
RUN dotnet restore "/app/TodoApi.csproj"
COPY . /app  # copying the rest of the files to /app
RUN dotnet build "/app/TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# שלב פרסום - להכין גרסה סופית
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "/app/TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# שלב סופי - להריץ את האפליקציה
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoApi.dll"]
